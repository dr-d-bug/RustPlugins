using System;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Announcer", "Dr.D.Bug", "0.1.1")]
    [Description("Event triggered Chat-Messages")]

    public class Announcer : RustPlugin
    {
        private string Prefix = "[Announcer] : ";                // CHAT PLUGIN PREFIX
        private string PrefixColor = "#008000";                  // CHAT PLUGIN PREFIX COLOR
        private ulong SteamIDIcon = 76561198979460917;           // SteamID FOR PLUGIN ICON
        public string ChatIcon = "76561198979460917";
        //   [ChatCommand("hallo")]
        //   void HelloCommand(BasePlayer player)
        //   {
        //       SendMessage(player, "Hey " + player.displayName);
        //   }

        void SendMessage(BasePlayer player, string msg, params object[] args)
        {
            PrintToChat(player, msg, args);
        }

        void Broadcast(string msg, params object[] args)
        {
            PrintToChat(msg, args);
        }

        void Loaded()
        {
            Puts("Das Plugin Announcer wurde geladen!");
        }

        void OnEntitySpawned(BaseNetworkable entity)
        {
            if (entity is BaseHelicopter)
            {
                // Broadcast("OnEntitySpawned: " + entity.ShortPrefabName + " Der Patrol-Helikopter ist im Anflug!");
                Puts("Der Patrol-Helikopter ist im Anflug!");
                Broadcast("Der Patrol-Helikopter ist im Anflug!");
            }

            if (entity is XMasRefill)
            {
                // Broadcast("OnEntitySpawned: " + entity.ShortPrefabName);
                Puts("Das X-Mas-Event ist gestartet!");
                Broadcast("Das X-Mas-Event ist gestartet!");
            }

            if (entity is SantaSleigh)
            {
                // Broadcast("OnEntitySpawned: " + entity.ShortPrefabName);
                Puts("Santa ist unterwegs und bringt Geschenke");
                Broadcast("Santa ist unterwegs und bringt Geschenke");
            }
            // + Geschenk-Abwurf

            if (entity is EggHuntEvent)
            {
                // Broadcast("OnEntitySpawned: " + entity.ShortPrefabName);
                Puts("Die Eiersuche geht los!");
                Broadcast("Die Eiersuche geht los!");
            }

            if (entity is CH47Helicopter)
            {
                // Broadcast("OnEntitySpawned: " + entity.ShortPrefabName);
                Puts("Der CH47-Helicopter ist unterwegs");
                Broadcast("Der CH47-Helicopter ist unterwegs");
            }

            if (entity is BradleyAPC)
            {
                // Broadcast("OnEntitySpawned: " + entity.ShortPrefabName);
                Puts("Der BradleyAPC ist auf seiner Patrouille");
                Broadcast("Der BradleyAPC ist auf seiner Patrouille");
            }

        }

        void OnEntityLeave(TriggerBase trigger, BaseEntity entity)
        {
            if (entity is BaseHelicopter)
            {
                // CreateAnnouncement(helicopterSpawnAnnouncementText, helicopterSpawnAnnouncementBannerColor, helicopterSpawnAnnouncementTextColor);
                // Broadcast("OnEntityLeave: " + entity.ShortPrefabName);
                Puts("OnEntityLeave works! Heli went off?");
            }
        }

        void OnEntityKill(BaseNetworkable entity)
        {
            if (entity is BaseHelicopter)
            {
                // CreateAnnouncement(helicopterSpawnAnnouncementText, helicopterSpawnAnnouncementBannerColor, helicopterSpawnAnnouncementTextColor);
                Puts("OnEntityKill works! Heli went off");
                Broadcast("Der Patrol-Helikopter hat die Insel verlassen.");
            }
        }

        // Airdrop
        void OnAirdrop(CargoPlane plane, Vector3 dropPosition)
        {
            Puts("OnAirdrop works!");
            Broadcast("Ein Cargo-Plane mit Airdrop ist unterwegs");
        }
        // + Airdrop-Abwurf!

        void OnCrateDropped(HackableLockedCrate crate)
        {
            Puts("OnCrateDropped works!");
            Puts("Der CH47-Helicopter hat seine verschlossene Ladung abgeworfen");
        }

        void OnCrateHack(HackableLockedCrate crate)
        {
            Puts("OnCrateHack works!");
            Broadcast("Die CH47-Ladung wird gehackt");
            crate.hackSeconds = 60f;          // seconds since hack (initial??)
        }

        // Player
        // - OK! - Called when a player is attempting to spawn for the first time
        object OnPlayerSpawn(BasePlayer player)
        {
            Puts("OnPlayerSpawn works!");
            Broadcast(player.displayName + " ist erstmalig auf der Insel gestrandet");
            return null;
        }
        // - OK! - Called when the player is initializing (after they've connected, before they wake up)
        void OnPlayerInit(BasePlayer player)
        {
            Puts("OnPlayerInit works!");
            Broadcast("OnPlayerInit works!");
        }
        // - OK! - Called after the player has disconnected from the server
        void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            Puts("OnPlayerDisconnected works!");
            Broadcast("OnPlayerDisconnected works!");
        }

    }
}
