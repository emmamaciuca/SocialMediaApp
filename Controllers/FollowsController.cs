using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Data;
using SocialMediaApp.Models;

namespace SocialMediaApp.Controllers
{

    //[Authorize]
    public class FollowsController : Controller
    {
        /*private readonly ApplicationDbContext _context;

        public FollowsController(ApplicationDbContext context)
        {
            _context = context;
        }*/

        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment _env;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public FollowsController(
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

        [HttpPost]
        public async Task<IActionResult> Follow(string followedId)
        {
            if (string.IsNullOrEmpty(followedId))
            {
                return BadRequest("Invalid user ID.");
            }

            //var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            var currentUser = await _userManager.GetUserAsync(User);


            if (currentUser.Id == null || currentUser.Id == followedId)
            {
                return BadRequest("Invalid operation.");
            }

            var existingFollow = await db.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == currentUser.Id && f.FollowedId == followedId);

            if (existingFollow == null)
            {
                var followRequest = new Follow
                {
                    FollowerId = currentUser.Id,
                    FollowedId = followedId,
                    Status = "Pending"
                };

                db.Follows.Add(followRequest);
                await db.SaveChangesAsync();

                return RedirectToAction("Index", "Users", new { message = "Friend request sent!" });
            }

            return RedirectToAction("Index", "Users", new { message = "You already sent a request." });
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            ViewBag.UserCurent = currentUser;


            //var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;


            var friendRequests = await db.Follows
                .Include(f => f.Follower)
                .Where(f => f.FollowedId == currentUser.Id && f.Status == "Pending")
                .ToListAsync();

            var friends = await db.Follows
                .Include(f => f.Follower)
                .Include(f => f.Followed)
                .Where(f => (f.FollowedId == currentUser.Id || f.FollowerId == currentUser.Id) && f.Status == "Accepted")
                .ToListAsync();

            var followers = await db.Follows
               .Include(f => f.Follower)
               .Where(f => f.FollowedId == currentUser.Id && f.Status == "Accepted")
               .Select(f => f.Follower)
               .ToListAsync();

            var following = await db.Follows
                .Include(f => f.Followed)
                .Where(f => f.FollowerId == currentUser.Id && f.Status == "Accepted")
                .Select(f => f.Followed)
                .ToListAsync();

            ViewBag.FriendRequests = friendRequests;
            ViewBag.Friends = friends;
            ViewBag.Followers = followers;
            ViewBag.Following = following;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Accept(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var followRequest = await db.Follows.FirstOrDefaultAsync(f => f.Id == id);

            if (followRequest == null || followRequest.FollowedId != currentUser.Id)
            {
                return NotFound();
            }

            followRequest.Status = "Accepted";
            db.Follows.Update(followRequest);
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Reject(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var followRequest = await db.Follows.FirstOrDefaultAsync(f => f.Id == id);

            if (followRequest == null || followRequest.FollowedId != currentUser.Id)
            {
                return NotFound();
            }

            db.Follows.Remove(followRequest);
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }


}