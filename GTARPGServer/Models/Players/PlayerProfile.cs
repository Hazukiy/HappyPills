using CitizenFX.Core;
using System;

namespace GTARPGServer.Models
{
    public class PlayerProfile
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string IP { get; set; }

        public string LicenseID { get; set; }

        public DateTime FirstJoined { get; set; }

        public DateTime LastLogged { get; set; }
    }
}
