namespace UBB_SE_2026_923_2.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Security.Claims;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;
    using UBB_SE_2026_923_2.Web.ViewModels;

    [Authorize]
    public class DoctorAppointmentsController : Controller
    {
        private const string ScheduledStatus = "Scheduled";
        private const string FinishedStatus = "Finished";
        private const string CanceledStatus = "Canceled";

        private readonly IDoctorAppointmentService appointmentService;

        public DoctorAppointmentsController(IDoctorAppointmentService appointmentService)
        {
            this.appointmentService = appointmentService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Index(int? doctorId)
        {
            var model = new DoctorAppointmentListViewModel();

            try
            {
                model.Doctors = await this.LoadDoctorOptionsAsync();
                if (model.Doctors.Count == 0)
                {
                    model.ErrorMessage = "No doctors available.";
                    return this.View(model);
                }

                model.SelectedDoctorId = doctorId ?? model.Doctors[0].DoctorId;

                var appointments = await this.appointmentService.GetAppointmentsForAdminAsync(model.SelectedDoctorId.Value);
                var doctorLookup = model.Doctors.ToDictionary(item => item.DoctorId, item => item.DoctorName);

                model.Appointments = appointments
                    .Select(appointment => MapListItem(appointment, doctorLookup))
                    .ToList();
            }
            catch (Exception exception)
            {
                model.ErrorMessage = exception.Message;
            }

            return this.View(model);
        }

        [Authorize(Roles = "Doctor")]
        [HttpGet]
        public async Task<IActionResult> Schedule(DateTime? fromDate, DateTime? toDate)
        {
            var model = new DoctorAppointmentScheduleViewModel
            {
                FromDate = fromDate?.Date ?? DateTime.Today,
                ToDate = toDate?.Date ?? DateTime.Today.AddDays(7),
            };

            int? doctorId = await this.GetCurrentDoctorIdAsync();
            if (!doctorId.HasValue)
            {
                model.ErrorMessage = "Unable to resolve your account.";
                return this.View(model);
            }

            if (model.ToDate <= model.FromDate)
            {
                model.ErrorMessage = "End date must be after start date.";
                return this.View(model);
            }

            try
            {
                var appointments = await this.appointmentService.GetAppointmentsInRangeAsync(
                    doctorId.Value,
                    model.FromDate,
                    model.ToDate);

                model.Appointments = appointments
                    .Select(appointment => MapListItem(appointment, new Dictionary<int, string>()))
                    .ToList();
            }
            catch (Exception exception)
            {
                model.ErrorMessage = exception.Message;
            }

            return this.View(model);
        }

        [Authorize(Roles = "Admin,Doctor")]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var appointment = await this.appointmentService.GetAppointmentDetailsAsync(id);
            if (appointment == null)
            {
                return this.NotFound();
            }

            if (this.User.IsInRole("Doctor"))
            {
                int? doctorId = await this.GetCurrentDoctorIdAsync();
                if (!doctorId.HasValue || appointment.Doctor?.StaffID != doctorId.Value)
                {
                    return this.Forbid();
                }
            }

            var doctorName = await this.ResolveDoctorNameAsync(appointment.Doctor?.StaffID);

            var model = new DoctorAppointmentDetailsViewModel
            {
                Id = appointment.Id,
                DoctorName = doctorName,
                PatientName = appointment.PatientName,
                Date = appointment.Date,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                Status = appointment.Status,
                Type = appointment.Type,
                Location = appointment.Location,
                Notes = appointment.Notes,
            };

            return this.View(model);
        }

        [Authorize(Roles = "Admin,Doctor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finish(int id)
        {
            var appointment = await this.appointmentService.GetAppointmentDetailsAsync(id);
            if (appointment == null)
            {
                return this.NotFound();
            }

            if (this.User.IsInRole("Doctor"))
            {
                int? doctorId = await this.GetCurrentDoctorIdAsync();
                if (!doctorId.HasValue || appointment.Doctor?.StaffID != doctorId.Value)
                {
                    return this.Forbid();
                }
            }

            try
            {
                await this.appointmentService.FinishAppointmentAsync(appointment);
                this.TempData["DetailsSuccess"] = "Appointment finished successfully! Doctor status updated.";
            }
            catch (Exception exception)
            {
                this.TempData["DetailsError"] = $"Error: {exception.Message}";
            }

            return this.RedirectToAction(nameof(this.Details), new { id });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new DoctorAppointmentCreateViewModel
            {
                Doctors = await this.LoadDoctorOptionsAsync(),
            };

            if (model.Doctors.Count > 0)
            {
                model.DoctorId = model.Doctors[0].DoctorId;
            }

            return this.View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DoctorAppointmentCreateViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                model.Doctors = await this.LoadDoctorOptionsAsync();
                return this.View(model);
            }

            try
            {
                await this.appointmentService.CreateAppointmentAsync(
                    model.PatientName,
                    model.DoctorId,
                    model.Date,
                    model.StartTime);
            }
            catch (Exception exception)
            {
                this.ModelState.AddModelError(string.Empty, exception.Message);
                model.Doctors = await this.LoadDoctorOptionsAsync();
                return this.View(model);
            }

            return this.RedirectToAction(nameof(this.Index), new { doctorId = model.DoctorId });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var appointment = await this.appointmentService.GetAppointmentDetailsAsync(id);
            if (appointment == null)
            {
                return this.NotFound();
            }

            var doctorName = await this.ResolveDoctorNameAsync(appointment.Doctor?.StaffID);

            var model = new DoctorAppointmentEditViewModel
            {
                Id = appointment.Id,
                DoctorName = doctorName,
                PatientName = appointment.PatientName,
                Date = appointment.Date,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                Status = appointment.Status,
            };

            if (!string.Equals(appointment.Status, ScheduledStatus, StringComparison.OrdinalIgnoreCase))
            {
                this.ModelState.AddModelError(string.Empty, "Only scheduled appointments can be finished.");
            }

            return this.View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DoctorAppointmentEditViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var appointment = await this.appointmentService.GetAppointmentDetailsAsync(model.Id);
            if (appointment == null)
            {
                return this.NotFound();
            }

            if (!string.Equals(model.Status, FinishedStatus, StringComparison.OrdinalIgnoreCase))
            {
                this.ModelState.AddModelError(nameof(model.Status), "Only finishing an appointment is supported here.");
                return this.View(model);
            }

            if (!string.Equals(appointment.Status, ScheduledStatus, StringComparison.OrdinalIgnoreCase))
            {
                this.ModelState.AddModelError(string.Empty, "Only scheduled appointments can be finished.");
                return this.View(model);
            }

            try
            {
                if (string.Equals(model.Status, FinishedStatus, StringComparison.OrdinalIgnoreCase))
                {
                    await this.appointmentService.FinishAppointmentAsync(appointment);
                }
                else if (string.Equals(model.Status, CanceledStatus, StringComparison.OrdinalIgnoreCase))
                {
                    await this.appointmentService.CancelAppointmentAsync(appointment);
                }
            }
            catch (Exception exception)
            {
                this.ModelState.AddModelError(string.Empty, exception.Message);
                model.DoctorName = await this.ResolveDoctorNameAsync(appointment.Doctor?.StaffID);
                model.PatientName = appointment.PatientName;
                model.Date = appointment.Date;
                model.StartTime = appointment.StartTime;
                model.EndTime = appointment.EndTime;
                model.Status = appointment.Status;
                return this.View(model);
            }

            return this.RedirectToAction(nameof(this.Details), new { id = model.Id });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var appointment = await this.appointmentService.GetAppointmentDetailsAsync(id);
            if (appointment == null)
            {
                return this.NotFound();
            }

            var model = new DoctorAppointmentDeleteViewModel
            {
                Id = appointment.Id,
                DoctorName = await this.ResolveDoctorNameAsync(appointment.Doctor?.StaffID),
                PatientName = appointment.PatientName,
                Date = appointment.Date,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                Status = appointment.Status,
            };

            return this.View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await this.appointmentService.GetAppointmentDetailsAsync(id);
            if (appointment == null)
            {
                return this.NotFound();
            }

            var model = new DoctorAppointmentDeleteViewModel
            {
                Id = appointment.Id,
                DoctorName = await this.ResolveDoctorNameAsync(appointment.Doctor?.StaffID),
                PatientName = appointment.PatientName,
                Date = appointment.Date,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                Status = appointment.Status,
            };

            try
            {
                await this.appointmentService.CancelAppointmentAsync(appointment);
            }
            catch (Exception exception)
            {
                model.ErrorMessage = exception.Message;
                return this.View(model);
            }

            return this.RedirectToAction(nameof(this.Index));
        }

        private async Task<int?> GetCurrentDoctorIdAsync()
        {
            string? email = this.User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            return await this.appointmentService.GetDoctorIdByEmailAsync(email);
        }

        private async Task<List<DoctorOptionViewModel>> LoadDoctorOptionsAsync()
        {
            var doctors = await this.appointmentService.GetAllDoctorsAsync();
            return doctors.Select(DoctorOptionViewModel.From).ToList();
        }

        private async Task<string> ResolveDoctorNameAsync(int? doctorId)
        {
            if (!doctorId.HasValue || doctorId.Value == 0)
            {
                return "Unknown";
            }

            var doctors = await this.appointmentService.GetAllDoctorsAsync();
            var match = doctors.FirstOrDefault(item => item.DoctorId == doctorId.Value);

            return string.IsNullOrWhiteSpace(match.DoctorName)
                ? $"Doctor #{doctorId.Value}"
                : match.DoctorName;
        }

        private static DoctorAppointmentListItemViewModel MapListItem(
            Appointment appointment,
            IReadOnlyDictionary<int, string> doctorLookup)
        {
            int doctorId = appointment.Doctor?.StaffID ?? 0;
            doctorLookup.TryGetValue(doctorId, out var doctorName);

            return new DoctorAppointmentListItemViewModel
            {
                Id = appointment.Id,
                DoctorId = doctorId,
                DoctorName = string.IsNullOrWhiteSpace(doctorName) ? $"Doctor #{doctorId}" : doctorName,
                PatientName = appointment.PatientName,
                Date = appointment.Date,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                Status = appointment.Status,
            };
        }
    }
}
