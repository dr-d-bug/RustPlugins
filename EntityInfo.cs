using System;
using System.Collections.Generic;
using UnityEngine;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;

namespace Oxide.Plugins
{
    [Info("EntityInfo", "Dr.D.Bug", "1.0.1")]
    [Description("Zeigt den Spielernamen des Besitzers des angesehenen Bauteils an.")]
    public class EntityInfo : RustPlugin
    {
        #region Configuration
        private Configuration config;

        public class Configuration
        {
            public float MaxDistance { get; set; } = 10f;
            public string Permission { get; set; } = "entityinfo.use";
            public float UpdateInterval { get; set; } = 0.5f;
        }

        protected override void LoadDefaultConfig()
        {
            config = new Configuration();
            Config.WriteObject(config, true);
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                config = Config.ReadObject<Configuration>();
                if (config == null)
                {
                    LoadDefaultConfig();
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
            if (config == null) LoadDefaultConfig();
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
        [ChatCommand("owner")]
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
                SendReply(player, "Entity-Info <color=green>aktiviert</color>. Schau ein Bauteil an, um den Besitzer zu sehen.");
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

            // Wenn die angesehene Entity einen Besitzer hat, zeige nur dessen Namen an.
            if (entity.OwnerID != 0)
            {
                var ownerPlayer = BasePlayer.FindByID(entity.OwnerID);
                string ownerName = ownerPlayer?.displayName ?? $"ID: {entity.OwnerID}";
                ShowOwnerNameUI(player, ownerName);
            }
            else
            {
                // Kein Besitzer: UI entfernen
                DestroyUI(player);
            }
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
        #endregion

        #region UI Methods
        void ShowOwnerNameUI(BasePlayer player, string name)
        {
            DestroyUI(player);

            var elements = new CuiElementContainer();

            // Einfaches Panel in der Mitte oben
            elements.Add(new CuiPanel
            {
                Image = { Color = "0 0 0 0.6" },
                RectTransform = { AnchorMin = "0.4 0.9", AnchorMax = "0.6 0.95" }
            }, "Overlay", UI_NAME);

            // Besitzername (nur der Name, hervorgehoben)
            elements.Add(new CuiLabel
            {
                Text = { Text = $"<color=yellow>{name}</color>", FontSize = 16, Align = TextAnchor.MiddleCenter },
                RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" }
            }, UI_NAME);

            CuiHelper.AddUi(player, elements);
        }

        void DestroyUI(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, UI_NAME);
        }
        #endregion
    }
}
