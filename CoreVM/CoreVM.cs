using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Core;

// ReSharper disable once CheckNamespace
namespace Global;
// ReSharper disable once InconsistentNaming
public class CoreVM {
    public static int Add2(int a, int b) {
        return a + b;
    }
    public static string[] ShuffulStringArray(string[] arr) {
        var cobj = CoreObject.FromObject(arr);
        return cobj.Shuffle().AsStringArray!;
    }
}