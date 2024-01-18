using GameNetcodeStuff;
using MoreShipUpgrades.Managers;
using MoreShipUpgrades.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MoreShipUpgrades.UpgradeComponents.Items
{
    internal class BarbedWire : GrabbableObject
    {
        private static LGULogger logger = new LGULogger(nameof(BarbedWire));
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

        protected int consumes;
        protected float cooldown;

        private bool prepared;
        private float radius;
        private InteractTrigger prepareBarbedWire;
        private Animator animator;
        private List<EnemyAI> affectedEnemies;
        private List<PlayerControllerB> affectedPlayers;

        private const string PREPARED = "Prepare";
        private const string PREPARE_INTERACT_MESSAGE = "Unfold Barbed Wire: [LMB]";
        private const string PICKUP_INTERACT_MESSAGE = "Fold Barbed Wire: [LMB]";

        public override void Start()
        {
            logger.LogDebug("Spawned in scene!");
            base.Start();
            animator = GetComponent<Animator>();

            prepared = false;
            prepareBarbedWire = GetComponents<InteractTrigger>().Where((x) => x.name == "PrepareBarbedWire").First();
            prepareBarbedWire.onInteract.AddListener(InteractBarbedWire);
            grabbable = true;
            grabbableToEnemies = true;
            affectedEnemies = new List<EnemyAI>();
            affectedPlayers = new List<PlayerControllerB>();
        }

        public override void Update()
        {
            base.Update();
            if (!prepared) return;
            if (slowDownEnemies || stunEnemies || damageEnemies) CheckForEnemies();
            if (damagePlayers || slowDownPlayers) CheckForPlayers();
        }
        private void CheckForEnemies()
        {
            RaycastHit[] enemyColliders = new RaycastHit[10];
            int numEnemiesHit = Physics.SphereCastNonAlloc(transform.position, radius, transform.forward, enemyColliders, radius, 524288, QueryTriggerInteraction.Collide);
            for(int i = 0; i < numEnemiesHit; i++)
            {
                EnemyAICollisionDetect enemyCollision = enemyColliders[i].transform.GetComponent<EnemyAICollisionDetect>();
                if (enemyCollision == null) break;
                EnemyAI enemyAI = enemyCollision.mainScript;

                if (affectedEnemies.Contains(enemyAI)) continue;
                if (slowDownEnemies) enemyAI.agent.speed *= slowEnemiesMultiplier;
                if (damageEnemies) enemyAI.HitEnemy(amountDamageEnemies);
                if (stunEnemies) enemyAI.SetEnemyStunned(true, stunTimer);
                affectedEnemies.Add(enemyAI);
            }
        }

        private void CheckForPlayers()
        {
            PlayerControllerB localPlayer = UpgradeBus.instance.GetLocalPlayer();
            float distance = Vector3.Distance(localPlayer.transform.position, transform.position);
            if (distance <= radius)
            {
                
            }
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
            animator.SetBool(PREPARED, enabled);
            grabbable = !enabled;
            grabbableToEnemies = !enabled;
            prepareBarbedWire.hoverTip = enabled ? PICKUP_INTERACT_MESSAGE : PREPARE_INTERACT_MESSAGE;
        }
    }
}
