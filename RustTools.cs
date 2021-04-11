// using System;
// using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oxide.Plugins
{
    [Info("RustTools", "Dr.D.Bug", "0.1.4")]
    [Description("Collection of Chat-Commands - neither config nor perms required")]
    // Minor Version 1 (1st release)
    // Chamge-Log: 0.1.4: Players() - Concatenation with Separator and Trim by the end of the line by separator
    class RustTools : RustPlugin
    {
        public Dictionary<ulong, string> activplayers = new Dictionary<ulong, string>();

        static void Init()
        {
        }

        void SendMessage(BasePlayer player, string msg, params object[] args)
        {
            PrintToChat(player, msg, args);
        }

        [ChatCommand("players")]
        void Players(BasePlayer player)
        {
            char[] separator = {',', ' '};
            activplayers.Clear();
            StringBuilder sb = new StringBuilder();
            List<BasePlayer> list = BasePlayer.activePlayerList.ToList();
            sb.Append($"Players online({list.Count}): ");
            for (int i = 0; i < list.Count; i++)
            {
                BasePlayer basePlayer = list[i];
                sb.Append($" {basePlayer.displayName},");
            }
            SendMessage(player, sb.ToString().TrimEnd(separator));
        }

    }
}
