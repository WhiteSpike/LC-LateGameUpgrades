﻿using BepInEx.Configuration;
using CSync.Lib;
using MoreShipUpgrades.Configuration.Upgrades.Abstractions.TIerUpgrades;
using MoreShipUpgrades.UpgradeComponents.TierUpgrades.Ship;

namespace MoreShipUpgrades.Configuration.Upgrades.Custom
{
    public class QuantumDisruptorUpgradeConfiguration : TierAlternativeMultiplePrimitiveUpgradeConfiguration<int, QuantumDisruptor.UpgradeModes>
    {
        [field: SyncedEntryField] public SyncedEntry<QuantumDisruptor.ResetModes> ResetMode { get; set; }
        public QuantumDisruptorUpgradeConfiguration(ConfigFile cfg, string topSection, string enabledDescription, string defaultPrices) : base(cfg, topSection, enabledDescription, defaultPrices)
        {
        }
    }
}
