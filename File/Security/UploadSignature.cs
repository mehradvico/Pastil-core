using System.Text;

namespace UploadGuards;

/// <summary>
/// Verifies that the first bytes of an upload really are the format its extension claims.
/// The extension and Content-Type are client-declared, so without this an attacker could store HTML/script
/// content under an allow-listed name (`.jpg`, `.mp4`, `.pdf`) on the public file origin.
/// </summary>
public static class UploadSignature
{
    private const int HeaderLength = 1024;

    public static async Task<bool> MatchesExtensionAsync(Stream stream, string extension, CancellationToken cancellationToken = default)
    {
        var header = new byte[HeaderLength];
        var read = 0;
        while (read < header.Length)
        {
            var count = await stream.ReadAsync(header.AsMemory(read), cancellationToken);
            if (count == 0) break;
            read += count;
        }

        if (stream.CanSeek) stream.Position = 0;
        return Matches(header.AsSpan(0, read), extension);
    }

    public static bool Matches(ReadOnlySpan<byte> header, string extension)
    {
        switch ((extension ?? string.Empty).ToLowerInvariant())
        {
            case ".jpg":
            case ".jpeg":
                return header.Length >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
            case ".png":
                return header.StartsWith(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
            case ".gif":
                return header.StartsWith("GIF87a"u8) || header.StartsWith("GIF89a"u8);
            case ".webp":
                return header.Length >= 12 && header[..4].SequenceEqual("RIFF"u8) && header.Slice(8, 4).SequenceEqual("WEBP"u8);
            case ".pdf":
                return header.IndexOf("%PDF-"u8) is >= 0 and < 1024;
            case ".mp4":
            case ".mov":
                if (header.Length < 12) return false;
                var box = Encoding.ASCII.GetString(header.Slice(4, 4));
                return box is "ftyp" or "moov" or "mdat" or "wide" or "free" or "skip";
            case ".webm":
                return header.StartsWith(new byte[] { 0x1A, 0x45, 0xDF, 0xA3 });
            case ".ogg":
                return header.StartsWith("OggS"u8);
            default:
                return false;
        }
    }
}
