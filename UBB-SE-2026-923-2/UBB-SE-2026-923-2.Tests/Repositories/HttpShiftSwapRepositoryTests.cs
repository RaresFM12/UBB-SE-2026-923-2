namespace UBB_SE_2026_923_2.Tests.Repositories
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using UBB_SE_2026_923_2.Models;
    using UBB_SE_2026_923_2.Repositories;

    [TestFixture]
    public class HttpShiftSwapRepositoryTests
    {
        [Test]
        public void AddShiftSwapRequest_PostsRequestTimestampAndStatus()
        {
            using var handler = new CapturingHandler();
            using var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://example.test/"),
            };

            var repository = new HttpShiftSwapRepository(httpClient);
            var requestedAt = new DateTime(2026, 5, 26, 10, 30, 0, DateTimeKind.Utc);
            var request = new ShiftSwapRequest
            {
                Shift = new Shift { Id = 7 },
                Requester = new Staff { StaffID = 1 },
                Colleague = new Staff { StaffID = 2 },
                RequestedAt = requestedAt,
                Status = ShiftSwapRequestStatus.PENDING,
            };

            var swapId = repository.AddShiftSwapRequest(request);

            Assert.That(swapId, Is.EqualTo(12));
            Assert.That(handler.CapturedRequestUri?.ToString(), Does.EndWith("/api/shiftswaps"));

            using var payload = JsonDocument.Parse(handler.CapturedBody);
            var root = payload.RootElement;
            Assert.That(root.GetProperty("shiftId").GetInt32(), Is.EqualTo(7));
            Assert.That(root.GetProperty("requesterId").GetInt32(), Is.EqualTo(1));
            Assert.That(root.GetProperty("colleagueId").GetInt32(), Is.EqualTo(2));
            Assert.That(root.GetProperty("requestedAt").GetDateTime(), Is.EqualTo(requestedAt));
            Assert.That(root.GetProperty("status").GetInt32(), Is.EqualTo((int)ShiftSwapRequestStatus.PENDING));
        }

        private sealed class CapturingHandler : HttpMessageHandler
        {
            public Uri? CapturedRequestUri { get; private set; }

            public string CapturedBody { get; private set; } = string.Empty;

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                this.CapturedRequestUri = request.RequestUri;
                this.CapturedBody = request.Content == null
                    ? string.Empty
                    : await request.Content.ReadAsStringAsync(cancellationToken);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(12),
                };
            }
        }
    }
}
