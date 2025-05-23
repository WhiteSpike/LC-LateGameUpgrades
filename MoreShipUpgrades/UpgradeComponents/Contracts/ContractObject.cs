﻿using MoreShipUpgrades.Managers;
using MoreShipUpgrades.Misc.Util;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace MoreShipUpgrades.UpgradeComponents.Contracts
{
    internal class ContractObject : NetworkBehaviour
    {
        protected string contractType = null;
        public bool SetPosition = false;
        public virtual void Start()
        {
            if (!IsServer) return;
            if (ContractManager.Instance.contractType != contractType || StartOfRound.Instance.currentLevel.PlanetName != ContractManager.Instance.contractLevel)
            {
                this.gameObject.GetComponent<NetworkObject>().Despawn();
                return;
            }
            if (SetPosition)
            {
                List<EntranceTeleport> mainDoors = FindObjectsOfType<EntranceTeleport>().Where(obj => obj.gameObject.transform.position.y <= -170).ToList();
                EnemyVent[] vents = FindObjectsOfType<EnemyVent>();
                EnemyVent spawnVent = null;
                if(UpgradeBus.Instance.PluginConfiguration.MAIN_OBJECT_FURTHEST.Value)
                {
                    spawnVent = vents.OrderByDescending(vent => Vector3.Distance(mainDoors[0].transform.position, vent.floorNode.position)).First();
                }
                else
                {
                    spawnVent = vents[Random.Range(0,vents.Length)];
                }
                Vector3 offsetVector = Quaternion.Euler(0f, spawnVent.floorNode.eulerAngles.y, 0f) * Vector3.forward;
                Vector3 newPosition = spawnVent.floorNode.position + offsetVector;
                transform.position = newPosition;
            }
            if (contractType != LguConstants.EXTERMINATOR_CONTRACT_NAME) return;

            Tools.SpawnMob("Hoarding bug", transform.position, UpgradeBus.Instance.PluginConfiguration.CONTRACT_BUG_SPAWNS.Value);
        }
    }
}
