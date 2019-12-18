using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteRoleplayClient.components.Models.Local
{
    public class ProfileModel
    {
        public int Id { get; set; }
        public string LicenseID { get; set; }
        public int Wallet { get; set; }
        public int Bank { get; set; }
        public int Salary { get; set; }
        public string Job { get; set; }
        public bool IsWanted { get; set; }

        public override string ToString()
        {
            return $"{Id}. Wallet: ${Wallet} \n Bank: ${Bank} \n Salary: ${Salary} \n Job: {Job} \n IsWanted: {IsWanted}";
        }
    }
}
