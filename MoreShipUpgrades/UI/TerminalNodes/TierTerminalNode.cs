﻿using UnityEngine;

namespace MoreShipUpgrades.UI.TerminalNodes
{
    internal class TierTerminalNode : CustomTerminalNode
    {
        public TierTerminalNode(string name, int unlockPrice, string description, GameObject prefab, int[] prices, int maxUpgrade, string originalName = "", bool sharedUpgrade = false, bool alternateCurrency = true, PurchaseMode purchaseMode = default) : base(name, unlockPrice, description, prefab, prices, maxUpgrade, originalName: originalName, sharedUpgrade: sharedUpgrade, alternateCurrency: alternateCurrency, purchaseMode: purchaseMode)
        {

        }
    }
}
