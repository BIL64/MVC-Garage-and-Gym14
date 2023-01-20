using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Garage3.Core;
using Garage3BL.Data;
using Microsoft.Data.SqlClient;
using System.Reflection;
using Microsoft.CodeAnalysis.Operations;
using Newtonsoft.Json;

namespace Garage3BL.Controllers
{
    public class VehiclesController : Controller
    {
        private readonly Garage3BLContext _context;

        public VehiclesController(Garage3BLContext context)
        {
            _context = context;
        }

        // Av Björn Lindqvist
        public int Readfile2int(string filename, int intal) // Läser från konfigurationsfiler.
        {
            int tal = 0;
            try
            {
                using (var sr = new StreamReader(filename))
                {
                    tal = int.Parse(sr.ReadToEnd()); // Läser in en sträng som konverteras till en int.
                }
            }
            catch (IOException e)
            {
                Console.WriteLine($"Kunde ej läsa från {filename}: ");
                Console.WriteLine(e.Message);
                tal = intal; // Misslyckas inläsningen så ges variabeln parametervärdet.
            }
            return tal;
        }

        // GET: Vehicles
        //public async Task<IActionResult> Index()
        //{
        //    var garage3BLContext = _context.Vehicle.Include(v => v.Member).Include(v => v.Vtype);
        //    return View(await garage3BLContext.ToListAsync());
        //}

        // GET: Vehicles (ändring: Björn Lindqvist)
        public async Task<IActionResult> Index()
        {
            if (Auxiliary.Start) // Görs bara första gången översikten körs.
            {
                Auxiliary.Capacity = new string[Readfile2int("Config_cap.txt", 20)]; // Läser in variabler från filer.
                Auxiliary.ArrayReset(Readfile2int("Config_cap.txt", 20));
                Auxiliary.Pricebase = Readfile2int("Config_bas.txt", 10);
                Auxiliary.Pricehour = Readfile2int("Config_tim.txt", 20);
                Auxiliary.Counter = 0;
                Statistic.InCome = 0;

                foreach (var item in _context.Vehicle)
                {
                    item.Place = string.Empty;

                    if (item.IsParked) // Parkerar in fordon från databasen.
                    {

                        switch (item.VtypeId) // Laddar arrayen och skapar nödvändig plats.
                        {
                            case 3:
                                Auxiliary.Counter++;
                                Auxiliary.Capacity[Auxiliary.Counter - 1] = item.RegNo;
                                item.Place = "[" + Auxiliary.Counter.ToString() + "] ";
                                Auxiliary.Counter++;
                                Auxiliary.Capacity[Auxiliary.Counter - 1] = item.RegNo;
                                item.Place += "[" + Auxiliary.Counter.ToString() + "] ";
                                Auxiliary.Counter++;
                                Auxiliary.Capacity[Auxiliary.Counter - 1] = item.RegNo;
                                item.Place += "[" + Auxiliary.Counter.ToString() + "] ";
                                Auxiliary.Counter++;
                                Auxiliary.Capacity[Auxiliary.Counter - 1] = item.RegNo;
                                item.Place += "[" + Auxiliary.Counter.ToString() + "] ";
                                Auxiliary.Counter++;
                                Auxiliary.Capacity[Auxiliary.Counter - 1] = item.RegNo;
                                item.Place += "[" + Auxiliary.Counter.ToString() + "]";
                                item.InCome = (int)(Auxiliary.Pricebase * 1.6);
                                Statistic.InCome += item.InCome;
                                break;
                            case 4:
                                Auxiliary.Counter++;
                                Auxiliary.Capacity[Auxiliary.Counter - 1] = item.RegNo;
                                item.Place = "[" + Auxiliary.Counter.ToString() + "] ";
                                Auxiliary.Counter++;
                                Auxiliary.Capacity[Auxiliary.Counter - 1] = item.RegNo;
                                item.Place += "[" + Auxiliary.Counter.ToString() + "] ";
                                Auxiliary.Counter++;
                                Auxiliary.Capacity[Auxiliary.Counter - 1] = item.RegNo;
                                item.Place += "[" + Auxiliary.Counter.ToString() + "]";
                                item.InCome = (int)(Auxiliary.Pricebase * 1.6);
                                Statistic.InCome += item.InCome;
                                break;
                            case 5 or 6:
                                Auxiliary.Counter++;
                                Auxiliary.Capacity[Auxiliary.Counter - 1] = item.RegNo;
                                item.Place = "[" + Auxiliary.Counter.ToString() + "] ";
                                Auxiliary.Counter++;
                                Auxiliary.Capacity[Auxiliary.Counter - 1] = item.RegNo;
                                item.Place += "[" + Auxiliary.Counter.ToString() + "]";
                                item.InCome = (int)(Auxiliary.Pricebase * 1.3);
                                Statistic.InCome += item.InCome;
                                break;
                            default:
                                Auxiliary.Counter++;
                                Auxiliary.Capacity[Auxiliary.Counter - 1] = item.RegNo;
                                item.Place = "[" + Auxiliary.Counter.ToString() + "]";
                                item.InCome = Auxiliary.Pricebase;
                                Statistic.InCome += item.InCome;
                                break;
                        }
                    }
                    item.ParkedTime = DateTime.Now.Subtract(item.ArrivalTime); // För alla fordon ges P-tid.
                }
            }
            else
            {
                var vehicle = _context.Vehicle;

                foreach (var items in vehicle)
                {
                    items.ParkedTime = DateTime.Now.Subtract(items.ArrivalTime); // För alla fordon ges P-tid.
                }
            }
            await _context.SaveChangesAsync();
            Auxiliary.Start = false;

            var garage3BLContext = _context.Vehicle.Include(v => v.Members).Include(v => v.Vtype);
            return View(await garage3BLContext.ToListAsync());
        }

        // Av Anna Vesslén
        public IActionResult SearchSort(string sortOrder, string searchString)
        {            
            ViewBag.RegnoSort = String.IsNullOrEmpty(sortOrder) ? "regno_desc" : "";
            ViewBag.TypeSort = sortOrder == "Type" ? "type_desc" : "Type";

            var model = _context.Vehicle // Alla egenskaper måste tas med innan ordningen ändras.
                .Select(v => new Vehicle {
                    Id = v.Id,
                    RegNo = v.RegNo,
                    Brand = v.Brand,
                    Vmodel = v.Vmodel,
                    Color = v.Color,
                    Wheels = v.Wheels,
                    Place = v.Place,
                    ArrivalTime = v.ArrivalTime,
                    ParkedTime = v.ParkedTime,
                    IsParked = v.IsParked,
                    InCome = v.InCome,
                    Vtype = v.Vtype,
                    Members = v.Members,
                    VtypeId = v.VtypeId,                    
                });

            if (!string.IsNullOrWhiteSpace(searchString)) // Görs bara om det finns en söksträng.
            {
                model = model.Where(v => v.VtypeId.Equals(searchString) || v.RegNo.Equals(searchString));
            }

            if (model.ToList().Count == 0 && Auxiliary.Counter > 0) ModelState
                    .AddModelError("CustomError", "Kunde inte hitta någon motsvarande fordonstyp eller regnummer..."); // Om ingen match visas detta.

            switch (sortOrder)
            {                
                case "Type":
                    model = model.OrderBy(v => v.Vtype);
                    break;
                case "type_desc":
                    model = model.OrderByDescending(v => v.Vtype);
                    break;
                case "regno_desc":
                    model = model.OrderByDescending(v => v.RegNo);
                    break;
                default:
                    model = model.OrderBy(v => v.RegNo);
                    break;
            }
            return View("Index", model);
        }

        // Av Björn Lindqvist
        public async Task<IActionResult> Parray() // Ritar upp alla parkeringsrutor.
        {
            Auxiliary.Reset(); // Raderar alla meddelanden.

            var garage3BLContext = _context.Vehicle.Include(v => v.Members).Include(v => v.Vtype);
            return View(await garage3BLContext.ToListAsync());
        }

        // Av Björn Lindqvist
        public IActionResult Statistics() // Count räknar hur många det finns i databasen av respektive typ.
        {
            Auxiliary.Reset(); // Raderar alla meddelanden.

            Statistic.Bilar = _context.Vehicle.Count(c => c.Vtype.Type == "Bil");
            Statistic.Mcyklar = _context.Vehicle.Count(c => c.Vtype.Type == "Motorcykel");
            Statistic.Lbilar = _context.Vehicle.Count(c => c.Vtype.Type == "Lastbil");
            Statistic.Bussar = _context.Vehicle.Count(c => c.Vtype.Type == "Buss");
            Statistic.Traktorer = _context.Vehicle.Count(c => c.Vtype.Type == "Traktor");
            Statistic.Emaskiner = _context.Vehicle.Count(c => c.Vtype.Type == "Entreprenadmaskin");
            Statistic.Hjul = _context.Vehicle.Sum(c => c.Wheels);

            return View();
        }

        // Av Björn Lindqvist
        public async Task<IActionResult> Plus(int capacity, int baseprice, int timprice, string subcap, string subbas, string subtim) // För inställning av ny kapacitet och pris.
        {
            Auxiliary.Reset(); // Raderar alla meddelanden.

            if (_context.Vehicle == null)
            {
                return Problem("Entity set 'Garage2_0MVCContext.vehicle'  is null.");
            }

            var vehicle = _context.Vehicle;

            if (subcap == "UTFÖR RADERING" && capacity >= 5)
            {
                foreach (var item in vehicle) // Tar bort alla fordon i databasen.
                {
                    _context.Vehicle.Remove(item);
                }
                await _context.SaveChangesAsync();
                Auxiliary.Counter = 0;
                Auxiliary.Capacity = new string[capacity];

                for (int i = 0; i < capacity; i++) // Laddar ny array med tomma poster.
                {
                    Auxiliary.Capacity[i] = string.Empty;
                }
                // Skriver in den nya kapaciteten (som en sträng) i konfigurationsfilen.
                System.IO.File.WriteAllText(Path.Combine("Config_cap.txt"), capacity.ToString());
            }

            if (subbas == "Nytt basbelopp" && baseprice >= 0)
            {
                Auxiliary.Pricebase = baseprice;
                // Skriver in det nya grundbasbeloppet (som en sträng) i konfigurationsfilen.
                System.IO.File.WriteAllText(Path.Combine("Config_bas.txt"), baseprice.ToString());
            }

            if (subtim == "Nytt timpris" && timprice >= 0)
            {
                Auxiliary.Pricehour = timprice;
                // Skriver in det nya timpriset (som en sträng) i konfigurationsfilen.
                System.IO.File.WriteAllText(Path.Combine("Config_tim.txt"), timprice.ToString());
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Vehicleadd(string newvehicle, string subadd, Vtype vtype)
        {
            if (subadd == "Lägg till ny fordonstyp" && newvehicle != "")
            {
                vtype.Type = newvehicle;
                _context.Add(vtype);
                await _context.SaveChangesAsync();
                Auxiliary.Operation = "Ett nytt fordon har lagts till...";
                return View("Plus");
            }
            return View("Plus");
        }

        //// GET: vehicles/Create - Lägger in hårdkodade fordon. (ändring: Björn Lindqvist)
        //public async Task<IActionResult> Seeda([Bind("Id,Regno,VehicleType,Color,Brand,Model,Wheels,Time")] vehicle vehicle)
        //{
        //    Auxiliary.Reset(); // Raderar alla meddelanden.

        //    if (ModelState.IsValid)
        //    {
        //        await Parking_in(vehicle); // Försöker att parkera ett fordon.
        //        //return RedirectToAction(nameof(Index));
        //    }
        //    return View(vehicle);
        //}

        //public IActionResult Vehicleadd()
        //{
        //    Auxiliary.Reset(); // Raderar alla meddelanden.
        //    return View("Plus");
        //}        

        //GET: Vehicles/Create
        public IActionResult Create()
        {
            ViewData["MemberId"] = new SelectList(_context.Member, "Id", "FullName");
            ViewData["VtypeId"] = new SelectList(_context.Set<Vtype>(), "Id", "Type");
            return View();
        }

        // POST: Vehicles/Create  (ändring: Björn Lindqvist)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Vehicle vehicle)
        {
            var mem = _context.Member
                .Where(v => v.Id == vehicle.MemberId)
                .Select(v => new Member
                 {
                     Id = v.Id,
                     MemberNo = v.MemberNo,
                     FirstName = v.FirstName,
                     LastName = v.LastName,
                     PersonalNo = v.PersonalNo
                 });

            foreach (var item in mem)
            {
                vehicle.Members.MemberNo = item.MemberNo;
                vehicle.Members.FirstName = item.FirstName;
                vehicle.Members.LastName = item.LastName;
                vehicle.Members.PersonalNo= item.PersonalNo;
            }

            var veh = _context.Set<Vtype>()
                .Where(v => v.Id == vehicle.VtypeId)
                .Select(v => new Vtype
                {
                    Id = v.Id,
                    Type = v.Type
                });

            foreach (var item in veh)
            {
                vehicle.Vtype.Type = item.Type;
            }

            vehicle.Place = "[0]";
            vehicle.ArrivalTime = DateTime.Now;
            vehicle.ParkedTime = DateTime.Now.Subtract(vehicle.ArrivalTime);
            vehicle.IsParked = false;                       

            vehicle.Members.Vehicles.Add(vehicle);

            vehicle.RegNo = vehicle.RegNo.ToUpper(); // Tvingar till versaler.
            bool flag = _context.Vehicle.Any(p => p.RegNo.ToUpper() == vehicle.RegNo); // Jämför regnummer i databasen med inkommande.

            if (!flag)
            {
                _context.Add(vehicle);
                await _context.SaveChangesAsync();
                Auxiliary.Operation = "Ditt fordon är nu inregistrerat...";
                return View(vehicle);
            }
            else
            {
                ViewData["MemberId"] = new SelectList(_context.Member, "Id", "FullName", vehicle.MemberId);
                ViewData["VtypeId"] = new SelectList(_context.Set<Vtype>(), "Id", "Type", vehicle.VtypeId);
                Auxiliary.WarningReg = "Regnumret är upptaget...";
                return View(vehicle);
            }
        }

        // Av Björn Lindqvist
        public IActionResult Parking_in(Vehicle park) // Sköter inparkering.
        {
            bool flag = false;
            int rak = 0;
            int tal = 0;

            switch (park.Vtype.Type)
            {
                case "Lastbil":
                    do
                    {
                        if (Auxiliary.Capacity[rak] == "") tal++; // Letar efter en lucka (5 p-rutor) i arrayen.
                        else tal = 0;
                        if (tal == 5) flag = true;
                        rak++;
                    } while (tal != 5 && rak < Auxiliary.Capacity.Length);
                    break;
                case "Buss":
                    do
                    {
                        if (Auxiliary.Capacity[rak] == "") tal++; // Letar efter en lucka (3 p-rutor) i arrayen.
                        else tal = 0;
                        if (tal == 3) flag = true;
                        rak++;
                    } while (tal != 3 && rak < Auxiliary.Capacity.Length);
                    break;
                case "Traktor" or "Entreprenadmaskin":
                    do
                    {
                        if (Auxiliary.Capacity[rak] == "") tal++; // Letar efter en lucka (2 p-rutor) i arrayen.
                        else tal = 0;
                        if (tal == 2) flag = true;
                        rak++;
                    } while (tal != 2 && rak < Auxiliary.Capacity.Length);
                    break;
                default:
                    do
                    {
                        if (Auxiliary.Capacity[rak] == "") tal++; // Letar efter en lucka (1 p-ruta) i arrayen.
                        else tal = 0;
                        if (tal == 1) flag = true;
                        rak++;
                    } while (tal != 1 && rak < Auxiliary.Capacity.Length);
                    break;
            }

            if (flag)
            {
                switch (park.Vtype.Type) // Genomför inläggning oavsett lucka.
                {
                    case "Lastbil":
                        Auxiliary.Counter += 5;
                        Auxiliary.Capacity[rak - 5] = park.RegNo;
                        park.Place = "[" + (rak - 4).ToString() + "] ";
                        Auxiliary.Capacity[rak - 4] = park.RegNo;
                        park.Place += "[" + (rak - 3).ToString() + "] ";
                        Auxiliary.Capacity[rak - 3] = park.RegNo;
                        park.Place += "[" + (rak - 2).ToString() + "] ";
                        Auxiliary.Capacity[rak - 2] = park.RegNo;
                        park.Place += "[" + (rak - 1).ToString() + "] ";
                        Auxiliary.Capacity[rak - 1] = park.RegNo;
                        park.Place += "[" + rak.ToString() + "]";
                        park.InCome += (int)(Auxiliary.Pricebase * 1.6);
                        Statistic.InCome += park.InCome;
                        break;
                    case "Buss":
                        Auxiliary.Counter += 3;
                        Auxiliary.Capacity[rak - 3] = park.RegNo;
                        park.Place = "[" + (rak - 2).ToString() + "] ";
                        Auxiliary.Capacity[rak - 2] = park.RegNo;
                        park.Place += "[" + (rak - 1).ToString() + "] ";
                        Auxiliary.Capacity[rak - 1] = park.RegNo;
                        park.Place += "[" + rak.ToString() + "]";
                        park.InCome += (int)(Auxiliary.Pricebase * 1.6);
                        Statistic.InCome += park.InCome;
                        break;
                    case "Traktor" or "entreprenadmaskin":
                        Auxiliary.Counter += 2;
                        Auxiliary.Capacity[rak - 2] = park.RegNo;
                        park.Place = "[" + (rak - 1).ToString() + "] ";
                        Auxiliary.Capacity[rak - 1] = park.RegNo;
                        park.Place += "[" + rak.ToString() + "]";
                        park.InCome += (int)(Auxiliary.Pricebase * 1.3);
                        Statistic.InCome += park.InCome;
                        break;
                    default:
                        Auxiliary.Counter++;
                        Auxiliary.Capacity[rak - 1] = park.RegNo;
                        park.Place = "[" + rak.ToString() + "]";
                        park.InCome += Auxiliary.Pricebase;
                        Statistic.InCome += park.InCome;
                        break;
                }
                Auxiliary.Thanks = $"Tack för att du {park.RegNo} parkerat hos BL GARAGE !";
            }
            else
            {
                Auxiliary.Fullt = "Tyvärr är vårt garage fullt för tillfället...";
            }
            return View();
        }

        // Av Björn Lindqvist
        //GET: Vehicles/Parking
        public IActionResult Parking()
        {
            Auxiliary.Reset(); // Raderar alla meddelanden.
            ViewData["MemberId"] = new SelectList(_context.Member, "Id", "MemberNo");
            ViewData["RegNo"] = new SelectList(_context.Vehicle, "Id", "RegNo");
            return View();
        }

        // Av Björn Lindqvist
        // POST: Vehicles/Parking (ändring: Björn Lindqvist)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Parking(Vehicle vehicle)
        //public IActionResult Parking([Bind("Id,RegNo,MemberId,VtypeId")] Vehicle vehicle)
        {
            bool flag1 = _context.Member.Any(p => p.Id == vehicle.MemberId); // Finns denna medlem?
            vehicle.RegNo = vehicle.RegNo.ToUpper(); // Tvingar till versaler.
            bool flag2 = _context.Vehicle.Any(p => p.RegNo.ToUpper() == vehicle.RegNo); // Har medlemmen detta fordon (regnummer)?

            if (flag1 && flag2)
            {
                vehicle.IsParked = true;
                vehicle.ArrivalTime = DateTime.Now;
                vehicle.ParkedTime = DateTime.Now.Subtract(vehicle.ArrivalTime);                
                vehicle.InCome += Auxiliary.Pricebase;
                Statistic.InCome += vehicle.InCome;
                Parking_in(vehicle); // Försöker att parkera ett fordon.

                _context.Attach(vehicle);
                _context.Entry(vehicle).Property("IsParked").IsModified = true;
                _context.Entry(vehicle).Property("ArrivalTime").IsModified = true;
                _context.Entry(vehicle).Property("ParkedTime").IsModified = true;
                _context.Entry(vehicle).Property("InCome").IsModified = true;
                _context.Entry(vehicle).Property("Place").IsModified = true;
                //_context.Entry(vehicle).State = EntityState.Modified;
                await _context.SaveChangesAsync();                
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ViewData["MemberId"] = new SelectList(_context.Member, "Id", "MemberNo", vehicle.MemberId);
                ViewData["RegNo"] = new SelectList(_context.Vehicle, "Id", "RegNo", vehicle.RegNo);
                Auxiliary.WarningReg = "Medlemmen eller regnumret finns inte...";
                return View(vehicle);
            }
        }
        // [Bind("Id,RegNo,Brand,Vmodel,Color,Wheels,Place,ArrivalTime,ParkedTime,IsParked,InCome,MemberId,VtypeId")]

        // GET: Vehicles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Vehicle == null)
            {
                return NotFound();
            }

            Auxiliary.Reset(); // Raderar alla meddelanden.
            var vehicle = await _context.Vehicle
                .Include(v => v.Members)
                .Include(v => v.Vtype)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vehicle == null)
            {
                return NotFound();
            }

            return View(vehicle);
        }

        // GET: Vehicles/Edit/5 (ändring: Björn Lindqvist)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Vehicle == null)
            {
                return NotFound();
            }            

            Auxiliary.Reset(); // Raderar alla meddelanden.
            var vehicle = await _context.Vehicle.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }
            ViewData["MemberId"] = new SelectList(_context.Member, "Id", "FullName", vehicle.MemberId);
            ViewData["VtypeId"] = new SelectList(_context.Set<Vtype>(), "Id", "Type", vehicle.VtypeId);
            return View(vehicle);
        }

        // POST: Vehicles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Vehicle vehicle)
        {
            if (id != vehicle.Id)
            {
                return NotFound();
            }

            var mem = _context.Member
                .Where(v => v.Id == id)
                .Select(v => new Member
                {
                    Id = v.Id,
                    MemberNo = v.MemberNo,
                    FirstName = v.FirstName,
                    LastName = v.LastName,
                    PersonalNo = v.PersonalNo
                });

            foreach (var item in mem)
            {
                vehicle.Members.MemberNo = item.MemberNo;
                vehicle.Members.FirstName = item.FirstName;
                vehicle.Members.LastName = item.LastName;
                vehicle.Members.PersonalNo = item.PersonalNo;
            }

            var veh = _context.Set<Vtype>()
                .Where(v => v.Id == id)
                .Select(v => new Vtype
                {
                    Id = v.Id,
                    Type = v.Type
                });            

            foreach (var item in veh)
            {
                vehicle.Vtype.Type = item.Type;
            }

            vehicle.Members.Vehicles.Add(vehicle);

            try
            {
                _context.Update(vehicle);
                await _context.SaveChangesAsync();
                Auxiliary.Operation = "Redigeringen är genomförd...";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehicleExists(vehicle.Id))
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

        // GET: Vehicles/Delete/5 (ändring: Björn Lindqvist)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Vehicle == null)
            {
                return NotFound();
            }

            Auxiliary.Reset(); // Raderar alla meddelanden.
            var vehicle = await _context.Vehicle
                .Include(v => v.Members)
                .Include(v => v.Vtype)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vehicle == null)
            {
                return NotFound();
            }

            return View(vehicle);
        }

        // POST: Vehicles/Delete/5 (ändring: Sorosh Farahmand och Björn Lindqvist)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Vehicle == null)
            {
                return Problem("Entity set 'Garage3BLContext.Vehicle'  is null.");
            }
            var vehicle = await _context.Vehicle.FindAsync(id);
            if (vehicle != null)
            {
                int antalreg = Auxiliary.Capacity.Count(c => c == vehicle.RegNo); // Räknar hur många gånger ett regnr förekommer i arrayen.
                Auxiliary.Counter -= antalreg;

                for (int i = 0; i < antalreg; i++)
                {
                    Auxiliary.Capacity[Array.IndexOf(Auxiliary.Capacity, vehicle.RegNo)] = ""; // Tar bort alla regnr från arrayen.
                }
                Auxiliary.Operation = $"{vehicle.RegNo} har checkat ut...";

                _context.Vehicle.Remove(vehicle);

                var price = Auxiliary.Pricehour;

                DateTime timeExit = DateTime.Now;

                TimeSpan span = timeExit.Subtract(vehicle.ArrivalTime);
                var spanInMinutes = span.TotalMinutes;
                var totalPrice = spanInMinutes * price / 60;

                var model = new Receipt
                {
                    PriceTotal = (int)totalPrice,
                };
                Statistic.InCome += model.PriceTotal;
                vehicle.InCome += model.PriceTotal;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VehicleExists(int id)
        {
          return (_context.Vehicle?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        // Av Sorosh Farahmand
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Receipt(int id)
        {
            if (_context.Vehicle == null)
            {
                return Problem("Entity set 'Garage2_0MVCContext.vehicle'  is null.");
            }
            var vehicle = await _context.Vehicle.FindAsync(id);

            //Får vi tillbaks ett fordon att ta bort? Hanterar null

            if (vehicle is null)
            {
                return NotFound();
            }

            int antalreg = Auxiliary.Capacity.Count(c => c == vehicle.RegNo); // Räknar hur många gånger ett regnr förekommer i arrayen.
            Auxiliary.Counter -= antalreg;

            for (int i = 0; i < antalreg; i++)
            {
                Auxiliary.Capacity[Array.IndexOf(Auxiliary.Capacity, vehicle.RegNo)] = ""; // Tar bort alla regnr från arrayen.
            }
            Auxiliary.Operation = "Det aktuella fordonet har checkat ut...";

            _context.Vehicle.Remove(vehicle);
            await _context.SaveChangesAsync();

            var price = Auxiliary.Pricehour;

            DateTime timeExit = DateTime.Now;

            TimeSpan span = timeExit.Subtract(vehicle.ArrivalTime);
            var spanInMinutes = span.TotalMinutes;
            var totalPrice = spanInMinutes * price / 60;
            //Create model for receipt
            //Add information from vehicle to model
            //Send model to Receipt View
            var model = new Receipt
            {
                Id = id,
                RegNo = vehicle.RegNo,
                Vtype = vehicle.Vtype,
                TimeEnter = vehicle.ArrivalTime,
                TimeExit = timeExit,
                Price = price,
                PriceTotal = (int)totalPrice,
            };
            Statistic.InCome += model.PriceTotal;
            vehicle.InCome += model.PriceTotal;

            return View(model);
        }
    }
}
