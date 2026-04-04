// //css_nuget NugetDownloader
//css_dir NugetWorker

//css_nuget log4net@2.0.8
//css_nuget Nuget.Packagemanagement@4.8.0
//css_nuget Nuget.Projectmodel@4.8.0
//css_nuget Nuget.Protocol@4.8.0
//css_nuget Nuget.resolver@4.8.0
//css_nuget System.Runtime.Loader@4.3.0
//css_nuget System.Runtime.Serialization.Json@4.3.0

//css_nuget EasyObject
using NugetWorker;
using static Global.EasyObject;
//using NugetEngine;
Log("hello");
try {
    string packageName="Newtonsoft.json";
    string version="10.2.1.0";
    NugetEngine nugetEngine = new NugetEngine();
    nugetEngine.GetPackage(packageName, version).Wait();
    Log("end");
} catch(System.Exception ex) {
    Abort(ex);
}
