using System;
using System.Collections.Generic;
using UnityEngine;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using System.Linq;

namespace Oxide.Plugins
{
    [Info("EntityInfo", "YourName", "1.0.0")]
    [Description("Zeigt Informationen über die Entity an, die der Spieler anschaut")]
    public class EntityInfo : RustPlugin
    {
        #region Configuration
        private Configuration config;

        public class Configuration
        {
            public float MaxDistance { get; set; } = 10f;
            public bool ShowHealth { get; set; } = true;
            public bool ShowPosition { get; set; } = true;
            public bool ShowNetworkId { get; set; } = true;
            public bool ShowOwner { get; set; } = true;
            public bool ShowDecay { get; set; } = true;
            public string Permission { get; set; } = "entityinfo.use";
            public float UpdateInterval { get; set; } = 0.5f;
        }

        protected override void LoadDefaultConfig()
        {
            config = new Configuration();
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                config = Config.ReadObject<Configuration>();
                if (config == null)
                {
                    throw new JsonException();
                }
            }
            catch
            {
                PrintWarning("Konfigurationsdatei ist korrupt, lade Standardkonfiguration");
                LoadDefaultConfig();
            }
        }

        protected override void SaveConfig() => Config.WriteObject(config);
        #endregion

        #region Data Storage
        private Dictionary<ulong, bool> playerStates = new Dictionary<ulong, bool>();
        private Dictionary<ulong, Timer> playerTimers = new Dictionary<ulong, Timer>();
        private const string UI_NAME = "EntityInfoUI";
        #endregion

        #region Hooks
        void Init()
        {
            permission.RegisterPermission(config.Permission, this);
            cmd.AddChatCommand("entityinfo", this, "ToggleEntityInfo");
            cmd.AddChatCommand("ei", this, "ToggleEntityInfo");
        }

        void OnPlayerDisconnected(BasePlayer player)
        {
            if (playerStates.ContainsKey(player.userID))
            {
                DestroyUI(player);
                StopTimer(player.userID);
                playerStates.Remove(player.userID);
            }
        }
        #endregion

        #region Commands
        [ChatCommand("entityinfo")]
        void ToggleEntityInfo(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, config.Permission))
            {
                SendReply(player, "Du hast keine Berechtigung für diesen Befehl.");
                return;
            }

            bool isEnabled = playerStates.ContainsKey(player.userID) && playerStates[player.userID];
            
            if (isEnabled)
            {
                DisableEntityInfo(player);
                SendReply(player, "Entity-Info <color=red>deaktiviert</color>");
            }
            else
            {
                EnableEntityInfo(player);
                SendReply(player, "Entity-Info <color=green>aktiviert</color>. Schaue eine Entity an, um Informationen zu sehen.");
            }
        }
        #endregion

        #region Core Methods
        void EnableEntityInfo(BasePlayer player)
        {
            playerStates[player.userID] = true;
            StartTimer(player);
        }

        void DisableEntityInfo(BasePlayer player)
        {
            playerStates[player.userID] = false;
            DestroyUI(player);
            StopTimer(player.userID);
        }

        void StartTimer(BasePlayer player)
        {
            StopTimer(player.userID);
            
            playerTimers[player.userID] = timer.Every(config.UpdateInterval, () =>
            {
                if (player == null || !player.IsConnected || !playerStates.ContainsKey(player.userID) || !playerStates[player.userID])
                {
                    StopTimer(player.userID);
                    return;
                }

                UpdateEntityInfo(player);
            });
        }

        void StopTimer(ulong userId)
        {
            if (playerTimers.ContainsKey(userId))
            {
                playerTimers[userId]?.Destroy();
                playerTimers.Remove(userId);
            }
        }

        void UpdateEntityInfo(BasePlayer player)
        {
            var entity = GetLookingAtEntity(player);
            
            if (entity == null)
            {
                DestroyUI(player);
                return;
            }

            var info = GetEntityInfo(entity);
            ShowEntityInfoUI(player, info);
        }

        BaseEntity GetLookingAtEntity(BasePlayer player)
        {
            RaycastHit hit;
            if (Physics.Raycast(player.eyes.HeadRay(), out hit, config.MaxDistance))
            {
                var entity = hit.GetEntity();
                return entity;
            }
            return null;
        }

        EntityInfoData GetEntityInfo(BaseEntity entity)
        {
            var data = new EntityInfoData();
            
            data.Name = entity.ShortPrefabName ?? entity.name ?? "Unbekannt";
            data.NetworkId = entity.net?.ID.ToString() ?? "N/A";
            data.Position = entity.transform.position;

            // Health Information
            if (config.ShowHealth && entity is BaseCombatEntity combatEntity)
            {
                data.Health = combatEntity.health;
                data.MaxHealth = combatEntity.MaxHealth();
                data.HasHealth = true;
            }

            // Owner Information
            if (config.ShowOwner && entity.OwnerID != 0)
            {
                var ownerPlayer = BasePlayer.FindByID(entity.OwnerID);
                data.Owner = ownerPlayer?.displayName ?? $"ID: {entity.OwnerID}";
            }

            // Decay Information
            if (config.ShowDecay && entity is DecayEntity decayEntity)
            {
                data.DecayPoints = decayEntity.decay?.GetDecayDelay(Vector3.zero) ?? 0f;
                data.HasDecay = true;
            }

            // Building Information
            if (entity is BuildingBlock buildingBlock)
            {
                data.BuildingGrade = buildingBlock.grade.ToString();
                data.IsBuildingBlock = true;
            }

            // Storage Information
            if (entity is StorageContainer storage)
            {
                data.StorageSlots = storage.inventory?.capacity ?? 0;
                data.StorageUsed = storage.inventory?.itemList?.Count ?? 0;
                data.IsStorage = true;
            }

            // Resource Information
            if (entity is ResourceEntity resource)
            {
                data.ResourceStage = resource.stage;
                data.IsResource = true;
            }

            return data;
        }
        #endregion

        #region UI Methods
        void ShowEntityInfoUI(BasePlayer player, EntityInfoData data)
        {
            DestroyUI(player);

            var elements = new CuiElementContainer();
            
            // Background Panel
            elements.Add(new CuiPanel
            {
                Image = { Color = "0 0 0 0.8" },
                RectTransform = { AnchorMin = "0.02 0.7", AnchorMax = "0.35 0.95" }
            }, "Overlay", UI_NAME);

            // Title
            elements.Add(new CuiLabel
            {
                Text = { Text = $"<color=yellow>Entity Info</color>", FontSize = 14, Align = TextAnchor.MiddleCenter },
                RectTransform = { AnchorMin = "0 0.85", AnchorMax = "1 1" }
            }, UI_NAME);

            // Entity Name
            elements.Add(new CuiLabel
            {
                Text = { Text = $"<color=orange>Name:</color> {data.Name}", FontSize = 11, Align = TextAnchor.MiddleLeft },
                RectTransform = { AnchorMin = "0.02 0.75", AnchorMax = "0.98 0.85" }
            }, UI_NAME);

            float yPos = 0.65f;
            float lineHeight = 0.08f;

            // Network ID
            if (config.ShowNetworkId)
            {
                elements.Add(new CuiLabel
                {
                    Text = { Text = $"<color=cyan>Network ID:</color> {data.NetworkId}", FontSize = 10, Align = TextAnchor.MiddleLeft },
                    RectTransform = { AnchorMin = $"0.02 {yPos}", AnchorMax = $"0.98 {yPos + lineHeight}" }
                }, UI_NAME);
                yPos -= lineHeight;
            }

            // Position
            if (config.ShowPosition)
            {
                elements.Add(new CuiLabel
                {
                    Text = { Text = $"<color=green>Position:</color> {data.Position.x:F1}, {data.Position.y:F1}, {data.Position.z:F1}", FontSize = 10, Align = TextAnchor.MiddleLeft },
                    RectTransform = { AnchorMin = $"0.02 {yPos}", AnchorMax = $"0.98 {yPos + lineHeight}" }
                }, UI_NAME);
                yPos -= lineHeight;
            }

            // Health
            if (config.ShowHealth && data.HasHealth)
            {
                var healthColor = data.Health > data.MaxHealth * 0.5f ? "green" : data.Health > data.MaxHealth * 0.25f ? "yellow" : "red";
                elements.Add(new CuiLabel
                {
                    Text = { Text = $"<color={healthColor}>Health:</color> {data.Health:F1} / {data.MaxHealth:F1}", FontSize = 10, Align = TextAnchor.MiddleLeft },
                    RectTransform = { AnchorMin = $"0.02 {yPos}", AnchorMax = $"0.98 {yPos + lineHeight}" }
                }, UI_NAME);
                yPos -= lineHeight;
            }

            // Owner
            if (config.ShowOwner && !string.IsNullOrEmpty(data.Owner))
            {
                elements.Add(new CuiLabel
                {
                    Text = { Text = $"<color=purple>Owner:</color> {data.Owner}", FontSize = 10, Align = TextAnchor.MiddleLeft },
                    RectTransform = { AnchorMin = $"0.02 {yPos}", AnchorMax = $"0.98 {yPos + lineHeight}" }
                }, UI_NAME);
                yPos -= lineHeight;
            }

            // Building Grade
            if (data.IsBuildingBlock)
            {
                elements.Add(new CuiLabel
                {
                    Text = { Text = $"<color=brown>Grade:</color> {data.BuildingGrade}", FontSize = 10, Align = TextAnchor.MiddleLeft },
                    RectTransform = { AnchorMin = $"0.02 {yPos}", AnchorMax = $"0.98 {yPos + lineHeight}" }
                }, UI_NAME);
                yPos -= lineHeight;
            }

            // Storage Info
            if (data.IsStorage)
            {
                elements.Add(new CuiLabel
                {
                    Text = { Text = $"<color=blue>Storage:</color> {data.StorageUsed} / {data.StorageSlots}", FontSize = 10, Align = TextAnchor.MiddleLeft },
                    RectTransform = { AnchorMin = $"0.02 {yPos}", AnchorMax = $"0.98 {yPos + lineHeight}" }
                }, UI_NAME);
                yPos -= lineHeight;
            }

            // Resource Stage
            if (data.IsResource)
            {
                elements.Add(new CuiLabel
                {
                    Text = { Text = $"<color=gray>Resource Stage:</color> {data.ResourceStage}", FontSize = 10, Align = TextAnchor.MiddleLeft },
                    RectTransform = { AnchorMin = $"0.02 {yPos}", AnchorMax = $"0.98 {yPos + lineHeight}" }
                }, UI_NAME);
                yPos -= lineHeight;
            }

            // Decay Info
            if (config.ShowDecay && data.HasDecay)
            {
                elements.Add(new CuiLabel
                {
                    Text = { Text = $"<color=red>Decay Delay:</color> {data.DecayPoints:F1}s", FontSize = 10, Align = TextAnchor.MiddleLeft },
                    RectTransform = { AnchorMin = $"0.02 {yPos}", AnchorMax = $"0.98 {yPos + lineHeight}" }
                }, UI_NAME);
                yPos -= lineHeight;
            }

            CuiHelper.AddUi(player, elements);
        }

        void DestroyUI(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, UI_NAME);
        }
        #endregion

        #region Data Classes
        public class EntityInfoData
        {
            public string Name { get; set; } = "";
            public string NetworkId { get; set; } = "";
            public Vector3 Position { get; set; }
            public float Health { get; set; }
            public float MaxHealth { get; set; }
            public bool HasHealth { get; set; }
            public string Owner { get; set; } = "";
            public float DecayPoints { get; set; }
            public bool HasDecay { get; set; }
            public string BuildingGrade { get; set; } = "";
            public bool IsBuildingBlock { get; set; }
            public int StorageSlots { get; set; }
            public int StorageUsed { get; set; }
            public bool IsStorage { get; set; }
            public int ResourceStage { get; set; }
            public bool IsResource { get; set; }
        }
        #endregion
    }
}