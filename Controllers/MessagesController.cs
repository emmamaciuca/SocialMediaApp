using SocialMediaApp.Data;
using SocialMediaApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace SocialMediaApp.Controllers
{
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public MessagesController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }


        // Stergerea unui mesaj asociat unui grup din baza de date
        // Se poate sterge mesajul doar de catre userii cu rolul de Admin 
        // sau de catre utilizatorii cu rolul de User, doar daca 
        // acel mesaj a fost scris de catre acestia

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Delete(int id)
        {
            Message message = db.Messages.Find(id);
            if(message.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Messages.Remove(message);
                db.SaveChanges();
                return Redirect("/Groups/Show/" + message.GroupId);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti mesajul";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Groups/Show/" + message.GroupId);
            }
        }

        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Edit(int id)
        {
            Message message = db.Messages.Find(id);
            if(message.UserId == _userManager.GetUserId(User))
            {
                return View(message);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa editati mesajul";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Groups/Show/" + message.GroupId);
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Edit(int id, Message requestMessage)
        {
            Message message = db.Messages.Find(id);

            if (message.UserId == _userManager.GetUserId(User))
            {
                if (ModelState.IsValid)
                {
                    message.Content = requestMessage.Content;

                    db.SaveChanges();

                    return Redirect("/Groups/Show/" + message.GroupId);
                }
                else
                {
                    return View(requestMessage);
                }
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa editati mesajul";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Groups/Show/" + message.GroupId);
            }
        }
    }
}