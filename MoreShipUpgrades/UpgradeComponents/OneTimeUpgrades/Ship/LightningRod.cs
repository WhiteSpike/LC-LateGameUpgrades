﻿using MoreShipUpgrades.Configuration;
using MoreShipUpgrades.Managers;
using MoreShipUpgrades.Misc;
using MoreShipUpgrades.Misc.Upgrades;
using MoreShipUpgrades.UI.TerminalNodes;
using MoreShipUpgrades.UpgradeComponents.Interfaces;
using Unity.Netcode;
using UnityEngine;

namespace MoreShipUpgrades.UpgradeComponents.OneTimeUpgrades
{
    public class LightningRod : OneTimeUpgrade, IUpgradeWorldBuilding
    {
        public const string UPGRADE_NAME = "Lightning Rod";
        internal const string WORLD_BUILDING_TEXT = "\n\nService key for the Ship's terminal which allows your crew to legally use the Ship's 'Static Attraction Field' module." +
            " Comes with a list of opt-in maintenance procedures that promise to optimize the module's function and field of influence. This Company-issued document " +
            "is saddled with the uniquely-awkward task of having to ransom a safety feature back to the employee in text while not also admitting to the existence of" +
            " an occupational hazard that was previously denied in court.\n\n";
        public static LightningRod instance;
        StormyWeather StormyWeather;

        // Configuration
        public const string ENABLED_SECTION = $"Enable {UPGRADE_NAME} Upgrade";
        public const bool ENABLED_DEFAULT = true;
        public const string ENABLED_DESCRIPTION = "A device which redirects lightning bolts to the ship.";

        public const string PRICE_SECTION = $"{UPGRADE_NAME} Price";
        public const int PRICE_DEFAULT = 1000;

        public const string ACTIVE_SECTION = "Active on Purchase";
        public const bool ACTIVE_DEFAULT = true;
        public const string ACTIVE_DESCRIPTION = $"If true: {UPGRADE_NAME} will be active on purchase.";

        // Toggle
        public const string ACCESS_DENIED_MESSAGE = $"You don't have access to this command yet. Purchase the '{UPGRADE_NAME}'.\n";
        public const string TOGGLE_ON_MESSAGE = $"{UPGRADE_NAME} has been enabled. Lightning bolts will now be redirected to the ship.\n";
        public const string TOGGLE_OFF_MESSAGE = $"{UPGRADE_NAME} has been disabled. Lightning bolts will no longer be redirected to the ship.\n";

        // distance
        public const string DIST_SECTION = $"Effective Distance of {UPGRADE_NAME}.";
        public const float DIST_DEFAULT = 175f;
        public const string DIST_DESCRIPTION = "The closer you are the more likely the rod will reroute lightning.";

        public const string UPGRADE_MODE_SECTION = $"Current Upgrade Mode for {UPGRADE_NAME}";
        public const UpgradeMode UPGRADE_MODE_DEFAULT = UpgradeMode.EffectiveRange;
        public const string UPGRADE_MODE_DESCRIPTION = "Supported Values:\n" +
                                                        "EffectiveRange: The closer the item is to the ship, the more likely the lightning directed to it will be redirected to the ship instead.\n" +
                                                        "AlwaysRerouteItem: Whenever an item is picked to be hit by lightning, the bolt will be redirected to the ship instead.\n" +
                                                        "AlwaysRerouteRandom: Whenever a random lightning bolt (not the one used to target items) is produced, the bolt will be redirected to the ship instead.\n" +
                                                        "AlwaysRerouteAll: Whenever any kind of lightning bolt is produced, the bolt will be redirected to the ship instead.\n";

        public static UpgradeMode CurrentUpgradeMode
        {
            get
            {
                return GetConfiguration().LightningRodConfiguration.AlternativeMode;
            }
        }

        public bool LightningIntercepted { get; internal set; }
        public override bool CanInitializeOnStart => GetConfiguration().LightningRodConfiguration.Price.Value <= 0;

        public enum UpgradeMode
        {
            EffectiveRange,
            AlwaysRerouteItem,
            AlwaysRerouteRandom,
            AlwaysRerouteAll
        }

        void Awake()
        {
            instance = this;
            upgradeName = UPGRADE_NAME;
            overridenUpgradeName = GetConfiguration().LightningRodConfiguration.OverrideName;
        }

        public static void TryInterceptLightning(ref StormyWeather __instance, ref GrabbableObject ___targetingMetalObject)
        {
            bool intercepted = false;
            switch(CurrentUpgradeMode)
            {
                case UpgradeMode.EffectiveRange:
                    {
                        if (___targetingMetalObject == null)
                        {
                            intercepted = false;
                            break;
                        }
                        Terminal terminal = UpgradeBus.Instance.GetTerminal();
                        float dist = Vector3.Distance(___targetingMetalObject.transform.position, terminal.transform.position);

                        if (dist > GetConfiguration().LightningRodConfiguration.Effect.Value) return;

                        dist /= GetConfiguration().LightningRodConfiguration.Effect.Value;
                        float prob = 1 - dist;
                        float rand = Random.value;
                        intercepted = rand < prob;
                        break;
                    }
                case UpgradeMode.AlwaysRerouteItem:
                    {
                        intercepted = ___targetingMetalObject != null;
                        break;
                    }
                case UpgradeMode.AlwaysRerouteRandom:
                    {
                        intercepted = ___targetingMetalObject == null;
                        break;
                    }
                case UpgradeMode.AlwaysRerouteAll:
                    {
                        intercepted = true;
                        break;
                    }
            }

            if (intercepted)
            {
                __instance.staticElectricityParticle.Stop();
                instance.LightningIntercepted = true;
                instance.CoordinateInterceptionClientRpc();
            }
        }
        public static void RerouteLightningBolt(ref Vector3 strikePosition, ref StormyWeather __instance)
        {
            Terminal terminal = UpgradeBus.Instance.GetTerminal();
            strikePosition = terminal.transform.position;
            instance.LightningIntercepted = false;
            __instance.staticElectricityParticle.gameObject.SetActive(true);
        }
        [ClientRpc]
        public void CoordinateInterceptionClientRpc()
        {
            LightningIntercepted = true;
            if (StormyWeather == null) StormyWeather = FindObjectOfType<StormyWeather>(true);
            StormyWeather.staticElectricityParticle.gameObject.SetActive(false);
        }

        public string GetWorldBuildingText(bool shareStatus = false)
        {
            return WORLD_BUILDING_TEXT;
        }

        public override string GetDisplayInfo(int price = -1)
        {
            return CurrentUpgradeMode switch
            {
                UpgradeMode.EffectiveRange => string.Format(AssetBundleHandler.GetInfoFromJSON(UPGRADE_NAME), GetUpgradePrice(price, GetConfiguration().LightningRodConfiguration.PurchaseMode), GetConfiguration().LightningRodConfiguration.Effect.Value),
                UpgradeMode.AlwaysRerouteItem => $"${GetUpgradePrice(price, GetConfiguration().LightningRodConfiguration.PurchaseMode)} - Reroutes all lightning bolts directed to metallic objects to the ship's lightning rod.",
                UpgradeMode.AlwaysRerouteRandom => $"${GetUpgradePrice(price, GetConfiguration().LightningRodConfiguration.PurchaseMode)} - Reroutes all non-targetting lightning bolts to the ship's lightning rod.",
                UpgradeMode.AlwaysRerouteAll => $"${GetUpgradePrice(price, GetConfiguration().LightningRodConfiguration.PurchaseMode)} - Reroutes all kind of lightning bolts to the ship's lightning rod",
                _ => string.Empty,
            };
        }
        public new static (string, string[]) RegisterScrapToUpgrade()
        {
            return (UPGRADE_NAME, GetConfiguration().LightningRodConfiguration.ItemProgressionItems.Value.Split(","));
        }
        public new static void RegisterUpgrade()
        {
            SetupGenericPerk<LightningRod>(UPGRADE_NAME);
        }
        public new static CustomTerminalNode RegisterTerminalNode()
        {
            return UpgradeBus.Instance.SetupOneTimeTerminalNode(UPGRADE_NAME,GetConfiguration().LightningRodConfiguration);
        }

        internal void ResetValues()
        {
            StormyWeather = null;
        }
    }
}
