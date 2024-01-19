using HarmonyLib;
using PlayerRoles;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;

namespace HiddenTutorials;

// Thanks to xNexusACS for helping to test!
public class EntryPoint
{
	public const string Version = "1.0.0.3";
	
	[PluginAPI.Core.Attributes.PluginConfig] public static PluginConfig Config;

	private readonly Harmony _harmony = new ("com.jesusqc.hiddentutorials");
	
	[PluginEntryPoint("HiddenTutorials", Version, "Makes tutorials hidden to spectators", "Jesus-QC")]
	private void Init()
	{
		if (!Config.IsEnabled)
			return;
        
		_harmony.PatchAll();
		Log.Raw($"<color=blue>Loading HiddenTutorials {Version} by Jesus-QC</color>");
	}

	public static bool IsWhitelisted(ReferenceHub referenceHub) => referenceHub.authManager.RemoteAdminGlobalAccess || referenceHub.serverRoles.RemoteAdmin || referenceHub.roleManager.CurrentRole.RoleTypeId is RoleTypeId.Overwatch;
}
