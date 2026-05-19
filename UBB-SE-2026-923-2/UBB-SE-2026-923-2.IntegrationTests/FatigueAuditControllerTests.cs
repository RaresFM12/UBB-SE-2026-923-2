namespace UBB_SE_2026_923_2.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using NUnit.Framework;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Services;

    internal sealed class StubFatigueAuditService : IFatigueAuditService
    {
        public AutoAuditResult RunAutoAudit(DateTime weekStart) => new AutoAuditResult
        {
            WeekStart = weekStart,
            HasConflicts = true,
            Summary = "Found 1 conflict(s). Publishing is blocked until resolved.",
            Violations = new List<AuditViolation>
            {
                new AuditViolation
                {
                    ShiftId = 10,
                    StaffId = 1,
                    StaffName = "Test Staff",
                    ShiftStart = weekStart.AddHours(8),
                    ShiftEnd = weekStart.AddHours(20),
                    Rule = "MAX_60H_PER_WEEK",
                    Message = "Weekly total is 65.0h (limit 60h).",
                },
            },
            Suggestions = new List<AutoSuggestRecommendation>
            {
                new AutoSuggestRecommendation
                {
                    ShiftId = 10,
                    OriginalStaffId = 1,
                    OriginalStaffName = "Test Staff",
                    SuggestedStaffId = 2,
                    SuggestedStaffName = "Replacement Staff",
                    Reason = "Lowest monthly load.",
                },
            },
        };

        public bool ReassignShift(int shiftId, int newStaffId) => shiftId >= 1 && newStaffId >= 1;
    }

    [TestFixture]
    public class FatigueAuditControllerTests
    {
        private WebApplicationFactory<UBB_SE_2026_923_2.Web.Program> factory = null!;
        private HttpClient client = null!;

        [SetUp]
        public void SetUp()
        {
            this.factory = new WebApplicationFactory<UBB_SE_2026_923_2.Web.Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        services.RemoveAll<IFatigueAuditService>();
                        services.AddSingleton<IFatigueAuditService, StubFatigueAuditService>();

                        services.AddAuthentication("TestScheme")
                            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", _ => { });
                        services.PostConfigure<AuthenticationOptions>(options =>
                        {
                            options.DefaultAuthenticateScheme = "TestScheme";
                            options.DefaultChallengeScheme = "TestScheme";
                        });
                    });
                });
            this.client = this.factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
            });
        }

        [TearDown]
        public void TearDown()
        {
            this.client?.Dispose();
            this.factory?.Dispose();
        }

        [Test]
        public async Task Index_ReturnsSuccessAndContainsViolation()
        {
            var response = await this.client.GetAsync("/FatigueAudit");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var body = await response.Content.ReadAsStringAsync();
            Assert.That(body, Does.Contain("Test Staff"));
            Assert.That(body, Does.Contain("MAX_60H_PER_WEEK"));
        }

        [Test]
        public async Task Index_WithWeekStart_ReturnsSuccess()
        {
            var weekStart = DateTime.Today.ToString("yyyy-MM-dd");
            var response = await this.client.GetAsync($"/FatigueAudit?weekStart={weekStart}");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Details_ExistingViolation_ReturnsSuccess()
        {
            var response = await this.client.GetAsync("/FatigueAudit/Details/10");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var body = await response.Content.ReadAsStringAsync();
            Assert.That(body, Does.Contain("Test Staff"));
        }

        [Test]
        public async Task Details_NonExistingViolation_ReturnsNotFound()
        {
            var response = await this.client.GetAsync("/FatigueAudit/Details/999");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task Reassign_Get_ReturnsConfirmationPage()
        {
            var response = await this.client.GetAsync("/FatigueAudit/Reassign?shiftId=10");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var body = await response.Content.ReadAsStringAsync();
            Assert.That(body, Does.Contain("Replacement Staff"));
        }
    }
}
