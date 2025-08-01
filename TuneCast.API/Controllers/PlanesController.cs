﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuneCastModelo;

namespace TuneCast.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlanesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PlanesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Planes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Plan>>> GetPlan()
        {
            var planes = await _context.Planes.ToListAsync();
            foreach (var plan in planes)
            {
                plan.Precio = Math.Round(plan.Precio, 2, MidpointRounding.AwayFromZero);
            }

            return planes;
        }

        // GET: api/Planes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Plan>> GetPlan(int id)
        {
            var plan = await _context.Planes.FindAsync(id);

            if (plan == null)
            {
                return NotFound();
            }
            plan.Precio = Math.Round(plan.Precio, 2, MidpointRounding.AwayFromZero);

            return plan;
        }

        // PUT: api/Planes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPlan(int id, Plan plan)
        {
            if (id != plan.Id)
            {
                return BadRequest();
            }
            plan.Precio = Math.Round(plan.Precio, 2, MidpointRounding.AwayFromZero);

            _context.Entry(plan).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlanExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Planes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Plan>> PostPlan(Plan plan)
        {
            plan.Precio = Math.Round(plan.Precio, 2, MidpointRounding.AwayFromZero);
            _context.Planes.Add(plan);
            await _context.SaveChangesAsync();
            plan.Precio = Math.Round(plan.Precio, 2, MidpointRounding.AwayFromZero);
            return CreatedAtAction("GetPlan", new { id = plan.Id }, plan);
        }

        // DELETE: api/Planes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlan(int id)
        {
            var plan = await _context.Planes.FindAsync(id);
            if (plan == null)
            {
                return NotFound();
            }

            _context.Planes.Remove(plan);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PlanExists(int id)
        {
            return _context.Planes.Any(e => e.Id == id);
        }
    }
}
