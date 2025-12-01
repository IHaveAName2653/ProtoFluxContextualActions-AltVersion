/*using Elements.Core;
using Newtonsoft.Json;
#if DEBUG
using ResoniteHotReloadLib;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;



namespace ProtoFluxContextualActions;


// This only affects Debug
internal class AutoUpdater
{
#if DEBUG
	public static string current_release = "v1.0.0";
	public static TimeSpan CheckerTimer = new(0, 1, 0);
	public static Uri fileDownloadPath = new Uri("https://github.com/IHaveAName2653/ProtoFluxContextualActions-AltVersion/releases/latest/download/ProtoFluxContextualActions-Debug.dll");
	public static HttpClient endpointClient = new();
	static readonly string modPath = Path.Combine(Directory.GetCurrentDirectory(), "rml_mods", "HotReloadMods", "ProtoFluxContextualActions-Debug.dll");
#endif
	public static void OnInit()
	{
#if DEBUG
		CheckLoop();
#endif
	}

	public static async void CheckLoop()
	{
#if DEBUG
		while (true)
		{
			await Task.Delay(CheckerTimer);
			CheckForUpdate();
		}
#endif
	}

	public static async void CheckForUpdate()
	{
#if DEBUG
		var res = await endpointClient.GetStringAsync("https://api.github.com/repos/IHaveAName2653/ProtoFluxContextualActions-AltVersion/releases/latest");
		var data = JsonConvert.DeserializeObject<GithubApiResponse>(res);
		if (data.tag_name != current_release)
		{
			HasUpdated();
		}
#endif
	}

	public static async void HasUpdated()
	{
#if DEBUG
		await endpointClient.DownloadFileTaskAsync(fileDownloadPath, modPath);
		HotReloader.HotReload(typeof(ProtoFluxContextualActions));
#endif
	}

	
}

#if DEBUG
public static class HttpClientUtils
{
	public static async Task DownloadFileTaskAsync(this HttpClient client, Uri uri, string FileName)
	{
		using (var s = await client.GetStreamAsync(uri))
		{
			using (var fs = new FileStream(FileName, FileMode.CreateNew))
			{
				await s.CopyToAsync(fs);
			}
		}
	}
}

public struct GithubApiResponse
{
	public string tag_name;
	public string name;
}

#endif*/