using System.Collections.Generic;
using HarmonyLib;
using MEC;
using Mirror;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using RelativePositioning;
using UnityEngine;

namespace HiddenTutorials;

// Thanks to xNexusACS for helping to test!
public class EntryPoint
{
	public const string Version = "1.0.0.2";
	
	[PluginAPI.Core.Attributes.PluginConfig] public static PluginConfig Config;

	private Harmony _harmony = new Harmony("com.jesusqc.hiddentutorials");
	
	[PluginEntryPoint("HiddenTutorials", Version, "Makes tutorials hidden to spectators", "Jesus-QC")]
	private void Init()
	{
		if (!Config.IsEnabled)
			return;
        
		Log.Raw($"<color=blue>Loading HiddenTutorials {Version} by Jesus-QC</color>");
		EventManager.RegisterEvents(this);
		
		_harmony.PatchAll();
	}
	
	[PluginEvent(ServerEventType.PlayerChangeSpectator)]
	private void OnPlayerChangeSpectator(PlayerChangeSpectatorEvent ev)
	{
		if (IsWhitelisted(ev.Player.ReferenceHub))
			return;
		
		if (ev.OldTarget?.Role is RoleTypeId.Tutorial)
			ResyncSpectator(ev.Player, ev.OldTarget);
		
		if (ev.NewTarget?.Role is not RoleTypeId.Tutorial)
			return;

		Timing.RunCoroutine(DesyncSpectator(ev.Player, ev.NewTarget));
	}
	
	// Contains code from:
	// -----------------------------------------------------------------------
	// <copyright file="MirrorExtensions.cs" company="Exiled Team">
	// Copyright (c) Exiled Team. All rights reserved.
	// Licensed under the CC BY-SA 3.0 license.
	// </copyright>
	// -----------------------------------------------------------------------
	private static void SendRoleAndPosition(Player spectator, Player target, RoleTypeId role, Vector3 position)
	{
		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteUShort(38952);
		writer.WriteUInt(target.NetworkId);
		writer.WriteRoleType(role);

		if (target.RoleBase is not IFpcRole fpc)
			return;
		
		fpc.FpcModule.MouseLook.GetSyncValues(0, out ushort syncH, out _);
		writer.WriteRelativePosition(new RelativePosition(position));
		writer.WriteUShort(syncH);
		
		spectator.Connection.Send(writer.ToArraySegment());
		NetworkWriterPool.Return(writer);
	}
	
	private static void ResyncSpectator(Player spectator, Player target)
	{
		SendRoleAndPosition(spectator, target, target.Role, Vector3.zero);
	}

	private static IEnumerator<float> DesyncSpectator(Player spectator, Player target)
	{
		yield return Timing.WaitForOneFrame;
		SendRoleAndPosition(spectator, target, RoleTypeId.Spectator, Vector3.zero);
	}

	public static bool IsWhitelisted(ReferenceHub referenceHub) => referenceHub.authManager.RemoteAdminGlobalAccess || referenceHub.serverRoles.RemoteAdmin;
}
