using System.Net;
using System.Reflection;
using csharp_course;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownProxies.Add(IPAddress.Loopback);
});


builder.Services.AddControllers()
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


            var apiResult = new ApiResultBadRequest()
            {
                Success = false,
                Errors = errors,
            };


            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILogger<Program>>();

            var errorsString = string.Join(",", errors.Select(kv => $"{kv.Key}: {string.Join(", ", kv.Value)}"));

            logger.LogWarning($"Ошибка валидации: {errors}", errorsString);

            return new BadRequestObjectResult(apiResult);
        };
    });

builder.Services.AddHttpLogging(o => { });
builder.Services.AddSingleton<IEventService, EventService>();

builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();


app.UseHttpLogging();
app.UseStaticFiles("/static");
app.UseRouting();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();