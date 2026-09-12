using System.Reflection;
using csharp_course;

var builder = WebApplication.CreateBuilder(args);

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

app.UseSwagger();
app.UseSwaggerUI(); 

app.Run();