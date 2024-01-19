using System.Collections.Generic;
using PlayerRoles;

namespace HiddenTutorials.API;

public static class HiddenTutorialsAPI
{
    /// <summary>
    /// Tutorials that can be spectated by anyone.
    /// </summary>
    public static readonly HashSet<ReferenceHub> WhitelistedTutorials = [];
    
    /// <summary>
    /// Spectators that can spectate any tutorial.
    /// </summary>
    public static readonly HashSet<ReferenceHub> WhitelistedSpectators = [];
    
    public static bool IsWhitelistedSpectator(ReferenceHub referenceHub)
        => referenceHub.authManager.RemoteAdminGlobalAccess 
           || referenceHub.serverRoles.RemoteAdmin 
           || referenceHub.roleManager.CurrentRole.RoleTypeId is RoleTypeId.Overwatch
           || HiddenTutorialsAPI.WhitelistedSpectators.Contains(referenceHub);

    public static bool IsWhitelistedTutorial(ReferenceHub referenceHub)
        => HiddenTutorialsAPI.WhitelistedTutorials.Contains(referenceHub);

    internal static void ClearData()
    {
        WhitelistedSpectators.Clear();
        WhitelistedTutorials.Clear();
    }
}