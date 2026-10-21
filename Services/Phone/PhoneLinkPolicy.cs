using System.IO;

namespace TabTower.Services.Phone;

/// <summary>
/// Which tasks-file links a paired phone may make the PC open (0.11.43).
///
/// A click on the PC opens any link the tasks file holds: that is the user's own hand. A tap on
/// the phone is a remote request, so it gets a narrower rule: the link must be a file or folder
/// that exists inside the tasks file's own folder, or below it. Everything else is refused with
/// <c>not_allowed</c>: programs and scripts anywhere else on the disk, network shares, device
/// paths, URL schemes (custom app links included), relative paths and alternate data streams.
///
/// What it does not close: a script someone plants in the tasks folder itself. Writing there is
/// the same trust as writing the tasks file, which already decides what the deck offers.
/// </summary>
public static class PhoneLinkPolicy
{
    /// <summary>Null when the phone may open <paramref name="link"/>; otherwise a short reason,
    /// fit for the deck log and the phone's error detail (it never repeats the link).</summary>
    public static string? Refusal(string? link, string? tasksFilePath)
    {
        string s = (link ?? "").Trim();
        if (s.Length == 0) return "empty link";
        if (string.IsNullOrWhiteSpace(tasksFilePath)) return "no tasks file";

        // UNC, \\?\ and \\.\ device paths, in either slash. Checked before anything else touches
        // the string, so no share is ever contacted by an existence check.
        if (IsNetworkOrDevice(s)) return "network or device path";
        // A drive path is one letter, a colon and a separator. Anything else with a colon is a
        // scheme (https:, file:, ms-settings:, a custom app link) or an alternate data stream.
        if (!IsDrivePath(s)) return "not a local path";
        if (s.IndexOf(':', 2) >= 0) return "not a local path";

        string full, root;
        try
        {
            full = Path.GetFullPath(s);
            string? dir = Path.GetDirectoryName(Path.GetFullPath(tasksFilePath.Trim()));
            if (string.IsNullOrEmpty(dir)) return "no tasks folder";
            root = dir;
        }
        catch (Exception) { return "not a valid path"; }

        // GetFullPath has folded every "..", "." and trailing dot by now; check the result again,
        // since canonicalisation is what turns an innocent-looking string into its real target.
        if (IsNetworkOrDevice(full) || !IsDrivePath(full)) return "network or device path";
        if (IsNetworkOrDevice(root) || !IsDrivePath(root)) return "tasks folder is not on a local drive";

        if (!IsInside(full, root)) return "outside the tasks folder";
        if (!File.Exists(full) && !Directory.Exists(full)) return "does not exist";
        return null;
    }

    public static bool IsAllowed(string? link, string? tasksFilePath) => Refusal(link, tasksFilePath) == null;

    /// <summary>Is <paramref name="full"/> the folder <paramref name="root"/> itself or below it.
    /// Both are full paths. The prefix carries a trailing separator, so a sibling such as
    /// <c>tasks-old</c> never matches <c>tasks</c>.</summary>
    internal static bool IsInside(string full, string root)
    {
        string r = Path.TrimEndingDirectorySeparator(root);
        string f = Path.TrimEndingDirectorySeparator(full);
        if (string.Equals(f, r, StringComparison.OrdinalIgnoreCase)) return true;
        return f.StartsWith(r + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNetworkOrDevice(string s)
        => s.Length >= 2 && (s[0] == '\\' || s[0] == '/') && (s[1] == '\\' || s[1] == '/');

    private static bool IsDrivePath(string s)
        => s.Length >= 3 && char.IsAsciiLetter(s[0]) && s[1] == ':' && (s[2] == '\\' || s[2] == '/');
}
