using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;

namespace GTARPG.Tools
{
    public class Utils : BaseScript
    {
        private const string Prefix = "[RPG]";
        private static Utils _instance;

        public static Utils Instance 
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new Utils();
                }
                return _instance;
            }
        }

        private Utils()
        {
        }

        public void PrintToChat(string printArgs, int[] printColor)
        {
            TriggerEvent("chat:addMessage", new
            {
                color = printColor,
                args = new[] { Prefix, $"{printArgs}" }
            });
        }
    }
}
