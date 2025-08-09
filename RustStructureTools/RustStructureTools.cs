using UnityEngine;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("RustStructureTools", "Dr.D.Bug", "1.0.0")]
    [Description("Erstellt Pyramiden, Low Walls mit Boden und Leitermasten.")]
    public class RustStructureTools : RustPlugin
    {
        // === /pyramide ===
        [ChatCommand("pyramide")]
        private void BuildPyramid(BasePlayer player, string command, string[] args)
        {
            Vector3 origin = player.transform.position + player.transform.forward * 3f;
            float foundationSize = 3f;

            int layers = 4;
            for (int y = 0; y < layers; y++)
            {
                int count = layers - y;
                for (int x = 0; x < count; x++)
                {
                    for (int z = 0; z < count; z++)
                    {
                        float offsetX = (x - count / 2f) * foundationSize;
                        float offsetZ = (z - count / 2f) * foundationSize;

                        // Versetzt dreieckige Böden leicht, um eine viereckige Pyramide zu simulieren
                        Vector3 position = origin + new Vector3(offsetX, y * 1.5f, offsetZ);

                        // Jede Ebene wird gedreht für besseres Pyramidenbild
                        Quaternion rotation = Quaternion.Euler(0, (x + z) % 2 == 0 ? 0 : 180, 0);
                        SpawnBuildingBlock("foundation.triangle", position, rotation, player);
                    }
                }
            }

            SendReply(player, "Viereckige Pyramide wurde erstellt.");
        }

        // === /wallfloor ===
        [ChatCommand("wallfloor")]
        private void WallWithFloor(BasePlayer player, string command, string[] args)
        {
            Vector3 wallPos = player.transform.position + player.transform.forward * 3f;

            // Spawne Low Wall
            BaseEntity wall = SpawnBuildingBlock("wall.low", wallPos, Quaternion.identity, player);

            // Positioniere Boden exakt oben drauf (Low Wall ist ~1m hoch)
            Vector3 floorPos = wallPos + new Vector3(0, 1.0f, 0);
            SpawnBuildingBlock("floor", floorPos, Quaternion.identity, player);

            SendReply(player, "Low Wall mit Boden oben drauf wurde erstellt.");
        }

        // === /mast ===
        [ChatCommand("mast")]
        private void BuildLadderMast(BasePlayer player, string command, string[] args)
        {
            Vector3 basePos = player.transform.position + player.transform.forward * 3f;

            for (int i = 0; i < 6; i++)
            {
                Vector3 ladderPos = basePos + new Vector3(0, i * 2.7f, 0);
                SpawnDeployable("ladder.wooden.wall", ladderPos, Quaternion.Euler(0, 0, 0), player);
            }

            SendReply(player, "Leitermast wurde erstellt.");
        }

        // === HELPER: Building Block Spawner ===
        private BaseEntity SpawnBuildingBlock(string prefabName, Vector3 position, Quaternion rotation, BasePlayer player)
        {
            string path = $"assets/prefabs/building core/{prefabName}/{prefabName}.prefab";
            BaseEntity entity = GameManager.server.CreateEntity(path, position, rotation);

            if (entity == null) return null;

            var block = entity as BuildingBlock;
            if (block != null)
            {
                block.grade = BuildingGrade.Enum.Stone;
                block.OwnerID = player.userID;
                block.Spawn();
                block.SetHealthToMax();
            }
            return entity;
        }

        // === HELPER: Deployable Spawner ===
        private void SpawnDeployable(string prefabName, Vector3 position, Quaternion rotation, BasePlayer player)
        {
            string path = $"assets/prefabs/deployable/{prefabName}/{prefabName}.prefab";
            BaseEntity entity = GameManager.server.CreateEntity(path, position, rotation);

            if (entity == null) return;

            entity.transform.position = position;
            entity.transform.rotation = rotation;
            entity.OwnerID = player.userID;
            entity.Spawn();
        }
    }
}
