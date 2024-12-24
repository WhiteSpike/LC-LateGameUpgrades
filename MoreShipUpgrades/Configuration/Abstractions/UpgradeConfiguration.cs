﻿using BepInEx.Configuration;
using CSync.Extensions;
using CSync.Lib;
using MoreShipUpgrades.Configuration.Interfaces;
using MoreShipUpgrades.Misc.Util;

namespace MoreShipUpgrades.Configuration.Abstractions
{
    public abstract class UpgradeConfiguration(ConfigFile cfg, string topSection, string enabledDescription) : IUpgradeConfiguration
    {
        [field: SyncedEntryField] public SyncedEntry<bool> Enabled { get; set; } = cfg.BindSyncedEntry(topSection, string.Format(LguConstants.ENABLED_FORMAT, topSection), true, enabledDescription);
        [field: SyncedEntryField] public SyncedEntry<int> MinimumSalePercentage { get; set; } = cfg.BindSyncedEntry(topSection, "Minimum Sale Percentage", 60, "Minimum percentage achieved when the upgrade goes on sale");
        [field: SyncedEntryField] public SyncedEntry<int> MaximumSalePercentage { get; set; } = cfg.BindSyncedEntry(topSection, "Maximum Sale Percentage", 90, "Maximum percentage achieved when the upgrade goes on sale");
        [field: SyncedEntryField] public SyncedEntry<string> OverrideName { get; set; } = cfg.BindSyncedEntry(topSection, string.Format(LguConstants.OVERRIDE_NAME_KEY_FORMAT, topSection), topSection);
        [field: SyncedEntryField] public SyncedEntry<string> ItemProgressionItems { get; set; } = cfg.BindSyncedEntry(topSection, LguConstants.ITEM_PROGRESSION_ITEMS_KEY, LguConstants.ITEM_PROGRESSION_ITEMS_DEFAULT, LguConstants.ITEM_PROGRESSION_ITEMS_DESCRIPTION);
    }
}