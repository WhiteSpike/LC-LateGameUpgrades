﻿using GameNetcodeStuff;
using LethalLib.Extras;
using LethalLib.Modules;
using MoreShipUpgrades.Configuration;
using MoreShipUpgrades.Configuration.Upgrades.Interfaces;
using MoreShipUpgrades.Configuration.Upgrades.Interfaces.OneTimeUpgrades;
using MoreShipUpgrades.Configuration.Upgrades.Interfaces.TierUpgrades;
using MoreShipUpgrades.Misc;
using MoreShipUpgrades.Misc.Upgrades;
using MoreShipUpgrades.Misc.Util;
using MoreShipUpgrades.UI.TerminalNodes;
using MoreShipUpgrades.UpgradeComponents.Interfaces;
using MoreShipUpgrades.UpgradeComponents.Items;
using MoreShipUpgrades.UpgradeComponents.OneTimeUpgrades;
using MoreShipUpgrades.UpgradeComponents.OneTimeUpgrades.Enemies;
using MoreShipUpgrades.UpgradeComponents.OneTimeUpgrades.Items;
using MoreShipUpgrades.UpgradeComponents.TierUpgrades.AttributeUpgrades;
using MoreShipUpgrades.UpgradeComponents.TierUpgrades.Player;
using MoreShipUpgrades.UpgradeComponents.TierUpgrades.Ship;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MoreShipUpgrades.Managers
{
    public class UpgradeBus : MonoBehaviour
    {
        internal static UpgradeBus Instance { get; set; }
        internal LategameConfiguration PluginConfiguration { get; set; }
        static readonly LguLogger logger = new(nameof(UpgradeBus));

        internal Dictionary<string, bool> activeUpgrades = [];
        internal Dictionary<string, int> upgradeLevels = [];
        internal Dictionary<string, List<string>> scrapToCollectionUpgrade = [];
        internal Dictionary<string, int> contributionValues = [];
        internal List<string> discoveredItems = [];
        internal int randomUpgradeSeed;
        internal Dictionary<string, float> SaleData = [];

        internal AudioClip flashNoise;
        internal GameObject modStorePrefab;

        internal Dictionary<string, SpawnableMapObjectDef> spawnableMapObjects = [];
        internal Dictionary<string, int> spawnableMapObjectsAmount = [];
        internal readonly List<Type> upgradeTypes = [];
        internal readonly List<Type> commandTypes = [];
        internal readonly List<Type> itemTypes = [];
        internal List<CustomTerminalNode> terminalNodes = [];
        internal Dictionary<CustomTerminalNode, ulong> lockedUpgrades = [];
        internal Dictionary<string, GameObject> UpgradeObjects = [];
        internal Dictionary<string, Item> ItemsToSync = [];
        internal AssetBundle UpgradeAssets;

        Terminal terminal;
        PlayerControllerB playerController;

        void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public Terminal GetTerminal()
        {
            if (terminal == null) terminal = GameObject.Find("TerminalScript").GetComponent<Terminal>();
            return terminal;
        }
        public PlayerControllerB GetLocalPlayer()
        {
            if (playerController == null) playerController = GameNetworkManager.Instance.localPlayerController;
            return playerController;
        }

        public void ResetAllValues(bool wipeObjRefs = true)
        {
            if (LguStore.Instance == null) return; // Quitting the game
            ResetPlayerAttributes();

            if (PluginConfiguration.SickBeatsUpgradeConfiguration.Enabled.Value)
            {
                SickBeats.Instance.EffectsActive = false;
                SickBeats.Instance.BoomboxIcon.SetActive(false);
            }
            if (PluginConfiguration.NightVisionUpgradeConfiguration.Enabled.Value) NightVision.Instance.nightVisionActive = false;
            if (PluginConfiguration.FedoraSuitConfiguration.Enabled) FedoraSuit.instance.wearingFedora.Clear();
            ContractManager.Instance.ResetAllValues();

            if (PluginConfiguration.WalkieGpsConfiguration.Enabled)
                WalkieGPS.instance.WalkieDeactivate();
            if (PluginConfiguration.DiscombobulatorUpgradeConfiguration.Enabled.Value) Discombobulator.instance.flashCooldown = 0f;
            if (PluginConfiguration.BackMusclesConfiguration.Enabled.Value) BackMuscles.Instance.alteredWeight = 1f;
            if (PluginConfiguration.LightningRodConfiguration.Enabled) LightningRod.instance.ResetValues();
            if (wipeObjRefs) {
                UpgradeObjects = [];
                discoveredItems.Clear();
                scrapToCollectionUpgrade.Clear();
                contributionValues.Clear();
            }
            foreach(CustomTerminalNode node in terminalNodes)
            {
                node.Unlocked = false;
                node.CurrentUpgrade = 0;
            }

            foreach (string key in activeUpgrades.Keys.ToList())
                activeUpgrades[key] = false;

            foreach (string key in upgradeLevels.Keys.ToList())
                upgradeLevels[key] = 0;

            terminal = null;
            playerController = null;
        }
        private void ResetPlayerAttributes()
        {
            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
            if (player == null) return; // Disconnecting the game

            logger.LogDebug($"Resetting {player.playerUsername}'s attributes");
            foreach (IPlayerSync upgrade in UpgradeObjects.Values.Select(upgrade => upgrade.GetComponent<BaseUpgrade>()).OfType<IPlayerSync>().ToArray())
            {
                upgrade.ResetPlayerAttribute();
            }
        }

        internal CustomTerminalNode GetUpgradeNode(string upgradeName)
        {
            foreach (CustomTerminalNode node in terminalNodes)
            {
                if (node.OriginalName == upgradeName || node.Name == upgradeName) return node;
            }
            return null;
        }

        internal static bool ContainsUpgradeNode(CustomTerminalNode node)
        {
            foreach (CustomTerminalNode loadedNode in Instance.GetTerminalNodes())
            {
                if (loadedNode.OriginalName == node.OriginalName && loadedNode.Name == node.Name) return true;
            }
            return false;
        }

        internal List<CustomTerminalNode> GetTerminalNodes()
        {
            return terminalNodes;
        }

        internal static List<CustomTerminalNode> GetUpgradeNodes()
        {
            return Instance.GetTerminalNodes();
        }

        internal void LoadSales()
        {
            if(SaleData.Count == 0)
            {
                logger.LogInfo("Sale data empty, continuing...");
                return;
            }
            foreach(CustomTerminalNode node in terminalNodes)
            {
                if (!SaleData.ContainsKey(node.Name)) continue;
                node.SalePercentage = SaleData[node.Name];
                if(node.SalePercentage != 1f)
                {
                    logger.LogInfo($"Loaded sale of {node.SalePercentage} for {node.Name}.");
                }
            }
        }

        internal void AlterStoreItem(string itemName, bool configuredEnable, int configuredPrice)
        {
            Item storeItem = ItemsToSync[itemName];
            if (!configuredEnable)
            {
                Items.RemoveShopItem(storeItem);
                logger.LogInfo($"Removing {itemName} from store.");
                return;
            }
            if (storeItem.creditsWorth != configuredPrice)
            {
                logger.LogInfo($"Changing {itemName}'s price from {storeItem.creditsWorth} to {configuredPrice}");
                Items.UpdateShopItemPrice(storeItem, configuredPrice);
            }
        }

        internal void AlterStoreItems()
        {
            AlterStoreItem(Medkit.ITEM_NAME, PluginConfiguration.MEDKIT_ENABLED, PluginConfiguration.MEDKIT_PRICE);
            AlterStoreItem(NightVisionGoggles.ITEM_NAME, PluginConfiguration.NightVisionUpgradeConfiguration.Enabled, PluginConfiguration.NightVisionUpgradeConfiguration.ItemPrice);
        }

        void SyncAvailableContracts()
        {
            if (!PluginConfiguration.DATA_CONTRACT.Value || !PluginConfiguration.CONTRACTS_ENABLED.Value)
            {
                logger.LogInfo("Removing data contract");
                int idx = CommandParser.contracts.IndexOf(LguConstants.DATA_CONTRACT_NAME);
                if (idx != -1)
                {
                    CommandParser.contractInfos.RemoveAt(idx);
                    CommandParser.contracts.RemoveAt(idx);
                }
            }
            if (!PluginConfiguration.EXTRACTION_CONTRACT.Value || !PluginConfiguration.CONTRACTS_ENABLED.Value)
            {
                logger.LogInfo("Removing extraction contract");
                int idx = CommandParser.contracts.IndexOf(LguConstants.EXTRACTION_CONTRACT_NAME);
                if (idx != -1)
                {
                    CommandParser.contractInfos.RemoveAt(idx);
                    CommandParser.contracts.RemoveAt(idx);
                }
            }
            if (!PluginConfiguration.EXORCISM_CONTRACT.Value || !PluginConfiguration.CONTRACTS_ENABLED.Value)
            {
                logger.LogInfo("Removing exorcism contract");
                int idx = CommandParser.contracts.IndexOf(LguConstants.EXORCISM_CONTRACT_NAME);
                if (idx != -1)
                {
                    CommandParser.contractInfos.RemoveAt(idx);
                    CommandParser.contracts.RemoveAt(idx);
                }
            }
            if (!PluginConfiguration.DEFUSAL_CONTRACT.Value || !PluginConfiguration.CONTRACTS_ENABLED.Value)
            {
                logger.LogInfo("Removing defusal contract");
                int idx = CommandParser.contracts.IndexOf(LguConstants.DEFUSAL_CONTRACT_NAME);
                if (idx != -1)
                {
                    CommandParser.contractInfos.RemoveAt(idx);
                    CommandParser.contracts.RemoveAt(idx);
                }
            }
            if (!PluginConfiguration.EXTERMINATOR_CONTRACT.Value || !PluginConfiguration.CONTRACTS_ENABLED.Value)
            {
                if(CommandParser.contracts.Count == 1 && PluginConfiguration.CONTRACTS_ENABLED.Value)
                {
                    logger.LogInfo("Why must you do the things you do");
                    logger.LogWarning("User tried to remove all contracts! Leaving exterminator present. Did you mean to disable contracts?");
                }
                else
                {
                    logger.LogInfo("Removing exterminator contract");
                    int idx = CommandParser.contracts.IndexOf(LguConstants.EXTERMINATOR_CONTRACT_NAME);
                    if (idx != -1)
                    {
                        CommandParser.contractInfos.RemoveAt(idx);
                        CommandParser.contracts.RemoveAt(idx);
                    }
                }
            }
        }

        internal void Reconstruct()
        {
            AlterStoreItems();
            BuildCustomNodes();
            SyncAvailableContracts();

            logger.LogInfo("Successfully reconstructed with hosts config.");
            if (LguStore.Instance.IsClient && !LguStore.Instance.IsHost) LguStore.Instance.RandomizeUpgradesServerRpc();
        }
        internal void BuildCustomNodes()
        {
            terminalNodes = [];

            foreach (Type type in upgradeTypes)
            {
                MethodInfo method = type.GetMethod(nameof(BaseUpgrade.RegisterTerminalNode), BindingFlags.Static | BindingFlags.Public);
                CustomTerminalNode node = (CustomTerminalNode)method.Invoke(null, null);
                if (node != null) terminalNodes.Add(node);
            }

            terminalNodes.Sort();
            ItemProgressionManager.InitializeContributionValues();
            ItemProgressionManager.InitializeBlacklistItems();
            ItemProgressionManager.InitializeApparatusItems();
        }
        /// <summary>
        /// Generic function where it adds a terminal node for an upgrade that can be purchased multiple times
        /// </summary>
        /// <param name="upgradeName"> Name of the upgrade </param>
        /// <param name="shareStatus"> Wether the upgrade is shared through all players or only for the player who purchased it</param>
        /// <param name="enabled"> Wether the upgrade is enabled for gameplay or not</param>
        /// <param name="initialPrice"> The initial price when purchasing the upgrade for the first time</param>
        /// <param name="prices"> Prices for any subsequent purchases of the upgrade</param>
        internal CustomTerminalNode SetupMultiplePurchasableTerminalNode(string upgradeName,
                                                        bool shareStatus,
                                                        bool enabled,
                                                        int initialPrice,
                                                        int[] prices,
                                                        string overrideName = "",
                                                        bool alternateCurrency = true,
                                                        PurchaseMode purchaseMode = default,
                                                        bool refundable = true,
                                                        float refundPercentage = 1f
                                                        )
        {
            GameObject multiPerk = AssetBundleHandler.GetPerkGameObject(upgradeName);
			return SetupMultiplePurchasableTerminalNode(upgradeName, shareStatus, enabled, initialPrice, prices, overrideName, multiPerk, alternateCurrency, purchaseMode, refundable, refundPercentage);
        }
        internal CustomTerminalNode SetupMultiplePurchasableTerminalNode(string upgradeName,
                                                                        bool shareStatus,
                                                                        bool enabled,
                                                                        int initialPrice,
                                                                        int[] prices,
                                                                        string overrideName,
                                                                        GameObject prefab,
                                                                        bool alternateCurrency = true,
                                                                        PurchaseMode purchaseMode = default,
                                                                        bool refundable = false,
                                                                        float refundPercentage = 1f)
        {
            if (prefab == null) return null;
            if (!enabled) return null;

            string moreInfo = SetupUpgradeInfo(upgrade: prefab.GetComponent<BaseUpgrade>(), price: initialPrice, incrementalPrices: prices);
            if (UpgradeBus.Instance.PluginConfiguration.SHOW_WORLD_BUILDING_TEXT && prefab.GetComponent<BaseUpgrade>() is IUpgradeWorldBuilding component) moreInfo += "\n\n" + component.GetWorldBuildingText(shareStatus) + "\n";

            return new TierTerminalNode(
                name: overrideName != "" ? overrideName : upgradeName,
                unlockPrice: initialPrice,
                description: moreInfo,
                prefab: prefab,
                prices: prices,
                maxUpgrade: prices.Length,
                originalName: upgradeName,
                sharedUpgrade: shareStatus,
                alternateCurrency: alternateCurrency,
                purchaseMode: purchaseMode,
                refundable: refundable,
                refundPercentage: refundPercentage);
        }
        public CustomTerminalNode SetupMultiplePurchaseableTerminalNode(string upgradeName, ITierUpgradeConfiguration configuration)
        {
            GameObject multiPerk = AssetBundleHandler.GetPerkGameObject(upgradeName);
            return SetupMultiplePurchaseableTerminalNode(upgradeName, configuration, multiPerk);
        }
        public CustomTerminalNode SetupMultiplePurchaseableTerminalNode(string upgradeName, ITierUpgradeConfiguration configuration, GameObject prefab)
        {
            bool shareStatus = configuration is not IIndividualUpgradeConfiguration individualConfig || (PluginConfiguration.SHARED_UPGRADES || !individualConfig.Individual);
            int[] prices = ParseUpgradePrices(configuration.Prices);
            int initialPrice = prices.Length > 0 ? prices[0] : -1;
            int[] incrementalPrices = prices.Length > 1 ? prices[1..] : [];
            PurchaseMode purchaseMode = PluginConfiguration.AlternativeCurrencyConfiguration.EnableGlobalPurchase ? PluginConfiguration.AlternativeCurrencyConfiguration.GlobalPurchaseMode : configuration.PurchaseMode;
			bool refundable = PluginConfiguration.REFUND_UPGRADES || configuration.Refundable;
			float refundPercentage = configuration.RefundPercentage / 100f;

			return SetupMultiplePurchasableTerminalNode(upgradeName,
                shareStatus: shareStatus,
                enabled: configuration.Enabled,
                initialPrice: initialPrice,
                prices: incrementalPrices,
                overrideName: PluginConfiguration.OVERRIDE_UPGRADE_NAMES ? configuration.OverrideName : "",
                prefab: prefab,
                alternateCurrency: CurrencyManager.Enabled,
                purchaseMode: purchaseMode,
				refundable: refundable,
				refundPercentage: refundPercentage);
        }
        public CustomTerminalNode SetupOneTimeTerminalNode(string upgradeName, IOneTimeUpgradeConfiguration configuration)
        {
            GameObject oneTimePerk = AssetBundleHandler.GetPerkGameObject(upgradeName);
            return SetupOneTimeTerminalNode(upgradeName, configuration, oneTimePerk);
        }
        public CustomTerminalNode SetupOneTimeTerminalNode(string upgradeName, IOneTimeUpgradeConfiguration configuration, GameObject prefab)
        {
            bool shareStatus = configuration is not IIndividualUpgradeConfiguration individualConfig || (PluginConfiguration.SHARED_UPGRADES || !individualConfig.Individual);
			PurchaseMode purchaseMode = PluginConfiguration.AlternativeCurrencyConfiguration.EnableGlobalPurchase ? PluginConfiguration.AlternativeCurrencyConfiguration.GlobalPurchaseMode : configuration.PurchaseMode;
            bool refundable = PluginConfiguration.REFUND_UPGRADES || configuration.Refundable;
            float refundPercentage = configuration.RefundPercentage / 100f;

			return SetupOneTimeTerminalNode(upgradeName,
                shareStatus: shareStatus,
                enabled: configuration.Enabled,
                price: configuration.Price,
                overrideName: PluginConfiguration.OVERRIDE_UPGRADE_NAMES ? configuration.OverrideName : "",
                prefab: prefab,
                alternateCurrency: CurrencyManager.Enabled,
                purchaseMode: purchaseMode,
                refundable: refundable,
                refundPercentage: refundPercentage
				);
        }
        /// <summary>
        /// Generic function where it adds a terminal node for an upgrade that can only be bought once
        /// </summary>
        /// <param name="upgradeName"> Name of the upgrade</param>
        /// <param name="shareStatus"> Wether the upgrade is shared through all players or only for the player who purchased it</param>
        /// <param name="enabled"> Wether the upgrade is enabled for gameplay or not</param>
        /// <param name="price"></param>
        internal CustomTerminalNode SetupOneTimeTerminalNode(string upgradeName,
                                              bool shareStatus,
                                              bool enabled,
                                              int price,
                                              string overrideName,
                                              GameObject prefab,
                                              bool alternateCurrency = true,
                                              PurchaseMode purchaseMode = default,
                                              bool refundable = false,
                                              float refundPercentage = 1f
                                              )
        {
            if (!enabled) return null;
            string info = SetupUpgradeInfo(upgrade: prefab.GetComponent<BaseUpgrade>(), price: price);
            string moreInfo = info;
            if (UpgradeBus.Instance.PluginConfiguration.SHOW_WORLD_BUILDING_TEXT && prefab.GetComponent<BaseUpgrade>() is IUpgradeWorldBuilding component) moreInfo += "\n\n" + component.GetWorldBuildingText(shareStatus) + "\n";

            return new OneTimeTerminalNode(
                name: overrideName != "" ? overrideName : upgradeName,
                unlockPrice: price,
                description: moreInfo,
                prefab: prefab,
                originalName: upgradeName,
                sharedUpgrade: shareStatus,
                alternateCurrency: alternateCurrency,
                purchaseMode: purchaseMode,
                refundable: refundable,
                refundPercentage: refundPercentage);
        }

        public string SetupUpgradeInfo(BaseUpgrade upgrade = null, int price = -1, int[] incrementalPrices = null)
        {
            string info = "";
            if (upgrade is OneTimeUpgrade upgradeInfo) info += upgradeInfo.GetDisplayInfo(price) + "\n";
            if (upgrade is TierUpgrade tierUpgradeInfo) info += tierUpgradeInfo.GetDisplayInfo(initialPrice: price, maxLevels: incrementalPrices.Length, incrementalPrices: incrementalPrices);
            return info;
        }
        /// <summary>
        /// Function which parses the prices present in a given string and inserts them into an array of integers
        /// </summary>
        /// <param name="upgradePrices">The string which contains the list of prices</param>
        /// <returns>An array of integers with the values that were present in the string</returns>
        internal static int[] ParseUpgradePrices(string upgradePrices)
        {
            string[] priceString = [.. upgradePrices.Split(',')];
            int[] prices = new int[priceString.Length];

            for (int i = 0; i < priceString.Length; i++)
            {
                if (int.TryParse(priceString[i], out int price))
                {
                    prices[i] = price;
                }
                else
                {
                    logger.LogWarning($"Invalid upgrade price submitted: {prices[i]}");
                    prices[i] = -1;
                }
            }
            if (prices.Length == 1 && prices[0] == -1) { prices = []; }
            return prices;
        }

        internal void SetConfiguration(LategameConfiguration config)
        {
            PluginConfiguration = config;
        }
    }
}