﻿using Microsoft.Win32;
using rail;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Terraria.Social;

namespace Terraria;

partial class WindowsLaunch
{
	private static void Main(string[] args)
	{
		Environment.SetEnvironmentVariable("FNA_WORKAROUND_WINDOW_RESIZABLE", "1");
		ResolveDependencies();
		Program.LaunchGame(args, monoArgs: true /*!OperatingSystem.IsWindows()*/);
	}

	internal static void InitializeWithParsedArguments(Dictionary<string, string> args)
	{
		bool dedServ = args.ContainsKey("-server");
		if (dedServ)
		{
			_handleRoutine = ConsoleCtrlCheck;
			SetConsoleCtrlHandler(_handleRoutine, add: true);
		}
	}

	private static void ResolveDependencies()
	{
		string platform = OperatingSystem.IsWindows()
			? "win"
			: OperatingSystem.IsMacOS()
				? "osx"
				: OperatingSystem.IsLinux()
					? "linux"
					: throw new PlatformNotSupportedException();

		// TODO: Support arm64/aarch64...
		string arch = Environment.Is64BitProcess ? "x64" : "x86";
		string path = Path.Combine("Libraries", "Natives", platform + '-' + arch);
		foreach (string file in Directory.GetFiles(path))
			NativeLibrary.Load(file);

		string managedPath = Path.Combine(path, "Managed");
		List<Assembly> dependentAssemblies = [];
		foreach (string file in Directory.GetFiles(managedPath))
			dependentAssemblies.Add(Assembly.LoadFrom(file));

		AppDomain.CurrentDomain.AssemblyResolve += (_, args) => {
			foreach (Assembly assembly in dependentAssemblies)
				if (assembly.FullName == args.Name)
					return assembly;

			return null;
		};
	}

	internal static string SocialModeToString(SocialMode mode)
	{
		return mode switch
		{
			SocialMode.None => "none",
			SocialMode.Steam => "steam",
			SocialMode.WeGame => "wegame",
			_ => throw new ArgumentOutOfRangeException(nameof(mode)),
		};
	}

	internal static SocialMode StringToSocialMode(string mode)
	{
		mode = mode.ToLower();
		return mode switch
		{
			"none" => SocialMode.None,
			"steam" => SocialMode.Steam,
			"wegame" => SocialMode.WeGame,
			_ => throw new InvalidOperationException($"Mode '{mode}' does not correspond to a known social mode."),
		};
	}

	internal static bool TryGetSocialMode(out SocialMode mode)
	{
		// Check possible launch arguments we've added that are used in
		// dedicated servers, but are applicable for regular clients.
		if (Program.LaunchParameters.ContainsKey("-steam"))
		{
			mode = SocialMode.Steam;
			return true;
		}

		if (Program.LaunchParameters.ContainsKey("-wegame"))
		{
			mode = SocialMode.WeGame;
			return true;
		}

		if (Program.LaunchParameters.ContainsKey("-none"))
		{
			mode = SocialMode.None;
			return true;
		}

		// If we can't detect the appropriate social mode that way, let's try
		// figuring it ourselves by initializing various social APIs and seeing
		// which one lands first.
		try
		{
			if (SteamAPI.Init())
			{
				mode = SocialMode.Steam;
				return true;
			}
		}
		catch (Exception)
		{
			// ignore
		}

		try
		{
			if (rail_api.RailInitialize())
			{
				mode = SocialMode.WeGame;
				return true;
			}
		}
		catch (Exception)
		{
			// ignore
		}

		// If that fails, we'll go with no social mode and return false.
		// The consumer of this API already sets the mode to None so we don't
		// care about this value.
		mode = SocialMode.None;
		return false;
	}

	internal static string GetContentDirectory()
	{
		TryGetSocialMode(out SocialMode mode);
		List<string> contentDirectories = [];

		contentDirectories.Add("Content");
		contentDirectories.Add(Path.Combine("..", "Content"));
		contentDirectories.Add(Path.Combine("..", "Terraria", "Content"));

		switch (mode)
		{
			case SocialMode.Steam:
				contentDirectories.AddRange(ResolveSteamContentDirectories().Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => Path.Combine(x, "Content")));
				break;
			case SocialMode.WeGame:
				contentDirectories.AddRange(ResolveWeGameContentDirectories().Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => Path.Combine(x, "Content")));
				break;
			case SocialMode.None:
				contentDirectories.AddRange(ResolveGogContentDirectories().Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => Path.Combine(x, "Content")));
				break;
		}

		foreach (string directory in contentDirectories)
		{
			if (Directory.Exists(directory))
				return directory;
		}

		return "Content";
	}

	private static IEnumerable<string> ResolveSteamContentDirectories()
	{
		if (OperatingSystem.IsWindows())
		{
			yield return Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Steam App 105600", "PATH", "") as string;
			string steamPath = Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Valve\\Steam", "SteamPath", "") as string;
			yield return Path.Combine(steamPath, "steamapps", "common", "Terraria");
			yield return "C:\\Program Files\\Steam\\steamapps\\common\\Terraria";
			yield return "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Terraria";
		}
		else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
		{
			string home = Environment.GetEnvironmentVariable("HOME");
			yield return Path.Combine(home, ".steam", "steam", "steamapps", "common", "Terraria");
			yield return Path.Combine(home, ".local", "share", "Steam", "steamapps", "common", "Terraria");
			yield return Path.Combine(home, ".var", "app", "com.valvesoftware.Steam", "data", "Steam", "steamapps", "common", "Terraria");
		}
	}

	private static IEnumerable<string> ResolveWeGameContentDirectories()
	{
		// TODO: WeGame content directory support.
		yield break;
	}

	private static IEnumerable<string> ResolveGogContentDirectories()
	{
		if (!OperatingSystem.IsWindows())
		{
			string home = Environment.GetEnvironmentVariable("HOME");
			yield return Path.Combine(home, "GOG Games", "Terraria", "game");
			yield break;
		}

		yield return Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\GOG.com\\Games\\1207665503", "PATH", "") as string;
		yield return "C:\\Program Files\\GalaxyClient\\Games\\Terraria";
		yield return "C:\\Program Files\\GOG Galaxy\\Games\\Terraria";
		yield return "C:\\Program Files\\GOG Games\\Terraria";
		yield return "C:\\Program Files (x86)\\GalaxyClient\\Games\\Terraria";
		yield return "C:\\Program Files (x86)\\GOG Galaxy\\Games\\Terraria";
		yield return "C:\\Program Files (x86)\\GOG Games\\Terraria";
	}
}
