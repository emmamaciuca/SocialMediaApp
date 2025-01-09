using SocialMediaApp.Data;
using SocialMediaApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AspNetCoreGeneratedDocument;
using System.Runtime.CompilerServices;

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

        //metoda join group 
        //in index exista buton cu join daca nu faci deja parte din grup/ nu esti moderatorul grupului
        [HttpPost]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> Join(int id)
        {

            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.UserCurent = currentUser;

            var currentUserId = currentUser.Id;

            //daca exista deja cerere sau e membru
            var existingJoin = db.UserGroups.FirstOrDefault(u => u.GroupId == id && u.UserId == currentUserId);

            //daca nu exista se creeaza o noua cerere
            if(existingJoin == null)
            {
                var userGroup = new UserGroup
                {
                    UserId = currentUserId,
                    GroupId = id,
                    Status = "Pending"
                };

                db.UserGroups.Add(userGroup);
                db.SaveChanges();
                TempData["message"] = "Cererea a fost trimisa";
                TempData["messageType"] = "alert-success";
            }
            else
            {
                TempData["message"] = "Cererea exista deja";
                TempData["messageType"] = "alert-danger";
            }

            return RedirectToAction("Index");
        }

        //metoda accept join - pentru moderator sa accepte cererile de join
        [HttpPost]
        public IActionResult AcceptJoin (int userGroupId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var userGroup = db.UserGroups.Include(u => u.Group)
                                          .FirstOrDefault(u => u.Id == userGroupId);

            //daca exista cerera si utiliztorul curent este moderatorul 
            if(userGroup != null && userGroup.Group != null && userGroup.Group.UserId == currentUserId)
            {
                userGroup.Status = "Accepted";
                db.SaveChanges();
                TempData["message"] = "Cererea a fost acceptata";
                TempData["messageType"] = "alert-success";
            }
            else
            {
                TempData["message"] = "Nu puteti accepta cererea";
                TempData["messageType"] = "alert-danger";
            }

            return RedirectToAction("Show", new {id = userGroup.GroupId});
            
        }

        //metoda reject join - pentru moderator sa respinga cererile de join
        [HttpPost]
        public IActionResult RejectJoin (int userGroupId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var userGroup = db.UserGroups.Include(u => u.Group)
                                         .FirstOrDefault(u => u.Id == userGroupId);

            //daca exista cerera si utiliztorul curent este moderatorul 
            if(userGroup != null && userGroup.Group != null && userGroup.Group.UserId == currentUserId)
            {
                db.UserGroups.Remove(userGroup);                
                db.SaveChanges();
                TempData["message"] = "Cererea a fost respinsa";
                TempData["messageType"] = "alert-danger";
            }
            else
            {
                TempData["message"] = "Nu puteti respinge cererea";
                TempData["messageType"] = "alert-danger";
            }

            return RedirectToAction("Show", new {id = userGroup.GroupId});
        }


        //Toti utilizatorii pot vedea toate grupurile existente 
        // Buton cu join - nu faci parte din grup
        // Buton cu vizualizare - faci parte din grup

        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            var groups = from Group in db.Groups.Include("User")
                        select Group;
            
            ViewBag.Groups = groups;

            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.UserCurent = currentUser;

            
            //grupurile in care utilizatorul e membru
            var userGroupsId = db.UserGroups.Where(u => u.UserId == currentUser.Id && u.Status == "Accepted")
                                            .Select(u => u.GroupId)
                                            .ToList();

            ViewBag.UserGroupsId = userGroupsId;

            return View();
        }


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


        //metoda show group
        //fiecare caseta de grup are buton de see group - daca esti moderator sau faci deja parte din grup
        //aici se vad toate persoanele din grup 
        //daca esti moderator la fiecare persoana ai buton de elimina 
        //daca esti utilizator ai buton de iesire grup

        public async Task<IActionResult> Show(int id)
        {
            
            var group = db.Groups.Include(g => g.User)  // Include moderatorul grupului
                                 .Include(g => g.UserGroups)
                                 .ThenInclude(ug => ug.User)  // Include membrii grupului
                                 .Include(g => g.Messages)
                                 .FirstOrDefault(g => g.Id == id);

                     
            if(group == null)
            {
                TempData["message"] = "Grupul nu a fost gasit";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.UserCurent = currentUser;

            // Verificari pentru a nu putea face Show/id si sa vezi grupul chiar daca nu esti moderator/faci parte din grup
            bool isMemberOrModerator = group.User.Id == currentUser.Id || 
                await db.UserGroups.AnyAsync(ug => 
                    ug.GroupId == id && 
                    ug.UserId == currentUser.Id && 
                    ug.Status == "Accepted");

            if (!isMemberOrModerator && !User.IsInRole("Admin"))
            {
                TempData["message"] = "Nu aveți acces la acest grup.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            //cererile active
            var requests = db.UserGroups.Include( u => u.User)
                                        .Where(u => u.GroupId == id && u.Status == "Pending")
                                        .ToList();
            ViewBag.Requests = requests;

            //membrii grupului
            var members = db.UserGroups.Include(u => u.User) // Include User pentru a avea informațiile despre utilizatori
                                        .Where(u => u.GroupId == id && u.Status == "Accepted")
                                        .Select(u => u.User) // Extragem doar utilizatorii
                                        .ToList();
            ViewBag.Members = members;



            return View(group);
        }


        //afisare mesaje
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Show([FromForm] Message message)
        {
            message.Date = DateTime.Now;

            // preluam Id-ul utilizatorului care posteaza mesajul
            message.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Messages.Add(message);
                db.SaveChanges();   //aici se adauga comentariul in baza de date
                return Redirect("/Groups/Show/" + message.GroupId);
            }
            else
            {  
                var group = db.Groups.Include(g => g.User)  // Include moderatorul grupului
                                 .Include(g => g.UserGroups)
                                 .ThenInclude(ug => ug.User)  // Include membrii grupului
                                 .Include(g => g.Messages)
                                 .FirstOrDefault(g => g.Id == message.GroupId);

                if(group == null)
                {
                    TempData["message"] = "Grupul nu a fost gasit";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index");
                }

                var currentUser = await _userManager.GetUserAsync(User);
                ViewBag.UserCurent = currentUser;

                //cererile active
                var requests = db.UserGroups.Include( u => u.User)
                                            .Where(u => u.GroupId == message.GroupId && u.Status == "Pending")
                                            .ToList();
                ViewBag.Requests = requests;

                //membrii grupului
                var members = db.UserGroups.Include(u => u.User) // Include User pentru a avea informațiile despre utilizatori
                                            .Where(u => u.GroupId == message.GroupId && u.Status == "Accepted")
                                            .Select(u => u.User) // Extragem doar utilizatorii
                                            .ToList();
                ViewBag.Members = members;

                return View(group);
            }
        }



        //metoda de parasire a grupului
        [HttpPost]
        public async Task<IActionResult> Leave(int groupId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.UserCurent = currentUser;
            
            var userGroup = db.UserGroups.FirstOrDefault(u => u.UserId == currentUser.Id && u.GroupId == groupId);

            //se sterge cerera din userGroups
            if(userGroup != null)
            {
                db.UserGroups.Remove(userGroup);
                db.SaveChanges();
                TempData["Message"] = "Ai părăsit grupul";
                TempData["messageType"] = "alert-danger";
            }
            else
            {
                TempData["Message"] = "Nu esti membru al acestui grup";
                TempData["messageType"] = "alert-danger";
            }

            return RedirectToAction("Index", "Groups");
        }

        //metoda pentru eliminarea unui membru de catre moderator
        [HttpPost]
        public async Task<IActionResult> Remove(string userId,int groupId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.UserCurent = currentUser;

            var userGroup = db.UserGroups.FirstOrDefault(u => u.UserId == userId && u.GroupId == groupId);
            if(userGroup != null)
            {
                //daca utilizatorul curent e moderator
                var group = db.Groups.FirstOrDefault(g => g.Id == userGroup.GroupId);

                if(group != null && group.UserId == currentUser.Id)
                {
                    db.UserGroups.Remove(userGroup);
                    db.SaveChanges();
                    TempData["Message"] = "Utilizatorul a fost eliminat din grup";
                    TempData["messageType"] = "alert-danger";
                }
                else
                {
                    TempData["Message"] = "Nu poti elimina utilizatorul din grup";
                    TempData["messageType"] = "alert-danger";
                }
            }
            else
            {
                TempData["Message"] = "Utilizatorul nu este in grup";
                TempData["messageType"] = "alert-danger";
            }

            return RedirectToAction("Show", "Groups",new{id = userGroup.GroupId});
        }

        // adminul poate sterge orice grup
        // utilizatorul doar grupurile create de el
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Delete(int id)
        {
            Group group = db.Groups.Include("Messages")
                                .Where(group => group.Id == id)
                                .First();

            if((group.UserId == _userManager.GetUserId(User))||User.IsInRole("Admin"))
            {
                db.Groups.Remove(group);
                db.SaveChanges();
                TempData["message"] = "Grupul a fost sters";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti un grup care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
            
        }


    }
}