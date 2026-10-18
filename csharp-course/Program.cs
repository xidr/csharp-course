using System.Net;
using System.Reflection;
using csharp_course;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);


builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownProxies.Add(IPAddress.Loopback);
});


builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = false;

        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(kv => kv.Value?.Errors.Count > 0)
                .ToDictionary(
                    kv => kv.Key,
                    kv => kv.Value!.Errors.Select(e => e.ErrorMessage));


            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILogger<Program>>();

            var errorsString = string.Join(",", errors.Select(kv => $"{kv.Key}: {string.Join(", ", kv.Value)}"));

            logger.LogInformation(
                "Ошибка с кодом {StatusCode}: {Message}. Method={Method}, Path={Path}, RequestId={RequestId}",
                StatusCodes.Status400BadRequest,
                errorsString,
                context.HttpContext.Request.Method,
                context.HttpContext.Request.Path,
                context.HttpContext.Request.Headers["x-request-id"]);

            var problem = new ValidationProblemDetails(context.ModelState)
            {
                Status = StatusCodes.Status400BadRequest
            };

            return new BadRequestObjectResult(problem);
        };
    });

builder.Services.AddHttpLogging(o => { });
builder.Services.AddSingleton<IEventRepository, EventRepositoryInMemoryDict>();
builder.Services.AddSingleton<IEventService, EventService>();

builder.Services.AddSingleton<IBookingRepository, BookingRepositoryInMemoryDict>();
builder.Services.AddSingleton<IBookingService, BookingService>();

builder.Services.AddHostedService<BookingBackgroundService>();

builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

app.UseForwardedHeaders();
app.UseHttpLogging();
app.UseGlobalExceptionHandling();
app.UseStaticFiles("/static");
app.UseRouting();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();