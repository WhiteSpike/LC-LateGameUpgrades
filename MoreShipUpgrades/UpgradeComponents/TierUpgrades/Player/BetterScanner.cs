﻿using MoreShipUpgrades.Compat;
using MoreShipUpgrades.Managers;
using MoreShipUpgrades.Misc;
using MoreShipUpgrades.Misc.TerminalNodes;
using MoreShipUpgrades.Misc.Upgrades;
using MoreShipUpgrades.UpgradeComponents.Interfaces;
using System.Text;

namespace MoreShipUpgrades.UpgradeComponents.TierUpgrades.Player
{
    class BetterScanner : TierUpgrade, IUpgradeWorldBuilding
    {
        public const string UPGRADE_NAME = "Better Scanner";
        internal const string WORLD_BUILDING_TEXT = "\n\nA serialized Company-Issue Magazine subscription, called 'Stuff Finders'." +
            " Uniquely, {0} must subscribe to each issue of 'Stuff Finders' individually. Each separate subscription promises and delivers a weekly issuance" +
            " of a magazine with the exact same information in it as last time, organized in a different order and with slightly different printing qualities each time." +
            " There are only three 'unique' issues, and each issue only has one or two pieces of actual useful information in it. The rest of the magazine is just ads" +
            " for The Company's other offerings. There is an extra fee for cancelling a subscription of 'Stuff Finders' before terminating your employment." +
            " The useful information always comes in the form of an unlabelled service key or Ship terminal hyperlink.\n\n";
        internal override void Start()
        {
            upgradeName = UPGRADE_NAME;
            overridenUpgradeName = UpgradeBus.Instance.PluginConfiguration.BETTER_SCANNER_OVERRIDE_NAME;
            base.Start();
        }

        public override void Load()
        {
            base.Load();
            if (GoodItemScanCompat.Enabled)
            {
                int level = GetUpgradeLevel(UPGRADE_NAME);
                if (level > 0)
                {
                    GoodItemScanCompat.IncreaseScanDistance((int)UpgradeBus.Instance.PluginConfiguration.NODE_DISTANCE_INCREASE);
                    GoodItemScanCompat.IncreaseEnemyScanDistance((int)UpgradeBus.Instance.PluginConfiguration.NODE_DISTANCE_INCREASE);
                }
                if (level == 2) GoodItemScanCompat.ToggleScanThroughWalls(true);
            }
        }

        public override void Increment()
        {
            base.Increment();
            if (GoodItemScanCompat.Enabled)
            {
                int level = GetUpgradeLevel(UPGRADE_NAME);
                if (level == 2) GoodItemScanCompat.ToggleScanThroughWalls(true);
            }
        }

        public static void AddScannerNodeToValve(ref SteamValveHazard steamValveHazard)
        {
            if (!GetActiveUpgrade(UPGRADE_NAME)) return;
            LguScanNodeProperties.AddGeneralScanNode(objectToAddScanNode: steamValveHazard.gameObject, header: "Bursted Steam Valve", subText: "Fix it to get rid of the steam", minRange: 3);
        }

        public static void RemoveScannerNodeFromValve(ref SteamValveHazard steamValveHazard)
        {
            LguScanNodeProperties.RemoveScanNode(steamValveHazard.gameObject);
        }
        public static string GetBetterScannerInfo(int level, int price)
        {
            switch (level)
            {
                case 1: return string.Format(AssetBundleHandler.GetInfoFromJSON("Better Scanner1"), level, price, UpgradeBus.Instance.PluginConfiguration.NODE_DISTANCE_INCREASE.Value, UpgradeBus.Instance.PluginConfiguration.SHIP_AND_ENTRANCE_DISTANCE_INCREASE.Value);
                case 2: return string.Format(AssetBundleHandler.GetInfoFromJSON("Better Scanner2"), level, price);
                case 3:
                    {
                        string result = string.Format(AssetBundleHandler.GetInfoFromJSON("Better Scanner3"), level, price, UpgradeBus.Instance.PluginConfiguration.BETTER_SCANNER_ENEMIES.Value ? " and enemies" : "");
                        result += "hives and scrap command display the location of the most valuable hives and scrap on the map.\n";
                        return result;
                    }
            }
            return "";
        }

        public string GetWorldBuildingText(bool shareStatus = false)
        {
            return string.Format(WORLD_BUILDING_TEXT, shareStatus ? "a department" : "one");
        }

        public override string GetDisplayInfo(int initialPrice = -1, int maxLevels = -1, int[] incrementalPrices = null)
        {
            StringBuilder stringBuilder = new();
            stringBuilder.Append(GetBetterScannerInfo(1, initialPrice));
            for (int i = 0; i < maxLevels; i++)
                stringBuilder.Append(GetBetterScannerInfo(i + 2, incrementalPrices[i]));
            return stringBuilder.ToString();
        }

        public override bool CanInitializeOnStart => UpgradeBus.Instance.PluginConfiguration.BETTER_SCANNER_PRICE.Value <= 0 &&
                UpgradeBus.Instance.PluginConfiguration.BETTER_SCANNER_PRICE2.Value <= 0 &&
                UpgradeBus.Instance.PluginConfiguration.BETTER_SCANNER_PRICE3.Value <= 0;
        public new static void RegisterUpgrade()
        {
            SetupGenericPerk<BetterScanner>(UPGRADE_NAME);
        }
        public new static (string, string[]) RegisterScrapToUpgrade()
        {
            return (UPGRADE_NAME, UpgradeBus.Instance.PluginConfiguration.BETTER_SCANNER_ITEM_PROGRESSION_ITEMS.Value.Split(","));
        }
        public new static CustomTerminalNode RegisterTerminalNode()
        {
            LategameConfiguration configuration = UpgradeBus.Instance.PluginConfiguration;

            return UpgradeBus.Instance.SetupMultiplePurchasableTerminalNode(UPGRADE_NAME,
                                                configuration.SHARED_UPGRADES.Value || !configuration.BETTER_SCANNER_INDIVIDUAL.Value,
                                                configuration.BETTER_SCANNER_ENABLED.Value,
                                                configuration.BETTER_SCANNER_PRICE.Value,
                                                [configuration.BETTER_SCANNER_PRICE2.Value, configuration.BETTER_SCANNER_PRICE3.Value],
                                                configuration.OVERRIDE_UPGRADE_NAMES ? configuration.BETTER_SCANNER_OVERRIDE_NAME : ""
                                                );
        }
    }
}
