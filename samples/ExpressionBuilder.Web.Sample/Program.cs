using ExpressionBuilder.Web.Sample.Customisation;
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

    // An operation and a manipulator of the app's own, offered by this filter builder only. They are
    // listed under their own heading, apart from the built-in ones, and a browser can only choose them by
    // name. (ManipulatorCatalog.Register and OperationCatalog.Register offer them in every builder.)
    options.AddOperation<IsWeekend>(
        new OperationDescriptor(
            "IsWeekend",
            "Is at the weekend",
            ValueArity.None,
            new HashSet<FieldKind> { FieldKind.Date, FieldKind.DateTime }
        )
    );
    options.AddManipulator(
        ManipulatorDescriptor.For<IgnorePunctuation>("Ignore punctuation")
    );
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Home/Error");

app.UseRouting();

// Serves the sample's own files and the filter builder's script and stylesheet (/_content/...), in Development
// from the build and once published from what was published
app.MapStaticAssets();
app.MapControllerRoute("default", "{controller=Products}/{action=Index}/{id?}")
    .WithStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();

// Lets the tests start the app
public partial class Program;
