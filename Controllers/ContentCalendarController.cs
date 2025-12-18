using System;
using System.Linq;
using System.Threading.Tasks;
using BLOGAURA.Data;
using BLOGAURA.Models.Content;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BLOGAURA.Controllers
{
    [Authorize(Roles = "Admin,Editor")]
    public class ContentCalendarController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContentCalendarController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? contentType, string? status, string? audience)
        {
            var now = DateTime.UtcNow.Date;
            var cutoff = now.AddDays(42);

            var query = _context.ContentCalendar
                .Where(c => c.PlannedPublishDate.Date >= now && c.PlannedPublishDate.Date <= cutoff)
                .OrderBy(c => c.PlannedPublishDate)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(contentType))
            {
                query = query.Where(c => c.ContentType == contentType);
            }
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(c => c.Status == status);
            }
            if (!string.IsNullOrWhiteSpace(audience))
            {
                query = query.Where(c => c.TargetAudience == audience);
            }

            var items = await query.ToListAsync();
            return View(items);
        }

        [HttpGet]
        public IActionResult Create()
        {
            // Optional: provide event posts to select from
            try
            {
                var eventPosts = _context.Posts
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(50)
                    .Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = $"#{p.Id} • {p.CreatedAt:yyyy-MM-dd} • {p.Title}"
                    })
                    .ToList();
                ViewBag.EventPosts = eventPosts;
            }
            catch { }

            return View(new ContentCalendarItem
            {
                PlannedPublishDate = DateTime.UtcNow.Date,
                Status = "Planned"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContentCalendarItem model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var editorUserId))
            {
                return RedirectToAction("Login", "Auth");
            }

            model.EditorUserId = editorUserId;
            _context.ContentCalendar.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.ContentCalendar.FirstOrDefaultAsync(c => c.Id == id);
            if (item == null) return NotFound();
            try
            {
                var eventPosts = await _context.Posts
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(50)
                    .Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = $"#{p.Id} • {p.CreatedAt:yyyy-MM-dd} • {p.Title}",
                    })
                    .ToListAsync();
                ViewBag.EventPosts = eventPosts;
            }
            catch { }
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ContentCalendarItem model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var existing = await _context.ContentCalendar.FirstOrDefaultAsync(c => c.Id == id);
            if (existing == null) return NotFound();

            existing.PlannedPublishDate = model.PlannedPublishDate;
            existing.Status = model.Status;
            existing.Notes = model.Notes;
            existing.PostId = model.PostId;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.ContentCalendar.FirstOrDefaultAsync(c => c.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.ContentCalendar.FirstOrDefaultAsync(c => c.Id == id);
            if (item == null) return NotFound();
            _context.ContentCalendar.Remove(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Monthly(int? year, int? month)
        {
            var today = DateTime.Today;
            var y = year ?? today.Year;
            var m = month ?? today.Month;
            var first = new DateTime(y, m, 1);
            var last = first.AddMonths(1).AddDays(-1);

            var items = await _context.ContentCalendar
                .Where(c => c.PlannedPublishDate.Date >= first.Date && c.PlannedPublishDate.Date <= last.Date)
                .OrderBy(c => c.PlannedPublishDate)
                .ToListAsync();

            ViewBag.Year = y;
            ViewBag.Month = m;
            return View(items);
        }
    }
}
