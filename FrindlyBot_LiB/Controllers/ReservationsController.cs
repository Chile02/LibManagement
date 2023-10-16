using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FrindlyBot_LiB.Data;
using FrindlyBot_LiB.Models;
using Microsoft.AspNetCore.Identity;
using System.Net.Mail;
using System.Net;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Hangfire;

namespace FrindlyBot_LiB.Controllers
{
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBackgroundJobClient backgroundJobClient;

        public ReservationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IBackgroundJobClient backgroundJobClient)
        {
            _context = context;
            _userManager = userManager;
            this.backgroundJobClient = backgroundJobClient;
        }

       
        public async Task<IActionResult> Index(string email)
        {

            if (String.IsNullOrEmpty(email))
            {
                var dataContext = _context.reservations.OrderByDescending(c => c.Status).Where(s => s.Status.StartsWith("A"));
                return View(await dataContext.ToListAsync());
            }
            else
            {
                var searchItem = await _context.reservations.Where(s => s.Email.Contains(email) && s.Status == "Approved").ToListAsync();
                return View(searchItem);
            }
        }

     

        public IActionResult Create()
        {
            return View();
        }

 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ReservationID,Email,BookID,StartDate,endDate,Status")] Reservations reservations)
        {
            if (ModelState.IsValid)
            {
                _context.Add(reservations);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(reservations);
        }

       
        public async Task<IActionResult> Approve(int? id)
        {
            if (id == null || _context.reservations == null)
            {
                return NotFound();
            }

            var reservations = await _context.reservations.FindAsync(id);
            if (reservations == null)
            {
                return NotFound();
            }
            return View(reservations);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, [Bind("ReservationID,Email,BookID,StartDate,endDate,Status")] Reservations reservations)
        {
            if (id != reservations.ReservationID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reservations);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservationsExists(reservations.ReservationID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                SendApproveConfirmationEmail(reservations.Email);
                return RedirectToAction(nameof(Index));
            }
            return View(reservations);
        }

       
        public async Task<IActionResult> Decline(int? id)
        {
            if (id == null || _context.reservations == null)
            {
                return NotFound();
            }

            var reservations = await _context.reservations
                .FirstOrDefaultAsync(m => m.ReservationID == id);
            if (reservations == null)
            {
                return NotFound();
            }

            await SendDeclineEmailAsync(reservations.ReservationID ,reservations.Email);
            return View(reservations);
        }

        public async Task SendDeclineEmailAsync(int? id, string recipientEmail)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("chilegaming02@gmail.com", "edio qmmy tlmx rxoy"),
                EnableSsl = true,
            };
            var reservations = await _context.reservations.FirstOrDefaultAsync(m => m.ReservationID == id);

            var mailMessage = new MailMessage
            {
                From = new MailAddress("chilegaming02@gmail.com"),
                Subject = "Reservation Decline",
                Body = $"Due to some none reasons, Your reservation with reservation ID {id} Has been declined.",
                IsBodyHtml = true,
            };

            mailMessage.To.Add(recipientEmail);

            try
            {
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {

            }
        }

        [HttpPost, ActionName("Decline")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeclineConfirmed(int id)
        {
            if (_context.reservations == null)
            {
                return Problem("Entity set 'ApplicationDbContext.reservations'  is null.");
            }
            var reservations = await _context.reservations.FindAsync(id);
            var addBook = await _context.Books.FindAsync(reservations.BookID);

            if (reservations != null)
            {
                addBook.Quantity += 1;
                _context.reservations.Remove(reservations);
            }

            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Pending));
        }

        private bool ReservationsExists(int id)
        {
          return (_context.reservations?.Any(e => e.ReservationID == id)).GetValueOrDefault();
        }

  
        public async Task<IActionResult> Pending(string Email)
        {

            if (String.IsNullOrEmpty(Email))
            {
                var dataContext = _context.reservations.OrderByDescending(c => c.Status).Where(s => s.Status.StartsWith("P"));
                return View(await dataContext.ToListAsync());
            }
            else
            {
                var searc = await _context.reservations.Where(s => s.Status.Contains(Email) && s.Status == "Pending").ToListAsync();
                return View(searc);
            }
        }

        public async Task PenaltyCalculator(int id,DateTime end)
        {
            var issues = await _context.reservations.FirstOrDefaultAsync(m => m.ReservationID == id);
            var pen = await _context.Issued.FindAsync(issues.ReservationID);
            DateTime today = DateTime.Now;
            int penaltyPerDay = 50;

            if(today > end)
            {
                TimeSpan numberOfDays = end - today;
                int finalNumberOfDays = (int)numberOfDays.TotalDays;

                if (finalNumberOfDays < 1)
                {
                    pen.Penalty = 0;
                }
                else
                {
                    int totalPenalty = finalNumberOfDays * penaltyPerDay;
                    pen.Penalty = totalPenalty;
                }
            }

        }

        public IActionResult Issue()
        {
            return View();
        }

      

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Issue(int id, [Bind("IssueID,IssueDate,ReturnDate,Penalty")] Issued issueBook)
        {
            if (id == null || _context.reservations == null)
            {
                return NotFound();
            }

            var issues = await _context.reservations.FirstOrDefaultAsync(m => m.ReservationID == id);


            if (issues == null)
            {
                return NotFound();
            }
            
            var issueBooks = new Issued()
            {
                Email = issues.Email,
                BookId = id,
                IssueDate = DateTime.Now,
                ReturnDate = issues.endDate,
                Penalty = 0
            };

            TimeSpan duration =  issues.endDate - DateTime.Now;
            int numberOfDays = (int)duration.TotalDays;
            int finaldays = numberOfDays;

            
            SendCollectionEmailAsync(issueBooks.BookId, issueBooks.ReturnDate, issueBooks.Email);
            

            if(finaldays < 1)
            {
                
                backgroundJobClient.Schedule(() => SendCollectionEmailAsync(issueBooks.BookId, issueBooks.ReturnDate, issueBooks.Email), DateTime.Now.AddMinutes(10));
            }
            else
            {
                RecurringJob.AddOrUpdate("myRecurringJob", () => PenaltyCalculator(issues.ReservationID, issues.endDate), Cron.Daily);
                
                backgroundJobClient.Schedule(() => SendCollectionEmailAsync(issueBooks.BookId, issueBooks.ReturnDate, issueBooks.Email), DateTime.Now.AddDays(numberOfDays));

            }
            issues.Status = "Issued";
            _context.Add(issueBooks);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }

        public async Task<IActionResult> CancelRes(int? id)
        {
            if (id == null || _context.reservations == null)
            {
                return NotFound();
            }

            var reservations = await _context.reservations
                .FirstOrDefaultAsync(m => m.ReservationID == id);
            if (reservations == null)
            {
                return NotFound();
            }

            await SendDeclineEmailAsync(reservations.ReservationID, reservations.Email);
            return View(reservations);
        }
        [HttpPost, ActionName("CancelRes")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelResConfirmed(int id)
        {
            if (_context.reservations == null)
            {
                return Problem("Entity set 'ApplicationDbContext.reservations'  is null.");
            }
            var reservations = await _context.reservations.FindAsync(id);
            var addBook = await _context.Books.FindAsync(reservations.BookID);

            if (reservations != null)
            {
                addBook.Quantity += 1;
                _context.reservations.Remove(reservations);
            }


            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Pending));
        }





        public async Task<IActionResult> ReturnBook(int? id)
        {
            if (id == null || _context.reservations == null)
            {
                return NotFound();
            }

            var reservations = await _context.reservations
                .FirstOrDefaultAsync(m => m.ReservationID == id);
            if (reservations == null)
            {
                return NotFound();
            }

            return View(reservations);
        }

    
        [HttpPost, ActionName("ReturnBook")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmReturned(int id)
        {
            if (_context.reservations == null)
            {
                return Problem("Entity set 'ApplicationDbContext.reservations'  is null.");
            }
            var reservations = await _context.reservations.FindAsync(id);
            if (reservations != null)
            {
                _context.reservations.Remove(reservations);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        public async Task SendCollectionEmailAsync(int? id, DateTime expected, string recipientEmail)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("Chilegaming02@gmail.com", "edio qmmy tlmx rxoy"),
                EnableSsl = true,
            };
            var reservations = await _context.reservations.FirstOrDefaultAsync(m => m.ReservationID == id);

            var mailMessage = new MailMessage
            {
                From = new MailAddress("Chilegaming02@gmail.com"),
                Subject = "Book collected",
                Body = $"Your reservation with reservation ID {id} Has been issued successfully And you're expected to return it on {expected} without fail",
                IsBodyHtml = true,
            };

            mailMessage.To.Add(recipientEmail);

            try
            {
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {

            }
        }

        private void SendApproveConfirmationEmail(string recipientEmail)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("Chilegaming02@gmail.com", "edio qmmy tlmx rxoy"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("Chilegaming02@gmail.com"),
                Subject = "Reservation Approved",
                Body = "Your book reservation has been approved and your book is ready for collection.",
                IsBodyHtml = true,
            };

            mailMessage.To.Add(recipientEmail);

            try
            {
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {

            }
        }

        public async Task<IActionResult> PersonalisedRes(string email)
        {
            var username = _userManager.GetUserName(User);
            if (username == null)
            {
                return NotFound();
            }

            if (String.IsNullOrEmpty(email))
            {
                var dataContext = _context.reservations.OrderByDescending(c => c.Status).Where(s => s.Email.Equals(username) && s.Status != "Issued");
                return View(await dataContext.ToListAsync());
            }
            else
            {
                var searchItem = await _context.reservations.Where(s => s.Email.Contains(email)).ToListAsync();
                return View(searchItem);
            }
        }
       
    }
}
    

