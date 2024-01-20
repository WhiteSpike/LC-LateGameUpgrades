using MoreShipUpgrades.Managers;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoreShipUpgrades.UpgradeComponents.Items.BarbedWire
{
    internal class BarbedWire : BaseBarbedWire
    {
        public override void Start()
        {
            base.Start();
            slowDownEnemies = UpgradeBus.instance.cfg.BARBED_WIRE_SLOW_ENEMIES;
            slowEnemiesMultiplier = UpgradeBus.instance.cfg.BARBED_WIRE_SLOW_MULTIPLIER;
            damageEnemies = UpgradeBus.instance.cfg.BARBED_WIRE_DAMAGE_ENEMIES;
            amountDamageEnemies = UpgradeBus.instance.cfg.BARBED_WIRE_DAMAGE_AMOUNT;
            stunEnemies = UpgradeBus.instance.cfg.BARBED_WIRE_STUN_ENEMIES;
            stunTimer = UpgradeBus.instance.cfg.BARBED_WIRE_STUN_TIME;

            slowDownPlayers = UpgradeBus.instance.cfg.BARBED_WIRE_SLOW_PLAYERS;
            slowPlayersMultiplier = UpgradeBus.instance.cfg.BARBED_WIRE_SLOW_PLAYER_MULTIPLIER;
            damagePlayers = UpgradeBus.instance.cfg.BARBED_WIRE_DAMAGE_PLAYERS;
            amountDamagePlayers = UpgradeBus.instance.cfg.BARBED_WIRE_DAMAGE_PLAYER_AMOUNT;

            consumes = UpgradeBus.instance.cfg.BARBED_WIRE_MAXIMUM_USES;
        }
    }
}
