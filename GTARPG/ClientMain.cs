using System;
using System.Collections.Generic;
using System.Text;
using CitizenFX.Core;
using System.Reflection;
using static CitizenFX.Core.Native.API;

namespace GTARPGServer
{
    public class ClientMain : BaseScript
    {
        private const string Prefix = "[RPG]";

        public ClientMain()
        {
            EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
        }

        private void OnClientResourceStart(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;

            //Commands
            RegisterCommand("report", new Action<int, List<object>, string>((source, args, raw) => { CommandReport(args); }), false);
        }



        private static void PrintToChat(string printArgs, int[] printColor)
        {
            TriggerEvent("chatMessage", new
            {
                color = printColor,
                args = new[] { Prefix, $"{printArgs}" }
            });
        }
    }
}
