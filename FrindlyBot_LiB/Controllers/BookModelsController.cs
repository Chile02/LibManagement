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

                // Save the file path and other information to the database.
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

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var uploadedImage = _context.Books.Find(id);

            if (uploadedImage == null)
            {
                return NotFound();
            }

            var imageModel = new BookModel
            {
                BookID = uploadedImage.BookID,
                Title = uploadedImage.Title,
                Description = uploadedImage.Description,
                Quantity = uploadedImage.Quantity,
                Author = uploadedImage.Author
            };

            return View(imageModel);
        }

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

                
                uploadedImage.Title = uploadedImage.Title;
                uploadedImage.Description = uploadedImage.Description;
                uploadedImage.Quantity = uploadedImage.Quantity;
                uploadedImage.Author = uploadedImage.Author;
            

                if (model.ImagePath != null && model.ImagePath.Length > 0)
                {

                    var fileName = Path.GetFileName(model.ImagePath.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImagePath.CopyToAsync(stream);
                    }


                    uploadedImage.BookCover = filePath;
                }

                _context.Update(uploadedImage);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            // If the model state is not valid, return to the edit form with validation errors.
            return View(model);
        }

        /*
                [Authorize(Roles = "Admin")]
                public async Task<IActionResult> Edit(int? id)
                {
                    if (id == null || _context.Books == null)
                    {
                        return NotFound();
                    }

                    var bookModel = await _context.Books.FindAsync(id);
                    if (bookModel == null)
                    {
                        return NotFound();
                    }
                    return View(bookModel);
                }


                [Authorize(Roles = "Admin")]
                [HttpPost]
                [ValidateAntiForgeryToken]
                public async Task<IActionResult> Edit(int id, [Bind("BookID,Title,Author,Description,Quantity,BookCover")] BookModel model)
                {
                    if (id != model.BookID)
                    {
                        return NotFound();
                    }

                    if (ModelState.IsValid)
                    {
                        try
                        {

                            var fileName = Path.GetFileName(model.ImagePath.FileName);
                            var filePath = Path.Combine("wwwroot", "images", fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await model.ImagePath.CopyToAsync(stream);
                            }

                            // Save the file path and other information to the database.
                            var uploaded = new BookModel()
                            {
                                Title = model.Title,
                                Description = model.Description,
                                Quantity = model.Quantity,
                                Author = model.Author,
                                BookCover = fileName
                            };

                            _context.Update(model);
                            await _context.SaveChangesAsync();
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!BookModelExists(model.BookID))
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
                }*/



        // GET: BookModels/Delete/5
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
