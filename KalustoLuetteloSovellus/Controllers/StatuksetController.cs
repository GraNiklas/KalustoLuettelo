﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KalustoLuetteloSovellus.Models;

namespace KalustoLuetteloSovellus.Controllers
{
    public class StatuksetController : Controller
    {
        private readonly KaluDbContext _context;

        public StatuksetController(KaluDbContext context)
        {
            _context = context;
        }

        // GET: Status
        [ServiceFilter(typeof(AdminOnlyFilter))]
        public async Task<IActionResult> Index()
        {
        return View(await _context.Statukset.ToListAsync());
        }

        // GET: Status/Details/5
        [ServiceFilter(typeof(AdminOnlyFilter))]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var status = await _context.Statukset
                .FirstOrDefaultAsync(m => m.StatusId == id);
            if (status == null)
            {
                return NotFound();
            }

            return View(status);
        }

        // GET: Status/Create
        [ServiceFilter(typeof(AdminOnlyFilter))]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Status/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(AdminOnlyFilter))]
        public async Task<IActionResult> Create([Bind("StatusId,StatusNimi")] Status status)
        {
                _context.Add(status);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
        }

        // GET: Status/Edit/5
        [ServiceFilter(typeof(AdminOnlyFilter))]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var status = await _context.Statukset.FindAsync(id);
            if (status == null)
            {
                return NotFound();
            }
            if (IsProtected(status))
            {
                TempData["ErrorMessage"] = "Kyseinen status ei ole muokattavissa...";
                return RedirectToAction(nameof(Index));
            }
            return View(status);
        }

        // POST: Status/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(AdminOnlyFilter))]
        public async Task<IActionResult> Edit(int id, [Bind("StatusId,StatusNimi")] Status status)
        {
            if (id != status.StatusId)
            {
                return NotFound();
            }

            
            
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(status);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StatusExists(status.StatusId))
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
            return View(status);
        }

        // GET: Status/Delete/5
        [ServiceFilter(typeof(AdminOnlyFilter))]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var status = await _context.Statukset
                .FirstOrDefaultAsync(m => m.StatusId == id);
            if (status == null)
            {
                return NotFound();
            }
            if (IsProtected(status))
            {
                TempData["ErrorMessage"] = "Kyseinen status ei ole poistettavissa...";
                return RedirectToAction(nameof(Index));
            }

            return View(status);
        }

        // POST: Status/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(AdminOnlyFilter))]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var status = await _context.Statukset.FindAsync(id);
            if (status != null)
            {
                _context.Statukset.Remove(status);
                
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StatusExists(int id)
        {
            return _context.Statukset.Any(e => e.StatusId == id);
        }

        public bool IsProtected(Status status)
        {
            var protectedStatuses = new List<String> { "Varattu", "Vapaa", "Huollossa", "Kadonnut", "Poistettu" };
            return protectedStatuses.Contains(status.StatusNimi);
        }
    }
}
