using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Garage3.Core;
using Garage3BL.Data;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Garage3BL.Controllers
{
    public class MembersController : Controller
    {
        private readonly Garage3BLContext _context;

        public MembersController(Garage3BLContext context)
        {
            _context = context;
        }

        // GET: Members
        public async Task<IActionResult> Index()
        {
              return _context.Member != null ? 
                          View(await _context.Member.ToListAsync()) :
                          Problem("Entity set 'Garage3BLContext.Member'  is null.");
        }

        // Av Anna Vesslén
        public IActionResult Search(string searchString)
        {
            var model = _context.Member // Alla egenskaper tas med här.
                .Select(v => new Member
                {
                    Id = v.Id,
                    MemberNo = v.MemberNo,
                    FirstName = v.FirstName,
                    LastName= v.LastName,
                    PersonalNo= v.PersonalNo,                    
                });

            if (!string.IsNullOrWhiteSpace(searchString)) // Görs bara om det finns en söksträng.
            {
                model = model.Where(v => v.MemberNo.Equals(searchString) || v.FirstName.Equals(searchString));
            }

            if (model.ToList().Count == 0) ModelState
                    .AddModelError("CustomError", "Kunde inte hitta någon medlem med detta förnamn eller medlems-ID..."); // Om ingen match visas detta.

            return View("Index", model);
        }

        // GET: Members/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Member == null)
            {
                return NotFound();
            }

            Auxiliary.Reset(); // Raderar alla meddelanden.
            var member = await _context.Member
                .FirstOrDefaultAsync(m => m.Id == id);
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        // GET: Members/Create
        public IActionResult Create()
        {
            Auxiliary.Reset(); // Raderar alla meddelanden.
            return View();
        }

        // POST: Members/Create (ändring: Björn Lindqvist)
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MemberNo,FirstName,LastName,PersonalNo")] Member member)
        {            
            bool flag1 = false;
            if (member.FirstName.ToUpper() == member.LastName.ToUpper()) flag1 = true; // Är för- och efternamnet samma?

            bool flag2 = _context.Member.Any(p => p.PersonalNo == member.PersonalNo); // Finns det fler med samma personnummer?

            if (!flag1 && !flag2)
            {
                if (ModelState.IsValid)
                {                                        
                    _context.Add(member);
                    await _context.SaveChangesAsync();
                    TempData["memid"] = (int)member.Id;
                    TempData["memno"] = member.MemberNo.ToString();
                    TempData["memfn"] = member.FirstName.ToString();
                    TempData["memln"] = member.LastName.ToString();
                    TempData["memfu"] = member.FullName.ToString();
                    TempData["mempn"] = member.PersonalNo.ToString();
                    TempData["bjorn"] = (int)member.Id; // test
                    Auxiliary.Thanks = $"{member.FullName} är nu medlem i BL GARAGE !";                    
                    return RedirectToAction("Create", "Vehicles");
                }
                Auxiliary.WarningName = "ModelState is not valid...";
                return View(member);
            }
            else
            {
                if (flag1) Auxiliary.WarningName = "Förnamnet och efternamnet måste vara olika...";
                if (flag2) Auxiliary.WarningName = "Fler än ett konto är inte tillåtet...";
                return View(member);
            }
        }

        // GET: Members/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Member == null)
            {
                return NotFound();
            }

            Auxiliary.Reset(); // Raderar alla meddelanden.
            var member = await _context.Member.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }
            return View(member);
        }

        // POST: Members/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MemberNo,FirstName,LastName,PersonalNo")] Member member)
        {
            if (id != member.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(member);
                    Auxiliary.Operation = $"Redigeringen är genomförd...";
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MemberExists(member.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(member);
        }

        // GET: Members/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Member == null)
            {
                return NotFound();
            }

            Auxiliary.Reset(); // Raderar alla meddelanden.
            var member = await _context.Member
                .FirstOrDefaultAsync(m => m.Id == id);
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        // POST: Members/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Member == null)
            {
                return Problem("Entity set 'Garage3BLContext.Member'  is null.");
            }
            var member = await _context.Member.FindAsync(id);
            if (member != null)
            {
                Auxiliary.Operation = $"{member.FullName} är avregistrerad...";
                _context.Member.Remove(member);
            }
            
            await _context.SaveChangesAsync();            
            return RedirectToAction(nameof(Index));
        }

        private bool MemberExists(int id)
        {
          return (_context.Member?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
