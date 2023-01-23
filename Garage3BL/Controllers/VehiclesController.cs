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
        public string Ptime(DateTime arrival) // Levererar tidsintervallet.
        {
            int d = DateTime.Now.Subtract(arrival).Days;
            int t = DateTime.Now.Subtract(arrival).Hours;
            int m = DateTime.Now.Subtract(arrival).Minutes;
            return $"{d}d {t}t {m}m";
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

        // GET: Vehicles (ändring: Björn Lindqvist)
        public async Task<IActionResult> Index()
        {
            string typ = string.Empty;

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
                    item.Place = "[0]";

                    if (item.IsParked) // Parkerar in fordon från databasen.
                    {
                        var model = _context.Set<Vtype>() // Hämtar fordonstypen.
                            .Where(x => x.Id == item.VtypeId);
                        foreach (var id in model)
                        {
                            typ = id.Type;
                        }

                        switch (typ) // Laddar arrayen och skapar nödvändig plats.
                        {
                            case "Lastbil":
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
                            case "Buss":
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
                            case "Entreprenadmaskin" or "Traktor":
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
                    item.ParkedTime = Ptime(item.ArrivalTime); // För alla fordon ges P-tid.

                }
                await _context.SaveChangesAsync();
                Auxiliary.Start = false;
            }
            else
            {
                var vehicle = _context.Vehicle;

                foreach (var items in vehicle)
                {
                    items.ParkedTime = Ptime(items.ArrivalTime); // För alla fordon ges P-tid.
                }
            }                        
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
                    MemberId = v.MemberId,
                    VtypeId = v.VtypeId,                    
                });

            if (!string.IsNullOrWhiteSpace(searchString)) // Görs bara om det finns en söksträng.
            {
                model = model.Where(v => v.VtypeId.Equals(searchString) || v.RegNo.Equals(searchString));
            }

            if (model.ToList().Count == 0 && Auxiliary.Counter > 0) ModelState
                    .AddModelError("CustomError", "Kunde inte hitta någon motsvarande fordonstyp eller regnummer..."); // Om ingen match visas detta.

            switch (sortOrder) // Olika ordningsföljder.
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
        // För inställning av ny kapacitet och pris.
        public async Task<IActionResult> Plus(int capacity, int baseprice, int timprice, string subcap, string subbas, string subtim)
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
        public async Task<IActionResult> Vehicleadd(string newvehicle, string subadd, Vtype vtype) // Ny fordonstyp.
        {
            if (subadd == "Lägg till ny fordonstyp" && newvehicle != "")
            {
                vtype.Type = newvehicle;
                _context.Add(vtype);
                await _context.SaveChangesAsync();
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
            var mem = _context.Member // Hämtar den medlem som äger fordonet från Member.
                .Where(v => v.Id == vehicle.MemberId);

            foreach (var item in mem) // Ägarens uppgifter tilldelas Members i Vehicle.
            {
                vehicle.Members.MemberNo = item.MemberNo;
                vehicle.Members.FirstName = item.FirstName;
                vehicle.Members.LastName = item.LastName;
                vehicle.Members.PersonalNo = item.PersonalNo;
            }

            var veh = _context.Set<Vtype>() // Hämtar ut Vtype i Vehicle och som har rätt fordons-id.
                .Where(v => v.Id == vehicle.VtypeId);

            foreach (var item in veh) // Vtype.Type tilldelas fordonstypen.
            {
                vehicle.Vtype.Type = item.Type;
                vehicle.Vtype.Vehicles = item.Vehicles;
            }

            // Andra nödvändiga tilldelningar.
            vehicle.Place = "[0]";
            vehicle.ArrivalTime = DateTime.Now;
            vehicle.ParkedTime = Ptime(vehicle.ArrivalTime);
            vehicle.IsParked = false;                       

            vehicle.Members.Vehicles.Add(vehicle); // Vehicles i Members tilldelas det som nu finns i Vehicle (allt).

            vehicle.RegNo = vehicle.RegNo.ToUpper(); // Tvingar till versaler.
            bool flag = _context.Vehicle.Any(p => p.RegNo.ToUpper() == vehicle.RegNo); // Jämför regnummer i databasen med inkommande.

            if (!flag)
            {
                _context.Add(vehicle);
                await _context.SaveChangesAsync();
                ViewData["MemberId"] = new SelectList(_context.Member, "Id", "FullName", vehicle.MemberId);
                ViewData["VtypeId"] = new SelectList(_context.Set<Vtype>(), "Id", "Type", vehicle.VtypeId);
                Auxiliary.Operation = $"Ditt fordon nr {Auxiliary.Rak++} är nu inregistrerat...";
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
        public async Task<IActionResult> Parking_in(Vehicle park) // Sköter inparkering.
        {
            string typ = string.Empty;
            bool flag = false;
            int rak = 0;
            int tal = 0;

            var model = _context.Set<Vtype>() // Hämtar fordonstypen.
                            .Where(x => x.Id == park.VtypeId);
            foreach (var id in model)
            {
                typ = id.Type;
            }

            switch (typ)
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
                switch (typ) // Genomför inläggning oavsett lucka.
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
                    case "Traktor" or "Entreprenadmaskin":
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
                await _context.SaveChangesAsync();
                Auxiliary.Thanks4P = $"Tack {park.Members.FullName} för att du parkerar hos BL GARAGE !";
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
            //var names = _context.Member.Distinct().Select(s => s.PersonalNo);//.Select(n => new { FirstName = n }).ToList();
            //ViewData["MemberId"] = new SelectList(names, "Id", "MemberNo");
            ViewData["MemberId"] = new SelectList(_context.Member, "Id", "MemberNo").Distinct().ToList();
            ViewData["RegNo"] = new SelectList(_context.Vehicle, "Id", "RegNo");
            return View();
        }

        // Av Björn Lindqvist
        // POST: Vehicles/Parking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Parking(Vehicle vehicle)
        {
            string persnr = string.Empty;
            int dagar = 0;
            bool flag1 = false;
            bool flag4 = false;

            vehicle.RegNo = vehicle.RegNo.ToUpper(); // Tvingar till versaler.

            foreach (var item in _context.Vehicle) // Hämtar kontrollflagga.
            {
                if (item.RegNo == vehicle.RegNo)
                {
                    if (!item.IsParked) flag4 = true;
                }
            }

            foreach (var item in _context.Member) // Hämtar personnummer.
            {
                if (item.Id == vehicle.MemberId)
                {
                    persnr = item.PersonalNo;
                }
            }

            persnr = $"{persnr.Substring(0, 4)}-{persnr.Substring(4, 2)}-{persnr.Substring(6, 2)} 00:00:00";
            dagar = DateTime.Now.Subtract(DateTime.Parse(persnr)).Days;

            if (dagar > 6570) flag1 = true; // Är dagar mer än 18 år?

            bool flag2 = _context.Member.Any(p => p.Id == vehicle.MemberId); // Finns denna medlem?
            bool flag3 = _context.Vehicle.Any(p => p.RegNo == vehicle.RegNo); // Har medlemmen detta fordon (regnummer)?            

            if (flag1 && flag2 && flag3 && flag4)
            {
                foreach (var item in _context.Vehicle) // Nödvändiga tilldelningar görs innan parkering sker.
                {
                    if (item.RegNo == vehicle.RegNo)
                    {
                        item.IsParked = true;
                        item.ArrivalTime = DateTime.Now;
                        await Parking_in(item); // Försöker att parkera ett fordon.         
                    }
                }         
                return RedirectToAction(nameof(Index));
            }
            else
            {
                //var names = _db.ActivityMaps.Select(s => s.TeamName).Distinct().Select(n => new { TeamName = n }).ToList();
                //ViewData["TeamNames"] = new SelectList(names, "TeamName", "TeamName", model.TeamName);
                //var names = _context.Member.Distinct().Select(s => s.PersonalNo);//.Select(n => new { FirstName = n }).ToList();
                //ViewData["MemberId"] = new SelectList(names, "Id", "MemberNo", vehicle.MemberId);
                ViewData["MemberId"] = new SelectList(_context.Member, "Id", "MemberNo", vehicle.MemberId).Distinct().ToList();
                ViewData["RegNo"] = new SelectList(_context.Vehicle, "Id", "RegNo", vehicle.RegNo);
                Auxiliary.WarningReg = "Medlemmen eller regnumret finns inte...";
                if (!flag1) Auxiliary.WarningReg = "Man måste vara minst 18 år för att kunna parkera...";
                if (!flag4) Auxiliary.WarningReg = "Fordonet är redan parkerad...";
                return View(vehicle);
            }
        }

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
            ViewData["VtypeId"] = new SelectList(_context.Set<Vtype>(), "Id", "Type");
            return View(vehicle);
        }

        // POST: Vehicles/Edit/5 (ändring: Björn Lindqvist)
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

            var veh = _context.Vehicle // Fordonet med motsvarande id hämtas och dess data läggs i veh.
                .Where(v => v.Id == id)
                .Select(v => new Vehicle
                 {
                    Id = v.Id,
                    Place = v.Place,
                    ArrivalTime = v.ArrivalTime,
                    ParkedTime = v.ParkedTime,
                    IsParked = v.IsParked,
                    InCome = v.InCome,                   
                    Members = v.Members,
                    MemberId = v.MemberId,
                });

            foreach (var item in veh) // All data som hör till det redigerade fordonet tilldelas vehicle.
            {
                vehicle.Id = item.Id;
                vehicle.Place = item.Place;
                vehicle.ArrivalTime = item.ArrivalTime;
                vehicle.ParkedTime = item.ParkedTime;
                vehicle.IsParked = item.IsParked;
                vehicle.InCome = item.InCome;
                vehicle.Members = item.Members;
                vehicle.MemberId = item.MemberId;
            }

            vehicle.RegNo = vehicle.RegNo.ToUpper();

            var vty = _context.Set<Vtype>() // Hämtar ut Vtype i Vehicle och som har rätt fordons-id.
                .Where(v => v.Id == vehicle.VtypeId);

            foreach (var item in vty) // Vtype.Type tilldelas fordonstypen.
            {
                vehicle.Vtype.Type = item.Type;
                vehicle.Vtype.Vehicles = item.Vehicles;
            }

            vehicle.Members.Vehicles.Add(vehicle); // Vehicles i Members tilldelas allt som nu finns i Vehicle.

            if (!vehicle.IsParked)
            {
                try
                {
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
                ViewData["VtypeId"] = new SelectList(_context.Set<Vtype>(), "Id", "Type", vehicle.VtypeId);
                return View(vehicle);
            }
            else
            {
                ViewData["VtypeId"] = new SelectList(_context.Set<Vtype>(), "Id", "Type", vehicle.VtypeId);
                Auxiliary.WarningReg = "Fordonet måste checka ut först innan någon ändring kan göras...";
                return View(vehicle);
            }

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
            int idmem = 0;
            //int vid = 0;

            if (_context.Vehicle == null)
            {
                return Problem("Entity set 'Garage3BLContext.Vehicle'  is null.");
            }

            var mem = _context.Vehicle
                .Where(v => v.Id == id)
                .Select(v => new Vehicle
                {
                    Id = v.Id,
                    MemberId = v.MemberId,
                    VtypeId = v.VtypeId
                });

            foreach (var item in mem)
            {
                idmem = item.MemberId;
                //vid = item.VtypeId; <-- finns det en bättre metod?
            }

            var vehicle = await _context.Vehicle.FindAsync(id);
            var member = await _context.Member.FindAsync(idmem);
            //var vtype = _context.Set<Vtype>().Find(idmem);

            if (vehicle != null && member != null)
            {
                if (!vehicle.IsParked)
                {                    
                    _context.Vehicle.Remove(vehicle);
                    _context.Member.Remove(member);
                    //_context.Set<Vtype>().Remove(vtype);
                    await _context.SaveChangesAsync();
                    Auxiliary.Operation = $"{vehicle.RegNo} är avregistrerad...";
                }
                else
                {
                    Auxiliary.WarningReg = $"{vehicle.RegNo} är parkerad och måste checkas ut innan avregistrering...";
                }
            }            
            return View(vehicle);
        }

        private bool VehicleExists(int id)
        {
          return (_context.Vehicle?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public void Parking_out(Vehicle park) // Sköter utparkering.
        {
            int antalreg = Auxiliary.Capacity.Count(c => c == park.RegNo); // Räknar hur många gånger ett regnr förekommer i arrayen.
            Auxiliary.Counter -= antalreg;

            for (int i = 0; i < antalreg; i++)
            {
                Auxiliary.Capacity[Array.IndexOf(Auxiliary.Capacity, park.RegNo)] = ""; // Tar bort alla regnr från arrayen.
            }
            park.Place = "[0]";
            park.IsParked = false;
        }

        // GET: Vehicles/Delete/5 (ändring: Björn Lindqvist)
        public async Task<IActionResult> Unparking(int? id)
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
        [HttpPost, ActionName("Unparking")]
        [ValidateAntiForgeryToken]
        // Av Björn Lindqvist)
        public async Task<IActionResult> Unparking(int id)
        {
            if (_context.Vehicle == null)
            {
                return Problem("Entity set 'Garage3BLContext.Vehicle'  is null.");
            }
            var vehicle = await _context.Vehicle.FindAsync(id);

            if (vehicle != null)
            {
                Parking_out(vehicle);
                Auxiliary.Operation = $"{vehicle.RegNo} checkade precis ut - Välkommen åter!";

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
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }

        // Av Sorosh Farahmand och Björn Lindqvist
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

            Parking_out(vehicle);
            Auxiliary.Operation = $"{vehicle.RegNo} checkade precis ut - Välkommen åter!";

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

            await _context.SaveChangesAsync();
            return View(model);
        }
    }
}
