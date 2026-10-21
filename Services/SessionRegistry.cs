using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace TabTower.Services;

/// <summary>Reads Claude Code's live-session registry for <see cref="CloseTabRoute"/>; the rules are in
/// <see cref="RegistryState.Classify"/>, which a test can reach without the file system.</summary>
public static class SessionRegistry
{
    /// <summary>Claude Code's live-session registry: one JSON per running process, deleted on a
    /// clean exit. CLAUDE_CONFIG_DIR moves it with the rest of the config.</summary>
    internal static string SessionsDir =>
        Path.Combine(Environment.GetEnvironmentVariable("CLAUDE_CONFIG_DIR") is { Length: > 0 } dir
            ? dir
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".claude"),
            "sessions");

    /// <summary>The registered pid is running AND is the process that registered: a recycled pid
    /// belongs to a stranger, so the start time has to match when the file records one.</summary>
    internal static bool ProcessIsTheOne(int pid, string? procStart)
    {
        try
        {
            using var proc = Process.GetProcessById(pid);
            if (proc.HasExited) return false;
            return procStart is not { Length: > 0 } ||
                   proc.StartTime.ToFileTimeUtc().ToString() == procStart;
        }
        catch { return false; }
    }

    public static (ProcessState State, string Why) Probe(string sessionId)
    {
        string[] files;
        try { files = Directory.GetFiles(SessionsDir, "*.json"); }
        catch { return RegistryState.Classify(sessionId, readable: false, Array.Empty<RegistryState.Entry>()); }
        var entries = new List<RegistryState.Entry>(files.Length);
        foreach (var file in files)
        {
            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(file));
                var r = doc.RootElement;
                string? id = r.TryGetProperty("sessionId", out var s) && s.ValueKind == JsonValueKind.String ? s.GetString() : null;
                string? start = r.TryGetProperty("procStart", out var ps) && ps.ValueKind == JsonValueKind.String ? ps.GetString() : null;
                bool running = r.TryGetProperty("pid", out var p) && p.TryGetInt32(out int pid) && ProcessIsTheOne(pid, start);
                entries.Add(new RegistryState.Entry(id, running));
            }
            catch { /* a file mid-write or of another shape answers nothing */ }
        }
        return RegistryState.Classify(sessionId, readable: true, entries);
    }

    /// <summary>Claude Code's transcripts root, beside the registry.</summary>
    internal static string ProjectsDir => Path.Combine(Path.GetDirectoryName(SessionsDir)!, "projects");

    /// <summary>Every registry file in <paramref name="sessionsDir"/>, read for
    /// <see cref="RegistryRecovery"/>. A file mid-write or of another shape is left out; the
    /// decision over what was read is <see cref="RegistryRecovery.Plan"/>.</summary>
    public static List<RegistryRecord> ReadRecords(string sessionsDir, string projectsDir)
    {
        var records = new List<RegistryRecord>();
        string[] files;
        try { files = Directory.GetFiles(sessionsDir, "*.json"); }
        catch { return records; }
        foreach (var file in files)
        {
            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(file));
                var r = doc.RootElement;
                if (r.ValueKind != JsonValueKind.Object) continue;
                string? Str(string name) => r.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
                int? pid = r.TryGetProperty("pid", out var p) && p.TryGetInt32(out int n) ? n : null;
                string? id = Str("sessionId"), start = Str("procStart"), cwd = Str("cwd");
                // Identity is proven only against a recorded start time: ProcessIsTheOne accepts a
                // bare running pid, which is right for Probe's question and wrong for this one.
                bool verified = pid is > 0 && start is { Length: > 0 } && ProcessIsTheOne(pid.Value, start);
                records.Add(new RegistryRecord(id, pid, start, cwd, Str("entrypoint"), Str("kind"),
                    verified, verified && IsIdeBinary(pid!.Value),
                    verified && id is { Length: > 0 } ? FindTranscript(projectsDir, cwd, id) : null));
            }
            catch { /* a file mid-write or of another shape answers nothing */ }
        }
        return records;
    }

    /// <summary>The VS Code extension runs its own claude binary from its install folder; a
    /// standalone CLI does not. An unreadable path answers no.</summary>
    private static bool IsIdeBinary(int pid)
    {
        try
        {
            using var proc = Process.GetProcessById(pid);
            string? path = proc.MainModule?.FileName;
            return path != null && path.Contains("anthropic.claude-code", StringComparison.OrdinalIgnoreCase);
        }
        catch { return false; }
    }

    /// <summary>The transcript of <paramref name="sessionId"/>: first in the folder its cwd slugs to
    /// (every non-alphanumeric character becomes '-', the drive letter in either case), then in any
    /// project folder, because a session's transcript stays where it started.</summary>
    internal static string? FindTranscript(string projectsDir, string? cwd, string sessionId)
    {
        string name = sessionId + ".jsonl";
        try
        {
            if (cwd is { Length: > 0 })
            {
                string slug = string.Concat(cwd.TrimEnd('\\', '/').Select(c => char.IsAsciiLetterOrDigit(c) ? c : '-'));
                foreach (var variant in new[] { slug, char.ToLowerInvariant(slug[0]) + slug[1..], char.ToUpperInvariant(slug[0]) + slug[1..] })
                {
                    string candidate = Path.Combine(projectsDir, variant, name);
                    if (File.Exists(candidate)) return candidate;
                }
            }
            foreach (var dir in Directory.GetDirectories(projectsDir))
            {
                string candidate = Path.Combine(dir, name);
                if (File.Exists(candidate)) return candidate;
            }
        }
        catch { }
        return null;
    }
}
