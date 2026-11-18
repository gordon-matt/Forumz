# Forumz

A Blazor forum you can mount in any ASP.NET Core site. It is a Razor class library, not an MVC app: the host calls `AddForumz()` and `MapForumz()`, and the pages run as their own interactive circuit.

Posts are plain text. The page encodes them, so a post cannot inject markup.

## Add it to a site

Pick the database you already use. Forumz keeps its own tables and its own migration history, so it can share the host database.

```csharp
builder.Services.AddForumz(options =>
{
    options.Title = "Forums";
    options.HomePath = "/";
    options.LogoutPath = "/Identity/Account/LogOut";
});

builder.Services.AddForumzSqlServer(connectionString);
// or AddForumzSqlite, AddForumzNpgsql, AddForumzMySql

app.MapForumz();
```

Browse to `/forums`.

Reading is anonymous. Posting needs a signed-in user. The default rules let anyone read, a signed-in user post, and an administrator (`Administrator` role) create forums and moderate. Replace `IForumAccess` to use the host's own permissions.

A multi-tenant host replaces `IForumTenantScope` and returns the current tenant on every call, including inside the Blazor circuit. Null means one shared forum.

Replace `IForumNotifier` to email subscribers. The default only logs.

`HomePath` and `LogoutPath` are full page loads. Sign-out is a normal form post, because Blazor would swallow a logout link.

## What you can do

- Browse groups and forums, start topics, and reply.
- Edit or delete your own posts, when settings allow it. Moderators can edit any post, move a topic, and remove it.
- Search subjects and posts, and follow the newest replies.
- Subscribe to a forum or a topic.
- Send a private message to a member who already has a forum profile.
- RSS at `/forums/feed` and `/forums/forum/{id}/feed`.
- Manage groups, forums, and settings from `/forums/manage`.

## Sample

`Forumz.Sample` seeds a welcome forum on an empty database. It opens signed in as Dev User, an administrator. The top bar switches to Blair Chen or Casey Nguyen, who are members, so a private message can be sent from one to the other.

```bash
dotnet run --project Forumz.Sample
```

## Inferno

The Inferno plugin `Inferno.Plugins.Messaging.Forumz` references this library, stores the tables in the CMS database, and scopes them to the current tenant. Posting requires the Forums permission. Moderating requires Moderate forums.
