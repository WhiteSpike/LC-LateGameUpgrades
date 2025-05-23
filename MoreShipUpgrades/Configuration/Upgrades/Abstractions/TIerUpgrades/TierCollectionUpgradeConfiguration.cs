﻿using BepInEx.Configuration;
using CSync.Lib;
using MoreShipUpgrades.Configuration.Upgrades.Interfaces.TierUpgrades;

namespace MoreShipUpgrades.Configuration.Upgrades.Abstractions.TIerUpgrades
{
    public class TierCollectionUpgradeConfiguration : TierUpgradeConfiguration, ITierCollectionUpgradeConfiguration
    {
        public TierCollectionUpgradeConfiguration(ConfigFile cfg, string topSection, string enabledDescription, string defaultPrices) : base(cfg, topSection, enabledDescription, defaultPrices)
        {
        }
        [field: SyncedEntryField] public SyncedEntry<string> TierCollection { get; set; }
    }
}
