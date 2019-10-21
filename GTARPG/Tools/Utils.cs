//Class Definition: Used for extra useful tools in the project

using System;
using System.Reflection;
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
            try
            {
                TriggerEvent("chat:addMessage", new
                {
                    color = printColor,
                    args = new[] { Prefix, $"{printArgs}" }
                });
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error at {MethodBase.GetCurrentMethod().Name}: {ex}");
            }
        }
    }
}
