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
using Microsoft.AspNetCore.Authorization;
using System.Net.Mail;
using System.Net;

namespace FrindlyBot_LiB.Controllers
{
    public class BookModels1Controller : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookModels1Controller(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [Authorize(Roles="User")]

        // GET: BookModels1
        public async Task<IActionResult> Browse(string Title)
        {
            if (String.IsNullOrEmpty(Title))
            {
                var dataContext = _context.Books.OrderBy(c => c.Title);
                return View(await dataContext.ToListAsync());
            }
            else
            {
                var searchItems = await _context.Books.Where(s => s.Title.Contains(Title)).ToListAsync();
                return View(searchItems);


            }

        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> ViewBook(int id)
        {
            if (id == null || _context.Books == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .FirstOrDefaultAsync(m => m.BookID == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        [Authorize(Roles = "User")]
        public IActionResult MakeReservation()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeReservation(int id, [Bind("ReservationID,Email,StartDate,endDate,Status")] Reservations rersevations)
        {
            if (id == null || _context.Books == null)
            {
                return NotFound();
            }

            var username = _userManager.GetUserName(User);

            if (username == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .FirstOrDefaultAsync(m => m.BookID == id);
            if (book == null)
            {
                return NotFound();
            }

            DateTime today = DateTime.Today;
            if (book.Quantity <= 2)
            {

                return NotFound();
            }
            else if (rersevations.StartDate < today)
            {
                return NotFound();

            }else if(rersevations.endDate < rersevations.StartDate)
            {
                return NotFound();
            }
            else
            {    
                var reservation = new Reservations()
                {
                    Email = username,
                    Status = "Pending",
                    StartDate = rersevations.StartDate,
                    endDate = rersevations.endDate,
                    BookID = id
                };

                string AdminEmail = "blessingsMwandira4@gmail.com";
                

                SendReservationConfirmationEmail(reservation.BookID , username);
                SendToAdminConfirmationEmail(reservation.Email, reservation.BookID, AdminEmail);
                TempData["ReservationSuccessMessage"] = "Book reservation made successfully!";
                book.Quantity -= 1;
                _context.Add(reservation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Browse));

            }


        }
        private void SendReservationConfirmationEmail(int id, string recipientEmail)
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
                Subject = "Book Reserved",
                Body = $"Reservation for Book with BookID {id} has been made Successfully.",
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

        private void SendToAdminConfirmationEmail(string user,int id, string recipientEmail)
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
                Subject = "Book Reservation",
                Body = $"User {user} has successfully made a reservation of BookID = {id}",
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




        [Authorize(Roles = "User")]
        private bool BookModelExists(int id)
        {
          return (_context.Books?.Any(e => e.BookID == id)).GetValueOrDefault();
        }
    }
}
