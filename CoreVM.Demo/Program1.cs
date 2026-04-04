namespace Global.CoreVM.Demo;
using System;
using Core;
using static Global.EasyObject;
public static class Program1 {
    public static void Main(string[] args) {
        try {
            UseAnsiConsole = true;
            DebugOutput = true;
            ShowLineNumbers = true;
            ShowDetail = false;
            SetupConsoleEncoding();
            //Core.Script.Run();
            //return; /**/
            Log(
                "⁅markup⁆[green]Hello! ハロー© ⁅EMOJI⁆◉▶▸⸝↪️ ↩️ ℴ𝓬➺➢ᰔヾ➠✅🈂️❓❗＼／：＊“≪≫￤；‘｀＃％＄＆＾～￤﴾﴿⁅⁆【】≪≫＋ー＊＝⚽ 𝑪𝒉𝒆𝒄𝒌 🌐🪩[/]");
            //Break();
            var greeting = new string[] { "Hello", "World", "my", "name", "is", "Tom" };
            //Break(CoreVM.ShuffulStringArray(greeting), "ShuffulStringArray(greeting)");
            EchoWebLink(
                "!! THIS TEXT CAN BE CLICKED FOR OPENING URL !! («YOUTUBE PLAYLIST»⭕️⁅🌐⁆@⁅反転mirror⁆パイパイ仮面でどうかしらん？ / 宝鐘マリン FULL 踊ってみた【練習用】 - YouTube)",
                "https://www.youtube.com/watch?v=sLpodTN4xhI&list=PLTvSv0jkjbk9-emLIV2vM-0p7CeMnTYG2");
            Debug(new { args });
            // var answer = CoreVM.Add2(11, 22);
            // Debug(new { answer });
            var answer = 11 + 22;
            ExpectEquivalent(
                expected: 33,
                actual: answer,
                hint: new {
                    answer,
                    now = DateTime.Now
                },
                exitCode: 1234
            );
            //new NativeScript().Run();
            Line();
            Log(code, "code");
            var assembly = CoreVM.CompileScript(
                code,
                [ /*"System.Threading.Tasks.Extensions"*/]
            );
            Line();
            ExpectBound(assembly);
            Line();
            dynamic? script = CoreVM.LookupScriptClass(assembly, "Script");
            Line();
            ExpectBound(script);
            Line();
            script!.Run();
            //Message("﴾↪️END OF PROGRAM↩️﴿");
            Line();
        }
        catch (Exception ex) {
            Abort(ex);
        }
    }
    private static string code = """
                                 //css_nuget YoutubeExplode
                                 //css_nuget YoutubeExplode.Converter
                                 //css_nuget System.Threading.Tasks.Extensions 
                                 using System;
                                 using System.Threading.Tasks;
                                 using YoutubeExplode;
                                 using YoutubeExplode.Converter;

                                 //new Script().Run();

                                 public class Script
                                 {
                                     public void Run()
                                     {
                                         try
                                         {
                                             var youtube = new YoutubeClient();
                                             //var videoUrl = "https://www.youtube.com/watch?v=gkdpDAhRsDk";
                                             var videoUrl = "https://www.youtube.com/watch?v=wzcdhDyNmMM";
                                             async Task<YoutubeExplode.Videos.Video> Getter(string videoUrl)
                                             {
                                                 return await youtube.Videos.GetAsync(videoUrl);
                                             }
                                             var videoAsync = Getter(videoUrl);
                                             videoAsync.Wait();
                                             // 1. まず動画の詳細情報（メタデータ）を取得
                                             var video = videoAsync.Result; //await youtube.Videos.GetAsync(videoUrl);
                                             var filePath = $"{video.Title}.mkv";
                                             Console.WriteLine($"filePath: {filePath}");
                                             // 進捗を表示するためのハンドラを作成
                                             var progressHandler = new Progress<double>(p =>
                                             {
                                                 // p は 0.0 ～ 1.0 の値（パーセンテージ）
                                                 Console.Write($"\rダウンロード中... {p:P1} ");
                                             });
                                             // DownloadAsync の引数に進捗ハンドラを追加
                                             async Task Downloader(string videoUrl, string filePath)
                                             {
                                                 await youtube.Videos.DownloadAsync(videoUrl, filePath, builder => builder
                                                     .SetContainer("matroska")
                                                     .SetPreset(ConversionPreset.VeryFast),
                                                     progressHandler // ここに進捗オブジェクトを渡す
                                                 );
                                             }
                                             var downloadAsync = Downloader(videoUrl, filePath);
                                             downloadAsync.Wait();
                                             Console.WriteLine("\n完了！");
                                         }
                                         catch (Exception ex)
                                         {
                                             Console.Error.WriteLine(ex.ToString());
                                         }
                                     }
                                 }
                                 """;
}