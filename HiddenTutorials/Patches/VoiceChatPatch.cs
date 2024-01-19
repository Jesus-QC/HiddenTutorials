using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using HiddenTutorials.API;
using NorthwoodLib.Pools;
using PlayerRoles;
using PlayerRoles.Spectating;
using PlayerRoles.Voice;

namespace HiddenTutorials.Patches;

[HarmonyPatch(typeof(SpectatorVoiceModule), nameof(SpectatorVoiceModule.ValidateReceive))]
public class VoiceChatPatch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

        Label skip = generator.DefineLabel();

        newInstructions[0].labels.Add(skip);
        
        newInstructions.InsertRange(0, new CodeInstruction[]
        {
            new (OpCodes.Ldarg_0),
            new (OpCodes.Ldarg_1),
            new (OpCodes.Call, AccessTools.Method(typeof(VoiceChatPatch), nameof(CheckProximity))),
            new (OpCodes.Brfalse_S, skip),
            new (OpCodes.Ldc_I4_0),
            new (OpCodes.Ret),
        });
        
        foreach (CodeInstruction instruction in newInstructions)
            yield return instruction;

        ListPool<CodeInstruction>.Shared.Return(newInstructions);
    }
    
    private static bool CheckProximity(VoiceModuleBase module, ReferenceHub speaker)
    {
        if (HiddenTutorialsAPI.IsWhitelistedTutorial(speaker))
            return false;
        
        if (speaker.roleManager.CurrentRole.RoleTypeId != RoleTypeId.Tutorial)
            return false;
        
        if (HiddenTutorialsAPI.IsWhitelistedSpectator(module.Owner))
            return false;

        if (module.Role is not SpectatorRole spectatorRole)
            return false;
        
        return spectatorRole.SyncedSpectatedNetId == speaker.netId;
    }
}