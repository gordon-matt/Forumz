namespace Forumz.Infrastructure;

/// <summary>Two letters for a member mark when the forum has no avatars.</summary>
internal static class ForumInitials
{
    public static string From(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "?";
        }

        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 1)
        {
            var word = parts[0];
            return word.Length == 1
                ? char.ToUpperInvariant(word[0]).ToString()
                : string.Concat(char.ToUpperInvariant(word[0]), char.ToUpperInvariant(word[1]));
        }

        return string.Concat(char.ToUpperInvariant(parts[0][0]), char.ToUpperInvariant(parts[^1][0]));
    }
}
