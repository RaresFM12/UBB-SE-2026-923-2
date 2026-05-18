using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UBB_SE_2026_923_2.Models;
using UBB_SE_2026_923_2.Services;
using UBB_SE_2026_923_2.Repositories;

namespace UBB_SE_2026_923_2.IntegrationTests;

public sealed class PeriodTrackerWebApplicationFactory : WebApplicationFactory<UBB_SE_2026_923_2.Web.Program>
{
	public FakePeriodTrackerService PeriodTrackerService { get; } = new FakePeriodTrackerService();

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureAppConfiguration(configurationBuilder =>
		{
			configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
			{
				["WebApiBaseUrl"] = "https://localhost:7100/",
			});
		});

		builder.ConfigureServices(services =>
		{
			services.AddAuthentication(options =>
				{
					options.DefaultAuthenticateScheme = TestAuthenticationHandler.SchemeName;
					options.DefaultChallengeScheme = TestAuthenticationHandler.SchemeName;
					options.DefaultScheme = TestAuthenticationHandler.SchemeName;
				})
				.AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
					TestAuthenticationHandler.SchemeName,
					options => { });

			services.AddSingleton<IPeriodTrackerService>(this.PeriodTrackerService);
			services.AddSingleton<IBasketService, FakeBasketService>();
			services.AddSingleton<IUsersRepository, FakeUsersRepository>();
		});
	}
}

public sealed class FakeUsersRepository : IUsersRepository
{
	private readonly User currentUser = new User();

	public FakeUsersRepository()
	{
		this.currentUser.Id = 1;
		this.currentUser.Email = "client@example.com";
		this.currentUser.PhoneNumber = "0700000000";
		this.currentUser.PasswordHash = "hash";
		this.currentUser.Username = "client";
		this.currentUser.Role = "Client";
	}

	public bool UserExists(string email)
	{
		return email == this.currentUser.Email;
	}

	public bool UserExists(int userId)
	{
		return userId == this.currentUser.Id;
	}

	public User GetUserById(int userId)
	{
		return userId == this.currentUser.Id ? this.currentUser : this.currentUser;
	}

	public User GetUserByEmail(string email)
	{
		return this.currentUser;
	}

	public void AddUser(string email, string phoneNumber, string passwordHash, string username, bool discountNotifications, bool isDisabled = false, bool isAdmin = false, int loyaltyPoints = 0, string role = "Client")
	{
	}

	public void UpdateUser(User user)
	{
	}

	public List<User> GetAllUsers()
	{
		return new List<User> { this.currentUser };
	}

	public bool UserHasPeriodTracker(int userId)
	{
		return true;
	}
}

public sealed class FakeBasketService : IBasketService
{
	public void AddToBasket(int itemId, int quantity, float extraDiscountPercentage = 0f)
	{
	}
}

public sealed class FakePeriodTrackerService : IPeriodTrackerService
{
	private readonly User currentUser = new User();

	public FakePeriodTrackerService()
	{
		this.currentUser.Id = 1;
		this.currentUser.Email = "client@example.com";
		this.currentUser.PhoneNumber = "0700000000";
		this.currentUser.PasswordHash = "hash";
		this.currentUser.Username = "client";
		this.currentUser.Role = "Client";
		this.currentUser.AddPeriodNoteToUser(1, "Cramps", false);
	}

	public User GetCurrentUser()
	{
		return this.currentUser;
	}

	public PeriodTrackerState GetTrackerState()
	{
		bool hasPeriodTracker = this.currentUser.StartPeriodDate.Year != default(DateOnly).Year;
		DateTimeOffset startPeriodDate = hasPeriodTracker
			? new DateTimeOffset(this.currentUser.StartPeriodDate.ToDateTime(TimeOnly.MinValue))
			: DateTimeOffset.MinValue;

		return new PeriodTrackerState
		{
			StartPeriodDate = startPeriodDate,
			CycleDays = this.currentUser.CycleDays,
			PeriodLasts = this.currentUser.PeriodLasts,
			PremenstrualSyndromeOption = this.currentUser.PremenstrualSyndromeOption,
			HasPeriodTracker = hasPeriodTracker,
		};
	}

	public PeriodTrackerDashboardSnapshot GetDashboardSnapshot(int monthOffset)
	{
		var state = this.GetTrackerState();
		if (!state.HasPeriodTracker)
		{
			return new PeriodTrackerDashboardSnapshot
			{
				HasPeriodTracker = false,
				StartPeriodDate = state.StartPeriodDate.DateTime,
				CycleDays = state.CycleDays,
				PeriodLasts = state.PeriodLasts,
				PMSOption = state.PremenstrualSyndromeOption,
				MonthOffset = monthOffset,
				CurrentMonthName = string.Empty,
				Notes = new List<PeriodTrackerNoteSnapshot>(),
				ShopItems = new List<PeriodTrackerShopItemSnapshot>(),
			};
		}

		var nextPeriodDate = state.StartPeriodDate.DateTime.AddDays(state.CycleDays);
		return new PeriodTrackerDashboardSnapshot
		{
			HasPeriodTracker = state.HasPeriodTracker,
			StartPeriodDate = state.StartPeriodDate.DateTime,
			CycleDays = state.CycleDays,
			PeriodLasts = state.PeriodLasts,
			PMSOption = state.PremenstrualSyndromeOption,
			MonthOffset = monthOffset,
			CurrentMonthName = "May 2026",
			PeriodIntervalText = "1 May - 5 May",
			LowFertilityIntervalText = "6 May - 8 May",
			OvulationIntervalText = "12 May - 16 May",
			PmsIntervalText = "23 May - 26 May",
			CurrentPhaseString = "Menstrual Phase",
			NextPeriodDateString = nextPeriodDate.ToString("d"),
			NextPeriodDistanceString = "14 days left",
			IsInMenstrualPhase = true,
			CurrentDayOfCycle = 15,
			DaysUntilOvulation = 0,
			OvulationDistanceString = "In Progress",
			Notes = new List<PeriodTrackerNoteSnapshot>
			{
				new PeriodTrackerNoteSnapshot { NoteId = 1, NoteBody = "Cramps", IsDone = false },
			},
			ShopItems = new List<PeriodTrackerShopItemSnapshot>
			{
				new PeriodTrackerShopItemSnapshot
				{
					RawItem = new Item(1, "Pain Relief", "Acme", "Wellness", 10f, 20, discount: 0f, quantity: 0),
					DisplayPrice = 8f,
					HasDiscountApplied = true,
				},
			},
		};
	}

	public Dictionary<int, Tuple<string, bool>> GetNotes()
	{
		return this.currentUser.PeriodNotes;
	}

	public int GetMaxNoteId()
	{
		return this.currentUser.PeriodNotes.Count == 0 ? 0 : this.currentUser.PeriodNotes.Keys.Max();
	}

	public void UpdatePeriodTracker(DateTimeOffset startPeriodDate, double cycleDays, double periodLasts, int premenstrualSyndromeOption)
	{
		this.currentUser.SetPeriodTracker(
			DateOnly.FromDateTime(startPeriodDate.DateTime),
			Convert.ToInt32(cycleDays),
			Convert.ToInt32(periodLasts),
			premenstrualSyndromeOption);
	}

	public void AddNote(string noteBody)
	{
		this.currentUser.AddPeriodNoteToUser(this.GetMaxNoteId() + 1, noteBody ?? string.Empty, false);
	}

	public void UpdateNote(int noteId, string noteBody, bool isDone)
	{
		this.currentUser.PeriodNotes[noteId] = new Tuple<string, bool>(noteBody ?? string.Empty, isDone);
	}

	public void DeleteNote(int noteId)
	{
		this.currentUser.PeriodNotes.Remove(noteId);
	}

	public void SaveCurrentUser()
	{
	}
}

public sealed class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
	public const string SchemeName = "Test";

	public TestAuthenticationHandler(
		IOptionsMonitor<AuthenticationSchemeOptions> options,
		ILoggerFactory logger,
		UrlEncoder encoder)
		: base(options, logger, encoder)
	{
	}

	protected override Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		var claims = new List<Claim>
		{
			new Claim(ClaimTypes.NameIdentifier, "1"),
			new Claim(ClaimTypes.Name, "client"),
			new Claim(ClaimTypes.Role, "Client"),
		};

		var identity = new ClaimsIdentity(claims, SchemeName);
		var principal = new ClaimsPrincipal(identity);
		var ticket = new AuthenticationTicket(principal, SchemeName);

		return Task.FromResult(AuthenticateResult.Success(ticket));
	}
}
