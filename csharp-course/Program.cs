using System.Net;
using System.Reflection;
using csharp_course;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownProxies.Add(IPAddress.Loopback);
});

builder.Services.AddControllers();
builder.Services.AddHttpLogging(o => { });
builder.Services.AddSingleton<IEventService, EventService>();

builder.Services.AddSwaggerGen(options =>
{
    // Путь к XML-файлу с документацией
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();



app.UseHttpLogging(); // 0. Компонент журналирования (логирование запросов
// app.UseHttpsRedirection();    // 1. Перенаправление на HTTPS
app.UseStaticFiles();         // 2. Статические файлы
app.UseRouting();             // 3. Маршрутизация
// app.UseCors();                // 4. CORS
// app.UseAuthentication();      // 5. Аутентификация
// app.UseAuthorization();       // 6. Авторизация
app.MapControllers();         // 7. Эндпоинты

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();