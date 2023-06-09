using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Gym14.Data;
using Gym14.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
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
            Auxx.HReset();
            var userId = _userman.GetUserId(User); // Den för tillfället inloggades sträng-id. Yes/No.

            var appgymclass = _context.AppGymClass // Söker bland gympass som är bokade av sträng-id.
                .Where(g => g.ApplicationUserId == userId);

            Auxx.Usergymlist.Clear();
            Auxx.Usergymlist = appgymclass.ToList(); // Lagrar endast bokade gympass i en statisk lista.

            Auxx.Chist = 0;
            foreach (var item in _context.Hstory) Auxx.Chist++;

            var model = _context.Gclass // Förhindrar att utgångna gympass visas.
                .Where(g => g.StartTime >= DateTime.Now);

            return View(model);
        }

        [AllowAnonymous]
        public IActionResult Memindex()
        {
            Auxx.HReset();
            var model = _context.AppUser; // Alla medlemmar.

            return View(model);
        }

        [AllowAnonymous]
        public IActionResult Historyindex()
        {
            Auxx.HReset();
            var model = _context.Hstory; // Historiska medlemmar.

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
                    ApplicationUserId = userId, // Släcker man null i kopplingstabellen så adderas nya appusers och gymklasser automagiskt.
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

            var isnotbook = _context.Gclass
                    .Where(u => u.Id == id);

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

            if (id == null || _context.Gclass == null) return BadRequest();

            var appgymclass = _context.AppGymClass // Gympasset med detta id hämtas.
                .Where(g => g.GymClassId == id);

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

            if (id == null || _context.Gclass == null) return BadRequest();

            var appgymclass = _context.AppGymClass // Gympasset med detta id hämtas.
                .Where(g => g.GymClassId == id);

            Auxx.Userlist.Clear();
            foreach (var item in appgymclass) // Söker efter medlemmar som är bokade på detta gympass.
            {
                foreach (var items in _userman.Users)
                {
                    if (items.Id == item.ApplicationUserId) Auxx.Userlist.Add(items);
                }
            }

            var gymClass = await _context.Gclass
                .FirstOrDefaultAsync(m => m.Id == id);

            if (gymClass == null) return BadRequest();

            return View(gymClass);
        }

        // GET: GymClasses/Create
        [AllowAnonymous]
        public IActionResult Create()
        {
            Auxx.Gymlist.Clear();
            Auxx.Gymlist = _context.Gclass.ToList(); // Hämtar alla gympass.

            return View();
        }

        // POST: GymClasses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Create([Bind("Name,StartTime,Duration,Description")] GymClass gymClass)
        {
            Auxx.Gymlist.Clear();
            Auxx.Gymlist = _context.Gclass.ToList(); // Hämtar alla gympass inklusive den nyss skapade.

            if (ModelState.IsValid)
            {
                HistoryUpdate(); // Endast när man skapar ett nytt gympass så uppdateras historiken.

                _context.Gclass.Add(gymClass);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Create));
            }

            return View(gymClass);
        }

        // Uppdaterar historik med ev nya medlemmar Av Björn Lindqvist.
        [AllowAnonymous]
        public void HistoryUpdate()
        {
            bool isreg;
            bool isphone;

            foreach (var item in _userman.Users)
            {
                isreg = false;
                isphone = false;

                var hist = new History();

                // För att posta (Add).
                hist.Name = $"{item.FirstName} {item.LastName}";
                hist.Rtype = item.Rtype;
                if (item.Email is not null) hist.Email = item.Email; else hist.Email = "-";
                if (item.PhoneNumber is not null) hist.Phone = item.PhoneNumber; else hist.Phone = "-";
                if (item.Arrived is not null) hist.Date = item.Arrived; else hist.Date = "-";

                foreach (var items in _context.Hstory)
                {
                    if (item.Email == items.Email)
                    {
                        if (item.PhoneNumber == items.Phone) isphone = true;
                        else
                        {
                            hist = items; // Put med ett Id. För att kunna köra Update så måste hist laddas med originaldataraden!
                            if (item.PhoneNumber is not null) hist.Phone = item.PhoneNumber; else hist.Phone = "-";
                        }
                        isreg = true;
                    }
                }

                if (!isreg || (isreg && !isphone))
                {
                    try
                    {
                        if (isreg && !isphone) _context.Hstory.Update(hist); // Uppdaterar telefonnumret om det inte redan fanns ett.
                        else
                        {
                            _context.Hstory.Add(hist); // En ny medlem.
                            Auxx.History = "History was updated...";
                            Auxx.Cwidth = '5';
                        }
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (GymClassExists(hist.Id)) throw;
                    }
                }
            }
        }

        // GET: GymClasses/Edit/5
        [AllowAnonymous]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Gclass == null) return BadRequest();

            var gymClass = await _context.Gclass.FindAsync(id);

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
                    _context.Gclass.Update(gymClass);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GymClassExists(gymClass.Id))
                    {
                    return BadRequest();
                    }
                    else throw;
                }
                return RedirectToAction(nameof(Create));
            }
            return View(gymClass);
        }

        // GET: GymClasses/Delete/5
        [AllowAnonymous]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Gclass == null) return BadRequest();

            var gymClass = await _context.Gclass
                .FirstOrDefaultAsync(g => g.Id == id);

            if (gymClass == null) return BadRequest();

            return View(gymClass);
        }

        // POST: GymClasses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Gclass == null)
            {
                return Problem("Entity set 'ApplicationDbContext.GymClass'  is null.");
            }
            var gymClass = await _context.Gclass.FindAsync(id);
            if (gymClass != null)
            {
                _context.Gclass.Remove(gymClass);
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
                return Problem("Entity set 'ApplicationDbContext.ApplicationUser'  is null.");
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

                _logger.LogInformation("User with ID '{UserId}' deleted another user.", userId);
            }

            return RedirectToAction(nameof(Memindex));
        }

        //Av Björn Lindqvist.
        [AllowAnonymous]
        public async Task<IActionResult> Historydelete(int? id)
        {
            if (_context.Hstory == null) return BadRequest();

            var history = await _context.Hstory
                .FirstOrDefaultAsync(h => h.Id == id);

            if (history == null) return BadRequest();

            return View(history);
        }

        // Av Björn Lindqvist.
        [HttpPost, ActionName("Historydelete")]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> HistorydeleteConfirmed(int id)
        {
            if (_context.Hstory == null)
            {
                return Problem("Entity set 'ApplicationDbContext.History'  is null.");
            }
            var history = await _context.Hstory.FindAsync(id);
            if (history != null)
            {
                _context.Hstory.Remove(history);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Historyindex));
        }

        public ClaimsPrincipal GetUser()
        {
            return User;
        }

        // Av Björn Lindqvist.
        [AllowAnonymous]
        public async Task<IActionResult> Memreg(string id)
        {
            if (id == null || _context.AppUser == null) return BadRequest();

            var appUser = await _context.AppUser
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appUser == null) return BadRequest();

            await _userman.AddToRoleAsync(appUser, "Members"); // Lägger till medlemsrollen till den oregistrerade. 
            appUser.Rtype = 'M'; // Görs bara en gång!
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Memindex));
        }

        // Av Björn Lindqvist.
        [AllowAnonymous]
        public async Task<IActionResult> Admreg(string id)
        {
            if (id == null || _context.AppUser == null) return BadRequest();

            var appUser = await _context.AppUser
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appUser == null) return BadRequest();

            await _userman.AddToRoleAsync(appUser, "Administrators"); // Lägger till admin-rollen till den oregistrerade. 
            appUser.Rtype = 'A'; // Görs bara en gång!
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Memindex));
        }

        private bool GymClassExists(int id)
        {
          return (_context.Gclass?.Any(g => g.Id == id)).GetValueOrDefault();
        }
    }
}
