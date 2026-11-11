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

        if (delta.TotalMinutes < 1)
        {
            return "just now";
        }

        if (delta.TotalMinutes < 60)
        {
            return $"{(int)delta.TotalMinutes} min ago";
        }

        if (delta.TotalHours < 24)
        {
            return $"{(int)delta.TotalHours} hr ago";
        }

        if (delta.TotalDays < 14)
        {
            return $"{(int)delta.TotalDays} days ago";
        }

        return utc.ToLocalTime().ToString("g");
    }

    public static string Exact(DateTime utc) => utc.ToLocalTime().ToString("f");
}
