using System;
using System.Linq;
using Spectre.Console;
using Tomat.Differ;

const string install_depots = "Install Depots";
const string diff_all_depots = "Diff All Depots";
const string diff_all_mods = "Diff All Mods";
const string diff_patch = "Diff Single Workspace";
const string patch_all_mods = "Patch All Mods";
const string patch_patch = "Patch Single Workspace";
const string regenerate_sources = "Regenerate Mod Sources";

if (args.Length != 1)
    Console.WriteLine("No patches file provided, assuming relative patches.json");

var patchesFile = DiffNode.FromFile(args.Length >= 1 ? args[0] : "patches.json");

var downloadDepots = false;
var decompileDepots = false;
var diffAllDepots = false;
var diffMods = false;
var patchMods = false;
var regenerateModSources = false;
var onlyNode = default(string?);

var task = AnsiConsole.Prompt(
    new SelectionPrompt<string>().Title("Select task")
        .AddChoices(install_depots, diff_all_depots, diff_all_mods, diff_patch, patch_all_mods, patch_patch, regenerate_sources)
);

switch (task) {
    // Reinstalls all depots.
    case install_depots:
        downloadDepots = true;
        decompileDepots = false;
        diffAllDepots = false;
        diffMods = false;
        patchMods = false;
        regenerateModSources = false;
        onlyNode = null;
        break;

    // Diffs all depots.
    case diff_all_depots:
        downloadDepots = false;
        decompileDepots = false;
        diffAllDepots = true;
        diffMods = false;
        patchMods = false;
        regenerateModSources = false;
        onlyNode = null;
        break;

    // Diffs all mods.
    case diff_all_mods:
        downloadDepots = false;
        decompileDepots = false;
        diffAllDepots = false;
        diffMods = true;
        patchMods = false;
        regenerateModSources = false;
        onlyNode = null;
        break;

    // Diffs a single workspace.
    case diff_patch:
        downloadDepots = false;
        decompileDepots = false;
        diffAllDepots = false;
        diffMods = true;
        patchMods = false;
        regenerateModSources = false;
        onlyNode = selectNode();
        break;

    // Applies patches to all mods.
    case patch_all_mods:
        downloadDepots = false;
        decompileDepots = false;
        diffAllDepots = false;
        diffMods = false;
        patchMods = true;
        regenerateModSources = false;
        onlyNode = null;
        break;

    // Applies patches to a single workspace.
    case patch_patch:
        downloadDepots = false;
        decompileDepots = false;
        diffAllDepots = false;
        diffMods = false;
        patchMods = true;
        regenerateModSources = false;
        onlyNode = selectNode();
        break;

    // Gets clean and updated copies of depots, decompiles them, and applies
    // patches to mods.
    case regenerate_sources:
        downloadDepots = false;
        decompileDepots = false;
        diffAllDepots = false;
        diffMods = false;
        patchMods = true;
        regenerateModSources = true;
        onlyNode = null;
        break;

    default:
        throw new Exception("No task selected");
}

Environment.SetEnvironmentVariable("SKIP_DOWNLOAD", downloadDepots ? "0" : "1");
Environment.SetEnvironmentVariable("SKIP_DECOMPILATION", decompileDepots ? "0" : "1");
Environment.SetEnvironmentVariable("SKIP_DIFFING", diffAllDepots ? "0" : "1");
Environment.SetEnvironmentVariable("DIFF_MODS", diffMods ? "1" : "0");
Environment.SetEnvironmentVariable("PATCH_MODS", patchMods ? "1" : "0");
Environment.SetEnvironmentVariable("REGENERATE_MOD_SOURCES", regenerateModSources ? "1" : "0");
Environment.SetEnvironmentVariable("PATCH_FILE", args.Length >= 1 ? args[0] : null);
if (onlyNode is { } node)
    Environment.SetEnvironmentVariable("ONLY_NODE", node);

Tomat.Differ.Program.Main();

return;

string selectNode() {
    var nodes = patchesFile.Children.SelectMany(x => x.Children)
        .Select(x => x.WorkspaceName)
        .ToArray();

    var node = AnsiConsole.Prompt(
        new SelectionPrompt<string>().Title("Select node")
            .AddChoices(nodes)
    );

    return node;
}
