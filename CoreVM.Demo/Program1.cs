using System.Runtime;
namespace Global.CoreVM.Demo;
using System;
using System.CodeDom;
using Global;
using static Global.EasyObject;
public static class Program1 {
    public static void Main(string[] args) {
        UseAnsiConsole = true;
        DebugOutput = true;
        ShowLineNumbers = true;
        ShowDetail = false;
        SetupConsoleEncoding();
        Log(
            "⁅markup⁆[green]Hello! ハロー© ⁅EMOJI⁆◉▶▸⸝↪️ ↩️ ℴ𝓬➺➢ᰔヾ➠✅🈂️❓❗＼／：＊“≪≫￤；‘｀＃％＄＆＾～￤﴾﴿⁅⁆【】≪≫＋ー＊＝⚽ 𝑪𝒉𝒆𝒄𝒌 🌐🪩[/]");
        Break();
        var greeting = new string[] { "Hello", "World", "my", "name", "is", "Tom" };
        Break(CoreVM.ShuffulStringArray(greeting), "ShuffulStringArray(greeting)");
        EchoWebLink(
            "!! THIS TEXT CAN BE CLICKED FOR OPENING URL !! («YOUTUBE PLAYLIST»⭕️⁅🌐⁆@⁅反転mirror⁆パイパイ仮面でどうかしらん？ / 宝鐘マリン FULL 踊ってみた【練習用】 - YouTube)",
            "https://www.youtube.com/watch?v=sLpodTN4xhI&list=PLTvSv0jkjbk9-emLIV2vM-0p7CeMnTYG2");
        Debug(new { args });
        var answer = CoreVM.Add2(11, 22);
        Debug(new { answer });
        ExpectEquivalent(
            expected: 333,
            actual: answer,
            hint: new {
                answer,
                now = DateTime.Now
            },
            exitCode: 1234
        );
    }
}