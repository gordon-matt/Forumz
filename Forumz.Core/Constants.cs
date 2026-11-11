namespace Forumz.Core;

/// <summary>
/// Names and limits shared by every Forumz database. The settings row can ask for a shorter
/// subject or post, but the columns stop at these lengths.
/// </summary>
public static class Constants
{
    /// <summary>
    /// Forumz usually shares the host database, so it keeps a private migration history.
    /// The host's own history table would otherwise make each side think the other's migrations are missing.
    /// </summary>
    public const string MigrationsHistoryTable = "__ForumzMigrationsHistory";

    /// <summary>Schema used by SQL Server and PostgreSQL.</summary>
    public const string Schema = "forumz";

    /// <summary>
    /// Prepended to every table name when the provider has no schema. In MySQL a schema is a
    /// database, and SQLite has none.
    /// </summary>
    public const string TablePrefix = "Forumz_";

    /// <summary>Unqualified table names. The context applies <see cref="Schema"/> or <see cref="TablePrefix"/>.</summary>
    public static class Tables
    {
        /// <summary>Forum groups.</summary>
        public const string Groups = "Groups";

        /// <summary>Forums.</summary>
        public const string Forums = "Forums";

        /// <summary>Topics.</summary>
        public const string Topics = "Topics";

        /// <summary>Posts.</summary>
        public const string Posts = "Posts";

        /// <summary>Forum and topic subscriptions.</summary>
        public const string Subscriptions = "Subscriptions";

        /// <summary>Private messages.</summary>
        public const string Messages = "Messages";

        /// <summary>Display names and post counts, keyed by the host's user id.</summary>
        public const string Members = "Members";

        /// <summary>One settings row per tenant.</summary>
        public const string Preferences = "Preferences";
    }

    /// <summary>Column lengths. Application settings may be stricter.</summary>
    public static class MaxLengths
    {
        /// <summary>Tenant key stored as text so the host can use an int, a guid or a slug.</summary>
        public const int TenantId = 64;

        /// <summary>Long enough for an ASP.NET Identity string key.</summary>
        public const int UserId = 450;

        /// <summary>Group and forum names.</summary>
        public const int Name = 255;

        /// <summary>Forum description.</summary>
        public const int Description = 4000;

        /// <summary>Topic subject. Settings can lower this, not raise it past the column.</summary>
        public const int Subject = 512;

        /// <summary>IPv6 textual form.</summary>
        public const int IpAddress = 45;

        /// <summary>Name shown on posts.</summary>
        public const int DisplayName = 256;

        /// <summary>Optional signature under a member's posts.</summary>
        public const int Signature = 1000;

        /// <summary>Private message subject.</summary>
        public const int MessageSubject = 256;
    }

    /// <summary>Values of <c>Database:Provider</c> the sample host understands.</summary>
    public static class DatabaseProviders
    {
        /// <summary>MySQL via <c>AddForumzMySql</c>.</summary>
        public const string MySql = "MySql";

        /// <summary>PostgreSQL via <c>AddForumzNpgsql</c>.</summary>
        public const string Npgsql = "Npgsql";

        /// <summary>SQLite via <c>AddForumzSqlite</c>.</summary>
        public const string Sqlite = "Sqlite";

        /// <summary>SQL Server via <c>AddForumzSqlServer</c>.</summary>
        public const string SqlServer = "SqlServer";
    }
}