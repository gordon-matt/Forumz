using Forumz;
using Forumz.Core;
using Forumz.Data.MySql;
using Forumz.Data.Npgsql;
using Forumz.Data.Sql;
using Forumz.Data.Sqlite;
using Forumz.Sample;
using Heroicsoft.NetForge.Abstractions.Auth;
using Heroicsoft.NetForge.Components.Layout;
using Heroicsoft.NetForge.Components.Theming;
using Microsoft.AspNetCore.Components.Authorization;
using Pickstack;
using Pickstack.FileManager.WebFilez;
using Pickstack.RichTextEditor.TinyMce8;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IUserContextService, SampleUserContext>();

// The file browser is an MVC page in the picker's frame, and it connects to a Blazor
// circuit at /_blazor. This sample's only other circuit is the forum under /forums,
// so Pickstack maps that root hub itself. UseHostBlazor() is for a host that already
// maps a circuit at the site root.
builder.Services.AddControllersWithViews();
builder.Services.AddPickstack(builder.Configuration)
    .AddWebFilez()
    .AddTinyMce8(editor => editor
        .WithHeight(280)
        .WithToolbar("undo redo | bold italic underline | bullist numlist blockquote | link image | code"));

builder.Services.AddForumz(options => builder.Configuration.GetSection(ForumzOptions.SectionName).Bind(options));
builder.Services.AddScoped<AuthenticationStateProvider, SampleAuthState>();
builder.Services.AddCascadingAuthenticationState();

builder.Services.Configure<ForgeUiOptions>(ui =>
{
    ui.ThemeStorageKey = "forumz.theme";
    ui.DefaultColorScheme = "system";
    ui.AccentColor = "#c2410c";
    ui.AccentColorDark = "#fb923c";
});

builder.Services.Configure<ForgeShellOptions>(shell =>
{
    shell.Brand = "Forumz";
    shell.BrandHref = "/forums";
});

string provider = builder.Configuration["Database:Provider"] ?? Constants.DatabaseProviders.Sqlite;
string? connectionString = builder.Configuration.GetConnectionString("Forumz");

switch (provider)
{
    case Constants.DatabaseProviders.Sqlite:
        builder.Services.AddForumzSqlite(connectionString);
        break;

    case Constants.DatabaseProviders.SqlServer:
        builder.Services.AddForumzSqlServer(connectionString!);
        break;

    case Constants.DatabaseProviders.Npgsql:
        builder.Services.AddForumzNpgsql(connectionString!);
        break;

    case Constants.DatabaseProviders.MySql:
        builder.Services.AddForumzMySql(connectionString!);
        break;

    default:
        throw new InvalidOperationException(
            $"Unknown Database:Provider '{provider}'. Expected Sqlite, SqlServer, Npgsql or MySql.");
}

builder.Services.AddHostedService<SampleSeeder>();

var app = builder.Build();

app.UseStaticFiles();
app.MapStaticAssets();
app.UseAntiforgery();
app.UsePickstack();
app.MapControllers();
app.MapGet("/", () => Results.Redirect("/forums"));
app.MapForumz();
app.Run();