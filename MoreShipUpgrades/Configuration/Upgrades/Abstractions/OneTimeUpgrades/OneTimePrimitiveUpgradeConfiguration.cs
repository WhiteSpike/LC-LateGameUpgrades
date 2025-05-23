﻿using BepInEx.Configuration;
using CSync.Lib;
using MoreShipUpgrades.Configuration.Upgrades.Interfaces.OneTimeUpgrades;

namespace MoreShipUpgrades.Configuration.Upgrades.Abstractions.OneTimeUpgrades
{
    public class OneTimePrimitiveUpgradeConfiguration<T> : OneTimeUpgradeConfiguration, IOneTimeEffectUpgrade<T>
    {
        public OneTimePrimitiveUpgradeConfiguration(ConfigFile cfg, string topSection, string enabledDescription, int defaultPrice) : base(cfg, topSection, enabledDescription, defaultPrice)
        {
        }

        [field: SyncedEntryField] public SyncedEntry<T> Effect { get; set; }
    }
}
