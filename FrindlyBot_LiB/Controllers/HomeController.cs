using FrindlyBot_LiB.Data;
using FrindlyBot_LiB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FrindlyBot_LiB.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        //public IActionResult Index()
        //{
        //    return View();
        //}

        public IActionResult AboutUs()
        {
            return View();
        }
        public async Task<IActionResult> Index(string Title)
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}