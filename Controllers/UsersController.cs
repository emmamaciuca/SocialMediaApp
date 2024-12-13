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
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment _env;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IWebHostEnvironment env
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;

            _env = env;

        }
        
        // toata lumea poate sa vada utilizatorii
        // inclusiv persoanele neinregistrate 
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Index()
        {
            var users = from user in db.Users
                        orderby user.UserName
                        select user;

            ViewBag.UsersList = users;
            ViewBag.UserCurent = await _userManager.GetUserAsync(User);

            return View();
        }


        // vezi profilul - vizualizat doar daca e public 
        [Authorize(Roles = "User,Editor,Admin")]
        public async Task<ActionResult> Show(string id)
        {
            ApplicationUser user = db.Users.Find(id);
            var roles = await _userManager.GetRolesAsync(user);    

            ViewBag.Roles = roles;

            ViewBag.UserCurent = await _userManager.GetUserAsync(User);

            //SetAccessRights();

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            return View(user);
        }


        // un user isi poate edita doar profilul lui
        // aici trebuie putin de lucru
        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult> Edit(string id)
        {
            ApplicationUser user = db.Users.Find(id);

            if(user.Id == _userManager.GetUserId(User))
            {
                return View(user);
            }
            else 
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui profil care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        // asta merge e ok dar nu salveaza imaginea noua 
        // aici de continua editarea 
        [HttpPost]
        public async Task<ActionResult> Edit(string id, ApplicationUser newData)
        {
            ApplicationUser user = db.Users.Find(id);

            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.UserCurent = currentUser;



            user.AllRoles = GetAllRoles();


                if (ModelState.IsValid)
                {
                    if (user.Id == _userManager.GetUserId(User))
                    {
                        user.FirstName = newData.FirstName;
                        user.LastName = newData.LastName;
                        user.UserName = newData.UserName;
                        user.Email = newData.Email;
                        
                        user.Content = newData.Content;

                        // daca vreau sa pun o poza noua de profil nu o salveaza in formular 

                        if (!string.IsNullOrEmpty(newData.Image))
                        {
                            user.Image = newData.Image;
                        }    
        
                        user.Visibility = newData.Visibility;
                        TempData["message"] = "Profilul a fost modificat";
                        TempData["messageType"] = "alert-success";
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui profil care nu va apartine";
                        TempData["messageType"] = "alert-danger";
                        return RedirectToAction("Index");
                    }

                }
                else
                {
                    return View(newData);
                }
            }


        //pentru a salva imaginea noua
        // [HttpPost]
        // public async Task<ActionResult> Edit(string id, ApplicationUser newData, IFormFile Image)
        // {
        //     ApplicationUser user = db.Users.Find(id);

        //     var currentUser = await _userManager.GetUserAsync(User);
        //     ViewBag.UserCurent = currentUser;



        //     user.AllRoles = GetAllRoles();


        //         if (ModelState.IsValid)
        //         {
        //             user.FirstName = newData.FirstName;
        //             user.LastName = newData.LastName;
        //             user.UserName = newData.UserName;
        //             user.Email = newData.Email;
                    
        //             user.Content = newData.Content;

        //             // daca vreau sa pun o poza noua de profil nu o salveaza in formular 
        //             if (Image != null && Image.Length > 0)
        //             {
        //                 // Verificăm extensia fișierului
        //                 var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        //                 var fileExtension = Path.GetExtension(Image.FileName).ToLower();
        //                 if (!allowedExtensions.Contains(fileExtension))
        //                 {
        //                     ModelState.AddModelError("Image", "Fișierul trebuie să fie o imagine (jpg, jpeg, png, gif).");
        //                     return View(newData);
        //                 }

        //                 // Salvăm imaginea în wwwroot/images
        //                 var fileName = Guid.NewGuid().ToString() + fileExtension; // Nume unic pentru fișier
        //                 var storagePath = Path.Combine(_env.WebRootPath, "images", fileName);
        //                 var databaseFileName = "/images/" + fileName;

        //                 // Salvăm fișierul în server
        //                 using (var fileStream = new FileStream(storagePath, FileMode.Create))
        //                 {
        //                     await Image.CopyToAsync(fileStream);
        //                 }

        //                 // Setăm noua imagine pentru utilizator
        //                 user.Image = databaseFileName;
        //             }


        //             // if (!string.IsNullOrEmpty(newData.Image))
        //             // {
        //             //     user.Image = newData.Image;
        //             // }    

                               
        //             user.Visibility = newData.Visibility;



        //             db.SaveChanges();
        //         }
        //         return RedirectToAction("Index");
        //     }


        // adminul poate sterge conturi
        // utilizatorul doar contul sau
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Delete(string id)
        {
            var user = db.Users
                         .Include("Posts")
                         .Include("Comments")
                         .Include("UserGroups")
                         .Include("Messages")
                         .Where(u => u.Id == id)
                         .First();

            if((user.Id == _userManager.GetUserId(User))||User.IsInRole("Admin"))
            {
                // Delete user comments
                if (user.Comments.Count > 0)
                {
                    foreach (var comment in user.Comments)
                    {
                        db.Comments.Remove(comment);
                    }
                }

                // Delete user groups
                // ?? oare sterge tot grupul sau doar pe utilizator
                if (user.UserGroups.Count > 0)
                {
                    foreach (var usergroup in user.UserGroups)
                    {
                        db.UserGroups.Remove(usergroup);
                    }
                }


                // Delete user posts
                if (user.Posts.Count > 0)
                {
                    foreach (var post in user.Posts)
                    {
                        db.Posts.Remove(post);
                    }
                }

                // Delete user messages
                if (user.Messages.Count > 0)
                {
                    foreach (var message in user.Messages)
                    {
                        db.Messages.Remove(message);
                    }
                }

                db.ApplicationUsers.Remove(user);

                db.SaveChanges();

                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti un profil care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
            
        }

        // Conditiile de afisare pentru butoanele de editare si stergere
        // butoanele aflate in view-uri
        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            if (User.IsInRole("Editor"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.UserCurent = _userManager.GetUserId(User);

            ViewBag.EsteAdmin = User.IsInRole("Admin");
        }


        //toate rolurile dar noi nu avem nevoie
        [NonAction]
        public IEnumerable<SelectListItem> GetAllRoles()
        {
            var selectList = new List<SelectListItem>();

            var roles = from role in db.Roles
                        select role;

            foreach (var role in roles)
            {
                selectList.Add(new SelectListItem
                {
                    Value = role.Id.ToString(),
                    Text = role.Name.ToString()
                });
            }
            return selectList;
        }
    }
}