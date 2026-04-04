// ReSharper disable once CheckNamespace
namespace CoreTest.CoreVM.Test;
using Core;
using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static Global.EasyObject;
public class Add2Test {
    /// Author: ❝Gemini (Google Large Language Model)❞さん
    /// See: https://gemini.google.com/share/90973a6196da
    /// <summary>
    /// Generates a formatted string that includes the specified object's fully qualified type name and the calling
    /// method's name.
    /// </summary>
    /// <remarks>This method is useful for logging or diagnostic scenarios where both the caller's method name
    /// and the object's type are needed. The method uses the CallerMemberName attribute to automatically capture the
    /// name of the calling method if the parameter is not specified.</remarks>
    /// <param name="obj">The object whose type information is included in the formatted string. Cannot be null.</param>
    /// <param name="methodName">The name of the calling method. This parameter is automatically supplied by the compiler and should not be set
    /// explicitly in most cases.</param>
    /// <returns>A string containing the calling method's name and the fully qualified type name of the specified object.</returns>
    private static string GeminiSuperFullName(object obj, [CallerMemberName] string methodName = "") {
        // 引数を省略しても、コンパイラが自動的に呼び出し元のメソッド名を入れます
        return $"#⁅✨{methodName}()✨⁆(CLASS:🌐⁅{obj.GetType().FullName}⁆🌐)";
    }
    [SetUp]
    public void Setup() {
        Echo($"▶START!!", title: GeminiSuperFullName(this));
        // Do some setup here...
        Echo($"▶END!!", title: GeminiSuperFullName(this));
    }
    [Test]
    public void Test001() {
        // TestContextから現在のメソッド名を取得
        //var methodName = TestContext.CurrentContext.Test.MethodName;
        AssertIdentical(expected: 333, actual: CoreVM.Add2(111, 222));
        Echo($"▶passed-01", title: GeminiSuperFullName(this));
        AssertEquivalent(expected: new object[] { 11, 22, 33 }, actual: NewArray(11, 22, 33));
        Echo($"▶passed-02", title: GeminiSuperFullName(this));
        var data1 = new Dictionary<string, object?> {
            { "Apple", 1 },
            { "Banana", 2 },
            { "List", NewArray(11, 22, 33) },
        };
        var data2 = new { Apple = 1, Banana = 2, List = NewArray(11, 22, 33) };
        Echo(data1, "▶data1");
        Echo(data2, "▶data2");
        AssertEquivalent(expected: data1, actual: data2);
        Echo($"▶passed-03", title: GeminiSuperFullName(this));
    }
}