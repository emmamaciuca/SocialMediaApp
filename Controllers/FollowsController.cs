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

    /*public class FollowsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public FollowsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Follow(string followedId)
        {
            var currentUserId = _userManager.GetUserId(User);

            // Verifica dacă cererea există deja sau utilizatorul este deja prieten
            var existingFollow = await _db.Follows.FirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.FollowedId == followedId);
            if (existingFollow == null)
            {
                var follow = new Follow
                {
                    FollowerId = currentUserId,
                    FollowedId = followedId
                };

                _db.Follows.Add(follow);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction("Users", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Accept(string followId)
        {
            var follow = await _db.Follows.FindAsync(followId);
            if (follow != null && follow.FollowedId == _userManager.GetUserId(User))
            {
                // Marchează ca prietenie acceptată
                _db.Follows.Remove(follow);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction("Friends");
        }

        [HttpPost]
        public async Task<IActionResult> Decline(string followId)
        {
            var follow = await _db.Follows.FindAsync(followId);
            if (follow != null && follow.FollowedId == _userManager.GetUserId(User))
            {
                _db.Follows.Remove(follow);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction("Friends");
        }

        public async Task<IActionResult> Friends()
        {
            var currentUserId = _userManager.GetUserId(User);

            var friendRequests = await _db.Follows
                .Include(f => f.Follower)
                .Where(f => f.FollowedId == currentUserId)
                .ToListAsync();

            var friends = await _db.Follows
                .Include(f => f.Followed)
                .Where(f => f.FollowerId == currentUserId)
                .ToListAsync();

            ViewBag.FriendRequests = friendRequests;
            ViewBag.Friends = friends;

            return View();
        }
    }


    /*[Authorize]
    public class FollowController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FollowController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Trimitere cerere de prietenie
        [HttpPost]
        public async Task<IActionResult> SendRequest(string followedId)
        {
            var followerId = _userManager.GetUserId(User);

            if (followerId == followedId)
            {
                TempData["message"] = "Nu puteți trimite o cerere de prietenie către voi înșivă.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Home");
            }

            // Verifică dacă cererea există deja
            var existingRequest = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowedId == followedId);

            if (existingRequest != null)
            {
                TempData["message"] = "Cererea de prietenie a fost deja trimisă.";
                TempData["messageType"] = "alert-warning";
                return RedirectToAction("Index", "Home");
            }

            var follow = new Follow
            {
                FollowerId = followerId,
                FollowedId = followedId,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _context.Follows.Add(follow);
            await _context.SaveChangesAsync();

            TempData["message"] = "Cererea de prietenie a fost trimisă.";
            TempData["messageType"] = "alert-success";
            return RedirectToAction("Index", "Home");
        }

        // Acceptare cerere de prietenie
        [HttpPost]
        public async Task<IActionResult> AcceptRequest(string followerId)
        {
            var followedId = _userManager.GetUserId(User);

            var follow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowedId == followedId && f.Status == "Pending");

            if (follow == null)
            {
                TempData["message"] = "Cererea de prietenie nu a fost găsită.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Home");
            }

            follow.Status = "Accepted";
            await _context.SaveChangesAsync();

            TempData["message"] = "Cererea de prietenie a fost acceptată.";
            TempData["messageType"] = "alert-success";
            return RedirectToAction("Index", "Home");
        }

        // Respingere cerere de prietenie
        [HttpPost]
        public async Task<IActionResult> DeclineRequest(string followerId)
        {
            var followedId = _userManager.GetUserId(User);

            var follow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowedId == followedId && f.Status == "Pending");

            if (follow == null)
            {
                TempData["message"] = "Cererea de prietenie nu a fost găsită.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Home");
            }

            _context.Follows.Remove(follow);
            await _context.SaveChangesAsync();

            TempData["message"] = "Cererea de prietenie a fost respinsă.";
            TempData["messageType"] = "alert-warning";
            return RedirectToAction("Index", "Home");
        }

        // Afișare cereri de prietenie primite
        public async Task<IActionResult> ReceivedRequests()
        {
            var userId = _userManager.GetUserId(User);

            var requests = await _context.Follows
                .Where(f => f.FollowedId == userId && f.Status == "Pending")
                .Include(f => f.Follower)
                .ToListAsync();

            return View(requests);
        }

        // Afișare cereri de prietenie trimise
        public async Task<IActionResult> SentRequests()
        {
            var userId = _userManager.GetUserId(User);

            var requests = await _context.Follows
                .Where(f => f.FollowerId == userId && f.Status == "Pending")
                .Include(f => f.Followed)
                .ToListAsync();

            return View(requests);
        }
    }*/


}