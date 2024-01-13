using System;
using System.IO;
using System.Runtime.CompilerServices;
using DotnetPatcher.Decompile;
using DotnetPatcher.Patch;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using Tomat.Differ.Identity;
using Tomat.Differ.Identity.Steam;
using Tomat.Differ.Transformation;
using Tomat.Differ.Transformation.Transformers;

[assembly: InternalsVisibleTo("Tomat.Differ.Build")]

namespace Tomat.Differ;

internal static class Program {
    private const string file_exclusion_regex = @"^.*(?<!\.xnb)(?<!\.xwb)(?<!\.xsb)(?<!\.xgs)(?<!\.bat)(?<!\.txt)(?<!\.xml)(?<!\.msi)$";

    internal static void Main() {
        if (Environment.GetEnvironmentVariable("SKIP_DOWNLOAD") != "1") {
            var username = Console.ReadLine()!;
            var password = Console.ReadLine()!;

            File.WriteAllText("filelist.txt", "regex:" + file_exclusion_regex);
            DownloadManifest(username, password, Games.Terraria.RELEASE);
            DownloadManifest(username, password, Games.Terraria.LINUX);
            DownloadManifest(username, password, Games.Terraria.MAC);
        }

        if (Environment.GetEnvironmentVariable("PATCH_FILE") is not { } patchFileName)
            patchFileName = "patches.json";

        var patchConfiguration = DiffNode.FromFile(patchFileName);

        DecompileAndDiffDepotNodes(patchConfiguration);

        if (Environment.GetEnvironmentVariable("DIFF_MODS") == "1")
            DiffModNodes(patchConfiguration);

        if (Environment.GetEnvironmentVariable("PATCH_MODS") == "1")
            PatchModNodes(patchConfiguration);
    }

    private static void DownloadManifest(string username, string password, Manifest manifest) {
        var appId = Games.Terraria.GAME.AppId;
        var depot = manifest.DepotId;

        if (Directory.Exists(manifest.Name))
            Directory.Delete(manifest.Name, true);

        DepotDownloader.Program.Main(
            new[] {
                "-app",
                appId.ToString(),
                "-depot",
                depot.ToString(),
                "-filelist",
                "filelist.txt",
                "-username",
                username,
                "-password",
                password,
                "-dir",
                Path.Combine("downloads", manifest.Name),
                //"-remember-password",
            }
        );
    }

    private static void DecompileAndDiffDepotNodes(DiffNode node, DiffNode? parent = null) {
        if (node is not DepotDiffNode depotNode) {
            Console.WriteLine($"Skipping {node.WorkspaceName} since it isn't a depot node...");
            foreach (var child in node.Children)
                DecompileAndDiffDepotNodes(child, node);

            return;
        }

        const string decompilation_dir = "decompiled";
        const string patches_dir = "patches";
        var dirName = Path.Combine(decompilation_dir, node.WorkspaceName);

        if (Environment.GetEnvironmentVariable("SKIP_DECOMPILATION") != "1") {
            Console.WriteLine($"Decompiling {node.WorkspaceName}...");

            Console.WriteLine("Transforming assemblies...");

            if (Directory.Exists(dirName))
                Directory.Delete(dirName, true);

            Directory.CreateDirectory(dirName);

            var depotDir = Path.Combine("downloads", depotNode.DepotName);
            if (!Directory.Exists(depotDir))
                throw new Exception($"Depot {depotNode.DepotName} was not downloaded!");

            var clonedDir = Path.Combine("cloned", depotNode.WorkspaceName);
            if (Directory.Exists(clonedDir))
                Directory.Delete(clonedDir, true);

            CopyRecursively(depotDir, clonedDir);
            var exePath = Path.Combine(clonedDir, depotNode.RelativePathToExecutable);

            var context = AssemblyTransformer.GetAssemblyContextWithUniversalAssemblyResolverFromPath(exePath);
            AssemblyTransformer.TransformAssembly(context, new DecompilerParityTransformer());

            // var formatting = FormattingOptionsFactory.CreateKRStyle();
            // formatting.IndentationString = "    ";
            var decompiler = new Decompiler(
                exePath,
                dirName,
                new DecompilerSettings {
                    CSharpFormattingOptions = FormattingOptionsFactory.CreateKRStyle(),
                    Ranges = false,
                }
            );

            decompiler.Decompile(new[] { "ReLogic", /*"LogitechLedEnginesWrapper",*/ "RailSDK.Net", "SteelSeriesEngineWrapper" });
        }

        foreach (var child in node.Children)
            DecompileAndDiffDepotNodes(child, node);

        if (parent is null) {
            // Create an empty patches directory for the root node.
            Directory.CreateDirectory(Path.Combine(patches_dir, node.WorkspaceName));
            return;
        }

        if (Environment.GetEnvironmentVariable("SKIP_DIFFING") == "1")
            return;

        Console.WriteLine($"Diffing {node.WorkspaceName}...");

        var patchDirName = Path.Combine(patches_dir, node.WorkspaceName);
        if (Directory.Exists(patchDirName))
            Directory.Delete(patchDirName, true);

        Directory.CreateDirectory(patchDirName);

        var differ = new DotnetPatcher.Diff.Differ(Path.Combine(decompilation_dir, parent.WorkspaceName), patchDirName, dirName);
        differ.Diff();
    }

    private static void DiffModNodes(DiffNode node, DiffNode? parent = null) {
        if (Environment.GetEnvironmentVariable("ONLY_NODE") is { } onlyNode && node.WorkspaceName != onlyNode) {
            Console.WriteLine($"Skipping {node.WorkspaceName} since it isn't the expected node ({onlyNode})...");
            foreach (var child in node.Children)
                DiffModNodes(child, node);

            return;
        }

        if (node is not ModDiffNode modNode) {
            Console.WriteLine($"Skipping {node.WorkspaceName} since it isn't a mod node...");
            foreach (var child in node.Children)
                DiffModNodes(child, node);

            return;
        }

        if (parent is null) {
            Console.WriteLine($"Skipping {node.WorkspaceName} since it is a root node...");
            return;
        }

        Console.WriteLine($"Diffing {node.WorkspaceName}...");

        var patchDirName = Path.Combine("patches", node.WorkspaceName);
        if (Directory.Exists(patchDirName))
            Directory.Delete(patchDirName, true);

        Directory.CreateDirectory(patchDirName);

        var differ = new DotnetPatcher.Diff.Differ(Path.Combine("decompiled", parent.WorkspaceName), patchDirName, Path.Combine("decompiled", node.WorkspaceName));
        differ.Diff();

        foreach (var child in node.Children)
            DiffModNodes(child, node);
    }

    private static void PatchModNodes(DiffNode node, DiffNode? parent = null) {
        if (Environment.GetEnvironmentVariable("ONLY_NODE") is { } onlyNode && node.WorkspaceName != onlyNode) {
            Console.WriteLine($"Skipping {node.WorkspaceName} since it isn't the expected node ({onlyNode})...");
            foreach (var child in node.Children)
                PatchModNodes(child, node);

            return;
        }

        if (node is not ModDiffNode /*modNode*/) {
            Console.WriteLine($"Skipping {node.WorkspaceName} since it isn't a mod node...");
            foreach (var child in node.Children)
                PatchModNodes(child, node);

            return;
        }

        if (parent is null) {
            Console.WriteLine($"Skipping {node.WorkspaceName} since it is a root node...");
            return;
        }

        Console.WriteLine($"Patching {node.WorkspaceName}...");

        var patchDirName = Path.Combine("patches", node.WorkspaceName);
        if (!Directory.Exists(patchDirName))
            Directory.CreateDirectory(patchDirName);

        if ( /*Environment.GetEnvironmentVariable("REGENERATE_MOD_SOURCES") == "1" &&*/ Directory.Exists(Path.Combine("decompiled", node.WorkspaceName)))
            Directory.Delete(Path.Combine("decompiled", node.WorkspaceName), true);

        var patcher = new Patcher(Path.Combine("decompiled", parent.WorkspaceName), Path.Combine("patches", node.WorkspaceName), Path.Combine("decompiled", node.WorkspaceName));
        patcher.Patch();

        foreach (var child in node.Children)
            PatchModNodes(child, node);
    }

    private static void CopyRecursively(string fromDir, string toDir) {
        foreach (var dir in Directory.GetDirectories(fromDir, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(dir.Replace(fromDir, toDir));

        foreach (var file in Directory.GetFiles(fromDir, "*", SearchOption.AllDirectories))
            File.Copy(file, file.Replace(fromDir, toDir), true);
    }
}
