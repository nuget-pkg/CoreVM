using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable once CheckNamespace
using Global;
namespace Core;
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

// ReSharper disable once InconsistentNaming
public class CoreVM {
    public static Assembly? CompileScript(string code, params string[] assemblyNames) {
        SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
        FindPackageByIdResource resource = repository.GetResource<FindPackageByIdResource>();
        Log(code);
        var lines = HyperOperatingSystem.TextToLines(code);
        Log(lines);
        foreach (var line in lines) {
            Log(line, "line");
            List<string>? m = HyperOperatingSystem.FindFirstMatch(line,
                @"^//css_nuget[ ]+([^ ;]+)[ ]*;?[ ]*",
                @"^//[+#][+#]nuget[ ]+([^ ;]+)[ ]*;?[ ]*"
            );
            if (m != null) {
                string pkgName = m[1];
                string pkgVersion = "*";
                string[] split = pkgName.Split('@');
                if (split.Length == 2) {
                    pkgName = split[0];
                    pkgVersion = split[1];
                }
                //Break(new { pkgName, pkgVersion });
                Log(new { pkgName, pkgVersion });
                string packageId = "Newtonsoft.Json";
                NuGetVersion packageVersion = new NuGetVersion("12.0.1");
                using MemoryStream packageStream = new MemoryStream();
                async Task<bool> Copier() {
                    return await resource.CopyNupkgToStreamAsync(
                        packageId,
                        packageVersion,
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
                string? packageExtractDir =
                    HyperOperatingSystem.GitProjectFolder(HyperOperatingSystem.GetCwd(), "tmp", pkgName);
                Log(new { pkgName, pkgVersion, packageExtractDir });
                Core.Installer.InstallZipFromStream(packageStream, packageExtractDir!);
            }
        }
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