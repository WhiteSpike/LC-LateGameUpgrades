using MoreShipUpgrades.Managers;
using MoreShipUpgrades.UpgradeComponents.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace MoreShipUpgrades.UpgradeComponents.Items.BarbedWire
{
    internal class BarbedWire : BaseBarbedWire, IDisplayInfo
    {
        public string GetDisplayInfo()
        {
            string slowDownEnemies = $"slow down by {(1f - UpgradeBus.instance.cfg.BARBED_WIRE_SLOW_MULTIPLIER.Value) / 100f}%";
            string damageEnemies = $"damage equivalent to {UpgradeBus.instance.cfg.BARBED_WIRE_DAMAGE_AMOUNT.Value} force";
            string stunEnemies = $"stun for {UpgradeBus.instance.cfg.BARBED_WIRE_STUN_TIME.Value} seconds";
            string slowdownPlayers = $"slow down by {(1f - UpgradeBus.instance.cfg.BARBED_WIRE_SLOW_PLAYER_MULTIPLIER.Value) / 100f}%";
            string damagePlayers = $"deal {UpgradeBus.instance.cfg.BARBED_WIRE_DAMAGE_PLAYER_AMOUNT} damage";
            return  $"A kit of barbed wire which can {(UpgradeBus.instance.cfg.BARBED_WIRE_SLOW_ENEMIES.Value ? $"{slowDownEnemies}" : "")}" +
                    $"{(UpgradeBus.instance.cfg.BARBED_WIRE_DAMAGE_ENEMIES.Value ? UpgradeBus.instance.cfg.BARBED_WIRE_SLOW_ENEMIES.Value ? !UpgradeBus.instance.cfg.BARBED_WIRE_STUN_ENEMIES.Value ? $"and {damageEnemies}" : $", {damageEnemies}" : $"{damageEnemies}" : "")}" +
                    $"{(UpgradeBus.instance.cfg.BARBED_WIRE_STUN_ENEMIES.Value ? UpgradeBus.instance.cfg.BARBED_WIRE_DAMAGE_ENEMIES.Value || UpgradeBus.instance.cfg.BARBED_WIRE_SLOW_ENEMIES.Value ? $"and {stunEnemies}" : $"{stunEnemies}" : "")}" +
                    $"{(UpgradeBus.instance.cfg.BARBED_WIRE_SLOW_ENEMIES.Value || UpgradeBus.instance.cfg.BARBED_WIRE_DAMAGE_ENEMIES.Value || UpgradeBus.instance.cfg.BARBED_WIRE_STUN_ENEMIES.Value ? "enemies" : "")}" +
                    $"{(UpgradeBus.instance.cfg.BARBED_WIRE_SLOW_PLAYERS.Value || UpgradeBus.instance.cfg.BARBED_WIRE_DAMAGE_PLAYERS.Value ? "but also " : "")}" +
                    $"{(UpgradeBus.instance.cfg.BARBED_WIRE_SLOW_PLAYERS.Value ? $" {slowdownPlayers}" : "")}" +
                    $"{(UpgradeBus.instance.cfg.BARBED_WIRE_DAMAGE_PLAYERS.Value ? UpgradeBus.instance.cfg.BARBED_WIRE_SLOW_PLAYERS.Value ? $" and {damagePlayers}" : $"{damagePlayers}" : "")}" +
                    $"{(!UpgradeBus.instance.cfg.BARBED_WIRE_SLOW_ENEMIES.Value && !UpgradeBus.instance.cfg.BARBED_WIRE_DAMAGE_ENEMIES.Value && !UpgradeBus.instance.cfg.BARBED_WIRE_STUN_ENEMIES.Value && !UpgradeBus.instance.cfg.BARBED_WIRE_SLOW_PLAYERS.Value && !UpgradeBus.instance.cfg.BARBED_WIRE_DAMAGE_PLAYERS.Value ? " do nothing." : ".")}";
        }

        public override void Start()
        {
            base.Start();
            slowDownEnemies = UpgradeBus.instance.cfg.BARBED_WIRE_SLOW_ENEMIES.Value;
            slowEnemiesMultiplier = UpgradeBus.instance.cfg.BARBED_WIRE_SLOW_MULTIPLIER.Value;
            damageEnemies = UpgradeBus.instance.cfg.BARBED_WIRE_DAMAGE_ENEMIES.Value;
            amountDamageEnemies = UpgradeBus.instance.cfg.BARBED_WIRE_DAMAGE_AMOUNT.Value;
            stunEnemies = UpgradeBus.instance.cfg.BARBED_WIRE_STUN_ENEMIES.Value;
            stunTimer = UpgradeBus.instance.cfg.BARBED_WIRE_STUN_TIME.Value;

            slowDownPlayers = UpgradeBus.instance.cfg.BARBED_WIRE_SLOW_PLAYERS.Value;
            slowPlayersMultiplier = UpgradeBus.instance.cfg.BARBED_WIRE_SLOW_PLAYER_MULTIPLIER.Value;
            damagePlayers = UpgradeBus.instance.cfg.BARBED_WIRE_DAMAGE_PLAYERS.Value;
            amountDamagePlayers = UpgradeBus.instance.cfg.BARBED_WIRE_DAMAGE_PLAYER_AMOUNT.Value;

            consumes = UpgradeBus.instance.cfg.BARBED_WIRE_MAXIMUM_USES.Value;
        }
    }
}
