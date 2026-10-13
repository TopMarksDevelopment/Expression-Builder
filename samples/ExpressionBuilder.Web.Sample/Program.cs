using ExpressionBuilder.Web.Sample.Models;
using TopMarksDevelopment.ExpressionBuilder.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSingleton<ProductCatalog>();

// Optional. Say what the filter builder may do; the same settings are used to draw it and to check
// what it posts. Without this the defaults apply.
builder.Services.AddFilterBuilder(options =>
{
    options.MaxConditions = 30;
    options.OperationLabels["Equal"] = "Is";
    options.OperationLabels["NotEqual"] = "Is not";
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Home/Error");

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute("default", "{controller=Products}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

// Lets the tests start the app
public partial class Program;
