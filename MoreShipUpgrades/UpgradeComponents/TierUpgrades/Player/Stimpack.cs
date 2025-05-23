﻿using GameNetcodeStuff;
using MoreShipUpgrades.Configuration;
using MoreShipUpgrades.Configuration.Upgrades.Interfaces.TierUpgrades;
using MoreShipUpgrades.Managers;
using MoreShipUpgrades.Misc;
using MoreShipUpgrades.Misc.Upgrades;
using MoreShipUpgrades.Misc.Util;
using MoreShipUpgrades.UI.TerminalNodes;
using MoreShipUpgrades.UpgradeComponents.Interfaces;
using System;
using System.Collections.Generic;
using Unity.Netcode;

namespace MoreShipUpgrades.UpgradeComponents.TierUpgrades.AttributeUpgrades
{
    internal class Stimpack : GameAttributeTierUpgrade, IUpgradeWorldBuilding
    {
        internal Dictionary<ulong, int> playerHealthLevels = [];
        internal static Stimpack Instance;

        public const string UPGRADE_NAME = "Stimpack";
        internal const string WORLD_BUILDING_TEXT = "\n\nAn experimental Company-offered 'health treatment' program advertised only on old, peeling Ship posters," +
            " which are themselves only present in about 40% of all Company-issued Ships. Some Ships even have multiple. Nothing is known from the outside about how it works," +
            " and in order to be eligible for the program, {0} must sign an NDA.\n\n";
        // Configuration
        public const string ENABLED_SECTION = $"Enable {UPGRADE_NAME} Upgrade";
        public const bool ENABLED_DEFAULT = true;
        public const string ENABLED_DESCRIPTION = "Increases player's health.";

        public const string PRICE_SECTION = $"{UPGRADE_NAME} Price";
        public const int PRICE_DEFAULT = 600;

        public const string PRICES_DEFAULT = "600,300, 450, 600";

        public const string ADDITIONAL_HEALTH_UNLOCK_SECTION = "Initial health boost";
        public const int ADDITIONAL_HEALTH_UNLOCK_DEFAULT = 20;
        public const string ADDITIONAL_HEALTH_UNLOCK_DESCRIPTION = "Amount of health gained when unlocking the upgrade";

        public const string ADDITIONAL_HEALTH_INCREMENT_SECTION = "Additional health boost";
        public const int ADDITIONAL_HEALTH_INCREMENT_DEFAULT = 20;
        public const string ADDITIONAL_HEALTH_INCREMENT_DESCRIPTION = $"Every time {UPGRADE_NAME} is upgraded this value will be added to the value above.";

        void Awake()
        {
            ITierEffectUpgradeConfiguration<int> config = GetConfiguration().StimpackConfiguration;
            upgradeName = UPGRADE_NAME;
            overridenUpgradeName = config.OverrideName;
            logger = new LguLogger(UPGRADE_NAME);
            Instance = this;
            changingAttribute = GameAttribute.PLAYER_HEALTH;
            initialValue = config.InitialEffect.Value;
            incrementalValue = config.IncrementalEffect.Value;
        }
        public override void Load()
        {
            base.Load();
            UpdatePlayerHealthLevel();
        }
        public override void Increment()
        {
            base.Increment();
            UpdatePlayerHealthLevel();
        }
        void UpdatePlayerHealthLevel()
        {
            PlayerControllerB player = UpgradeBus.Instance.GetLocalPlayer();
            PlayerHealthUpdateLevelServerRpc(player.playerSteamId, GetUpgradeLevel(UPGRADE_NAME));
        }
        public override void Unwind()
        {
            base.Unwind();
            PlayerControllerB player = UpgradeBus.Instance.GetLocalPlayer();
            PlayerHealthUpdateLevelServerRpc(player.playerSteamId, -1);
        }
        public static int CheckForAdditionalHealth(int health)
        {
            ITierEffectUpgradeConfiguration<int> config = GetConfiguration().StimpackConfiguration;
            if (!config.Enabled.Value) return health; // this is stupid to check
            PlayerControllerB player = UpgradeBus.Instance.GetLocalPlayer();
            if (!Instance.playerHealthLevels.ContainsKey(player.playerSteamId)) return health;
            int currentLevel = Instance.playerHealthLevels[player.playerSteamId];

            return health + config.InitialEffect.Value + (currentLevel * config.IncrementalEffect.Value);
        }
        /// <summary>
        /// Returns the maximum health possible for the player with given steam identifier <br></br>
        /// Precondition: playerHealthLevels contains the steam identifier as a key
        /// </summary>
        /// <param name="health">Health before applying the Stimpack upgrade</param>
        /// <param name="steamId">Identifier of the client through steam</param>
        /// <returns>Health of the player after applying the Stimpack effects</returns>
        public static int GetHealthFromPlayer(int health, ulong steamId)
        {
            int currentLevel = Instance.playerHealthLevels[steamId];
            ITierEffectUpgradeConfiguration<int> config = GetConfiguration().StimpackConfiguration;
            return health + config.InitialEffect.Value + (currentLevel * config.IncrementalEffect.Value);
        }
        public string GetWorldBuildingText(bool shareStatus = false)
        {
            return string.Format(WORLD_BUILDING_TEXT, shareStatus ? "your crew" : "you");
        }

        public override string GetDisplayInfo(int initialPrice = -1, int maxLevels = -1, int[] incrementalPrices = null)
        {
            static float infoFunction(int level)
            {
                ITierEffectUpgradeConfiguration<int> config = GetConfiguration().StimpackConfiguration;
                return config.InitialEffect.Value + (level * config.IncrementalEffect.Value);
            }
            string infoFormat = AssetBundleHandler.GetInfoFromJSON(UPGRADE_NAME);
            return Tools.GenerateInfoForUpgrade(infoFormat, initialPrice, incrementalPrices, infoFunction, purchaseMode: GetConfiguration().StimpackConfiguration.PurchaseMode);
        }

        [ServerRpc(RequireOwnership = false)]
        public void PlayerHealthUpdateLevelServerRpc(ulong id, int level)
        {
            logger.LogInfo("Request to update player max healths received. Calling ClientRpc...");
            PlayerHealthUpdateLevelClientRpc(id, level);
        }

        [ClientRpc]
        private void PlayerHealthUpdateLevelClientRpc(ulong id, int level)
        {
            logger.LogInfo($"Setting max health level for player {id} to {level}");
            if (level == -1)
            {
                Instance.playerHealthLevels.Remove(id);
                return;
            }

            Instance.playerHealthLevels[id] = level;
        }

        public override bool CanInitializeOnStart
        {
            get
            {
                ITierUpgradeConfiguration upgradeConfig = GetConfiguration().StimpackConfiguration;
                string[] prices = upgradeConfig.Prices.Value.Split(',');
                return prices.Length == 0 || (prices.Length == 1 && (prices[0].Length == 0 || prices[0] == "0"));
            }
        }

        public new static (string, string[]) RegisterScrapToUpgrade()
        {
            return (UPGRADE_NAME, GetConfiguration().StimpackConfiguration.ItemProgressionItems.Value.Split(","));
        }
        public new static void RegisterUpgrade()
        {
            SetupGenericPerk<Stimpack>(UPGRADE_NAME);
        }
        public new static CustomTerminalNode RegisterTerminalNode()
        {
            return UpgradeBus.Instance.SetupMultiplePurchaseableTerminalNode(UPGRADE_NAME, GetConfiguration().StimpackConfiguration);
        }
    }
}
