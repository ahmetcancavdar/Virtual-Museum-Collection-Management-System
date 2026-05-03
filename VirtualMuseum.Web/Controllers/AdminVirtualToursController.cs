using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VirtualMuseum.Web.Data;
using VirtualMuseum.Web.Models;
using VirtualMuseum.Web.Models.ViewModels;

namespace VirtualMuseum.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminVirtualToursController : Controller
    {
        private readonly VirtualMuseumDbContext _context;

        public AdminVirtualToursController(VirtualMuseumDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var items = await (
                from p in _context.VirtualTourPlans
                join e in _context.Exhibitions on p.ExhibitionId equals e.ExhibitionId
                orderby e.Title
                select new VirtualTourPlanListItemViewModel
                {
                    PlanId = p.PlanId,
                    ExhibitionId = e.ExhibitionId,
                    ExhibitionTitle = e.Title,
                    Title = p.Title,
                    EstimatedDurationMinutes = p.EstimatedDurationMinutes,
                    IsActive = p.IsActive,
                    StopCount = _context.VirtualTourStops.Count(s => s.PlanId == p.PlanId)
                }
            ).ToListAsync();

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateExhibitionsDropDownAsync();
            return View(new VirtualTourPlanFormViewModel
            {
                EstimatedDurationMinutes = 20,
                IsActive = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VirtualTourPlanFormViewModel model)
        {
            model.Title = model.Title.Trim();
            model.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();

            if (!await _context.Exhibitions.AnyAsync(e => e.ExhibitionId == model.ExhibitionId))
            {
                ModelState.AddModelError(nameof(model.ExhibitionId), "Selected exhibition was not found.");
            }

            var duplicatePlan = await _context.VirtualTourPlans.AnyAsync(p =>
                p.ExhibitionId == model.ExhibitionId);

            if (duplicatePlan)
            {
                ModelState.AddModelError("", "This exhibition already has a virtual tour plan.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateExhibitionsDropDownAsync(model.ExhibitionId);
                return View(model);
            }

            var plan = new VirtualTourPlan
            {
                ExhibitionId = model.ExhibitionId,
                Title = model.Title,
                Description = model.Description,
                EstimatedDurationMinutes = model.EstimatedDurationMinutes,
                IsActive = model.IsActive
            };

            _context.VirtualTourPlans.Add(plan);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Virtual tour plan created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var plan = await _context.VirtualTourPlans.FindAsync(id);

            if (plan == null)
            {
                return NotFound();
            }

            var vm = new VirtualTourPlanFormViewModel
            {
                PlanId = plan.PlanId,
                ExhibitionId = plan.ExhibitionId,
                Title = plan.Title,
                Description = plan.Description,
                EstimatedDurationMinutes = plan.EstimatedDurationMinutes,
                IsActive = plan.IsActive
            };

            await PopulateExhibitionsDropDownAsync(vm.ExhibitionId);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VirtualTourPlanFormViewModel model)
        {
            if (id != model.PlanId)
            {
                return BadRequest();
            }

            model.Title = model.Title.Trim();
            model.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();

            if (!await _context.Exhibitions.AnyAsync(e => e.ExhibitionId == model.ExhibitionId))
            {
                ModelState.AddModelError(nameof(model.ExhibitionId), "Selected exhibition was not found.");
            }

            var duplicatePlan = await _context.VirtualTourPlans.AnyAsync(p =>
                p.PlanId != model.PlanId &&
                p.ExhibitionId == model.ExhibitionId);

            if (duplicatePlan)
            {
                ModelState.AddModelError("", "Another plan already exists for this exhibition.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateExhibitionsDropDownAsync(model.ExhibitionId);
                return View(model);
            }

            var plan = await _context.VirtualTourPlans.FindAsync(id);

            if (plan == null)
            {
                return NotFound();
            }

            plan.ExhibitionId = model.ExhibitionId;
            plan.Title = model.Title;
            plan.Description = model.Description;
            plan.EstimatedDurationMinutes = model.EstimatedDurationMinutes;
            plan.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Virtual tour plan updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await (
                from p in _context.VirtualTourPlans
                join e in _context.Exhibitions on p.ExhibitionId equals e.ExhibitionId
                where p.PlanId == id
                select new VirtualTourPlanDeleteViewModel
                {
                    PlanId = p.PlanId,
                    ExhibitionTitle = e.Title,
                    Title = p.Title,
                    EstimatedDurationMinutes = p.EstimatedDurationMinutes,
                    IsActive = p.IsActive,
                    StopCount = _context.VirtualTourStops.Count(s => s.PlanId == p.PlanId)
                }
            ).FirstOrDefaultAsync();

            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var plan = await _context.VirtualTourPlans.FindAsync(id);

            if (plan == null)
            {
                return NotFound();
            }

            var stops = await _context.VirtualTourStops
                .Where(s => s.PlanId == id)
                .ToListAsync();

            _context.VirtualTourStops.RemoveRange(stops);
            _context.VirtualTourPlans.Remove(plan);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Virtual tour plan and stops deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ManageStops(int id)
        {
            var planInfo = await (
                from p in _context.VirtualTourPlans
                join e in _context.Exhibitions on p.ExhibitionId equals e.ExhibitionId
                where p.PlanId == id
                select new
                {
                    p.PlanId,
                    p.Title,
                    ExhibitionTitle = e.Title
                }
            ).FirstOrDefaultAsync();

            if (planInfo == null)
            {
                return NotFound();
            }

            ViewBag.PlanTitle = planInfo.Title;
            ViewBag.ExhibitionTitle = planInfo.ExhibitionTitle;
            ViewBag.PlanId = planInfo.PlanId;

            var stops = await _context.VirtualTourStops
                .Where(s => s.PlanId == id)
                .OrderBy(s => s.StepNo)
                .Select(s => new VirtualTourStopListItemViewModel
                {
                    StopId = s.StopId,
                    PlanId = s.PlanId,
                    StepNo = s.StepNo,
                    RoomId = s.RoomId,
                    RoomName = s.RoomName,
                    StopTitle = s.StopTitle,
                    Notes = s.Notes,
                    EstimatedMinutes = s.EstimatedMinutes
                })
                .ToListAsync();

            return View(stops);
        }

        [HttpGet]
        public async Task<IActionResult> CreateStop(int planId)
        {
            var planInfo = await (
                from p in _context.VirtualTourPlans
                join e in _context.Exhibitions on p.ExhibitionId equals e.ExhibitionId
                where p.PlanId == planId
                select new
                {
                    p.PlanId,
                    PlanTitle = p.Title,
                    ExhibitionTitle = e.Title
                }
            ).FirstOrDefaultAsync();

            if (planInfo == null)
            {
                return NotFound();
            }

            var maxStep = await _context.VirtualTourStops
                .Where(s => s.PlanId == planId)
                .Select(s => (int?)s.StepNo)
                .MaxAsync() ?? 0;

            var vm = new VirtualTourStopFormViewModel
            {
                PlanId = planInfo.PlanId,
                PlanTitle = planInfo.PlanTitle,
                ExhibitionTitle = planInfo.ExhibitionTitle,
                StepNo = maxStep + 1,
                EstimatedMinutes = 5
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStop(VirtualTourStopFormViewModel model)
        {
            model.RoomName = model.RoomName.Trim();
            model.StopTitle = model.StopTitle.Trim();
            model.Notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim();

            await ValidateStopModelAsync(model, null);

            if (!ModelState.IsValid)
            {
                await PopulateStopPlanInfoAsync(model);
                return View(model);
            }

            var stop = new VirtualTourStop
            {
                PlanId = model.PlanId,
                StepNo = model.StepNo,
                RoomId = model.RoomId,
                RoomName = model.RoomName,
                StopTitle = model.StopTitle,
                Notes = model.Notes,
                EstimatedMinutes = model.EstimatedMinutes
            };

            _context.VirtualTourStops.Add(stop);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Route stop created successfully.";
            return RedirectToAction(nameof(ManageStops), new { id = model.PlanId });
        }

        [HttpGet]
        public async Task<IActionResult> EditStop(int id)
        {
            var stop = await _context.VirtualTourStops.FindAsync(id);

            if (stop == null)
            {
                return NotFound();
            }

            var vm = new VirtualTourStopFormViewModel
            {
                StopId = stop.StopId,
                PlanId = stop.PlanId,
                StepNo = stop.StepNo,
                RoomId = stop.RoomId,
                RoomName = stop.RoomName,
                StopTitle = stop.StopTitle,
                Notes = stop.Notes,
                EstimatedMinutes = stop.EstimatedMinutes
            };

            await PopulateStopPlanInfoAsync(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStop(int id, VirtualTourStopFormViewModel model)
        {
            if (id != model.StopId)
            {
                return BadRequest();
            }

            model.RoomName = model.RoomName.Trim();
            model.StopTitle = model.StopTitle.Trim();
            model.Notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim();

            await ValidateStopModelAsync(model, model.StopId);

            if (!ModelState.IsValid)
            {
                await PopulateStopPlanInfoAsync(model);
                return View(model);
            }

            var stop = await _context.VirtualTourStops.FindAsync(id);

            if (stop == null)
            {
                return NotFound();
            }

            stop.StepNo = model.StepNo;
            stop.RoomId = model.RoomId;
            stop.RoomName = model.RoomName;
            stop.StopTitle = model.StopTitle;
            stop.Notes = model.Notes;
            stop.EstimatedMinutes = model.EstimatedMinutes;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Route stop updated successfully.";
            return RedirectToAction(nameof(ManageStops), new { id = model.PlanId });
        }

        [HttpGet]
        public async Task<IActionResult> DeleteStop(int id)
        {
            var item = await (
                from s in _context.VirtualTourStops
                join p in _context.VirtualTourPlans on s.PlanId equals p.PlanId
                join e in _context.Exhibitions on p.ExhibitionId equals e.ExhibitionId
                where s.StopId == id
                select new VirtualTourStopDeleteViewModel
                {
                    StopId = s.StopId,
                    PlanId = s.PlanId,
                    PlanTitle = p.Title,
                    ExhibitionTitle = e.Title,
                    StepNo = s.StepNo,
                    RoomId = s.RoomId,
                    RoomName = s.RoomName,
                    StopTitle = s.StopTitle,
                    EstimatedMinutes = s.EstimatedMinutes
                }
            ).FirstOrDefaultAsync();

            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpPost, ActionName("DeleteStop")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStopConfirmed(int id)
        {
            var stop = await _context.VirtualTourStops.FindAsync(id);

            if (stop == null)
            {
                return NotFound();
            }

            var planId = stop.PlanId;

            _context.VirtualTourStops.Remove(stop);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Route stop deleted successfully.";
            return RedirectToAction(nameof(ManageStops), new { id = planId });
        }

        private async Task PopulateExhibitionsDropDownAsync(int? selectedExhibitionId = null)
        {
            var exhibitions = await _context.Exhibitions
                .OrderBy(e => e.Title)
                .Select(e => new SelectListItem
                {
                    Value = e.ExhibitionId.ToString(),
                    Text = e.Title
                })
                .ToListAsync();

            ViewBag.Exhibitions = new SelectList(exhibitions, "Value", "Text", selectedExhibitionId?.ToString());
        }

        private async Task PopulateStopPlanInfoAsync(VirtualTourStopFormViewModel model)
        {
            var info = await (
                from p in _context.VirtualTourPlans
                join e in _context.Exhibitions on p.ExhibitionId equals e.ExhibitionId
                where p.PlanId == model.PlanId
                select new
                {
                    PlanTitle = p.Title,
                    ExhibitionTitle = e.Title
                }
            ).FirstOrDefaultAsync();

            if (info != null)
            {
                model.PlanTitle = info.PlanTitle;
                model.ExhibitionTitle = info.ExhibitionTitle;
            }
        }

        private async Task ValidateStopModelAsync(VirtualTourStopFormViewModel model, int? currentStopId)
        {
            if (!await _context.VirtualTourPlans.AnyAsync(p => p.PlanId == model.PlanId))
            {
                ModelState.AddModelError(nameof(model.PlanId), "Selected plan was not found.");
            }

            var duplicateStep = await _context.VirtualTourStops.AnyAsync(s =>
                s.PlanId == model.PlanId &&
                s.StepNo == model.StepNo &&
                (!currentStopId.HasValue || s.StopId != currentStopId.Value));

            if (duplicateStep)
            {
                ModelState.AddModelError(nameof(model.StepNo), "Another stop with the same step number already exists in this plan.");
            }
        }
    }
}
