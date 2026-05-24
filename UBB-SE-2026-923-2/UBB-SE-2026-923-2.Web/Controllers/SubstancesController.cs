namespace UBB_SE_2026_923_2.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.Web.ViewModels;

    [Authorize(Roles = "Admin")]
    public class SubstancesController : Controller
    {
        private readonly IAdminService adminService;

        public SubstancesController(IAdminService adminService)
        {
            this.adminService = adminService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            List<Substance> substances = this.adminService.GetAllSubstances();
            return this.View(substances);
        }

        [HttpGet]
        public IActionResult Details(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return this.NotFound();
            }

            Substance substance = this.adminService.GetSubstanceByName(name);
            if (substance == null)
            {
                return this.NotFound();
            }

            return this.View(substance);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return this.View(new SubstanceViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(SubstanceViewModel viewModel)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(viewModel);
            }

            var newSubstance = new Substance(viewModel.Name, viewModel.LethalDose, viewModel.Description);

            try
            {
                this.adminService.AddSubstance(newSubstance);
                return this.RedirectToAction(nameof(this.Index));
            }
            catch (ArgumentException argumentException)
            {
                this.ModelState.AddModelError(string.Empty, argumentException.Message);
                return this.View(viewModel);
            }
        }

        [HttpGet]
        public IActionResult Edit(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return this.NotFound();
            }

            Substance substance = this.adminService.GetSubstanceByName(name);
            if (substance == null)
            {
                return this.NotFound();
            }

            var viewModel = new SubstanceViewModel
            {
                Name = substance.Name,
                LethalDose = substance.LethalDose,
                Description = substance.Description,
            };

            return this.View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string name, SubstanceViewModel viewModel)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(viewModel);
            }

            var updatedSubstance = new Substance(viewModel.Name, viewModel.LethalDose, viewModel.Description);

            try
            {
                this.adminService.UpdateSubstanceByName(name, updatedSubstance);
                return this.RedirectToAction(nameof(this.Index));
            }
            catch (ArgumentException argumentException)
            {
                this.ModelState.AddModelError(string.Empty, argumentException.Message);
                return this.View(viewModel);
            }
        }

        [HttpGet]
        public IActionResult Delete(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return this.NotFound();
            }

            Substance substance = this.adminService.GetSubstanceByName(name);
            if (substance == null)
            {
                return this.NotFound();
            }

            return this.View(substance);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string name)
        {
            Substance substance = this.adminService.GetSubstanceByName(name);
            if (substance != null)
            {
                this.adminService.RemoveSubstanceByName(substance);
            }

            return this.RedirectToAction(nameof(this.Index));
        }
    }
}
