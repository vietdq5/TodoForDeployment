using Microsoft.EntityFrameworkCore;
using Todos.Api.Hubs;
using Todos.Api.Middleware;
using Todos.Application;
using Todos.Infrastructure;
using Todos.Infrastructure.Data;
using Todos.Infrastructure.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);
// Configure services
builder.Services.Configure<RabbitMQSetting>(builder.Configuration.GetSection("RabbitMQ"));
// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS for SignalR
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowAll", policy =>
//     {
//         policy.WithOrigins("http://127.0.0.1:5500")
//                   .AllowAnyHeader()
//                   .AllowAnyMethod()
//                   .AllowCredentials();
//     });
// });

// Add SignalR
// builder.Services.AddSignalR(options =>
// {
//     options.EnableDetailedErrors = builder.Environment.IsDevelopment();
//     options.KeepAliveInterval = TimeSpan.FromSeconds(15);
//     options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
// });

// Add layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API v1");
        c.RoutePrefix = string.Empty;
    });
}

// Middleware pipeline
app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseCors("AllowAll"); // for prod
app.UseRouting();
// app.UseAuthorization();

// Map endpoints
app.MapControllers();
// app.MapHub<TodoNotificationHub>("/todoHub");

// Serve static files for SignalR test client
app.UseStaticFiles();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
    await context.Database.EnsureCreatedAsync();
}

app.Run();