﻿using MoreShipUpgrades.Managers;
using MoreShipUpgrades.Misc.TerminalNodes;
using MoreShipUpgrades.Misc;
using MoreShipUpgrades.Misc.Upgrades;
using MoreShipUpgrades.Misc.Util;
using System;
using UnityEngine;

namespace MoreShipUpgrades.UpgradeComponents.TierUpgrades
{
    public class ClayGlasses : TierUpgrade
    {
        public const string UPGRADE_NAME = "Clay Glasses";
        internal const string PRICES_DEFAULT = "300,400,500,600";
        internal override void Start()
        {
            upgradeName = UPGRADE_NAME;
            overridenUpgradeName = UpgradeBus.Instance.PluginConfiguration.CLAY_GLASSES_OVERRIDE_NAME;
            base.Start();
        }
        public override bool CanInitializeOnStart
        {
            get
            {
                string[] prices = UpgradeBus.Instance.PluginConfiguration.CLAY_GLASSES_PRICES.Value.Split(',');
                return UpgradeBus.Instance.PluginConfiguration.CLAY_GLASSES_PRICE.Value <= 0 && prices.Length == 1 && (prices[0] == "" || prices[0] == "0");
            }
        }
        public static float GetAdditionalMaximumDistance(float defaultValue)
        {
            if (!UpgradeBus.Instance.PluginConfiguration.CLAY_GLASSES_ENABLED) return defaultValue;
            if (!GetActiveUpgrade(UPGRADE_NAME)) return defaultValue;
            float additionalValue = UpgradeBus.Instance.PluginConfiguration.CLAY_GLASSES_DISTANCE_INITIAL_INCREASE + (GetUpgradeLevel(UPGRADE_NAME) * UpgradeBus.Instance.PluginConfiguration.CLAY_GLASSES_DISTANCE_INCREMENTAL_INCREASE);
            return Mathf.Clamp(defaultValue + additionalValue, defaultValue, float.MaxValue);
        }

        public override string GetDisplayInfo(int initialPrice = -1, int maxLevels = -1, int[] incrementalPrices = null)
        {
            System.Func<int, float> infoFunction = level => UpgradeBus.Instance.PluginConfiguration.CLAY_GLASSES_DISTANCE_INITIAL_INCREASE.Value + (level * UpgradeBus.Instance.PluginConfiguration.CLAY_GLASSES_DISTANCE_INCREMENTAL_INCREASE.Value);
            const string infoFormat = "LVL {0} - ${1} - The maximum distance to spot a \"Clay Surgeon\" entity is increased by {2} additional units.\n";
            return Tools.GenerateInfoForUpgrade(infoFormat, initialPrice, incrementalPrices, infoFunction);
        }

        public new static (string, string[]) RegisterScrapToUpgrade()
        {
            return (UPGRADE_NAME, UpgradeBus.Instance.PluginConfiguration.CLAY_GLASSES_ITEM_PROGRESSION_ITEMS.Value.Split(","));
        }
        public new static void RegisterUpgrade()
        {
            SetupGenericPerk<ClayGlasses>(UPGRADE_NAME);
        }
        public new static CustomTerminalNode RegisterTerminalNode()
        {
            LategameConfiguration configuration = UpgradeBus.Instance.PluginConfiguration;

            return UpgradeBus.Instance.SetupMultiplePurchasableTerminalNode(UPGRADE_NAME,
                                                shareStatus: configuration.SHARED_UPGRADES.Value || !configuration.CLAY_GLASSES_INDIVIDUAL,
                                                configuration.CLAY_GLASSES_ENABLED.Value,
                                                configuration.CLAY_GLASSES_PRICE.Value,
                                                UpgradeBus.ParseUpgradePrices(configuration.CLAY_GLASSES_PRICES.Value),
                                                configuration.OVERRIDE_UPGRADE_NAMES ? configuration.CLAY_GLASSES_OVERRIDE_NAME : "");
        }

    }
}