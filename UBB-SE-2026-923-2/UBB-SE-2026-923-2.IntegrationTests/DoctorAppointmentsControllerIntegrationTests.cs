namespace UBB_SE_2026_923_2.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Testing;
    using NUnit.Framework;
    using UBB_SE_2026_923_2.Models;

    [TestFixture]
    public class DoctorAppointmentsControllerIntegrationTests
    {
        private WebMvcApplicationFactory factory;
        private HttpClient client;

        [SetUp]
        public void Setup()
        {
            this.factory = new WebMvcApplicationFactory();

            var doctor = new Doctor
            {
                StaffID = 1,
                FirstName = "Ana",
                LastName = "Medic",
                DoctorStatus = DoctorStatus.AVAILABLE,
            };
            this.factory.StaffRepository.Seed(doctor);

            this.client = this.factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
            });
        }

        [TearDown]
        public void TearDown()
        {
            this.client.Dispose();
            this.factory.Dispose();
        }

        [Test]
        public async Task Index_Admin_ReturnsOk()
        {
            this.factory.AppointmentsRepository.Seed(new Appointment
            {
                Id = 1,
                PatientName = "101",
                Doctor = new Doctor { StaffID = 1, FirstName = "Ana", LastName = "Medic" },
                Date = DateTime.Today,
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(9.5),
                Status = "Scheduled",
            });

            var response = await this.client.GetAsync("/DoctorAppointments?doctorId=1");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content = await response.Content.ReadAsStringAsync();
            Assert.That(content, Does.Contain("Doctor appointments"));
        }

        [Test]
        public async Task Create_PostValid_AddsAppointment()
        {
            var form = new Dictionary<string, string>
            {
                ["PatientName"] = "PAT-101",
                ["DoctorId"] = "1",
                ["Date"] = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"),
                ["StartTime"] = "10:00",
            };

            var response = await this.client.PostAsync(
                "/DoctorAppointments/Create",
                new FormUrlEncodedContent(form));

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
            Assert.That(this.factory.AppointmentsRepository.Appointments.Count, Is.EqualTo(1));
            Assert.That(this.factory.AppointmentsRepository.Appointments.Single().Doctor?.StaffID, Is.EqualTo(1));
        }
    }
}
