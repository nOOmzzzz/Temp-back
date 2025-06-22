using customhost_backend.GuestExperience.Application.Internal.CommandServices;
using customhost_backend.GuestExperience.Application.Internal.QueryServices;
using customhost_backend.GuestExperience.Domain.Repositories;
using customhost_backend.GuestExperience.Domain.Services;
using customhost_backend.GuestExperience.Infrastructure.Persistence.EFC.Repositories;
using customhost_backend.Shared.Infrastructure.Interfaces.ASP.Configuration;
using customhost_backend.Shared.Infrastructure.Persistence.EFC.Configuration;
using customhost_backend.Shared.Infrastructure.Persistence.EFC.Repositories;
using customhost_backend.Shared.Domain.Repositories;
using customhost_backend.crm.Domain.Repositories;
using customhost_backend.crm.Domain.Services;
using customhost_backend.crm.Application.Internal.CommandServices;
using customhost_backend.crm.Application.Internal.QueryServices;
using customhost_backend.crm.Infrastructure.Persistence.EFC.Repositories;
using customhost_backend.crm.Infrastructure.Repositories;
using customhost_backend.crm.Infrastructure.Persistence.Repositories;
using customhost_backend.billings.Domain.Repositories;
using customhost_backend.billings.Domain.Services;
using customhost_backend.billings.Application.Internal.CommandServices;
using customhost_backend.billings.Application.Internal.QueryServices;
using customhost_backend.billings.Infrastructure.Persistence.EFC.Repositories;
using customhost_backend.profiles.Domain.Repositories;
using customhost_backend.profiles.Domain.Services;
using customhost_backend.profiles.Application.Internal.CommandServices;
using customhost_backend.profiles.Application.Internal.QueryServices;
using customhost_backend.profiles.Infrastructure.Persistence.EFC.Repositories;
using customhost_backend.analytics.Domain.Repositories;
using customhost_backend.analytics.Domain.Services;
using customhost_backend.analytics.Domain.Services.External;
using customhost_backend.analytics.Application.Internal.QueryServices;
using customhost_backend.analytics.Infrastructure.Persistence.EFC.Repositories;
using customhost_backend.analytics.Infrastructure.ACL.External;
using Microsoft.EntityFrameworkCore;



var builder = WebApplication.CreateBuilder(args);

// Railway expone el puerto en la variable de entorno PORT
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
    builder.WebHost.UseUrls($"http://*:{port}");

Console.WriteLine("Configuration Sources:");
foreach (var source in builder.Configuration.Sources)
{
    Console.WriteLine(source.ToString());
}

Console.WriteLine("Connection String: " + 
                  (builder.Configuration.GetConnectionString("DefaultConnection") ?? "NOT FOUND"));


// Add services to the container.
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers(options => options.Conventions.Add(new KebabCaseRouteNamingConvention()))
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });


// Permite que la cadena de conexión venga de una variable de entorno (Railway)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("RAILWAY_DATABASE_URL")
    ?? Environment.GetEnvironmentVariable("MYSQLDATABASEURL");


//Add CORS Policy for Frontend Integration

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendPolicy",
        policy => policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "http://127.0.0.1:3000", "http://127.0.0.1:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
    
    // Keep AllowAll for development/testing purposes
    options.AddPolicy("AllowAllPolicy",
        policy => policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});


if(connectionString== null) throw new InvalidOperationException("Connection string not found.");



builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
        options.UseMySQL(connectionString)
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();
    else if (builder.Environment.IsProduction())
        options.UseMySQL(connectionString)
            .LogTo(Console.WriteLine, LogLevel.Error);
});

builder.Services.AddSwaggerGen(options=> { options.EnableAnnotations(); });

// Dependency Injection

// Shared Bounded Context
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// CRM Bounded Context
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IStaffMemberRepository, StaffMemberRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IServiceRequestRepository, ServiceRequestRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IHotelCommandService, HotelCommandService>();
builder.Services.AddScoped<IHotelQueryService, HotelQueryService>();
builder.Services.AddScoped<IBookingCommandService, BookingCommandService>();
builder.Services.AddScoped<IBookingQueryService, BookingQueryService>();
builder.Services.AddScoped<IStaffMemberCommandService, StaffMemberCommandService>();
builder.Services.AddScoped<IStaffMemberQueryService, StaffMemberQueryService>();
builder.Services.AddScoped<IRoomCommandService, RoomCommandService>();
builder.Services.AddScoped<IRoomQueryService, RoomQueryService>();
builder.Services.AddScoped<IServiceRequestCommandService, ServiceRequestCommandService>();
builder.Services.AddScoped<IServiceRequestQueryService, ServiceRequestQueryService>();
builder.Services.AddScoped<INotificationCommandService, NotificationCommandService>();
builder.Services.AddScoped<INotificationQueryService, NotificationQueryService>();

// Billings Bounded Context
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentCommandService, PaymentCommandService>();
builder.Services.AddScoped<IPaymentQueryService, PaymentQueryService>();

// Profiles Bounded Context
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserCommandService, UserCommandService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();

// Analytics Bounded Context
// Repositories
builder.Services.AddScoped<IAnalyticsSnapshotRepository, AnalyticsSnapshotRepository>();
builder.Services.AddScoped<IMetricDataRepository, MetricDataRepository>();

// Query Services
builder.Services.AddScoped<IAnalyticsSnapshotQueryService, AnalyticsSnapshotQueryService>();
builder.Services.AddScoped<IAnalyticsMetricQueryService, AnalyticsMetricQueryService>();

// ACL Facades
builder.Services.AddScoped<IGuestExperienceContextFacade, GuestExperienceContextFacade>();
builder.Services.AddScoped<ICrmContextFacade, CrmContextFacade>();
builder.Services.AddScoped<IBillingsContextFacade, BillingsContextFacade>();

// GuestExperience Bounded Context
// Repositories
builder.Services.AddScoped<IIoTDeviceRepository, IoTDeviceRepository>();
builder.Services.AddScoped<IRoomDeviceRepository, RoomDeviceRepository>();
builder.Services.AddScoped<IRoomDevicePreferenceRepository, RoomDevicePreferenceRepository>();
builder.Services.AddScoped<IUserDevicePreferenceRepository, UserDevicePreferenceRepository>();

// Command Services
builder.Services.AddScoped<IIoTDeviceCommandService, IoTDeviceCommandService>();
builder.Services.AddScoped<IRoomDeviceCommandService, RoomDeviceCommandService>();
builder.Services.AddScoped<IRoomDevicePreferenceCommandService, RoomDevicePreferenceCommandService>();
builder.Services.AddScoped<IUserDevicePreferenceCommandService, UserDevicePreferenceCommandService>();

// Query Services
builder.Services.AddScoped<IIoTDeviceQueryService, IoTDeviceQueryService>();
builder.Services.AddScoped<IRoomDeviceQueryService, RoomDeviceQueryService>();
builder.Services.AddScoped<IRoomDevicePreferenceQueryService, RoomDevicePreferenceQueryService>();
builder.Services.AddScoped<IUserDevicePreferenceQueryService, UserDevicePreferenceQueryService>();


var app = builder.Build();

// Verify if the database exists and create it if it doesn't
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Apply CORS Policy - Use specific frontend policy in production
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAllPolicy"); // More permissive for development
}
else
{
    app.UseCors("AllowFrontendPolicy"); // Restricted to frontend origins
}

// Aplica redirección HTTPS solo en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();