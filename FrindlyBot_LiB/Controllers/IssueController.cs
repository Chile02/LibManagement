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
using System.Drawing;
using Hangfire;
using Microsoft.AspNetCore.Authorization;

namespace FrindlyBot_LiB.Controllers
{
    public class IssueController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public IssueController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IBackgroundJobClient backgroundJobClient)
        {
            _context = context;
            _userManager = userManager;
            _backgroundJobClient = backgroundJobClient;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string Wmail)
        {
            if (String.IsNullOrEmpty(Wmail))
            {
                var dataContext = _context.Issued.OrderBy(c => c.ReturnDate);
                return View(await dataContext.ToListAsync());
            }
            else
            {
                var searchItems = await _context.Issued.Where(s => s.Email.Contains(Wmail)).ToListAsync();
                return View(searchItems);


            }
        }





        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Return(int? id)
        {
            if (id == null || _context.Issued == null)
            {
                return NotFound();
            }
     
            var addI = await _context.Books.FindAsync(id);
            

            var issued = await _context.Issued.FirstOrDefaultAsync(m => m.IssueID == id);
            

            if (issued == null)
            {
                return NotFound();
            }

            return View(issued);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Return")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnConfirmed(int id)
        {
            if (_context.Issued == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Issued'  is null.");
            }
            var issued = await _context.Issued.FindAsync(id);
            var resID = await _context.reservations.FindAsync(issued.BookId);
            var addBook = await _context.Books.FindAsync(resID.BookID);
            if (issued != null)
            {
                addBook.Quantity += 1;
                _context.Issued.Remove(issued);
            }
            SendReturnConfirmationEmail(issued.Penalty, issued.Email);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool IssuedExists(int id)
        {
          return (_context.Issued?.Any(e => e.IssueID == id)).GetValueOrDefault();
        }


        public async Task<IActionResult> PersonalisedII(string email)
        {
            var username = _userManager.GetUserName(User);
            if (username == null)
            {
                return NotFound();
            }

            if (String.IsNullOrEmpty(email))
            {
                var dataContext = _context.Issued.OrderByDescending(c => c.IssueDate).Where(s => s.Email.Equals(username));
                return View(await dataContext.ToListAsync());
            }
            else
            {
                var searchItem = await _context.Issued.Where(s => s.Email.Contains(email)).ToListAsync();
                return View(searchItem);
            }
        }


        private void SendReturnConfirmationEmail(int pen, string recipientEmail)
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
                Subject = "Book Returned",
                Body = $"Book has been returned successfull with a penalty of K {pen}",
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
    }
}
