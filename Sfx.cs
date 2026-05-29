using System.IO;
using System.Reflection;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace BuckshotFluentte;

public static class Sfx
{
    private static string? _soundDir;
    private static readonly object _lock = new();

    private static string EnsureSoundDir()
    {
        if (_soundDir != null) return _soundDir;
        lock (_lock)
        {
            if (_soundDir != null) return _soundDir;

            var dir = Path.Combine(Path.GetTempPath(), "BuckshotFluentte_sounds");
            Directory.CreateDirectory(dir);

            var assembly = Assembly.GetExecutingAssembly();
            var prefix = "BuckshotFluentte.sounds.";

            foreach (var name in assembly.GetManifestResourceNames())
            {
                if (!name.StartsWith(prefix)) continue;
                var fileName = name.Substring(prefix.Length);
                var filePath = Path.Combine(dir, fileName);
                if (File.Exists(filePath)) continue;

                using var stream = assembly.GetManifestResourceStream(name);
                if (stream == null) continue;
                using var fs = File.Create(filePath);
                stream.CopyTo(fs);
            }

            _soundDir = dir;
            return dir;
        }
    }

    public static void Play(string fileName)
    {
        try
        {
            var path = Path.Combine(EnsureSoundDir(), fileName);
            if (!File.Exists(path)) return;
            var player = new MediaPlayer();
            player.Source = MediaSource.CreateFromUri(new Uri(path));
            player.MediaEnded += (s, e) => player.Dispose();
            player.MediaFailed += (s, e) => player.Dispose();
            player.Play();
        }
        catch { /* ignore audio errors silently */ }
    }

    public static async Task PlayDelayedAsync(string fileName, int delayMs)
    {
        await Task.Delay(delayMs);
        Play(fileName);
    }
}
