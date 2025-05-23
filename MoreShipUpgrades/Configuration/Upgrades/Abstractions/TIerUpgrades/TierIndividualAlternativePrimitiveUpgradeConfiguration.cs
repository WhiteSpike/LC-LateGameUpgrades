﻿using BepInEx.Configuration;
using CSync.Extensions;
using CSync.Lib;
using MoreShipUpgrades.Configuration.Upgrades.Interfaces;
using MoreShipUpgrades.Misc.Upgrades;

namespace MoreShipUpgrades.Configuration.Upgrades.Abstractions.TIerUpgrades
{
    public class TierIndividualPrimitiveUpgradeConfiguration<T> : TierPrimitiveUpgradeConfiguration<T>, IIndividualUpgradeConfiguration
    {
        [field: SyncedEntryField] public SyncedEntry<bool> Individual { get; set; }
        public TierIndividualPrimitiveUpgradeConfiguration(ConfigFile cfg, string topSection, string enabledDescription, string defaultPrices) : base(cfg, topSection, enabledDescription, defaultPrices)
        {
            Individual = cfg.BindSyncedEntry(topSection,
            BaseUpgrade.INDIVIDUAL_SECTION,
            BaseUpgrade.INDIVIDUAL_DEFAULT,
            BaseUpgrade.INDIVIDUAL_DESCRIPTION);
        }
    }
}
