var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

var app = builder.Build();

// As strict as a site can reasonably be: nothing inline, nothing from anywhere else
app.Use(
    async (context, next) =>
    {
        context.Response.Headers.ContentSecurityPolicy =
            "default-src 'none'; script-src 'self'; style-src 'self'; img-src 'self' data:; "
            + "connect-src 'self'; form-action 'self'; base-uri 'none'; frame-ancestors 'none'";

        await next();
    }
);

// The older way of serving static files, which is what many apps still have
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();

app.Run();
