using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using CitizenFX.Core;
using LiteRoleplayClient.components.Chat;
using LiteRoleplayClient.components.Models.Local;
using static CitizenFX.Core.Native.API;

namespace LiteRoleplayClient
{
    public class InitialLoad : BaseScript
    {
        //Local copy of player profile
        private ProfileModel PlayerProfile { get; set; }

        //Local timer
        private int PlayerSalaryTimer { get; set; }

        public InitialLoad()
        {
            //Built-in events
            EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
            EventHandlers["playerSpawned"] += new Action(OnPlayerSpawned);

            //Custom events
            EventHandlers[SharedProperties.ProfileCallback] += new Action<ProfileModel>(OnPlayerProfileCallback);

            //Set salary timer
            PlayerSalaryTimer = SharedProperties.SalaryTimer;

            //Start timers
            Tick += PlayerSalaryTick;

            //Load profile
            TriggerServerEvent(SharedProperties.EventLoadProfile);

            //Notify
            Console.WriteLine($"[LiteRoleplay v{Assembly.GetExecutingAssembly().GetName().Version}][CLIENT] - Loaded successfully.");
        }

        #region Events
        private void OnClientResourceStart(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;

            //Player commands
            RegisterCommand("profile", new Action<int, List<object>, string>((source, args, raw) => { Command_GetProfile(); }), false);


            //Admin commands
            RegisterCommand("ban", new Action<int, List<object>, string>((source, args, raw) => { Command_BanPlayer(args); }), false);
            RegisterCommand("unban", new Action<int, List<object>, string>((source, args, raw) => { Command_UnBanPlayer(args); }), false);
            RegisterCommand("invokeowner", new Action<int, List<object>, string>((source, args, raw) => { Command_InvokeOwner(); }), false);
            RegisterCommand("players", new Action<int, List<object>, string>((source, args, raw) => { Command_GetOnline(); }), false);
        }

        private void OnPlayerSpawned()
        {
            StartPlayerTeleport(new Player(GetPlayerIndex()).Handle, -1329.704f, -1512.436f, 4.379375f, 1.0f, false, true, false);
        }
        #endregion

        #region Custom Events
        public void OnPlayerProfileCallback(dynamic profile)
        {
            if (profile != null && PlayerProfile == null)
            {
                PlayerProfile = ConvertToProfile(profile);
                Debug.WriteLine("Profile loaded from callback.");
                ChatUtils.Instance.PrintToChat($"Profile Loaded: {PlayerProfile}", SharedProperties.ColorGood);
            }
            else
            {
                //Do some trying logic here...
                Debug.WriteLine("Critial error - failed to load profile.");
                ChatUtils.Instance.PrintToChat("Failed to load profile", SharedProperties.ColorError);
            }
        }
        #endregion

        #region Timers
        private async Task PlayerSalaryTick()
        {
            if(PlayerProfile != null)
            {
                if(PlayerSalaryTimer > 0)
                {
                    PlayerSalaryTimer--;
                }
                else if(PlayerSalaryTimer == 0)
                {
                    //Give player money
                    PlayerProfile.Wallet += PlayerProfile.Salary;

                    //Save
                    TriggerServerEvent(SharedProperties.EventSaveProfile, PlayerProfile);

                    //Notify
                    ChatUtils.Instance.PrintToChat($"You've received ${PlayerProfile.Salary} from your salary!", SharedProperties.ColorGood);

                    //Reset
                    PlayerSalaryTimer = SharedProperties.SalaryTimer;
                }
                await Delay(1000);
            }
        }

        #endregion

        #region Commands 
        private void Command_BanPlayer(List<object> args)
        {
            if(args.Count == 3)
            {
                var netID = Convert.ToInt32(args[0]);
                var reason = args[1];
                var hours = Convert.ToInt32(args[2]);

                TriggerServerEvent(SharedProperties.EventBanPlayer, new[] { netID, reason, hours });
            }
            else
            {
                ChatUtils.Instance.PrintToChat("Usage: /ban <netid> 'reason' <hours>", SharedProperties.ColorWarning);
            }
        }

        private void Command_UnBanPlayer(List<object> args)
        {
            if (args.Count == 1)
            {
                var dbID = Convert.ToInt32(args[0]);
                TriggerServerEvent(SharedProperties.EventUnbanPlayer, new[] { dbID });
            }
            else
            {
                ChatUtils.Instance.PrintToChat("Usage: /unban <dbID>", SharedProperties.ColorWarning);
            }
        }

        private void Command_InvokeOwner()
        {
            TriggerServerEvent(SharedProperties.EventInvokeOwnership);
        }

        private void Command_GetProfile()
        {
            if(PlayerProfile != null)
            {
                ChatUtils.Instance.PrintToChat($"{PlayerProfile.ToString()}", SharedProperties.ColorGood);
            }
            else
            {
                ChatUtils.Instance.PrintToChat($"Profile was null.", SharedProperties.ColorError);
            }
        }

        //TODO: Handle this request at server side?
        private void Command_GetOnline()
        {
            foreach (var player in Players)
            {
                ChatUtils.Instance.PrintToChat($"Player {player.Name} (Handle: {player.Handle}, ServerID: {player.ServerId}, NetID: {player.Character.NetworkId})", SharedProperties.ColorNormal);
            }
        }
        #endregion

        #region Private Methods
        private ProfileModel ConvertToProfile(dynamic obj)
        {
            return new ProfileModel()
            {
                Id = obj.Id,
                LicenseID = obj.LicenseID,
                Wallet = obj.Wallet,
                Bank = obj.Bank,
                Salary = obj.Salary,
                Job = obj.Job,
                IsWanted = obj.IsWanted
            };
        }
        #endregion
    }
}
