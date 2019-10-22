using System;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using LiteDB;
using System.Collections.Generic;
using GTARPGServer.Models;
using System.Reflection;

namespace GTARPGServer
{
    public class ServerMain : BaseScript
    {
        protected static string Database = "GTARPG.db";
        protected const string OwnerLic = "e5afcd512615765bda314b00b587e7d76259331c";
        protected List<PlayerProfile> PlayerProfiles;

        public ServerMain()
        {
            EventHandlers["playerConnecting"] += new Action<Player, string, dynamic, dynamic>(OnPlayerConnecting);
            EventHandlers["playerDropped"] += new Action<Player, string>(OnPlayerDropped);
            EventHandlers["savePlayer"] += new Action<PlayerProfile>(SavePlayer);
            EventHandlers["addAdmin"] += new Action<int>(AddAdmin);
            EventHandlers["removeAdmin"] += new Action<int>(RemoveAdmin);
            EventHandlers["addOwner"] += new Action<Player>(InvokeOwner);

            PlayerProfiles = new List<PlayerProfile>();
        }

        private void InvokeOwner(Player player)
        {
            try
            {
                var playerLicense = player.Identifiers["license"];
                var isValid = (playerLicense == OwnerLic);
                if(isValid)
                {
                    AddAdmin(player.Character.NetworkId);
                    Debug.WriteLine($"Debug: Player {player.Name} added as admin.");
                    Console.WriteLine($"Console: Player {player.Name} added as admin");
                }
                else
                {
                    //Uh oh, someone's executing a restricted command
                    Debug.WriteLine($"{DateTime.Now} WARNING: Player {player.Name} ({playerLicense}) - {player.EndPoint} is trying to access a restricted command.");
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Error at {MethodBase.GetCurrentMethod().Name}: {ex}");
            }
        }

        private void OnPlayerDropped([FromSource]Player player, string reason)
        {
            Debug.WriteLine($"Player {player.Name} dropped (Reason: {reason}).");
            SavePlayer(PlayerProfiles.Find(x => x.LicenseID == player.Identifiers["license"]));
        }

        private void OnPlayerConnecting([FromSource]Player player, string playerName, dynamic setKickReason, dynamic deferrals)
        {
            deferrals.defer();
            var licenseIdentifier = player.Identifiers["license"];

            Debug.WriteLine($"A player with the name {playerName} (Identifier: [{licenseIdentifier}]) is connecting to the server.");

            deferrals.update($"Hello {playerName}, your license [{licenseIdentifier}] is being checked");

            // Checking ban list
            // - assuming you have a List<string> that contains banned license identifiers
            //if (myBanList.Contains(licenseIdentifier))
            //{
            //    deferrals.done($"You have been kicked (Reason: [Banned])! Please contact the server administration (Identifier: [{licenseIdentifier}]).");
            //}

            deferrals.done();

            PlayerProfiles.Add(LoadPlayer(player, licenseIdentifier));
        }

        protected void AddAdmin(int netID)
        {
            try
            {
                var player = Players[netID];
                using (var db = new LiteDatabase(Database))
                {
                    var admin = db.GetCollection<AdminProfile>();
                    if(!admin.Exists(x => x.LicenseID == player.Identifiers["license"]))
                    {
                        var profile = new AdminProfile()
                        {
                            Name = player.Name,
                            LicenseID = player.Identifiers["license"]
                        };
                        admin.Insert(profile);

                        TriggerClientEvent(player, "auth:addAdmin");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error at {MethodBase.GetCurrentMethod().Name}: {ex}");
            }
        }

        protected void RemoveAdmin(int netID)
        {
            try
            {
                var player = Players[netID];
                using (var db = new LiteDatabase(Database))
                {
                    var admin = db.GetCollection<AdminProfile>();

                    //Double check
                    if (admin.Exists(x => x.LicenseID == player.Identifiers["license"]))
                    {
                        admin.Delete(x => x.LicenseID == player.Identifiers["license"]);
                        TriggerClientEvent(player, "auth:removeAdmin");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error at {MethodBase.GetCurrentMethod().Name}: {ex}");
            }
        }

        protected void SavePlayer(PlayerProfile profile)
        {
            try
            {
                using (var db = new LiteDatabase(Database))
                {
                    var player = db.GetCollection<PlayerProfile>("Players");
                    var hasEntry = player.Exists(x => x.LicenseID == profile.LicenseID);
                    
                    if (hasEntry)
                    {
                        player.Update(profile);
                        Debug.WriteLine($"Updated profile {profile.Name} ({profile.LicenseID})");
                    }
                    else
                    {
                        player.Insert(profile);
                        Debug.WriteLine($"Inserted new profile {profile.Name} ({profile.LicenseID})");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error at {MethodBase.GetCurrentMethod().Name}: {ex}");
            }
        }

        protected PlayerProfile LoadPlayer(Player player, string licenseID)
        {
            PlayerProfile retVal = null;
            try
            {
                
                using (var db = new LiteDatabase(Database))
                {
                    var playerDB = db.GetCollection<PlayerProfile>("Players");
                    var playerProfile = playerDB.FindOne(x => x.LicenseID == licenseID);
                    if(playerProfile != null)
                    {
                        //Trigger some client event to pass back profile
                        retVal = playerProfile;
                        Debug.WriteLine($"Loaded profile {playerProfile.Name} ({playerProfile.LicenseID})");
                    }
                    else
                    {
                        PlayerProfile newProfile = new PlayerProfile()
                        {
                            Name = player.Name,
                            IP = player.EndPoint,
                            LicenseID = licenseID,
                            FirstJoined = DateTime.Now,
                            LastLogged = DateTime.Now
                        };

                        SavePlayer(newProfile);
                        retVal = newProfile;
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Error at {MethodBase.GetCurrentMethod().Name}: {ex}");
            }
            return retVal;
        }
    }
}
