namespace UBB_SE_2026_923_2.Web.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class CreateHangoutViewModel
    {
        [Required]
        [StringLength(25, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 25 characters.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Description must be at most 100 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; } = DateTime.Now.AddDays(8);

        [Required]
        [Range(2, 50, ErrorMessage = "Max participants must be between 2 and 50.")]
        public int MaxParticipants { get; set; } = 10;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a doctor.")]
        public int SelectedDoctorId { get; set; }

        public List<DoctorOptionViewModel> Doctors { get; set; } = new List<DoctorOptionViewModel>();
    }
}
