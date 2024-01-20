using GameNetcodeStuff;
using MoreShipUpgrades.Managers;
using MoreShipUpgrades.Misc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MoreShipUpgrades.UpgradeComponents.Items.BarbedWire
{
    internal abstract class BaseBarbedWire : GrabbableObject
    {
        private static LGULogger logger = new LGULogger(nameof(BaseBarbedWire));
        protected bool slowDownEnemies;
        protected float slowEnemiesMultiplier;

        protected bool stunEnemies;
        protected float stunTimer;

        protected bool damageEnemies;
        protected int amountDamageEnemies;

        protected bool damagePlayers;
        protected int amountDamagePlayers;

        protected bool slowDownPlayers;
        protected float slowPlayersMultiplier;
        protected float previousMovementSpeed;

        protected int consumes;

        private bool prepared;
        private float radius;
        private InteractTrigger prepareBarbedWire;
        //private Animator animator;
        private List<EnemyAI> affectedEnemies;
        private bool affectingPlayer;

        private const string PREPARED = "Prepare";
        private const string PREPARE_INTERACT_MESSAGE = "Unfold Barbed Wire: [LMB]";
        private const string PICKUP_INTERACT_MESSAGE = "Fold Barbed Wire: [LMB]";

        public override void Start()
        {
            logger.LogDebug("Spawned in scene!");
            base.Start();
            UpgradeBus.instance.barbedWires.Add(this);
            //animator = GetComponent<Animator>();
            radius = 2f;

            prepared = false;
            foreach (InteractTrigger interact in GetComponentsInChildren<InteractTrigger>())
            {
                if (interact.name != "PrepareBarbedWire") continue;

                prepareBarbedWire = interact;
                break;
            }
            prepareBarbedWire.interactable = true;
            prepareBarbedWire.oneHandedItemAllowed = true;
            prepareBarbedWire.twoHandedItemAllowed = false;
            prepareBarbedWire.tag = nameof(InteractTrigger);
            prepareBarbedWire.interactCooldown = false;
            prepareBarbedWire.cooldownTime = 0;
            prepareBarbedWire.timeToHold = 2;
            prepareBarbedWire.timeToHoldSpeedMultiplier = 1f;

            prepareBarbedWire.hoverTip = PREPARE_INTERACT_MESSAGE;
            prepareBarbedWire.onInteract.AddListener(InteractBarbedWire);
            grabbable = true;
            grabbableToEnemies = true;
            affectedEnemies = new List<EnemyAI>();
            affectingPlayer = false;
        }

        public override void Update()
        {
            base.Update();
            if (!prepared) return;
            if (slowDownEnemies || stunEnemies || damageEnemies) CheckForEnemies();
            if (damagePlayers || slowDownPlayers) CheckForPlayers();
            if (consumes <= 0)
            {
                PlayerControllerB localPlayer = UpgradeBus.instance.GetLocalPlayer();
                if (affectingPlayer) RemovePlayerEffects(ref localPlayer);
                Destroy(gameObject);
            }
        }
        private void CheckForEnemies()
        {
            RaycastHit[] enemyColliders = new RaycastHit[10];
            int numEnemiesHit = Physics.SphereCastNonAlloc(transform.position, radius, transform.forward, enemyColliders, radius, 524288, QueryTriggerInteraction.Collide);
            for (int i = 0; i < numEnemiesHit; i++)
            {
                EnemyAICollisionDetect enemyCollision = enemyColliders[i].transform.GetComponent<EnemyAICollisionDetect>();
                if (enemyCollision == null) break;
                EnemyAI enemyAI = enemyCollision.mainScript;

                if (affectedEnemies.Contains(enemyAI)) continue;
                logger.LogDebug("Applying effects on enemy");
                if (damageEnemies) enemyAI.HitEnemy(amountDamageEnemies);
                if (stunEnemies) enemyAI.SetEnemyStunned(true, stunTimer);
                affectedEnemies.Add(enemyAI);
            }

            for (int i = 0; i < affectedEnemies.Count; i++)
            {
                EnemyAI affectedEnemy = affectedEnemies[i];
                bool leftRadius = true;
                for (int j = 0; j < numEnemiesHit && leftRadius; j++)
                {
                    EnemyAICollisionDetect enemyCollision = enemyColliders[i].transform.GetComponent<EnemyAICollisionDetect>();
                    if (enemyCollision == null) break;
                    EnemyAI enemyAI = enemyCollision.mainScript;
                    if (enemyAI == affectedEnemy) leftRadius = false;
                }
                if (!leftRadius) continue;
                affectedEnemies.Remove(affectedEnemy);
                consumes--;
            }
        }

        private void CheckForPlayers()
        {
            PlayerControllerB localPlayer = UpgradeBus.instance.GetLocalPlayer();
            if (localPlayer.isPlayerDead)
            {
                RemovePlayerEffects(ref localPlayer);
                return;
            }
            float distance = Vector3.Distance(localPlayer.transform.position, transform.position);

            if (distance <= radius)
            {
                if (affectingPlayer) return;
                affectingPlayer = true;
                logger.LogDebug("Applying effects on player");
                if (slowDownPlayers)
                {
                    previousMovementSpeed = localPlayer.movementSpeed;
                    logger.LogDebug($"Player movement speed before effect: {localPlayer.movementSpeed}");
                    localPlayer.movementSpeed *= slowPlayersMultiplier;
                    logger.LogDebug($"Player movement speed after effect: {localPlayer.movementSpeed}");
                }
                if (damagePlayers) localPlayer.DamagePlayer(amountDamagePlayers, false, true, CauseOfDeath.Unknown, 1, false, Vector3.zero);
            }
            else RemovePlayerEffects(ref localPlayer);
        }

        private void RemovePlayerEffects(ref PlayerControllerB localPlayer)
        {
            if (affectingPlayer && slowDownPlayers) localPlayer.movementSpeed /= slowPlayersMultiplier;
            affectingPlayer = false;
        }

        public override void DiscardItem()
        {
            logger.LogDebug("Being dropped...");
            base.DiscardItem();
            prepareBarbedWire.interactable = true;
            prepareBarbedWire.hoverTip = PREPARE_INTERACT_MESSAGE;
        }

        public override void EquipItem()
        {
            base.EquipItem();
        }

        public override void GrabItem()
        {
            logger.LogDebug("Being grabbed...");
            base.GrabItem();
            prepareBarbedWire.interactable = false;
            prepareBarbedWire.hoverTip = PICKUP_INTERACT_MESSAGE;
        }

        public override void PocketItem()
        {
            base.PocketItem();
        }

        void InteractBarbedWire(PlayerControllerB playerInteractor)
        {
            logger.LogDebug($"Being interacted by {playerInteractor}");
            if (prepared) MakeBarbedWirePickable();
            else PrepareBarbedWire();
        }

        void MakeBarbedWirePickable()
        {
            SetBarbedWirePrepare(false);
        }

        void PrepareBarbedWire()
        {
            SetBarbedWirePrepare(true);
        }

        void SetBarbedWirePrepare(bool enabled)
        {
            prepared = enabled;
            //animator.SetBool(PREPARED, enabled);
            grabbable = !enabled;
            grabbableToEnemies = !enabled;
            prepareBarbedWire.hoverTip = enabled ? PICKUP_INTERACT_MESSAGE : PREPARE_INTERACT_MESSAGE;
        }

        public static float CheckForBarbedWires(float defaultSpeed, EnemyAI instance)
        {
            List<BaseBarbedWire> barbedWires = UpgradeBus.instance.barbedWires;
            for (int i = 0; i < barbedWires.Count; i++)
            {
                BaseBarbedWire barbedWire = barbedWires[i];
                if (barbedWire == null)
                {
                    UpgradeBus.instance.barbedWires.RemoveAt(i);
                    continue;
                }
                /*
                RaycastHit[] enemyColliders = new RaycastHit[10];
                int numEnemiesHit = Physics.SphereCastNonAlloc(barbedWire.transform.position, barbedWire.radius, barbedWire.transform.forward, enemyColliders, barbedWire.radius, 524288, QueryTriggerInteraction.Collide);
                for (int j = 0; i < numEnemiesHit; i++)
                {
                    EnemyAICollisionDetect enemyCollision = enemyColliders[i].transform.GetComponent<EnemyAICollisionDetect>();
                    if (enemyCollision == null) break;
                    EnemyAI enemyAI = enemyCollision.mainScript;

                    if (enemyAI == instance) return defaultSpeed * barbedWires[i].slowEnemiesMultiplier;
                }
                */
                if (Vector3.Distance(instance.agent.transform.position, barbedWires[i].transform.position) <= barbedWires[i].radius)
                {
                    return defaultSpeed * barbedWires[i].slowEnemiesMultiplier;
                }
            }
            return defaultSpeed;
        }
    }
}
