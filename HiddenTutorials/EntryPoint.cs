using HarmonyLib;
using HiddenTutorials.API;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;

namespace HiddenTutorials;

// Thanks to xNexusACS for helping to test!
public class EntryPoint
{
	public const string Version = "1.0.0.4";
	
	[PluginAPI.Core.Attributes.PluginConfig] public static PluginConfig Config;

	private readonly Harmony _harmony = new ("com.jesusqc.hiddentutorials");
	
	[PluginEntryPoint("HiddenTutorials", Version, "Makes tutorials hidden to spectators", "Jesus-QC")]
	private void Init()
	{
		if (!Config.IsEnabled)
			return;
        
		_harmony.PatchAll();
		EventManager.RegisterEvents(this);
		Log.Raw($"<color=blue>Loading HiddenTutorials {Version} by Jesus-QC</color>");
	}
	
	[PluginEvent(ServerEventType.RoundRestart)]
	public void OnRoundRestart(RoundRestartEvent _)
	{
		HiddenTutorialsAPI.ClearData();
	}
}
