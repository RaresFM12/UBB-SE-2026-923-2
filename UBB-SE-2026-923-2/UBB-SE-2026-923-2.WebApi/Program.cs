using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using UBB_SE_2026_923_2.Data;
using UBB_SE_2026_923_2.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        // EF navigation collections form cycles between entities; ignore them
        // rather than letting the serializer throw.
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("AppDatabase")
    ?? throw new InvalidOperationException("ConnectionStrings:AppDatabase is not configured.");

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();

// ShiftRepository implements three interfaces; resolve them to the same instance per scope.
builder.Services.AddScoped<ShiftRepository>();
builder.Services.AddScoped<IShiftRepository>(sp => sp.GetRequiredService<ShiftRepository>());
builder.Services.AddScoped<IShiftManagementShiftRepository>(sp => sp.GetRequiredService<ShiftRepository>());
builder.Services.AddScoped<IPharmacyShiftRepository>(sp => sp.GetRequiredService<ShiftRepository>());

// StaffRepository implements three interfaces; same forwarding pattern.
builder.Services.AddScoped<StaffRepository>();
builder.Services.AddScoped<IStaffRepository>(sp => sp.GetRequiredService<StaffRepository>());
builder.Services.AddScoped<IShiftManagementStaffRepository>(sp => sp.GetRequiredService<StaffRepository>());
builder.Services.AddScoped<IPharmacyStaffRepository>(sp => sp.GetRequiredService<StaffRepository>());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
