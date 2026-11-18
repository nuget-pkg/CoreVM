namespace ConsoleApp1
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using NuGet.Common;
    using NuGet.Configuration;
    using NuGet.Frameworks;
    using NuGet.Packaging;
    using NuGet.Packaging.Core;
    using NuGet.Protocol;
    using NuGet.Protocol.Core.Types;
    using NuGet.Resolver;
    using NuGet.Versioning;

    class Program
    {
        static async Task Main(string[] args)
        {
            // 1. 設定と各種初期化
            //string packageId = "YoutubeExplode.Converter"; // ダウンロードしたいパッケージ
            string packageId = "YoutubeExplode"; // ダウンロードしたいパッケージ
            string targetVersion = "6.6.3";       // バージョン（nullなら最新）
            string outputDirectory = Path.Combine(AppContext.BaseDirectory, "DownloadedPackages");

            ILogger logger = NullLogger.Instance;
            CancellationToken cancellationToken = CancellationToken.None;

            // NuGet公式フィードのソース
            SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
            SourceCacheContext cacheContext = new SourceCacheContext();

            // 2. メタデータ（依存関係）を取得するためのリソースを用意
            DependencyInfoResource dependencyInfoResource = await repository.GetResourceAsync<DependencyInfoResource>();
            FindPackageByIdResource findPackageByIdResource = await repository.GetResourceAsync<FindPackageByIdResource>();

            Console.WriteLine($"{packageId} の依存関係を解析中...");

            // 3. 依存関係の解決 (Dependency Resolution)
            var targetFramework = NuGetFramework.Parse("net8.0"); // 対象とする.NETバージョン
            var availablePackages = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);

            // 再帰的に依存関係を収集するキュー
            var queue = new Queue<PackageIdentity>();

            // 最初のターゲットパッケージを指定
            NuGetVersion version = string.IsNullOrEmpty(targetVersion)
                ? (await findPackageByIdResource.GetAllVersionsAsync(packageId, cacheContext, logger, cancellationToken)).Max()
                : NuGetVersion.Parse(targetVersion);

            queue.Enqueue(new PackageIdentity(packageId, version));

            while (queue.Count > 0)
            {
                var dependency = queue.Dequeue();
                if (availablePackages.Any(p => p.Id.Equals(dependency.Id, StringComparison.OrdinalIgnoreCase))) continue;

                var dependencyInfo = await dependencyInfoResource.ResolvePackage(
                    dependency, targetFramework, cacheContext, logger, cancellationToken);

                if (dependencyInfo != null)
                {
                    availablePackages.Add(dependencyInfo);
                    foreach (var dep in dependencyInfo.Dependencies)
                    {
                        // 依存パッケージの最新（または条件に合う）バージョンを取得してキューに追加
                        var depVersions = await findPackageByIdResource.GetAllVersionsAsync(dep.Id, cacheContext, logger, cancellationToken);
                        var bestVersion = dep.VersionRange.FindBestMatch(depVersions);
                        queue.Enqueue(new PackageIdentity(dep.Id, bestVersion));
                    }
                }
            }

            // 4. パッケージのダウンロード
            Directory.CreateDirectory(outputDirectory);
            Console.WriteLine($"\nダウンロードを開始します。保存先: {outputDirectory}");

            FindPackageByIdResource downloadResource = await repository.GetResourceAsync<FindPackageByIdResource>();

            foreach (var package in availablePackages)
            {
                string nupkgPath = Path.Combine(outputDirectory, $"{package.Id}.{package.Version}.nupkg");

                if (File.Exists(nupkgPath))
                {
                    Console.WriteLine($"[スキップ] すでに存在します: {package.Id} ({package.Version})");
                    continue;
                }

                using (var fs = new FileStream(nupkgPath, FileMode.Create, FileAccess.Write))
                {
                    await downloadResource.CopyNupkgToStreamAsync(
                        package.Id, package.Version, fs, cacheContext, logger, cancellationToken);
                }
                Console.WriteLine($"[完了] ダウンロード成功: {package.Id} ({package.Version})");
            }

            Console.WriteLine("\nすべての処理が完了しました。");
        }
    }
}
