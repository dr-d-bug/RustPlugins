using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Pyramid", "dr-d-bug", "1.0.0")]
    [Description("Ermöglicht das Platzieren von dreieckigen Böden auf Foundation-Kanten zum Bauen von Pyramiden.")]
    public class Pyramid : RustPlugin
    {
        // Liste der ItemIDs für Foundations und triangle floors (typische Prefab-Namen)
        private string foundationPrefab = "assets/prefabs/building/foundation/foundation.prefab";
        private string triangleFloorPrefab = "assets/prefabs/building/floor/floor.triangle.prefab";

        // Command: pyramid.build
        [ChatCommand("pyramid")]
        private void CmdPyramid(BasePlayer player, string command, string[] args)
        {
            var ray = player.eyes.BodyRay();
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 5f))
            {
                var foundation = hit.GetEntity() as BuildingBlock;
                if (foundation == null || foundation.PrefabName != foundationPrefab)
                {
                    SendReply(player, "Du schaust nicht auf eine Foundation.");
                    return;
                }

                // Berechne die Kante, an der er baut (z.B. Vor dem Spieler)
                // Hole Drehung und Kanten-Position
                Quaternion foundationRot = foundation.transform.rotation;
                Vector3 foundationPos = foundation.transform.position;

                // Platziere einen dreieckigen Boden an der Seite (vor dem Spieler)
                Vector3 triangleOffset = foundationRot * new Vector3(0, 0.01f, 2.5f); // Beispielhafte Verschiebung
                Vector3 trianglePos = foundationPos + triangleOffset;

                // Wird auf der gleichen Höhe plaziert
                Quaternion triangleRot = foundationRot; // Bei Bedarf anpassen, um die richtige Richtung zu bekommen

                // Spawn triangle floor
                var triangle = GameManager.server.CreateEntity(triangleFloorPrefab, trianglePos, triangleRot);
                if (triangle == null)
                {
                    SendReply(player, "Konnte keinen Dreieckboden erzeugen!");
                    return;
                }
                triangle.Spawn();
                SendReply(player, "Dreieckboden plaziert!");
            }
        }
    }
}
