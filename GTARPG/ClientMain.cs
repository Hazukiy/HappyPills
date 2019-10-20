using System;
using System.Collections.Generic;
using System.Text;
using CitizenFX.Core;
using System.Reflection;
using static CitizenFX.Core.Native.API;
using GTARPGClient.Models;
using System.Linq;
using System.Threading.Tasks;

namespace GTARPGClient
{
    public class ClientMain : BaseScript
    {
        private const string Prefix = "[RPG]";
        private List<CatProfile> catProfiles;

        public ClientMain()
        {
            EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);

            Tick += ClientMain_Tick;

            //Normal loading
            catProfiles = new List<CatProfile>();
        }

        private async Task ClientMain_Tick()
        {
            if(catProfiles != null)
            {
                foreach(var item in catProfiles.Where(x => x.IsFollow == true))
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
            RegisterCommand("cat", new Action<int, List<object>, string>((source, args, raw) => { Command_SpawnCat(source, args); }), false);
            RegisterCommand("getcats", new Action<int, List<object>, string>((source, args, raw) => { Command_GetCats(source); }), false);
            RegisterCommand("car", new Action<int, List<object>, string>((source, args, raw) => { Command_SpawnCar(source); }), false);
            RegisterCommand("sunnyday", new Action<int, List<object>, string>((source, args, raw) => { Command_SetSunnyAndDay(); }), false);
            RegisterCommand("antiannoy", new Action<int, List<object>, string>((source, args, raw) => { Command_NoAnnoy(source); }), false);
            RegisterCommand("getplayers", new Action<int, List<object>, string>((source, args, raw) => { Command_GetPlayers(); }), false);
            RegisterCommand("clearpolice", new Action<int, List<object>, string>((source, args, raw) => { Command_ClearPolice(source); }), false);
        }

        private void Command_ClearPolice(int source)
        {
            var player = new Player(source);
            player.WantedLevel = 0;
            PrintToChat("Wanted level cleared.", ChatColor.Green);
        }

        private void Command_GetPlayers()
        {
            var counter = 1;
            PrintToChat($"=====PlayerList=====", ChatColor.Red);
            foreach (Player item in Players)
            {
                PrintToChat($"{counter}.{item.Name}(NID:{item.Character.NetworkId}) SID:{item.ServerId}", ChatColor.Green);
                counter++;
            }
        }

        private void Command_NoAnnoy(int source)
        {
            var player = new Player(source);
            player.Character.CanBeKnockedOffBike = false;
            player.Character.CanBeDraggedOutOfVehicle = false;
            player.Character.IsInvincible = true;
            player.Character.Weapons.Give(WeaponHash.AdvancedRifle, 1000, true, true);
            PrintToChat("Anti-annoy now on.", ChatColor.Green);
        }

        private void Command_SetSunnyAndDay()
        {
            NetworkOverrideClockTime(10, 00, 00);

            World.Weather = Weather.ExtraSunny;
            World.TransitionToWeather(Weather.ExtraSunny, 10f);
            PrintToChat($"Weather set to sunny and time set to 10AM", ChatColor.Green);
        }

        private void Command_SpawnCar(int source)
        {
            var car = new Model(VehicleHash.T20);
            var player = new Player(source);
            var result = World.CreateVehicle(car, player.Character.Position + 5);
            if (result.IsCompleted)
            {
                var ownedCar = result.Result;
                ownedCar.IsInvincible = true;

                PrintToChat($"Your new T20 has spawned!", ChatColor.Green);
            }

            if (result.IsFaulted || result.IsCanceled)
            {
                PrintToChat("There was an issue spawning a car.", ChatColor.Red);
            }
        }

        private void Command_GetCats(int source)
        {
            var player = new Player(source);
            var itemNo = 1;
            foreach(var item in catProfiles.Where(x => x.Owner == player))
            {
                PrintToChat($"{itemNo}.{item.Name} - {item.Age} years old. Has {item.Hunger} hunger left.", ChatColor.Blue);
                itemNo++;
            }
        }

        private void Command_SpawnCat(int source, List<object> args)
        {
            try
            {
                if(args.Count != 1)
                {
                    PrintToChat($"Command usage: /cat <catname>", ChatColor.Red);
                    return;
                }

                var mdl = new Model(PedHash.Cat);
                var player = new Player(source);
                var playerPos = player.Character.Position;
                playerPos[0] = playerPos[0] += 4.0f;

                var result = World.CreatePed(mdl, playerPos);
                if (result.IsCompleted)
                {
                    var cat = result.Result;
                    cat.CanBeTargetted = false;
                    cat.IsInvincible = true;

                    var newCat = new CatProfile()
                    {
                        Owner = player,
                        Ped = cat,
                        Name = args[0].ToString(),
                        Age = 1,
                        Hunger = 100,
                        IsFollow = true

                    };
                    catProfiles.Add(newCat);
                    PrintToChat($"{newCat.Name} has spawned!", ChatColor.Green);
                }

                if (result.IsFaulted || result.IsCanceled)
                {
                    PrintToChat("There was an issue spawning.", ChatColor.Red);
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Error at {MethodBase.GetCurrentMethod().Name}: {ex}");
            }
        }

        private static void PrintToChat(string printArgs, int[] printColor)
        {
            TriggerEvent("chat:addMessage", new
            {
                color = printColor,
                args = new[] { Prefix, $"{printArgs}" }
            });
        }
    }
}
