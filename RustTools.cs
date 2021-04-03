using Oxide.Core.Libraries.Covalence;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oxide.Plugins
{
    [Info("RustTools", "Dr.D.Bug", "0.0.1")]

    class RustTools : RustPlugin
    {
        public Dictionary<ulong, string> activplayers = new Dictionary<ulong, string>();

        void Init()
        {
            // Puts("RustTools geladen")
        }

        void SendMessage(BasePlayer player, string msg, params object[] args)
        {
            PrintToChat(player, msg, args);
        }

        [ChatCommand("players")]
        void player(BasePlayer player)
        {
            activplayers.Clear();
            StringBuilder sb = new StringBuilder();
            sb.Append("Players online: ");
            List<BasePlayer> list = BasePlayer.activePlayerList.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                BasePlayer basePlayer = list[i];
                // SendMessage(player, $"{basePlayer.displayName} ");
                sb.Append($"{basePlayer.displayName} ");
            }
            SendMessage(player, sb.ToString());
        }

    }
}
