//css_nuget NuGet.Protocol
//css_nuget EasyObject
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System.IO;
using System.Threading;
using static Global.EasyObject;

try {
    PackageSource localSource = new PackageSource(@"C:\LocalSource");
    SourceRepository localRepository = Repository.Factory.GetCoreV3(localSource);
    SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json"); FindPackageByIdResource resource = await repository.GetResourceAsync<FindPackageByIdResource>();
    string packageId = "Newtonsoft.Json";
    NuGetVersion packageVersion = new NuGetVersion("12.0.1");
    using MemoryStream packageStream = new MemoryStream();
    await resource.CopyNupkgToStreamAsync(
        packageId,
        packageVersion,
        packageStream,
        new SourceCacheContext(),
        NullLogger.Instance,
        CancellationToken.None);
    packageStream.Seek(0, SeekOrigin.Begin);
    Log(packageStream.Length);
    ExpectTrue(packageStream.Length > 0, "Package stream should not be empty.");
    using FileStream fileStream = new FileStream(@"C:\home17\tmp\Newtonsoft.Json.nupkg", FileMode.Create, FileAccess.Write);
   packageStream.CopyTo(fileStream);
} catch(System.Exception ex) {
    Abort(ex);
}
