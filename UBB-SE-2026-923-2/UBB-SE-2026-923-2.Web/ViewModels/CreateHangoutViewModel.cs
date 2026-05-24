namespace UBB_SE_2026_923_2.Web.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class CreateHangoutViewModel
    {
        private const int TitleMinLength = 5;
        private const int TitleMaxLength = 25;
        private const int DescriptionMaxLength = 100;
        private const int MinParticipants = 2;
        private const int MaxParticipants = 50;
        private const int DefaultMaxParticipants = 10;
        private const int MinimumDaysInAdvance = 8;
        private const int MinDoctorId = 1;

        [Required]
        [StringLength(TitleMaxLength, MinimumLength = TitleMinLength, ErrorMessage = "Title must be between 5 and 25 characters.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(DescriptionMaxLength, ErrorMessage = "Description must be at most 100 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; } = DateTime.Now.AddDays(MinimumDaysInAdvance);

        [Required]
        [Range(MinParticipants, MaxParticipants, ErrorMessage = "Max participants must be between 2 and 50.")]
        public int MaxParticipantsCount { get; set; } = DefaultMaxParticipants;

        [Required]
        [Range(MinDoctorId, int.MaxValue, ErrorMessage = "Please select a doctor.")]
        public int SelectedDoctorId { get; set; }

        public List<DoctorOptionViewModel> Doctors { get; set; } = new List<DoctorOptionViewModel>();
    }
}
