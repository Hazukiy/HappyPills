﻿using System;
using CitizenFX.Core;
using LiteDB;
using LiteRoleplay.Shared;
using LiteRoleplayServer.Components.Utils;

namespace LiteRoleplayServer.Components.Clients
{
    public class ProfileActions : BaseScript
    {
        #region Singleton & Constructor
        private static ProfileActions _instance;
        public static ProfileActions Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ProfileActions();
                }
                return _instance;
            }
        }
        private ProfileActions() {}
        #endregion

        /// <summary>
        /// Initial event to load player.
        /// </summary>
        /// <param name="player"></param>
        public void InitialProfileLoad([FromSource]Player player)
        {
            var playerLicense = player.Identifiers["license"];
            if (DoesProfileExist(player))
            {
                using (var db = new LiteDatabase(SharedProperties.DatabaseName))
                {
                    var col = db.GetCollection<ProfileModel>(SharedProperties.DatabaseTableProfile);
                    var playerProfile = col.FindOne(x => x.LicenseID.Equals(playerLicense));
                    if (playerProfile != null)
                    {
                        TriggerClientEvent(player, SharedProperties.ProfileCallback, playerProfile);
                    }
                }
            }
            else
            {
                InsertNewProfile(player, playerLicense);
            }
        }

        /// <summary>
        /// Saves the player's profile
        /// </summary>
        /// <param name="player"></param>
        /// <param name="profile"></param>
        public void SavePlayerProfile([FromSource]Player player, dynamic profile)
        {
            var playerProfile = ConvertToProfile(profile);
            if(DoesProfileExist(player) && playerProfile != null)
            {
                using (var db = new LiteDatabase(SharedProperties.DatabaseName))
                {
                    var col = db.GetCollection<ProfileModel>(SharedProperties.DatabaseTableProfile);
                    col.Update(playerProfile);
                    Console.WriteLine($"Profile saved = {playerProfile}");
                }
            }
        }

        public void DepositWallet([FromSource]Player player, int amount)
        {
            var playerLicense = player.Identifiers["license"];
            using (var db = new LiteDatabase(SharedProperties.DatabaseName))
            {
                var col = db.GetCollection<ProfileModel>(SharedProperties.DatabaseTableProfile);
                var playerProfile = col.FindOne(x => x.LicenseID.Equals(playerLicense));
                if (playerProfile != null)
                {
                    playerProfile.Bank += amount;
                    playerProfile.Wallet -= amount;
                    col.Update(playerProfile);

                    //Update local profile
                    TriggerClientEvent(player, SharedProperties.ProfileCallback, playerProfile);

                    //Notify (TODO: Formatting on number i.e. 1,000,000 instead of 1000000)
                    ChatUtils.Instance.PrintToClient(player, $"You've deposited ${amount}.", SharedProperties.ColorGood);
                }
            }
        }

        public void WithdrawBank([FromSource]Player player, int amount)
        {

        }
        #region Private Methods
        /// <summary>
        /// Returns true if player already has a profile
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private bool DoesProfileExist(Player player)
        {
            var retVal = false;
            var playerLicense = player.Identifiers["license"];
            using (var db = new LiteDatabase(SharedProperties.DatabaseName))
            {
                var col = db.GetCollection<ProfileModel>(SharedProperties.DatabaseTableProfile);
                retVal = col.Exists(x => x.LicenseID.Equals(playerLicense));
            }
            return retVal;
        }

        /// <summary>
        /// Insert a new profile if it doesn't exist
        /// </summary>
        /// <param name="player"></param>
        /// <param name="id"></param>
        private void InsertNewProfile(Player player, string id)
        {
            using (var db = new LiteDatabase(SharedProperties.DatabaseName))
            {
                var col = db.GetCollection<ProfileModel>(SharedProperties.DatabaseTableProfile);

                //Construct model
                ProfileModel newProfile = new ProfileModel()
                {
                    LicenseID = id,
                    Wallet = SharedProperties.DefaultWallet,
                    Bank = SharedProperties.DefaultBank,
                    Salary = SharedProperties.DefaultSalary,
                    Job = SharedProperties.DefaultJob,
                    IsWanted = false,
                    IsAdmin = false
                };

                //Insert new profile
                col.Insert(newProfile);

                //Notify
                Console.WriteLine($"New profile inserted = {newProfile}");
                TriggerClientEvent(player, SharedProperties.ProfileCallback, newProfile);
            }
        }

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
                IsWanted = obj.IsWanted,
                IsAdmin = obj.IsAdmin
            };
        }
        #endregion
    }
}
