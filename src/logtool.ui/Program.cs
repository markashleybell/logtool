using logtool.ui.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<IJobQueue, JobQueue>();

builder.Services.AddSingleton<IAppClient, AppClient>();

builder.Services.AddControllers();

builder.Services.AddHostedService<FileProcessingService>();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
