using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using UnityEngine;
using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("RustStructureBuilder", "Dr.D.Bug", "1.0.0")]
    [Description("Baut Pyramiden, Dächer und Foundation-Türme auf Kommando.")]
    public class RustStructureBuilder : RustPlugin
    {
        [Command("pyramid")]
        private void CmdPyramid(BasePlayer player, string command, string[] args)
        {
            int baseCount = args.Length > 0 ? Mathf.Clamp(int.Parse(args[0]), 2, 10) : 5;
            float size = 3f;
            Vector3 startPos = player.transform.position + player.transform.forward * 2f;

            for (int layer = 0; layer < baseCount; layer++)
            {
                int pieces = baseCount - layer;
                float y = layer * size * Mathf.Sqrt(3f) / 2f;
                for (int i = 0; i < pieces; i++)
                {
                    float angle = i * 360f / pieces;
                    Vector3 pos = startPos + Quaternion.Euler(0, angle, 0) * (Vector3.forward * size * layer);
                    pos.y += y;
                    SpawnEntity("assets/prefabs/building/triangle.foundation/triangle.foundation.prefab", pos, Quaternion.identity);
                }
            }
            SendReply(player, $"Pyramide mit {baseCount} Basis erstellt.");
        }

        [Command("roof")]
        private void CmdRoof(BasePlayer player, string command, string[] args)
        {
            int layers = args.Length > 0 ? Mathf.Clamp(int.Parse(args[0]), 1, 10) : 3;
            float step = 2.5f;
            Vector3 startPos = player.transform.position + player.transform.forward * 2f;
            for (int layer = 0; layer < layers; layer++)
            {
                float y = -layer * step;
                Vector3 pos = startPos + Vector3.up * y;
                SpawnEntity("assets/prefabs/building/roof.triangle/roof.triangle.prefab", pos, Quaternion.identity);
            }
            SendReply(player, $"Dachfläche mit {layers} Schichten erstellt.");
        }

        [Command("foundationtower")]
        private void CmdFoundationTower(BasePlayer player, string command, string[] args)
        {
            int height = args.Length > 0 ? Mathf.Clamp(int.Parse(args[0]), 1, 20) : 5;
            float step = 3f;
            Vector3 startPos = player.transform.position + player.transform.forward * 2f;
            for (int i = 0; i < height; i++)
            {
                float y = -i * step;
                Vector3 pos = startPos + Vector3.up * y;
                SpawnEntity("assets/prefabs/building/block/foundation/foundation.prefab", pos, Quaternion.identity);
            }
            SendReply(player, $"Foundation-Turm mit {height} Höhe erstellt.");
        }

        private void SpawnEntity(string prefab, Vector3 position, Quaternion rotation)
        {
            var entity = GameManager.server.CreateEntity(prefab, position, rotation);
            if (entity == null) return;
            entity.Spawn();
        }
    }
}
