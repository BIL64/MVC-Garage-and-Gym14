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
using System.Diagnostics;

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
        public IActionResult Index()
        {
            List<Member> lista1 = new List<Member>(); // Två listor krävs för att åstadkomma en distinct funktion.
            List<Member> lista2 = new List<Member>();

            lista1.Clear();
            lista2.Clear();
            foreach (var item in _context.Member) // Laddar lista1 med alla medlemmar.
            {
                lista1.Add(item);
            }

            foreach (var item1 in lista1) // Kollar om de två listorna redan har samma medlem, i så fall sker ingen tilldelning.
            {
                int rak = 0;
                foreach (var item2 in lista2) // Lista2 fylls succesivt på ifall jämförelsen går bra.
                {
                    if (item2.PersonalNo == item1.PersonalNo) rak++; // Personnr jämförs.
                }
                if (rak < 1) lista2.Add(item1);
            }

            return _context.Member != null ?
                    View(lista2) :
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
                    LastName = v.LastName,
                    PersonalNo = v.PersonalNo,                    
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
                    Auxiliary.Thanks4R = $"{member.FullName} är nu medlem i BL GARAGE !";                    
                    return RedirectToAction(nameof(Index));
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

            foreach (var item in _context.Member) // Medlemmar som äger fordon förekommer flera gånger - med olika id'n men med samma personnummer.
            {
                if (member.PersonalNo == item.PersonalNo) // De som har angivet PN uppdateras.
                {
                    item.MemberNo = member.MemberNo;
                    item.FirstName = member.FirstName;
                    item.LastName = member.LastName;
                }
            }

            if (ModelState.IsValid)
            {
                try
                {                    
                    await _context.SaveChangesAsync();
                    Auxiliary.Operation = "Redigeringen är genomförd...";
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
            Auxiliary.WarningName = "ModelState is not valid...";
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
            int tal = 0;
            var mem = _context.Member // Först hämtas det personnummer som stämmer med id't.
                .Where(v => v.Id == id);

            foreach (var item1 in mem) // Det enda sättet att frigöra korrekt PN är att iterera den.
            {
                foreach (var item2 in _context.Member) // Alla medlemmar kollas.
                {
                    if (item1.PersonalNo == item2.PersonalNo) tal++; // Finns det fler av samma PN blir tal > 1.
                }
            }

            var member = await _context.Member.FindAsync(id);

            if (tal == 1)
            { 
                if (_context.Member == null)
                {
                    return Problem("Entity set 'Garage3BLContext.Member'  is null.");
                }
                if (member != null)
                {                
                    _context.Member.Remove(member);
                    Auxiliary.Operation = $"{member.MemberNo} är avregistrerad...";
                }            
                await _context.SaveChangesAsync();            
                return RedirectToAction(nameof(Index));            
            }
            Auxiliary.WarningName = "Alla fordon som tillhör en medlem måste först avregistreras innan medlemmen kan avregistreras...";
            if (tal < 1) Auxiliary.WarningName = "Denna medlem finns inte...";
            return View(member);
        }

        private bool MemberExists(int id)
        {
          return (_context.Member?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
