using Forumz;
using Forumz.Core;
using Forumz.Data.MySql;
using Forumz.Data.Npgsql;
using Forumz.Data.Sql;
using Forumz.Data.Sqlite;
using Forumz.Sample;
using Heroicsoft.NetForge.Abstractions.Auth;
using Heroicsoft.NetForge.Components;
using Heroicsoft.NetForge.Components.Layout;
using Heroicsoft.NetForge.Components.Theming;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IUserContextService, SampleUserContext>();
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
app.MapGet("/", () => Results.Redirect("/forums"));
app.MapForumz();
app.Run();
