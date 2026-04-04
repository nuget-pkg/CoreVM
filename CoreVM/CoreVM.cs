using System.Reflection;
// ReSharper disable once CheckNamespace
using CSScriptLib;
namespace Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System;
using System.Drawing;
using System.Threading.Tasks;
using CSScripting;
using YoutubeExplode;
using YoutubeExplode.Converter;
using static Global.EasyObject;

// ReSharper disable once InconsistentNaming
public class CoreVM {
    public static int Add2(int a, int b) {
        return a + b;
    }
    public static string[] ShuffulStringArray(string[] arr) {
        var cobj = CoreObject.FromObject(arr);
        return cobj.Shuffle().AsStringArray!;
    }
    public static Assembly? CompileScript(string code, params string[] assemblyNames) {
        CSScript.Evaluator.With(static eval => { eval.IsCachingEnabled = false; });
        var script = CSScript.Evaluator
            //.ReferenceAssembliesFromCode(code)
            //.ReferenceAssemblyByName("System.Runtime")
            //.ReferenceAssemblyByName("System.Threading.Tasks.Extensions")
            ;
        var assemblyNamesList = assemblyNames.ToList();
        if (!assemblyNamesList.Contains("System.Threading.Tasks.Extensions")) {
            assemblyNamesList.Add("System.Threading.Tasks.Extensions");
        }
        foreach (var assemblyName in assemblyNamesList) {
            script.ReferenceAssemblyByName(assemblyName);
        }
        var assembly = script.CompileMethod(code);
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