﻿using BepInEx.Logging;
using HarmonyLib;
using MoreShipUpgrades.Managers;
using MoreShipUpgrades.Misc;
using MoreShipUpgrades.Misc.Upgrades;
using UnityEngine;

namespace MoreShipUpgrades.Patches.NetworkManager
{
    [HarmonyPatch(typeof(GameNetworkManager))]
    internal static class GameNetworkManagerPatcher
    {
        static LguLogger logger = new LguLogger(nameof(GameNetworkManagerPatcher));
        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameNetworkManager.Disconnect))]
        static void ResetUpgradeBus()
        {
            logger.LogDebug("Resetting the Upgrade Bus due to disconnecting...");
            UpgradeBus.Instance.ResetAllValues();
            BaseUpgrade[] upgradeObjects = Object.FindObjectsOfType<BaseUpgrade>();
            foreach (BaseUpgrade upgrade in upgradeObjects)
            {
                Object.Destroy(upgrade.gameObject);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameNetworkManager.Disconnect))]
        static void ResetCredits()
        {
            if (!GameNetworkManager.Instance.isHostingGame) return;
            Terminal terminal = UpgradeBus.Instance.GetTerminal();
            terminal.groupCredits += PlayerManager.instance.upgradeSpendCredits;
            PlayerManager.instance.ResetUpgradeSpentCredits();
        }
    }
}
