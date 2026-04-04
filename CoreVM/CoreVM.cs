using System.Globalization;
namespace Core;
using Global;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using CSScripting;
using CSScriptLib;
using System;
using System.Linq;
using System.Reflection;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using static Global.EasyObject;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable once InconsistentNaming
public class CoreVM {
    public static EasyObject packageTargetFrameworkList = NewArray();
    public static EasyObject LoadNupkgSpec(string filePath) {
        //Break(new { method = "LoadNupkgSpec", filePath });
        string nuspec = File.ReadAllText(filePath);
        Log(nuspec, "nuspec");
        var nuspecObject = Global.NewtonsoftJsonUtil.DeserializeFromXml(nuspec);
        nuspecObject.Dump(title: "nuspecObject");
        string packageId = nuspecObject.Dynamic.package.metadata.id;
        ExpectBound(packageId, new { packageId });
        //Break(new { packageId });
        EasyObject dependencies = nuspecObject.Dynamic.package.metadata.dependencies.group;
        Log(dependencies, "dependencies");
        //Log(dependencies.Count);
        for (int i = 0; i < dependencies.Count; i++) {
            //Log(new { packageId, dependency = dependencies[i].Keys });
            string fw = dependencies[i]["@targetFramework"].Cast<string>();
            //Log(new { packageId, fw });
            if (!fw.StartsWith(".NETStandard2.") && !fw.StartsWith(".NETFramework4.")) {
                // AD-HOK: only searcing fo .NET Framework
                Log(fw, "SKIPPING");
                continue;
            }
            var d = dependencies[i];
            d.Dynamic.packageId = packageId;
            packageTargetFrameworkList.Add(d);
        }
        //Abort();
        return "*not-implemented*";
    }
    public static byte[] StreamToByteArray(Stream sourceStream) {
        using (var memoryStream = new MemoryStream()) {
            sourceStream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
    }
    public static string ComputeStrongDigest(byte[] data) {
#pragma warning disable SYSLIB0021
        SHA256 crypto = new SHA256CryptoServiceProvider();
#pragma warning restore SYSLIB0021
        byte[] hashValue = crypto.ComputeHash(data);
        string sha256 = string.Join("", hashValue.Select(x => x.ToString("x2")).ToArray());
        return sha256;
    }
    public static void SafeFileWrite(string filePath, byte[] contents) {
        if (File.Exists(filePath)) return;
        string guid = Guid.NewGuid().ToString("D");
        File.WriteAllBytes($"{filePath}.{guid}", contents);
        try {
            File.Move($"{filePath}.{guid}", filePath);
        }
        catch (Exception) {
            ;
        }
    }
    public static void SafeFileWrite(string filePath, string text) {
        SafeFileWrite(filePath, Encoding.UTF8.GetBytes(text));
    }
    public static void SafeZipExtract(string zipPath, string dirPath) {
        if (Directory.Exists(dirPath)) {
            return;
        }
        string guid = Guid.NewGuid().ToString("D");
        ZipFile.ExtractToDirectory(zipPath, $"{dirPath}.{guid}");
        try {
            Directory.Move($"{dirPath}.{guid}", dirPath);
        }
        catch (Exception) {
            ;
        }
    }
    public static Assembly? CompileScript(string code, params string[] assemblyNames) {
        SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
        FindPackageByIdResource resource = repository.GetResource<FindPackageByIdResource>();
        //Log(code);
        var lines = HyperOperatingSystem.TextToLines(code);
        //Log(lines);
        foreach (var line in lines) {
            //Log(line, "line");
            List<string>? m = HyperOperatingSystem.FindFirstMatch(line,
                @"^//css_nuget[ ]+([^ ;]+)[ ]*;?[ ]*",
                @"^//[+#][+#]nuget[ ]+([^ ;]+)[ ]*;?[ ]*"
            );
            if (m != null) {
                Log(line, "line");
                string packageId = m[1];
                string packageVersion = "*";
                string[] split = packageId.Split('@');
                if (split.Length == 2) {
                    packageId = split[0];
                    packageVersion = split[1];
                }
                if (packageVersion == "*") {
                    string? packageVersionMaybeNull = NuGetHelper.GetLatestVersion(packageId);
                    ExpectBound(packageVersionMaybeNull, "packageVersion");
                    packageVersion = packageVersionMaybeNull!;
                }
                //Break(new { pkgName, pkgVersion });
                Log(new { pkgName = packageId, pkgVersion = packageVersion });
                //string packageId = "Newtonsoft.Json";
                NuGetVersion packageVersionObject = new NuGetVersion(packageVersion);
                using (MemoryStream packageStream = new MemoryStream()) {
                    async Task<bool> Copier() {
                        return await resource.CopyNupkgToStreamAsync(
                            packageId,
                            packageVersionObject,
                            packageStream,
                            new SourceCacheContext(),
                            NullLogger.Instance,
                            CancellationToken.None);
                    }
                    var copier = Copier();
                    copier.Wait();
                    bool copierResult = copier.Result;
                    Log(copierResult);
                    packageStream.Seek(0, SeekOrigin.Begin);
                    Log(packageStream.Length);
                    ExpectTrue(packageStream.Length > 0, "Package stream should not be empty.");
                    packageStream.Seek(0, SeekOrigin.Begin);
                    var nupkgBytes = StreamToByteArray(packageStream);
                    var nupkgHash = ComputeStrongDigest(nupkgBytes);
                    var nupkgBaseName = $"{packageId}{nupkgHash}.nupkg";
                    string packageExtractDir =
                        HyperOperatingSystem.GitProjectFolder(HyperOperatingSystem.GetCwd(), "tmp", packageId)!;
                    string nupkgFullPath = Path.Combine(packageExtractDir, nupkgBaseName);
                    SafeFileWrite(nupkgFullPath, nupkgBytes);
                    Log(new { pkgName = packageId, pkgVersion = packageVersionObject, packageExtractDir });
                    string relativePath = "?";
                    if (packageVersion == "*") {
                        relativePath = Path.Combine(packageExtractDir, nupkgHash);
                    }
                    else {
                        relativePath = Path.Combine(packageExtractDir, packageVersion);
                    }
                    string pkgInstallPath = Path.Combine(packageExtractDir, relativePath);
                    SafeZipExtract(nupkgFullPath, pkgInstallPath);
                    string nuspecPath = Path.Combine(pkgInstallPath, $"{packageId}.nuspec");
                    Log(new { nuspecPath });
                    ExpectTrue(File.Exists(nuspecPath), "nuspec file not found.");
                    LoadNupkgSpec(nuspecPath);
                }
            }
        }
        packageTargetFrameworkList.Dump();
        Abort();
        var script = CSScript.Evaluator
                .ReferenceAssembliesFromCode(code)
                .ReferenceAssemblyByName("System.Threading.Tasks.Extensions")
            ;
        CSScript.Evaluator.With(static eval => { eval.IsCachingEnabled = false; });
        var assemblyNamesList = assemblyNames.ToList();
        if (!assemblyNamesList.Contains("System.Threading.Tasks.Extensions")) {
            assemblyNamesList.Add("System.Threading.Tasks.Extensions");
        }
        foreach (var assemblyName in assemblyNamesList) {
            script.ReferenceAssemblyByName(assemblyName);
        }
        CompileInfo compileInfo = new() {
            LanguageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion.Latest,
            PreferLoadingFromFile = true,
            //CodeKind = Microsoft.CodeAnalysis.SourceCodeKind.Script,
            CodeKind = Microsoft.CodeAnalysis.SourceCodeKind.Regular,
            AssemblyName = "DynamicClass",
            AssemblyFile = "DynamicClass.dll",
            RootClass = "RootClass",
            //CompilerOptions = "/unsafe",
            LoadedAssembly = null,
            PdbFile = null,
        };
        var assembly = script.CompileMethod(code, compileInfo);
        return assembly;
    }
    public static dynamic? LookupScriptClass(Assembly? assembly, string className) {
        if (assembly == null) {
            return null;
        }
        var scriptType = assembly.GetType($"DynamicClass+{className}");
        if (scriptType == null) {
            return null;
        }
        var scriptInstance = Activator.CreateInstance(scriptType);
        return scriptInstance;
    }
}
public class NuGetHelper {
    public static string? GetLatestVersion(string packageId) {
        var asyncObject = GetLatestVersionAsync(packageId);
        asyncObject.Wait();
        return asyncObject.Result;
    }
    public static async Task<string?> GetLatestVersionAsync(string packageId) {
        ILogger logger = NullLogger.Instance;
        CancellationToken cancellationToken = CancellationToken.None;
        // 1. Connect to the NuGet V3 feed
        SourceCacheContext cache = new SourceCacheContext();
        SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
        // 2. Get the FindPackageByIdResource
        FindPackageByIdResource resource = await repository.GetResourceAsync<FindPackageByIdResource>();
        // 3. Fetch all versions for the package
        IEnumerable<NuGetVersion> versions = await resource.GetAllVersionsAsync(
            packageId,
            cache,
            logger,
            cancellationToken);
        // 4. Filter for the latest stable version
        // Use .Where(v => !v.IsPrerelease) to exclude betas/previews
        NuGetVersion? latestVersion = versions
            .Where(v => !v.IsPrerelease)
            .OrderByDescending(v => v)
            .FirstOrDefault();
        return latestVersion?.ToNormalizedString();
    }
}