using SocialMediaApp.Data;
using SocialMediaApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AspNetCoreGeneratedDocument;

namespace SocialMediaApp.Controllers
{
    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public GroupsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        //metode

        //Toti utilizatorii pot vedea toate grupurile existente 
        // Buton cu join - nu faci parte din grup
        // Buton cu vizualizare - faci parte din grup

        [Authorize(Roles = "User,Admin")]
        public IActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            var groups = from Group in db.Groups.Include("User")
                        select Group;
            
            ViewBag.Groups = groups;

            return View();
        }

        //Afisarea grupului cu tot cu mesaje 
        // daca faci parte din grup





        //formularul in care se completeaza datele unui nou grup
        [Authorize(Roles = "User,Admin")]
        public IActionResult New()
        {
            return View();
        }

        //adaugare in bd
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult New(Group gr)
        {
            //moderatorul este cel care creaza grupul
            gr.UserId = _userManager.GetUserId(User);

            if(ModelState.IsValid)
            {
                db.Groups.Add(gr);
                db.SaveChanges();
                TempData["message"] = "Grupul a fost adaugat";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                return View(gr);
            }
        }

        


    }
}