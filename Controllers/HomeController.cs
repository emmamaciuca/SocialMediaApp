using System.Data.Common;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Data;
using SocialMediaApp.Models;

namespace SocialMediaApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext db;

    private readonly UserManager<ApplicationUser> _userManager;

    private readonly RoleManager<IdentityRole> _roleManager;

    public HomeController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<HomeController> logger)
    {
        db = context;

        _userManager = userManager;

        _roleManager = roleManager;

        _logger = logger;

    }

    
    public IActionResult Index()
    {
        if (User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index","Posts");
        }

        //utilizatorii neinregistrati pot vedea doar userii
        //au si motor de cautare
        var users = from user in db.Users
                    select user;
        
        

        //motor de cautare
        var search = "";

            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).
                Trim(); // eliminam spatiile libere 


                //Cautare dupa nume complet + username
                // sau cautare dupa nume complet "Nume Prenume"
                List<string> userIds = db.Users.Where
                                        (
                                         a => a.LastName.Contains(search)
                                         || a.FirstName.Contains(search) 
                                         || a.UserName.Contains(search)
                                         || (a.LastName + " "+ a.FirstName).Contains(search)
                                         || (a.FirstName + " "+ a.LastName).Contains(search)
                                         ).Select(a => a.Id).ToList();



                // Lista userilor care contin cuvantul cautat
                users = db.Users.Where(user => userIds.Contains(user.Id)).OrderBy(user => user.UserName);
            }
            
        ViewBag.Users = users;
        

        return View();
    }

    //pagina de show separata pentru utilizatorii neinregistrati
    public IActionResult Show(string id)
    {
        var user = db.Users.Include(u=>u.Posts).FirstOrDefault(u=> u.Id == id);
        return View(user);
    }


    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
