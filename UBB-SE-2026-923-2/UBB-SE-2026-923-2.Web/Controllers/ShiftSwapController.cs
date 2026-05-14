using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UBB_SE_2026_923_2.Repositories;
using UBB_SE_2026_923_2.Services;

namespace UBB_SE_2026_923_2.Web.Controllers
{
    [Authorize(Roles = "Manager,Nurse")] // TODO re-check this in the destop app, see exaclty which roles have this permission
    public class ShiftSwapController : Controller
    {
        private readonly IShiftSwapService _shiftSwapService;

        public ShiftSwapController(IShiftSwapService shiftSwapService)
        {
            _shiftSwapService = shiftSwapService;
        }

        // GET: ShiftSwapController
        public ActionResult Index()
        {
            return View();
        }

        // GET: ShiftSwapController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ShiftSwapController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ShiftSwapController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ShiftSwapController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ShiftSwapController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ShiftSwapController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ShiftSwapController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
