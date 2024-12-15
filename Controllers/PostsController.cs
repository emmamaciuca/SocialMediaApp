using SocialMediaApp.Data;
using SocialMediaApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ganss.Xss;



namespace SocialMediaApp.Controllers
{

    public class PostsController : Controller
    {

        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public PostsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(Roles = "User,Editor,Admin")]
        //[Authorize]
        public IActionResult Index()
        {
            var posts = db.Posts.Include("User")
                                .OrderByDescending(a => a.Date);

            ViewBag.Posts = posts;
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            //motor de cautare
            //afisare paginata
            return View();
        }
        //afisare postare
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Show(int id)
        {
            Post post = db.Posts.Include("User")
                                .Include("Comments")
                                .Include("Comments.User")
                            .Where(post => post.Id == id)
                            .First();
            SetAccessRights();
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }
            
            return View(post);
        }
        //afisare parte comentarii
        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Show([FromForm] Comment comment)
        {
            comment.Date = DateTime.Now;

            // preluam Id-ul utilizatorului care posteaza comentariul
            comment.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();   //aici se adauga comentariul in baza de date
                return Redirect("/Posts/Show/" + comment.PostId);
            }
            else
            {
                Post post = db.Posts.Include("User")
                                    .Include("Comments")
                                    .Include("Comments.User")
                                    .Where(post => post.Id == comment.PostId)
                                         .First();

                return View(post);
            }
        }
        //pagina postare noua
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult New()
        {
            Post post = new Post();
           
            return View(post);
        }

        //pentru a pune postarea in baza de date
        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult New(Post post)
        {
            var sanitizer = new HtmlSanitizer();

            post.Date = DateTime.Now;

            // preluam Id-ul utilizatorului care posteaza 
            post.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                post.Content = sanitizer.Sanitize(post.Content);

                db.Posts.Add(post);
                db.SaveChanges();
                TempData["message"] = "Postarea a fost adaugata";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                return View(post);
            }
        }
        
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Edit(int id)
        {

            Post post = db.Posts.Where(post => post.Id == id)
                                .First();

            if ((post.UserId == _userManager.GetUserId(User)) ||
                User.IsInRole("Admin"))
            {
                return View(post);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unei postari care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }
        //pt a edita si in baza de date
        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Edit(int id, Post requestPost)
        {
            var sanitizer = new HtmlSanitizer();
            Post post = db.Posts.Find(id);

            if (ModelState.IsValid)
            {
                if ((post.UserId == _userManager.GetUserId(User))
                    || User.IsInRole("Admin"))
                {
                    
                    requestPost.Content = sanitizer.Sanitize(requestPost.Content);

                    post.Content = requestPost.Content;

                    //post.Image = requestPost.Image;

                    post.Date = DateTime.Now;
                    
                    TempData["message"] = "Postarea a fost modificata";
                    TempData["messageType"] = "alert-success";
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unei postari care nu va apartine";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                return View(requestPost);
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public ActionResult Delete(int id)
        {
            Post post = db.Posts.Include("Comments")
                                .Where(post => post.Id == id)
                                .First();
          
            if ((post.UserId == _userManager.GetUserId(User))
                    || User.IsInRole("Admin"))
            {
                db.Posts.Remove(post);
                db.SaveChanges();
                TempData["message"] = "Postarea a fost stearsa";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti o postare care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }
        // Conditiile de afisare pentru butoanele de editare si stergere
        // butoanele aflate in view-uri
        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            if (User.IsInRole("User"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.UserCurent = _userManager.GetUserId(User);

            ViewBag.EsteAdmin = User.IsInRole("Admin");
        }

    }
}