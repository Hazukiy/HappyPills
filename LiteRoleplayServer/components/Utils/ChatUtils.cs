using CitizenFX.Core;
using LiteRoleplay.Shared;

namespace LiteRoleplayServer.Components.Utils
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

        /// <summary>
        /// Prints a message to a client
        /// </summary>
        /// <param name="message"></param>
        /// <param name="chatColor"></param>
        public void PrintToClient(Player player, string message, int[] chatColor)
        {
            TriggerClientEvent(player, "chat:addMessage", new
            {
                color = chatColor,
                args = new[] { SharedProperties.ChatPrefix, $"{message}" }
            });
        }

        /// <summary>
        /// Prints a message to all players.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="chatColor"></param>
        public void PrintToAll(string message, int[] chatColor)
        {
            TriggerClientEvent("chat:addMessage", new
            {
                color = chatColor,
                args = new[] { SharedProperties.ChatPrefix, $"{message}" }
            });
        }
    }
}
