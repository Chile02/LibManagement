using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FrindlyBot_LiB.Data;
using FrindlyBot_LiB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace FrindlyBot_LiB.Controllers
{
    public class BookModelsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookModelsController(ApplicationDbContext context)
        {
            _context = context;
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
              return _context.Books != null ? 
                          View(await _context.Books.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Books'  is null.");
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Books == null)
            {
                return NotFound();
            }

            var bookModel = await _context.Books
                .FirstOrDefaultAsync(m => m.BookID == id);
            if (bookModel == null)
            {
                return NotFound();
            }

            return View(bookModel);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }



        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookModel model)
        {
            if (model.ImagePath != null && model.ImagePath.Length > 0)
            {
                var fileName = Path.GetFileName(model.ImagePath.FileName);
                var filePath = Path.Combine("wwwroot", "images", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImagePath.CopyToAsync(stream);
                }

          
                var uploaded = new BookModel()
                {
                    Title = model.Title,
                    Description = model.Description,
                    Quantity = model.Quantity,
                    Author = model.Author,
                    BookCover = fileName 
                };

                
                _context.Books.Add(uploaded);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            return View(model);
        }



        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Books == null)
            {
                return NotFound();
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, BookModel model)
        {
            if (id != model.BookID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingBook = await _context.Books.FindAsync(id);

                if (existingBook == null)
                {
                    return NotFound();
                }
                existingBook.BookID = model.BookID;
                existingBook.Title = model.Title;
                existingBook.Description = model.Description;
                existingBook.Quantity = model.Quantity;
                existingBook.Author = model.Author;

                // Check if a new book cover image is provided
                if (model.ImagePath != null && model.ImagePath.Length > 0)
                {
                    var fileName = Path.GetFileName(model.ImagePath.FileName);
                    var filePath = Path.Combine("wwwroot", "images", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImagePath.CopyToAsync(stream);
                    }

                    // Update the book cover image file name
                    existingBook.BookCover = fileName;
                    //model.BookCover = fileName;
                }

                try
                {
                    _context.Update(existingBook);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookModelExists(existingBook.BookID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }




        /*       [Authorize(Roles = "Admin")]
                [HttpGet]
                public IActionResult Edit(int id)
                {
                    var uploadedImage = _context.Books.Find(id);

                    if (uploadedImage == null)
                    {
                        return NotFound();
                    }

                    return View(uploadedImage);
                }






                 [Authorize(Roles = "Admin")]
                 [HttpPost]
                 public async Task<IActionResult> Edit(BookModel model)
                 {
                     if (ModelState.IsValid)
                     {
                         var uploadedImage = _context.Books.Find(model.BookID);

                         if (uploadedImage == null)
                         {
                             return NotFound();
                         }


                         if (model.ImagePath != null && model.ImagePath.Length > 0)
                         {

                             var fileName = Path.GetFileName(model.ImagePath.FileName);
                             var filePath = Path.Combine("wwwroot", "images", fileName);

                             using (var stream = new FileStream(filePath, FileMode.Create))
                             {
                                 await model.ImagePath.CopyToAsync(stream);
                             }

                           *//* var uploaded = new BookModel()
                            {
                                BookID = model.BookID,
                                Title = uploadedImage.Title,
                                Description = uploadedImage.Description,
                                Quantity = uploadedImage.Quantity,
                                Author = uploadedImage.Author,
                                BookCover = fileName
                            };*//*

                            model.BookCover = fileName;
                        }
                        _context.Update(model);
                        await _context.SaveChangesAsync();

                        return RedirectToAction("Index");

                     }


                     return View(model);
                 }*/





        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Books == null)
            {
                return NotFound();
            }

            var bookModel = await _context.Books
                .FirstOrDefaultAsync(m => m.BookID == id);
            if (bookModel == null)
            {
                return NotFound();
            }

            return View(bookModel);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Books == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Books'  is null.");
            }
            var bookModel = await _context.Books.FindAsync(id);
            if (bookModel != null)
            {
                _context.Books.Remove(bookModel);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookModelExists(int id)
        {
          return (_context.Books?.Any(e => e.BookID == id)).GetValueOrDefault();
        }
    }
}
