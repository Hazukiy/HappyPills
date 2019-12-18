using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace LiteRoleplayClient.components.Chat
{
    public class ChatUtils : BaseScript
    {
        #region Singleton & Constructor
        private static ChatUtils _instance;
        public static ChatUtils Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ChatUtils();
                }
                return _instance;
            }
        }
        private ChatUtils() { }

        #endregion
        public void PrintToChat(string printArgs, int[] chatColor)
        {
            TriggerEvent("chat:addMessage", new
            {
                color = chatColor,
                args = new[] { SharedProperties.ChatPrefix, $"{printArgs}" }
            });
        }
    }
}
