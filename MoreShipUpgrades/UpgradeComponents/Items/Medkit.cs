﻿using LethalLib.Extras;
using LethalLib.Modules;
using MoreShipUpgrades.Managers;
using MoreShipUpgrades.Misc;
using MoreShipUpgrades.UpgradeComponents.Contracts;
using MoreShipUpgrades.UpgradeComponents.Interfaces;
using MoreShipUpgrades.UpgradeComponents.TierUpgrades.AttributeUpgrades;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MoreShipUpgrades.UpgradeComponents.Items
{
    /// <summary>
    /// Logical class which represents an item that can heal the player when activated
    /// </summary>
    internal class Medkit : LategameItem, IDisplayInfo
    {
        internal const string ITEM_NAME = "Medkit";
        /// <summary>
        /// Sounds played when interacting with the medkit
        /// </summary>
        public AudioClip error, use;
        /// <summary>
        /// Source of the sounds played when interacting with the medkit
        /// </summary>
        private AudioSource audio;
        /// <summary>
        /// Instance responsible of managing the player's HUD
        /// </summary>
        private HUDManager hudManager;
        /// <summary>
        /// Current amount of heals the instance of this class has made
        /// </summary>
        private int uses = 0;

        /// <summary>
        /// Maximum amount of heals the instance of the class allows
        /// </summary>
        protected int maximumUses;
        /// <summary>
        /// How much a given instance of the class heals when interacted
        /// </summary>
        protected int healAmount;
        protected override bool KeepScanNode
        {
            get
            {
                return UpgradeBus.Instance.PluginConfiguration.MEDKIT_SCAN_NODE;
            }
        }

        public override void Start()
        {
            base.Start();
            audio = GetComponent<AudioSource>();
            hudManager = HUDManager.Instance;
            maximumUses = UpgradeBus.Instance.PluginConfiguration.MEDKIT_USES.Value;
            healAmount = UpgradeBus.Instance.PluginConfiguration.MEDKIT_HEAL_VALUE.Value;
        }
        public override void DiscardItem()
        {
            playerHeldBy.activatingItem = false;
            base.DiscardItem();
        }
        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            AttemptToHealPlayer();
        }
        /// <summary>
        /// Attempts to provide healing to the player that activated the medkit item
        /// </summary>
        private void AttemptToHealPlayer()
        {
            int health = 100;
            if (UpgradeBus.Instance.PluginConfiguration.StimpackConfiguration.Enabled.Value) health = Stimpack.Instance.playerHealthLevels.ContainsKey(playerHeldBy.playerSteamId) ? Stimpack.GetHealthFromPlayer(100, playerHeldBy.playerSteamId) : health;
            if (!CanUseMedkit(health)) return;
            UseMedkit(health);
        }
        /// <summary>
        /// <para>Consumes an use of the medkit to heal the player that is holding it.</para>
        /// <para>If it has run out of uses to heal, a tip will be displayed to the player to warn that it cannot heal anymore.</para>
        /// </summary>
        /// <param name="maximumHealth"></param>
        private void UseMedkit(int maximumHealth)
        {
            audio.PlayOneShot(use);
            int heal_value = healAmount;
            int potentialHealth = playerHeldBy.health + heal_value;
            if (potentialHealth > maximumHealth)
            {
                heal_value -= potentialHealth - maximumHealth;
            }
            playerHeldBy.health += heal_value;
            hudManager.UpdateHealthUI(playerHeldBy.health, hurtPlayer: false);

            if (IsHost || IsServer) UpdateMedkitUsesClientRpc();
            else UpdateMedkitUsesServerRpc();

            if (playerHeldBy.health >= 20) playerHeldBy.MakeCriticallyInjured(false);
        }
        /// <summary>
        /// Checks if it's in conditions of consume a use of the medkit
        /// </summary>
        /// <param name="maximumHealth">Maximum health allowed for the player that is using the medkit</param>
        /// <returns></returns>
        private bool CanUseMedkit(int maximumHealth)
        {
            if (itemUsedUp)
            {
                hudManager.DisplayTip("NO MORE USES!", "This medkit doesn't have anymore supplies!", true, false, "LC_Tip1");
                return false;
            }
            if (!Mouse.current.leftButton.isPressed) return false;

            if (playerHeldBy.health >= maximumHealth)
            {
                audio.PlayOneShot(error);
                return false;
            }
            return true;
        }
        [ServerRpc]
        void UpdateMedkitUsesServerRpc()
        {
            UpdateMedkitUsesClientRpc();
        }
        [ClientRpc]
        void UpdateMedkitUsesClientRpc()
        {
            UpdateMedkitUsagesLocal();
        }

        void UpdateMedkitUsagesLocal()
        {
            uses++;
            if (uses < maximumUses) return;

            itemUsedUp = true;
            if (playerHeldBy == UpgradeBus.Instance.GetLocalPlayer()) hudManager.DisplayTip("NO MORE USES!", "This medkit doesn't have anymore supplies!", true, false, "LC_Tip1");
        }

        public string GetDisplayInfo()
        {
            return $"MEDKIT - ${UpgradeBus.Instance.PluginConfiguration.MEDKIT_PRICE.Value}\n\n" +
                $"Left click to heal yourself for {UpgradeBus.Instance.PluginConfiguration.MEDKIT_HEAL_VALUE.Value} health.\n" +
                $"Can be used {UpgradeBus.Instance.PluginConfiguration.MEDKIT_USES.Value} times.";
        }

        public static new void LoadItem()
        {
            AudioClip errorUse = AssetBundleHandler.GetAudioClip("Error");
            AudioClip buttonPressed = AssetBundleHandler.GetAudioClip("Button Press");
            Item MedKitItem = AssetBundleHandler.GetItemObject("Medkit");
            if (MedKitItem == null) return;
            AnimationCurve curve = new(new Keyframe(0f, UpgradeBus.Instance.PluginConfiguration.EXTRACTION_CONTRACT_AMOUNT_MEDKITS.Value), new Keyframe(1f, UpgradeBus.Instance.PluginConfiguration.EXTRACTION_CONTRACT_AMOUNT_MEDKITS.Value));

            MedKitItem.creditsWorth = UpgradeBus.Instance.PluginConfiguration.MEDKIT_PRICE.Value;
            MedKitItem.itemId = 492016;
            MedKitItem.itemName = "Medic Bag";
            Medkit medScript = MedKitItem.spawnPrefab.AddComponent<Medkit>();
            medScript.itemProperties = MedKitItem;
            medScript.grabbable = true;
            medScript.useCooldown = 2f;
            medScript.grabbableToEnemies = true;
            medScript.error = errorUse;
            medScript.use = buttonPressed;
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(MedKitItem.spawnPrefab);

            ItemManager.SetupStoreItem(MedKitItem);

            Item MedKitMapItem = AssetBundleHandler.GetItemObject("MedkitMapItem");
            if (MedKitMapItem == null) return;
            Medkit medMapScript = MedKitMapItem.spawnPrefab.AddComponent<Medkit>();
            MedKitMapItem.spawnPrefab.AddComponent<ExtractionContract>();
            MedKitMapItem.itemName = "Medic Bag";
            medMapScript.itemProperties = MedKitMapItem;
            medMapScript.grabbable = true;
            medMapScript.useCooldown = 2f;
            medMapScript.grabbableToEnemies = true;
            medMapScript.error = errorUse;
            medMapScript.use = buttonPressed;
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(MedKitMapItem.spawnPrefab);

            SpawnableMapObjectDef mapObjDef = ScriptableObject.CreateInstance<SpawnableMapObjectDef>();
            mapObjDef.spawnableMapObject = new SpawnableMapObject
            {
                prefabToSpawn = MedKitMapItem.spawnPrefab
            };
            MapObjects.RegisterMapObject(mapObjDef, Levels.LevelTypes.All, (_) => curve);
            UpgradeBus.Instance.spawnableMapObjects["MedkitMapItem"] = mapObjDef;
            UpgradeBus.Instance.spawnableMapObjectsAmount["MedkitMapItem"] = UpgradeBus.Instance.PluginConfiguration.EXTRACTION_CONTRACT_AMOUNT_MEDKITS.Value;

            UpgradeBus.Instance.ItemsToSync.Add(ITEM_NAME, MedKitItem);
        }
    }
}
