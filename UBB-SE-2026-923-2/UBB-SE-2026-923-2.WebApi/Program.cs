using System;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using UBB_SE_2026_923_2.Data;
using UBB_SE_2026_923_2.Repositories;
using UBB_SE_2026_923_2.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers(options =>
    {
        // Navigation properties on domain models are never populated when the
        // model is returned from GET and then round-tripped back in PUT bodies.
        // Suppressing implicit-required on non-nullable reference types prevents
        // ASP.NET Core from rejecting those null navigations during model validation.
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    })
    .AddJsonOptions(options =>
    {
        // EF navigation collections form cycles between entities; ignore them
        // rather than letting the serializer throw.
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new TupleStringBoolConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Several controllers expose nested request records that share short
    // names (UpdateStatusRequest in Appointments, ERRequests, ShiftSwaps...).
    // Use the full type name as the schemaId so Swashbuckle does not collide.
    // The "+" separator that the CLR uses for nested types breaks JSON-Pointer
    // refs ("#/components/schemas/...+...") in the Swagger UI, so replace it
    // with "." to keep the ids ref-safe.
    options.CustomSchemaIds(type => type.FullName?.Replace('+', '.'));
});

var connectionString = builder.Configuration.GetConnectionString("AppDatabase")
    ?? throw new InvalidOperationException("ConnectionStrings:AppDatabase is not configured.");
var webApiAccessKey = builder.Configuration["WebApiAccessKey"]
    ?? throw new InvalidOperationException("WebApiAccessKey is not configured.");

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();

// ShiftRepository implements three interfaces; resolve them to the same instance per scope.
builder.Services.AddScoped<ShiftRepository>();
builder.Services.AddScoped<IShiftRepository>(serviceProvider => serviceProvider.GetRequiredService<ShiftRepository>());
builder.Services.AddScoped<IShiftManagementShiftRepository>(serviceProvider => serviceProvider.GetRequiredService<ShiftRepository>());
builder.Services.AddScoped<IPharmacyShiftRepository>(serviceProvider => serviceProvider.GetRequiredService<ShiftRepository>());

// StaffRepository implements three interfaces; same forwarding pattern.
builder.Services.AddScoped<StaffRepository>();
builder.Services.AddScoped<IStaffRepository>(serviceProvider => serviceProvider.GetRequiredService<StaffRepository>());
builder.Services.AddScoped<IShiftManagementStaffRepository>(serviceProvider => serviceProvider.GetRequiredService<StaffRepository>());
builder.Services.AddScoped<IPharmacyStaffRepository>(serviceProvider => serviceProvider.GetRequiredService<StaffRepository>());

builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IHangoutRepository, HangoutRepository>();
builder.Services.AddScoped<IHangoutParticipantRepository, HangoutParticipantRepository>();
builder.Services.AddScoped<IERDispatchRepository, ERDispatchRepository>();
builder.Services.AddScoped<IEvaluationsRepository, EvaluationsRepository>();
builder.Services.AddScoped<IHighRiskMedicineRepository, HighRiskMedicineRepository>();
builder.Services.AddScoped<IPharmacyHandoverRepository, PharmacyHandoverRepository>();
builder.Services.AddScoped<IShiftSwapRepository, ShiftSwapRepository>();
builder.Services.AddScoped<IUsersRepository, SQLUsersRepository>();
builder.Services.AddSingleton<IBasketRepository, InMemoryBasketRepository>();
builder.Services.AddScoped<IItemsRepository, SQLItemsRepository>();
builder.Services.AddScoped<IOrdersRepository, SQLOrdersRepository>();
builder.Services.AddScoped<ISubstancesRepository, SQLSubstancesRepository>();

var app = builder.Build();

using (var databaseScope = app.Services.CreateScope())
{
    var databaseContextFactory = databaseScope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    using var databaseContext = databaseContextFactory.CreateDbContext();

    if (databaseContext.Database.IsRelational())
    {
        databaseContext.Database.Migrate();
    }
    else
    {
        databaseContext.Database.EnsureCreated();
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/swagger"))
    {
        await next();
        return;
    }

    if (!context.Request.Headers.TryGetValue("X-Api-Key", out var providedAccessKey) ||
        !string.Equals(providedAccessKey.ToString(), webApiAccessKey, StringComparison.Ordinal))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("Unauthorized");
        return;
    }

    await next();
});

app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
