namespace Forumz.Infrastructure;

/// <summary>Formats a UTC timestamp for a post. Relative text is approximate; the title attribute keeps the clock time.</summary>
internal static class ForumTime
{
    public static string Format(DateTime utc, bool relative, TimeProvider time)
    {
        if (!relative)
        {
            return utc.ToLocalTime().ToString("g");
        }

        var delta = time.GetUtcNow().UtcDateTime - DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        if (delta < TimeSpan.Zero)
        {
            delta = TimeSpan.Zero;
        }

        return delta.TotalMinutes < 1
            ? "just now"
            : delta.TotalMinutes < 60
            ? $"{(int)delta.TotalMinutes} min ago"
            : delta.TotalHours < 24
            ? $"{(int)delta.TotalHours} hr ago"
            : delta.TotalDays < 14 ? $"{(int)delta.TotalDays} days ago" : utc.ToLocalTime().ToString("g");
    }

    public static string Exact(DateTime utc) => utc.ToLocalTime().ToString("f");
}