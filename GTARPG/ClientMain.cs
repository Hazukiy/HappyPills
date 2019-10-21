using System;
using System.Collections.Generic;
using System.Text;
using CitizenFX.Core;
using System.Reflection;
using static CitizenFX.Core.Native.API;
using GTARPGClient.Models;
using System.Linq;
using System.Threading.Tasks;
using GTARPG.Logic;

namespace GTARPGClient
{
    public class ClientMain : BaseScript
    {
        public ClientMain()
        {
            EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);

            Tick += ClientMain_Tick;
        }

        private async Task ClientMain_Tick()
        {
            if(Spawning.Instance.CatProfiles != null)
            {
                foreach(var item in Spawning.Instance.CatProfiles.Where(x => x.IsFollow == true))
                {
                    item.Ped.Task.RunTo(item.Owner.Character.Position);
                }
            }

            await Delay(3000);
        }

        private void OnClientResourceStart(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;

            //Commands
            RegisterCommand("cat", new Action<int, List<object>, string>((source, args, raw) => { Spawning.Instance.Command_SpawnCat(source, args); }), false);
            RegisterCommand("getcats", new Action<int, List<object>, string>((source, args, raw) => { Spawning.Instance.Command_GetCats(source); }), false);
            RegisterCommand("car", new Action<int, List<object>, string>((source, args, raw) => { Spawning.Instance.Command_SpawnCar(source); }), false);
            RegisterCommand("sunnyday", new Action<int, List<object>, string>((source, args, raw) => { Admin.Instance.Command_SetSunnyAndDay(); }), false);
            RegisterCommand("antiannoy", new Action<int, List<object>, string>((source, args, raw) => { Admin.Instance.Command_NoAnnoy(source); }), false);
            RegisterCommand("getplayers", new Action<int, List<object>, string>((source, args, raw) => { Admin.Instance.Command_GetPlayers(); }), false);
            RegisterCommand("clearpolice", new Action<int, List<object>, string>((source, args, raw) => { Admin.Instance.Command_ClearPolice(source); }), false);
        }
    }
}
