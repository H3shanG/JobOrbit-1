using System.IO.Compression;
using JobOrbit.Application.Interfaces;
namespace JobOrbit.Application.Services;
public sealed class ResumeFileValidator : IResumeFileValidator
{
    private static readonly byte[] Pdf = "%PDF-"u8.ToArray();
    private static readonly byte[] Ole = [0xD0,0xCF,0x11,0xE0,0xA1,0xB1,0x1A,0xE1];
    public async Task<bool> IsValidAsync(Stream content, string extension, CancellationToken cancellationToken = default)
    {
        if (!content.CanSeek) return false;
        var original = content.Position;
        try
        {
            content.Position=0; var header=new byte[8]; var read=await content.ReadAsync(header,cancellationToken); content.Position=0;
            return extension.ToLowerInvariant() switch
            {
                ".pdf" => read>=Pdf.Length && header.AsSpan(0,Pdf.Length).SequenceEqual(Pdf),
                ".doc" => read>=Ole.Length && header.AsSpan(0,Ole.Length).SequenceEqual(Ole),
                ".docx" => read>=4 && header[0]==0x50 && header[1]==0x4B && IsWordPackage(content),
                _ => false
            };
        }
        catch (InvalidDataException) { return false; }
        finally { content.Position=original; }
    }
    private static bool IsWordPackage(Stream content)
    {
        using var archive=new ZipArchive(content,ZipArchiveMode.Read,true);
        return archive.GetEntry("[Content_Types].xml") is not null && archive.GetEntry("word/document.xml") is not null;
    }
}
