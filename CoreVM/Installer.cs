using NUnit.Framework.Internal;
namespace Core;
using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
internal class Installer {
    public static byte[] ReadAllBytesFromStream(Stream sourceStream) {
        using (var memoryStream = new MemoryStream()) {
            sourceStream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
    }
    public static string? InstallZipFromStream(Stream stream, string extractPath) {
        Global.EasyObject.Log(new { method = "InstallZipFromStream", extractPath });
        //stream.Seek(0, SeekOrigin.Begin);
        //byte[] zipBytes = ReadAllBytesFromStream(stream);
#pragma warning disable SYSLIB0021
        SHA256 crypto = new SHA256CryptoServiceProvider();
#pragma warning restore SYSLIB0021
        //byte[] hashValue = crypto.ComputeHash(zipBytes);
        SafeZipExtract(stream, extractPath);
        return extractPath;
    }
    public static void SafeFileWrite(string filePath, byte[] contents) {
        if (File.Exists(filePath)) {
            return;
        }
        string guid = Guid.NewGuid().ToString("D");
        File.WriteAllBytes($"{filePath}.{guid}", contents);
        try {
            File.Move($"{filePath}.{guid}", filePath);
        }
        catch (Exception) {
            ;
        }
    }
    public static void SafeZipExtract(Stream zipStream, string destinationDirectory) {
        if (Directory.Exists(destinationDirectory)) {
            return;
        }
        System.Console.Error.WriteLine($"Extracting to {destinationDirectory}...");
        string guid = Guid.NewGuid().ToString("D");
        using (ZipArchive archive = new ZipArchive(
                   zipStream,
                   ZipArchiveMode.Read,
                   false)) {
            foreach (ZipArchiveEntry entry in archive.Entries) {
                Global.EasyObject.Log(entry.FullName, "entry.FullName");
                string destinationPath = Path.Combine($"{destinationDirectory}.{guid}", entry.FullName);
                entry.ExtractToFile($"{destinationDirectory}.{guid}", true);
            }
        }
        try {
            Directory.Move($"{destinationDirectory}.{guid}", destinationDirectory);
        }
        catch {
            ;
        }
    }
}