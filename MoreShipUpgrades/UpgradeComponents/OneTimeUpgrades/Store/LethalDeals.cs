﻿using MoreShipUpgrades.Managers;
using MoreShipUpgrades.Misc;
using MoreShipUpgrades.Misc.Upgrades;
using MoreShipUpgrades.UI.TerminalNodes;
using MoreShipUpgrades.UpgradeComponents.Interfaces;

namespace MoreShipUpgrades.UpgradeComponents.OneTimeUpgrades.Store
{
    internal class LethalDeals : OneTimeUpgrade, IUpgradeWorldBuilding
    {
        public const string UPGRADE_NAME = "Lethal Deals";
        internal const string WORLD_BUILDING_TEXT = "\n\nThe Company Store Loyalty Rewards Program. For a small fee and all of your personally-identifying information," +
            " you will be made eligible for one GUARANTEED random discount each Store rotation. Comes with a little plastic card for your keychain." +
            " There's a barcode on it, but the Terminal has nothing to scan the barcode with, so who knows what it's really for.\n\n";
        const int GUARANTEED_ITEMS_AMOUNT = 1;
        public override bool CanInitializeOnStart => GetConfiguration().LETHAL_DEALS_PRICE.Value <= 0;
        public string GetWorldBuildingText(bool shareStatus = false)
        {
            return WORLD_BUILDING_TEXT;
        }
        void Awake()
        {
            upgradeName = UPGRADE_NAME;
            overridenUpgradeName = GetConfiguration().LETHAL_DEALS_OVERRIDE_NAME;
        }
        public static int GetLethalDealsGuaranteedItems(int amount)
        {
            if (!GetActiveUpgrade(UPGRADE_NAME)) return amount;
            return GUARANTEED_ITEMS_AMOUNT;
        }
        public override string GetDisplayInfo(int price = -1)
        {
            return $"${price} - Guarantees at least one item will be on sale in the store.";
        }
        public new static (string, string[]) RegisterScrapToUpgrade()
        {
            return (UPGRADE_NAME, GetConfiguration().LETHAL_DEALS_ITEM_PROGRESSION_ITEMS.Value.Split(","));
        }
        public new static void RegisterUpgrade()
        {
            SetupGenericPerk<LethalDeals>(UPGRADE_NAME);
        }
        public new static CustomTerminalNode RegisterTerminalNode()
        {
            LategameConfiguration configuration = GetConfiguration();

            return UpgradeBus.Instance.SetupOneTimeTerminalNode(UPGRADE_NAME,
                                    shareStatus: true,
                                    configuration.LETHAL_DEALS_ENABLED.Value,
                                    configuration.LETHAL_DEALS_PRICE.Value,
                                    configuration.OVERRIDE_UPGRADE_NAMES ? configuration.LETHAL_DEALS_OVERRIDE_NAME : "");
        }
    }
}
