using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using GTARPG.Tools;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using GTARPGClient.Models;

namespace GTARPG.Logic
{
    public class Spawning : BaseScript
    {
        private static Spawning _instance;
        public static Spawning Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new Spawning();
                }
                return _instance;
            }
        }

        public List<CatProfile> CatProfiles { get; set; }

        private Spawning()
        {
            CatProfiles = new List<CatProfile>();
        }

        public void Command_GetCats(int source)
        {
            try
            {
                var counter = 1;
                Utils.Instance.PrintToChat($"=====Your Pets=====", ChatColor.Red);
                foreach (var item in CatProfiles.Where(x => x.Owner == new Player(source)))
                {
                    Utils.Instance.PrintToChat($"{counter}.{item.Name} - {item.Age} years old. Has {item.Hunger} hunger left.", ChatColor.Blue);
                    counter++;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error at {MethodBase.GetCurrentMethod().Name}: {ex}");
            }
        }

        public void Command_SpawnCar(int source)
        {
            try
            {
                if (!Admin.Instance.IsAdmin) return;

                var player = new Player(source);
                var result = World.CreateVehicle(new Model(VehicleHash.T20), player.Character.Position + 5);
                if (result.IsCompleted)
                {
                    //Godmode the car
                    result.Result.IsInvincible = true;
                    result.Result.Mods.PrimaryColor = VehicleColor.MatteRed;
                    result.Result.Mods.SecondaryColor = VehicleColor.MatteRed;
                    result.Result.Mods.LicensePlate = $"ADMINCAR";
                    Utils.Instance.PrintToChat("Your new admin [T20] has spawned!", ChatColor.Green);
                }

                if (result.IsFaulted || result.IsCanceled)
                {
                    Utils.Instance.PrintToChat("There was an issue spawning a car.", ChatColor.Red);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error at {MethodBase.GetCurrentMethod().Name}: {ex}");
            }
        }

        public void Command_SpawnCat(int source, List<object> args)
        {
            try
            {
                var arguments = args.OfType<string>();
                if(arguments.Count() == 1)
                {
                    var player = new Player(source);
                    var result = World.CreatePed(new Model(PedHash.Cat), player.Character.Position + 4);

                    if (result.IsCompleted)
                    {
                        var cat = result.Result;
                        cat.CanBeTargetted = false;
                        cat.IsInvincible = true;

                        var newCat = new CatProfile()
                        {
                            Owner = player,
                            Ped = cat,
                            Name = arguments.FirstOrDefault(),
                            Age = 1,
                            Hunger = 100,
                            IsFollow = true
                        };
                        CatProfiles.Add(newCat);
                        Utils.Instance.PrintToChat($"[{newCat.Name}] has spawned!", ChatColor.Green);
                    }

                    if (result.IsFaulted || result.IsCanceled)
                    {
                        Utils.Instance.PrintToChat("There was an issue spawning.", ChatColor.Red);
                    }
                }
                else
                {
                    Utils.Instance.PrintToChat("Command Usage: /spawncat 'CatNameHere'", ChatColor.Red);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error at {MethodBase.GetCurrentMethod().Name}: {ex}");
            }
        }
    }
}
