using System;
using System.Collections.Generic;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System.Linq;
using System.Threading.Tasks;
using GTARPG.Logic;
using GTARPG.Tools;
using GTARPGClient.Models;

namespace GTARPGClient
{
    public class ClientMain : BaseScript
    {
        public ClientMain()
        {
            EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
            EventHandlers["playerSpawned"] += new Action<Player>(OnClientSpawned);
            EventHandlers["auth:addAdmin"] += new Action(AdminAddCallback);
            EventHandlers["auth:removeAdmin"] += new Action(AdminRemoveCallback);

            Tick += ClientMain_Tick_CatBehaviour;
            Tick += ClientMain_Tick_CatHunger;
        }

        private void AdminRemoveCallback()
        {
            Admin.Instance.IsAdmin = false;
            Utils.Instance.PrintToChat("Your admin has been revoked.", ChatColor.Red);
        }

        private void AdminAddCallback()
        {
            Admin.Instance.IsAdmin = true;
            Utils.Instance.PrintToChat("You've been given admin.", ChatColor.Green);
        }

        private void OnClientSpawned([FromSource]Player player)
        {
            try
            {
                StartPlayerTeleport(new Player(GetPlayerIndex()).Handle, -1592.545f, 2098.692f, 67.76393f, 1.0f, false, true, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error at OnClientSpawned: {ex}");
            }
        }

        private async Task ClientMain_Tick_CatBehaviour()
        {
            try
            {
                if (Spawning.Instance.CatProfiles != null)
                {
                    foreach (var item in Spawning.Instance.CatProfiles.Where(x => x.IsFollow == true))
                    {
                        if(item.Hunger > 61)
                        {
                            item.Ped.Task.RunTo(item.Owner.Character.Position);
                        }
                        else if(item.Hunger >= 1 && item.Hunger <= 49)
                        {
                            item.IsFollow = false;
                        }
                        else if(item.Hunger == 0)
                        {
                            Utils.Instance.PrintToChat("Your cat died, you're a terrible owner. :-(", ChatColor.Red);
                            item.Ped.Kill();
                            Spawning.Instance.CatProfiles.Remove(item);
                        }
                    }
                }
                await Delay(2500);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error in Tick method: {ex}");
            }
        }

        private async Task ClientMain_Tick_CatHunger()
        {
            try
            {
                if (Spawning.Instance.CatProfiles != null)
                {
                    foreach (var item in Spawning.Instance.CatProfiles.Where(x => x.Hunger > 0))
                    {
                        item.Hunger--;
                    }
                }
                await Delay(1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Tick method: {ex}");
            }
        }

        private void OnClientResourceStart(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;

            ///===COMMANDS===\\\

            //Spawning Related
            RegisterCommand("spawncat", new Action<int, List<object>, string>((source, args, raw) => { Spawning.Instance.Command_SpawnCat(source, args); }), false);
            RegisterCommand("getcats", new Action<int, List<object>, string>((source, args, raw) => { Spawning.Instance.Command_GetCats(source); }), false);
            RegisterCommand("spawncar", new Action<int, List<object>, string>((source, args, raw) => { Spawning.Instance.Command_SpawnCar(source); }), false);

            
            //Admin Related
            RegisterCommand("sunnyday", new Action<int, List<object>, string>((source, args, raw) => { Admin.Instance.Command_SetSunnyAndDay(args); }), false);
            RegisterCommand("god", new Action<int, List<object>, string>((source, args, raw) => { Admin.Instance.Command_God(source, args); }), false);
            RegisterCommand("getplayers", new Action<int, List<object>, string>((source, args, raw) => { Admin.Instance.Command_GetPlayers(); }), false);
            RegisterCommand("clearpolice", new Action<int, List<object>, string>((source, args, raw) => { Admin.Instance.Command_ClearPolice(source); }), false);
            RegisterCommand("getpos", new Action<int, List<object>, string>((source, args, raw) => { Admin.Instance.Command_GetPosition(source); }), false);
            RegisterCommand("showid", new Action<int, List<object>, string>((source, args, raw) => { Admin.Instance.Command_ShowID(source); }), false);
            RegisterCommand("invokeowner", new Action<int, List<object>, string>((source, args, raw) => { Admin.Instance.Command_InvokeOwner(source); }), false);
            RegisterCommand("addadmin", new Action<int, List<object>, string>((source, args, raw) => { Admin.Instance.Command_AddAdmin(source); }), false);
            RegisterCommand("removeadmin", new Action<int, List<object>, string>((source, args, raw) => { Admin.Instance.Command_RemoveAdmin(source); }), false);
            RegisterCommand("isadmin", new Action<int, List<object>, string>((source, args, raw) => { Admin.Instance.Command_IsAdmin(); }), false);
        }
    }
}
