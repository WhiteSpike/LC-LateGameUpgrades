﻿using MoreShipUpgrades.Configuration;
using MoreShipUpgrades.Managers;
using MoreShipUpgrades.Misc.Upgrades;
using MoreShipUpgrades.UI.TerminalNodes;
using MoreShipUpgrades.UpgradeComponents.Interfaces;

namespace MoreShipUpgrades.UpgradeComponents.OneTimeUpgrades.Ship
{
    class FastEncryption : OneTimeUpgrade, IUpgradeWorldBuilding
    {
        public const string UPGRADE_NAME = "Fast Encryption";
        internal const string WORLD_BUILDING_TEXT = "\n\nA small tweak for the signal translator that optimizes the way it compresses text over the Proprietary Network." +
            " Don't forget that the Company keeps records of all department transmissions for training purposes, and that your on-the-job conversations may be subject to playback and review by your Employer.\n\n";
        public static FastEncryption instance;
        public override bool CanInitializeOnStart => GetConfiguration().FastEncryptionConfiguration.Price.Value <= 0;

        const float TRANSMIT_MULTIPLIER = 0.2f;

        public string GetWorldBuildingText(bool shareStatus = false)
        {
            return WORLD_BUILDING_TEXT;
        }
        void Awake()
        {
            upgradeName = UPGRADE_NAME;
            overridenUpgradeName = GetConfiguration().FastEncryptionConfiguration.OverrideName;
            instance = this;
        }
        public static int GetLimitOfCharactersTransmit(int defaultLimit, string message)
        {
            if (!GetActiveUpgrade(UPGRADE_NAME)) return defaultLimit;
            return message.Length;
        }
        public static float GetMultiplierOnSignalTextTimer(float defaultMultiplier)
        {
            if (!GetActiveUpgrade(UPGRADE_NAME)) return defaultMultiplier;
            return defaultMultiplier * TRANSMIT_MULTIPLIER;
        }
        public override string GetDisplayInfo(int price = -1)
        {
            return $"{GetUpgradePrice(price, GetConfiguration().FastEncryptionConfiguration.PurchaseMode)} - The transmitter will write the letters faster and the restriction of characters will be lifted.";
        }
        public new static (string, string[]) RegisterScrapToUpgrade()
        {
            return (UPGRADE_NAME, GetConfiguration().FastEncryptionConfiguration.ItemProgressionItems.Value.Split(","));
        }
        public new static void RegisterUpgrade()
        {
            SetupGenericPerk<FastEncryption>(UPGRADE_NAME);
        }
        public new static CustomTerminalNode RegisterTerminalNode()
        {
            return UpgradeBus.Instance.SetupOneTimeTerminalNode(UPGRADE_NAME, GetConfiguration().FastEncryptionConfiguration);
        }
    }
}
