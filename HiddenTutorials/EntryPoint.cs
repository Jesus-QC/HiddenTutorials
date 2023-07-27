using System;
using System.Collections.Generic;
using System.Linq;
using MEC;
using Mirror;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using RelativePositioning;

namespace HiddenTutorials;

public class EntryPoint
{
	public const string Version = "1.0.0.1";
	
	[PluginAPI.Core.Attributes.PluginConfig] public static PluginConfig Config;

	[PluginEntryPoint("HiddenTutorials", Version, "Makes tutorials hidden to spectators", "Jesus-QC")]
	private void Init()
	{
		if (!Config.IsEnabled)
			return;
        
		Log.Raw($"<color=blue>Loading HiddenTutorials {Version} by Jesus-QC</color>");
		EventManager.RegisterEvents(this);
	}
	
	[PluginEvent(ServerEventType.PlayerChangeRole)]
	private void OnPlayerChangeRole(PlayerChangeRoleEvent ev)
	{
		switch (ev.NewRole)
		{
			case RoleTypeId.Tutorial:
				Timing.CallDelayed(0.1f, () =>
				{
					ChangeAppearance(ev.Player, RoleTypeId.Filmmaker, Player.GetPlayers().Where(ply => ply.Role is RoleTypeId.Spectator && ply != ev.Player));
				});
				break;

			case RoleTypeId.Spectator:
				Timing.CallDelayed(0.1f, () =>
				{
					foreach (Player ply in Player.GetPlayers().Where(ply => ply.Role is RoleTypeId.Tutorial && ply != ev.Player))
						ChangeAppearance(ply, RoleTypeId.Filmmaker, new []{ev.Player});
				});
				return;
		}
		
		if (ev.OldRole.RoleTypeId is not RoleTypeId.Spectator)
			return;

		foreach (Player ply in Player.GetPlayers())
		{
			if (ply.Role is not RoleTypeId.Tutorial || ply == ev.Player)
				continue;
			
			ChangeAppearance(ply, RoleTypeId.Tutorial, new []{ev.Player});
		}
	}
	
	// Method grabbed from exiled
	// -----------------------------------------------------------------------
	// <copyright file="MirrorExtensions.cs" company="Exiled Team">
	// Copyright (c) Exiled Team. All rights reserved.
	// Licensed under the CC BY-SA 3.0 license.
	// </copyright>
	// -----------------------------------------------------------------------
	private static void ChangeAppearance(Player player, RoleTypeId type, IEnumerable<Player> playersToAffect, byte unitId = 0)
	{
		if (player.Role is RoleTypeId.Spectator or RoleTypeId.Filmmaker or RoleTypeId.Overwatch)
			throw new InvalidOperationException("You cannot change a spectator into non-spectator via change appearance.");

		NetworkWriterPooled writer = NetworkWriterPool.Get();
		writer.WriteUShort(38952);
		writer.WriteUInt(player.NetworkId);
		writer.WriteRoleType(type);
		if (type.GetTeam() == Team.FoundationForces)
			writer.WriteByte(unitId);

		if (type != RoleTypeId.Spectator && player.RoleBase is IFpcRole fpc)
		{
			fpc.FpcModule.MouseLook.GetSyncValues(0, out ushort syncH, out _);
			writer.WriteRelativePosition(new RelativePosition(player.ReferenceHub.transform.position));
			writer.WriteUShort(syncH);
		}

		foreach (Player target in playersToAffect)
			target.Connection.Send(writer.ToArraySegment());
		
		NetworkWriterPool.Return(writer);
	}
}
