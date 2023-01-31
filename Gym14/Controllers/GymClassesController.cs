using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Gym14.Data;
using Gym14.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Gym14.Areas.Identity.Pages.Account.Manage;

namespace Gym14.Controllers
{
    public class GymClassesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userman;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;

        public GymClassesController(ApplicationDbContext context, UserManager<ApplicationUser> UserManager,
            SignInManager<ApplicationUser> signInManager, ILogger<DeletePersonalDataModel> logger)
        {
            _context = context;
            this._userman = UserManager;
            this._signInManager = signInManager;
            this._logger = logger;
        }

        // GET: GymClasses
        [AllowAnonymous]
        public IActionResult Index()
        {
            var userId = _userman.GetUserId(User); // Den för tillfället inloggades sträng-id. Yes/No.

            var appgymclass = _context.AppGymClass // Söker bland gympass som är bokade av sträng-id.
                .Where(v => v.ApplicationUserId == userId);

            Auxx.Usergymlist.Clear();
            foreach (var item in appgymclass) // Lagrar endast bokade gympass i en statisk lista.
            {
                Auxx.Usergymlist.Add(item);
            }

            var model = _context.GymClass // Förhindrar att utgångna gympass visas.
                .Where(v => v.StartTime >= DateTime.UtcNow);

            return View(model);
        }

        [AllowAnonymous]
        public IActionResult Memindex()
        {
            var model = _context.AppUser; // Alla medlemmar.

            return View(model);
        }

        // Av Dimitris Björlingh (ombyggd).
        [Authorize(Roles = "Administrators, Members")]
        public async Task<IActionResult> Booking_in(int? id)
        {
            Auxx.Reset();

            if (id == null) return BadRequest();

            //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userId = _userman.GetUserId(User);

            if (userId == null) return BadRequest();

            //var currentGymClass = await _context.GymClass.Include(g => g.AttendingMembers)
            //                                     .FirstOrDefaultAsync(global => global.Id == id);

            //var attending = currentGymClass?.AttendingMembers.FirstOrDefault(a => a.ApplicationUserId == userId)

            var attending = await _context.AppGymClass.FindAsync(userId, id);

            if (attending == null)
            {
                var booking = new ApplicationUserGymClass
                {
                    ApplicationUserId = userId, // Släcker man null i kopplingstabellen så addaeras nya appusers och gymklasser automagiskt.
                    GymClassId = (int)id
                };
                _context.AppGymClass.Add(booking);
                await _context.SaveChangesAsync();
                Auxx.Book_Yes = "You are now booked for this pass...";

            }
            else
            {
                //_context.AppGymClass.Remove(attending);
                Auxx.Book_No = "You are already booked for this pass...";
            }
            
            return RedirectToAction("Index");
        }

        // Av Dimitris Björlingh (ombyggd).
        [Authorize(Roles = "Administrators, Members")]
        public async Task<IActionResult> Booking_out(int? id)
        {
            Auxx.Reset();

            if (id == null) return BadRequest();

            var userId = _userman.GetUserId(User);

            if (userId == null) return BadRequest();

            var attending = await _context.AppGymClass.FindAsync(userId, id);

            var isnotbook = _context.GymClass
                    .Where(b => b.Id == id);

            if (attending == null)
            {
                var booking = new ApplicationUserGymClass
                {
                    ApplicationUserId = userId,
                    GymClassId = (int)id
                };
                Auxx.Book_No = "You have not booked this pass...";
            }
            else
            {
                _context.AppGymClass.Remove(attending);
                await _context.SaveChangesAsync();
                Auxx.Book_No = "You are now canceled for this pass...";
            }
            
            return RedirectToAction("Index");
        }

        // Av Björn Lindqvist
        [AllowAnonymous]
        public IActionResult Alreadybook(int? id)
        {
            Auxx.Reset();

            string appid = string.Empty;

            if (id == null || _context.GymClass == null) return BadRequest();

            var appgymclass = _context.AppGymClass // Gympasset med detta id hämtas.
                .Where(v => v.GymClassId == id);

            Auxx.Userlist.Clear();
            foreach (var item in appgymclass) // Söker efter medlemmar som är bokade på detta gympass.
            {                
                foreach (var items in _userman.Users)
                {
                    if (items.Id == item.ApplicationUserId) Auxx.Userlist.Add(items);
                }
            }

            return View();
        }

        // GET: GymClasses/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            Auxx.Reset();

            string appid = string.Empty;

            if (id == null || _context.GymClass == null) return BadRequest();

            var appgymclass = _context.AppGymClass // Gympasset med detta id hämtas.
                .Where(v => v.GymClassId == id);

            Auxx.Userlist.Clear();
            foreach (var item in appgymclass) // Söker efter medlemmar som är bokade på detta gympass.
            {
                foreach (var items in _userman.Users)
                {
                    if (items.Id == item.ApplicationUserId) Auxx.Userlist.Add(items);
                }
            }

            var gymClass = await _context.GymClass
                .FirstOrDefaultAsync(m => m.Id == id);

            if (gymClass == null) return BadRequest();

            return View(gymClass);
        }

        // GET: GymClasses/Create
        [AllowAnonymous]
        public IActionResult Create()
        {
            Auxx.Gymlist.Clear();
            foreach (var item in _context.GymClass) // Hämtar alla gympass.
            {
                Auxx.Gymlist.Add(item);
            }

            return View();
        }

        // POST: GymClasses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Create([Bind("Id,Name,StartTime,Duration,Description")] GymClass gymClass)
        {
            Auxx.Gymlist.Clear();
            foreach (var item in _context.GymClass) // Hämtar alla gympass inklusive den nyss skapade.
            {
                Auxx.Gymlist.Add(item);
            }

            if (ModelState.IsValid)
            {
                _context.Add(gymClass);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Create));
            }

            return View(gymClass);
        }

        // GET: GymClasses/Edit/5
        [AllowAnonymous]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.GymClass == null) return BadRequest();

            var gymClass = await _context.GymClass.FindAsync(id);

            if (gymClass == null) return BadRequest();

            return View(gymClass);
        }

        // POST: GymClasses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,StartTime,Duration,Description")] GymClass gymClass)
        {
            if (id != gymClass.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(gymClass);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GymClassExists(gymClass.Id))
                    {
                    return BadRequest();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Create));
            }
            return View(gymClass);
        }

        // GET: GymClasses/Delete/5
        [AllowAnonymous]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.GymClass == null) return BadRequest();

            var gymClass = await _context.GymClass
                .FirstOrDefaultAsync(m => m.Id == id);

            if (gymClass == null) return BadRequest();

            return View(gymClass);
        }

        // POST: GymClasses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.GymClass == null)
            {
                return Problem("Entity set 'ApplicationDbContext.GymClass'  is null.");
            }
            var gymClass = await _context.GymClass.FindAsync(id);
            if (gymClass != null)
            {
                _context.GymClass.Remove(gymClass);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Create));
        }

        // Av Björn Lindqvist.
        [AllowAnonymous]
        public async Task<IActionResult> Memdelete(string id)
        {
            if (id == null || _context.AppUser == null) return BadRequest();

            var appUser = await _context.AppUser
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appUser == null) return BadRequest();

            return View(appUser);
        }

        // Av Björn Lindqvist.
        [HttpPost, ActionName("Memdelete")]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> MemdeleteConfirmed(string id)
        {
            if (_context.AppUser == null)
            {
                return Problem("Entity set 'ApplicationDbContext.GymClass'  is null.");
            }
            var appUser = await _context.AppUser.FindAsync(id);
            if (appUser != null)
            {
                var result = await _userman.DeleteAsync(appUser);
                var userId = await _userman.GetUserIdAsync(appUser);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Unexpected error occurred deleting user.");
                };

                _logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);
            }

            return RedirectToAction(nameof(Memindex));
        }

        private bool GymClassExists(int id)
        {
          return (_context.GymClass?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
