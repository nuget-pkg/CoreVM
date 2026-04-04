//css_nuget NugetDownloader
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
