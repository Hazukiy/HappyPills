using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteRoleplayServer.Models.Economy
{
    public class JobsModel
    {
        //Default
        public const string Unemployed = "Unemployed";

        //Authority jobs
        public const string President = "President";
        public const string Mayor = "Mayor";
        public const string PoliceChief = "Police Chief";
        public const string PoliceOfficer = "Police Officer";
        public const string BountyHunter = "Bounty Hunter";

        //Emergency jobs
        public const string Paramedic = "Paramedic";
        public const string Fireman = "Fireman";
        public const string Mechanic = "Mechanic"; 

        //Standard jobs
        public const string Chef = "Chef";
        public const string StreetCleaner = "Street Cleaner";
        public const string StreetRacer = "Street Racer";

        //Illigal jobs
        public const string DrugLord = "Drug Lord";
        public const string DrugMember = "Drug Member";
    }
}
