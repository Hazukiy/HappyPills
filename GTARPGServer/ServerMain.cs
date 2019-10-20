using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace GTARPGServer
{
    public class ServerMain : BaseScript
    {
        public ServerMain()
        {
            //EventHandlers["BanPlayer"] += new Action<Player, string>(Event_BanPlayer);
        }

        //public void Event_BanPlayer(Player target, string reason)
        //{
        //    TempBanPlayer(target.Name, reason);
        //}
    }
}
