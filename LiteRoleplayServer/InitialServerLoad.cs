using System;
using System.Reflection;
using CitizenFX.Core;
using LiteRoleplayServer.Components.Admin;
using LiteRoleplayServer.Components.Clients;
using LiteRoleplayServer.Components.Utils;
using LiteRoleplay.Shared;
using static CitizenFX.Core.Native.API;

namespace LiteRoleplayServer
{
    public class InitialServerLoad : BaseScript
    {
        public InitialServerLoad()
        {
            //Built-in events
            EventHandlers["playerConnecting"] += new Action<Player, string, dynamic, dynamic>(OnPlayerConnecting);
            EventHandlers["playerDropped"] += new Action<Player, string>(OnPlayerDropped);

            //Player profile events
            EventHandlers[SharedProperties.EventLoadProfile] += new Action<Player>(ProfileActions.Instance.InitialProfileLoad);
            EventHandlers[SharedProperties.EventSaveProfile] += new Action<Player, dynamic>(ProfileActions.Instance.SavePlayerProfile);
            EventHandlers[SharedProperties.EventDepositWallet] += new Action<Player, int>(ProfileActions.Instance.DepositWallet);

            //Admin events
            EventHandlers[SharedProperties.EventInvokeOwnership] += new Action<Player>(Commands.Instance.InvokeOwnership);
            EventHandlers[SharedProperties.EventBanPlayer] += new Action<Player, int, string, int>(Commands.Instance.BanPlayer);
            EventHandlers[SharedProperties.EventUnbanPlayer] += new Action<Player, int>(Commands.Instance.UnbanPlayer);
            EventHandlers[SharedProperties.EventKickPlayer] += new Action<Player, int, string>(Commands.Instance.KickPlayer);
            EventHandlers[SharedProperties.EventFreezePlayer] += new Action<Player, int, int>(Commands.Instance.FreezePlayer);
            EventHandlers[SharedProperties.EventAddBan] += new Action<Player, string, string, int>(Commands.Instance.AddBan);

            EventHandlers[SharedProperties.EventGiveAdmin] += new Action<Player, int>(Commands.Instance.GiveAdmin);
            EventHandlers[SharedProperties.EventRemoveAdmin] += new Action<Player, int>(Commands.Instance.RemoveAdmin);

            //Notify that server is loaded
            Console.WriteLine($"[LiteRoleplay v{Assembly.GetExecutingAssembly().GetName().Version}][SERVER] - Loaded successfully.");
        }

        private void OnPlayerConnecting([FromSource]Player player, string playerName, dynamic setKickReason, dynamic deferrals)
        {
            deferrals.defer();

            //Get license
            var playerLicense = player.Identifiers["license"];
            var bannedProfile = Commands.Instance.IsPlayerBanned(playerLicense); 
            
            //Uh oh, looks like someone is banned
            if(bannedProfile != null)
            {
                //Check if its time to unban them yet.
                if(bannedProfile.BannedUntil < DateTime.UtcNow)
                {
                    Commands.Instance.UnbanPlayer(playerLicense);
                }
                else
                {
                    setKickReason = $"You have been banned until ({bannedProfile.BannedUntil.ToString("dd/MM/yyyy HH:mm:ss")}) for {bannedProfile.BanReason} by {bannedProfile.BannedBy}. Banned date: {bannedProfile.FirstBanDate}";
                    deferrals.done(setKickReason);
                    Console.WriteLine($"Player {player.Name}({player.EndPoint})-({player.Identifiers["license"]}) tried to join but is banned.");
                    return;
                }
            }

            //Load in info profile
            InfoActions.Instance.InitialInfoLoad(player);

            //Load in profile
            ProfileActions.Instance.InitialProfileLoad(player);

            //Notify
            ChatUtils.Instance.PrintToAll($"Player [{player.Name}] is joining the game.", SharedProperties.ColorNormal);
            Console.WriteLine($"Player {player.Name}({player.EndPoint})-({playerLicense}) is joining the game.");

            //Done
            deferrals.done();
        }

        private void OnPlayerDropped([FromSource]Player player, string reason)
        {
            //Notify
            ChatUtils.Instance.PrintToAll($"Player [{player.Name}] has left the game. (reason: {reason})", SharedProperties.ColorNormal);
            Console.WriteLine($"Player {player.Name}({player.EndPoint}) has left the game.");
        }
    }
}
