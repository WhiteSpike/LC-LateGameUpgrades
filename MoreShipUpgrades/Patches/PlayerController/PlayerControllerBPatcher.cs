﻿using GameNetcodeStuff;
using HarmonyLib;
using MoreShipUpgrades.Managers;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using UnityEngine;
using MoreShipUpgrades.UpgradeComponents.TierUpgrades;
using MoreShipUpgrades.UpgradeComponents.TierUpgrades.AttributeUpgrades;
using MoreShipUpgrades.Misc.Upgrades;
using MoreShipUpgrades.Misc.Util;
using MoreShipUpgrades.UpgradeComponents.TierUpgrades.Player;
using MoreShipUpgrades.UpgradeComponents.OneTimeUpgrades.Items;
using MoreShipUpgrades.Compat;
using MoreShipUpgrades.UpgradeComponents.Commands;
using MoreShipUpgrades.UpgradeComponents.TierUpgrades.Ship;
using MoreShipUpgrades.Extensions;
using MoreShipUpgrades.Configuration;

namespace MoreShipUpgrades.Patches.PlayerController
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal static class PlayerControllerBPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlayerControllerB.Awake))]
        static void StartPostfix(PlayerControllerB __instance)
        {
            __instance.gameObject.AddComponent<PlayerManager>();
        }
        [HarmonyPatch(nameof(PlayerControllerB.KillPlayerClientRpc))]
        [HarmonyPostfix]
        static void KillPlayerClientRpcPostfix(int playerId)
        {
            LategameConfiguration config = UpgradeBus.Instance.PluginConfiguration;
            if (!config.INTERN_ENABLED || config.INTERNS_TELEPORT_RESTRICTION == Interns.TeleportRestriction.None) return;
            PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[playerId];
            Interns.instance.RemoveRecentlyInterned(player);
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayerControllerB.KillPlayer))]
        static void DisableUpgradesOnDeath(PlayerControllerB __instance)
        {
            if (!__instance.IsOwner) return;
            if (__instance.isPlayerDead) return;
            if (!__instance.AllowPlayerDeath()) return;
            if (UpgradeBus.Instance.PluginConfiguration.INTERN_ENABLED)
            {
                Interns.instance.RemoveRecentlyInterned(__instance);
            }
            LoseNightVisionOnDeath(ref __instance);
        }

        static void LoseNightVisionOnDeath(ref PlayerControllerB __instance)
        {
            if (!UpgradeBus.Instance.PluginConfiguration.NightVisionUpgradeConfiguration.LoseOnDeath.Value) return;

            if (__instance != UpgradeBus.Instance.GetLocalPlayer()) return;
            if (!BaseUpgrade.GetActiveUpgrade("Night Vision")) return;

            Plugin.mls.LogDebug($"Player that died: {__instance.playerUsername}");
            Plugin.mls.LogDebug($"Local player we are deactivating to: {UpgradeBus.Instance.GetLocalPlayer().playerUsername}");

            UpgradeBus.Instance.UpgradeObjects[NightVision.UPGRADE_NAME].GetComponent<NightVision>().DisableOnClient();
            if (!UpgradeBus.Instance.PluginConfiguration.NightVisionUpgradeConfiguration.DropOnDeath.Value) return;
            NightVision.Instance.SpawnNightVisionItemOnDeathServerRpc(__instance.transform.position);
        }

        [HarmonyPatch(nameof(PlayerControllerB.DamagePlayer))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> DamagePlayerTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo maximumHealthMethod = typeof(Stimpack).GetMethod(nameof(Stimpack.CheckForAdditionalHealth));
            MethodInfo boomboxDefenseMethod = typeof(SickBeats).GetMethod(nameof(SickBeats.CalculateDefense));

            List<CodeInstruction> codes = instructions.ToList();
            /*
             * ldfld int32 GameNetcodeStuff.PlayerControllerB::health
             * ldarg.1 -> damageNumber
             * sub -> health - damageNumber
             * ldc.i4.0
             * ldc.i4.s 100
             * call int32 clamp(int32, int32, int32) -> Mathf.Clamp(health - damageNumber, 0, 100)
             * stfld int32 health -> health = Mathf.Clamp(health - damageNumber, 0, 100)
             * 
             * We want change 100 to the maximum possible value the player's health can be due to Stimpack upgrade
             */
            bool foundHealthMaximum = false;
            for (int i = 0; i < codes.Count && !foundHealthMaximum; i++)
            {
                if (codes[i].opcode == OpCodes.Ldarg_1)
                {
                    codes.Insert(i+1, new CodeInstruction(OpCodes.Call, boomboxDefenseMethod));
                    continue;
                }
                if (!(codes[i].opcode == OpCodes.Ldc_I4_S && codes[i].operand.ToString() == "100")) continue;
                if (!(codes[i + 1] != null && codes[i + 1].opcode == OpCodes.Call && codes[i + 1].operand.ToString() == "Int32 Clamp(Int32, Int32, Int32)")) continue;
                if (!(codes[i + 2] != null && codes[i + 2].opcode == OpCodes.Stfld && codes[i + 2].operand.ToString() == "System.Int32 health")) continue;

                //Mathf.Clamp(health - damageNumber, 0, playerHealthScript.CheckForAdditionalHealth(100))
                codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, maximumHealthMethod));
                foundHealthMaximum = true;
            }
            if (!foundHealthMaximum) Plugin.mls.LogError("Could not find the maximum of Mathf.Clamp that changes the health value");
            return codes.AsEnumerable();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(PlayerControllerB.PlayerHitGroundEffects))]
        static IEnumerable<CodeInstruction> PlayerHitGroundEffectsTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo reduceFallDamageMethod = typeof(ReinforcedBoots).GetMethod(nameof(ReinforcedBoots.ReduceFallDamage));
            List<CodeInstruction> codes = new(instructions);
            int index = 0;
            Tools.FindInteger(ref index, ref codes, findValue: 100, addCode: reduceFallDamageMethod, errorMessage: "Couldn't find 100 fall damage");
            Tools.FindInteger(ref index, ref codes, findValue: 80, addCode: reduceFallDamageMethod, errorMessage: "Couldn't find 80 fall damage");
            Tools.FindInteger(ref index, ref codes, findValue: 50, addCode: reduceFallDamageMethod, errorMessage: "Couldn't find 50 fall damage");
            Tools.FindInteger(ref index, ref codes, findValue: 30, addCode: reduceFallDamageMethod, errorMessage: "Couldn't find 30 fall damage");

            return codes.AsEnumerable();
        }
        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlayerControllerB.SwitchToItemSlot))]
        [HarmonyPatch(nameof(PlayerControllerB.BeginGrabObject))]
        static void SwitchToItemSlotPostfix(PlayerControllerB __instance)
        {
            DeepPocketsTwoHandedCheck(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlayerControllerB.GrabObjectClientRpc))]
        static void GrabObjectClientRpcPostfix(PlayerControllerB __instance)
        {
            DeepPocketsTwoHandedCheck(__instance);
        }
        static void DeepPocketsTwoHandedCheck(PlayerControllerB player)
        {
            if (!player.twoHanded) return;
            if (!BaseUpgrade.GetActiveUpgrade(DeepPockets.UPGRADE_NAME)) return;

            if (player.currentlyHeldObjectServer != null)
            {
                EnemyAI enemy = player.currentlyHeldObjectServer.GetComponent<EnemyAI>();
                if (enemy != null && !enemy.isEnemyDead) return;
            }

            if (CustomItemBehaviourLibraryCompat.Enabled
                && !UpgradeBus.Instance.PluginConfiguration.DeeperPocketsConfiguration.AllowWheelbarrows
                && CustomItemBehaviourLibraryCompat.CheckForContainers(ref player))
            {
                return;
            }
            int twoHandedCount = 0;
            int maxTwoHandedCount = 1 + UpgradeBus.Instance.PluginConfiguration.DeeperPocketsConfiguration.InitialEffect + (BaseUpgrade.GetUpgradeLevel(DeepPockets.UPGRADE_NAME) * UpgradeBus.Instance.PluginConfiguration.DeeperPocketsConfiguration.IncrementalEffect);

            for(int i = 0; i < player.ItemSlots.Length && twoHandedCount < maxTwoHandedCount; i++)
            {
                GrabbableObject item = player.ItemSlots[i];
                if (item == null) continue;
                if (!item.itemProperties.twoHanded) continue;
                twoHandedCount++;
            }
            if (twoHandedCount < maxTwoHandedCount)
            {
                player.twoHanded = false;
                HUDManager.Instance.holdingTwoHandedItem.enabled = false;
            }
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(PlayerControllerB.BeginGrabObject))]
        [HarmonyPatch(nameof(PlayerControllerB.GrabObjectClientRpc))]
        [HarmonyPatch(nameof(PlayerControllerB.DestroyItemInSlot))]
        [HarmonyPatch(nameof(PlayerControllerB.DespawnHeldObjectOnClient))]
        [HarmonyPatch(nameof(PlayerControllerB.SetObjectAsNoLongerHeld))]
        [HarmonyPatch(nameof(PlayerControllerB.PlaceGrabbableObject))]
        static IEnumerable<CodeInstruction> ItemWeightTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new(instructions);
            int index = 0;
            AddBackMusclesCodeInstruction(ref index, ref codes);

            return codes;
        }
        internal static void AddBackMusclesCodeInstruction(ref int index, ref List<CodeInstruction> codes)
        {
            MethodInfo affectWeight = typeof(BackMuscles).GetMethod(nameof(BackMuscles.DecreasePossibleWeight));
            Tools.FindSub(ref index, ref codes, addCode: affectWeight, errorMessage: "Couldn't find item weight");
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlayerControllerB.Update))]
        static void CheckForBoomboxes(PlayerControllerB __instance)
        {
            if (!UpgradeBus.Instance.PluginConfiguration.SickBeatsUpgradeConfiguration.Enabled.Value) return;
            if (!BaseUpgrade.GetActiveUpgrade(SickBeats.UPGRADE_NAME) || __instance != GameNetworkManager.Instance.localPlayerController) return;
            SickBeats.Instance.boomBoxes.RemoveAll(b => b == null);
            SickBeats.Instance.vehicleControllers.RemoveAll(c => c == null);
            bool result = false;
            if (__instance.isPlayerDead)
            {
                if (!SickBeats.Instance.EffectsActive) return; // No need to do anything
                SickBeats.Instance.EffectsActive = result;
                SickBeats.HandlePlayerEffects(__instance);
                return; // Clean all effects from Sick Beats since the player's dead
            }
            foreach (BoomboxItem boom in SickBeats.Instance.boomBoxes)
            {
                if (!boom.isPlayingMusic) continue;

                if (Vector3.Distance(boom.transform.position, __instance.transform.position) >= UpgradeBus.Instance.PluginConfiguration.SickBeatsUpgradeConfiguration.Radius.Value) continue;

                result = true;
                break;
            }

            foreach (VehicleController radio in SickBeats.Instance.vehicleControllers)
            {
                if (!radio.radioOn) continue;

                if (Vector3.Distance(radio.transform.position, __instance.transform.position) >= UpgradeBus.Instance.PluginConfiguration.SickBeatsUpgradeConfiguration.Radius.Value) continue;

                result = true;
                break;
            }

            if (result == SickBeats.Instance.EffectsActive) return;

            SickBeats.Instance.EffectsActive = result;
            SickBeats.HandlePlayerEffects(__instance);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(PlayerControllerB.LateUpdate))]
        static IEnumerable<CodeInstruction> LateUpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo biggerLungsRegenMethod = typeof(BiggerLungs).GetMethod(nameof(BiggerLungs.ApplyPossibleIncreasedStaminaRegen));
            MethodInfo sickBeatsRegenMethod = typeof(SickBeats).GetMethod(nameof(SickBeats.ApplyPossibleIncreasedStaminaRegen));
            MethodInfo sickBeatsDrainMethod = typeof(SickBeats).GetMethod(nameof(SickBeats.ApplyPossibleDecreasedStaminaDrain));
            MethodInfo additionalSprintTime = typeof(BiggerLungs).GetMethod(nameof(BiggerLungs.GetAdditionalStaminaTime));
            MethodInfo backMusclesStaminaWeight = typeof(BackMuscles).GetMethod(nameof(BackMuscles.DecreaseStrain));
            MethodInfo medicalNanobotsHealthRegenMethod = typeof(MedicalNanobots).GetMethod(nameof(MedicalNanobots.GetIncreasedHealthRegeneration));
            MethodInfo effectiveBandaidsHealthAmountMethod = typeof(EffectiveBandaids).GetMethod(nameof(EffectiveBandaids.GetIncreasedHealthRegenerated));

            FieldInfo sprintTime = typeof(PlayerControllerB).GetField(nameof(PlayerControllerB.sprintTime));
            FieldInfo carryWeight = typeof(PlayerControllerB).GetField(nameof(PlayerControllerB.carryWeight));

            List<CodeInstruction> codes = new(instructions);
            int index = 0;
            Tools.FindField(ref index, ref codes, findField: sprintTime, addCode: additionalSprintTime, errorMessage: "Couldn't find the first occurence of sprintTime field");
            Tools.FindDiv(ref index, ref codes, addCode: sickBeatsDrainMethod, errorMessage: "Couldn't find first mul instruction to include our drain method from Sick Beats");
            Tools.FindField(ref index, ref codes, findField: carryWeight, addCode: backMusclesStaminaWeight, errorMessage: "Couldn't find the occurence of carryWeight field");
            Tools.FindField(ref index, ref codes, findField: sprintTime, addCode: additionalSprintTime, errorMessage: "Couldn't find the second occurence of sprintTime field");
            Tools.FindDiv(ref index, ref codes, addCode: sickBeatsDrainMethod, errorMessage: "Couldn't find second mul instruction to include our drain method from Sick Beats");
            Tools.FindField(ref index, ref codes, findField: sprintTime, addCode: additionalSprintTime, errorMessage: "Couldn't find the third occurence of sprintTime field");
            Tools.FindMul(ref index, ref codes, addCode: biggerLungsRegenMethod, errorMessage: "Couldn't find first mul instruction to include our regen method from Bigger Lungs");
            codes.Insert(index + 1, new CodeInstruction(OpCodes.Call, sickBeatsRegenMethod));
            Tools.FindField(ref index, ref codes, findField: sprintTime, addCode: additionalSprintTime, errorMessage: "Couldn't find the fourth occurence of sprintTime field");
            Tools.FindMul(ref index, ref codes, addCode: biggerLungsRegenMethod, errorMessage: "Couldn't find second mul instruction to include our regen method from Bigger Lungs");
            codes.Insert(index + 1, new CodeInstruction(OpCodes.Call, sickBeatsRegenMethod));
            Tools.FindInteger(ref index, ref codes, findValue: 20, addCode: medicalNanobotsHealthRegenMethod, errorMessage: "Couldn't find the value used to represent the maximum health regenerable");
            Tools.FindInteger(ref index, ref codes, findValue: 1, addCode: effectiveBandaidsHealthAmountMethod, errorMessage: "Couldn't find the value used to increment the player's health");
            return codes;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(PlayerControllerB.Jump_performed))]
        static IEnumerable<CodeInstruction> JumpPerformedTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo biggerLungsReduceJumpCost = typeof(BiggerLungs).GetMethod(nameof(BiggerLungs.ApplyPossibleReducedJumpStaminaCost));
            List<CodeInstruction> codes = new(instructions);
            int index = 0;
            Tools.FindFloat(ref index, ref codes, findValue: 0.08f, addCode: biggerLungsReduceJumpCost, errorMessage: "Couldn't find jump stamina cost");
            return codes;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(PlayerControllerB.PlayFootstepLocal))]
        static IEnumerable<CodeInstruction> PlayFootstepLocalTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return AddReduceNoiseRangeFunctionToPlayerFootsteps(instructions);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(PlayerControllerB.PlayFootstepServer))]
        static IEnumerable<CodeInstruction> PlayFootstepServerTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return AddReduceNoiseRangeFunctionToPlayerFootsteps(instructions);
        }

        static IEnumerable<CodeInstruction> AddReduceNoiseRangeFunctionToPlayerFootsteps(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo runningShoesReduceNoiseRange = typeof(RunningShoes).GetMethod(nameof(RunningShoes.ApplyPossibleReducedNoiseRange));
            List<CodeInstruction> codes = new(instructions);
            int index = 0;
            Tools.FindFloat(ref index, ref codes, findValue: 22, addCode: runningShoesReduceNoiseRange, errorMessage: "Couldn't find footstep noise");
            Tools.FindFloat(ref index, ref codes, findValue: 17, addCode: runningShoesReduceNoiseRange, errorMessage: "Couldn't find footstep noise");
            return codes.AsEnumerable();
        }

        [HarmonyPatch(nameof(PlayerControllerB.PlayerJump), MethodType.Enumerator)]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PlayerJumpTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo additionalJumpForce = typeof(StrongLegs).GetMethod(nameof(StrongLegs.GetAdditionalJumpForce));
            FieldInfo jumpForce = typeof(PlayerControllerB).GetField(nameof(PlayerControllerB.jumpForce));

            List<CodeInstruction> codes = new(instructions);
            int index = 0;

            Tools.FindField(ref index, ref codes, findField: jumpForce, addCode: additionalJumpForce, errorMessage: "Couldn't find first occurence of jump force field");
            Tools.FindField(ref index, ref codes, findField: jumpForce, addCode: additionalJumpForce, errorMessage: "Couldn't find second occurence of jump force field");
            return codes;
        }

        [HarmonyPatch(nameof(PlayerControllerB.Update))] // We're all going to die
        [HarmonyTranspiler] // Just kidding
        static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo reduceCarryLoss = typeof(BackMuscles).GetMethod(nameof(BackMuscles.DecreaseCarryLoss));
            MethodInfo additionalMovement = typeof(RunningShoes).GetMethod(nameof(RunningShoes.GetAdditionalMovementSpeed));
            MethodInfo additionalClimbSpeed = typeof(ClimbingGloves).GetMethod(nameof(ClimbingGloves.GetAdditionalClimbingSpeed));
            MethodInfo additionalJumpForce = typeof(StrongLegs).GetMethod(nameof(StrongLegs.GetAdditionalJumpForce));
            MethodInfo additionalTractionForce = typeof(TractionBoots).GetMethod(nameof(TractionBoots.GetAdditionalTractionForce));
            MethodInfo uphillSlopeMultiplier = typeof(HikingBoots).GetMethod(nameof(HikingBoots.ReduceUphillSlopeDebuff));
            MethodInfo ReduceCrouchMovementSpeedDebuff = typeof(CarbonKneejoints).GetMethod(nameof(CarbonKneejoints.ReduceCrouchMovementSpeedDebuff));
            MethodInfo ReduceFallDamage = typeof(ReinforcedBoots).GetMethod(nameof(ReinforcedBoots.ReduceFallDamage));

            FieldInfo carryWeight = typeof(PlayerControllerB).GetField(nameof(PlayerControllerB.carryWeight));
            FieldInfo movementSpeed = typeof(PlayerControllerB).GetField(nameof(PlayerControllerB.movementSpeed));
            FieldInfo climbSpeed = typeof(PlayerControllerB).GetField(nameof(PlayerControllerB.climbSpeed));
            FieldInfo jumpForce = typeof(PlayerControllerB).GetField(nameof(PlayerControllerB.jumpForce));
            FieldInfo slopeIntensity = typeof(PlayerControllerB).GetField(nameof(PlayerControllerB.slopeIntensity));
            FieldInfo slopeModifier = typeof(PlayerControllerB).GetField(nameof(PlayerControllerB.slopeModifier), BindingFlags.NonPublic | BindingFlags.Instance);

            List<CodeInstruction> codes = new(instructions);
            int index = 0;
            Tools.FindField(ref index, ref codes, findField: movementSpeed, addCode: additionalMovement, errorMessage: "Couldn't find first occurence of movement speed field");
            Tools.FindField(ref index, ref codes, findField: movementSpeed, addCode: additionalMovement, errorMessage: "Couldn't find second occurence of movement speed field");
            Tools.FindField(ref index, ref codes, findField: carryWeight, addCode: reduceCarryLoss, errorMessage: "Couldn't find first carryWeight occurence");
            Tools.FindFloat(ref index, ref codes, findValue: 1.5f, addCode: ReduceCrouchMovementSpeedDebuff, errorMessage: "Couldn't find the movement speed loss value while crouching");
            Tools.FindField(ref index, ref codes, findField: slopeIntensity, skip: true, errorMessage: "Couldn't find skip slope Intensity");
            Tools.FindField(ref index, ref codes, findField: slopeModifier, addCode: uphillSlopeMultiplier, errorMessage: "Couldn't find occurence of slopeModifier");
            Tools.FindFloat(ref index, ref codes, findValue: 5f, addCode: additionalTractionForce, errorMessage: "Couldn't find the numerator used when sprinting");
            Tools.FindFloat(ref index, ref codes, findValue: 10f, addCode: additionalTractionForce, errorMessage: "Couldn't find the numerator used when walking");
            Tools.FindField(ref index, ref codes, findField: jumpForce, addCode: additionalJumpForce, errorMessage: "Couldn't find occurence of jump force field");
            Tools.FindInteger(ref index, ref codes, findValue: 85, addCode: ReduceFallDamage, errorMessage: "Couldn't find first occurence of fall damage when using jetpack");
            Tools.FindInteger(ref index, ref codes, findValue: 30, addCode: ReduceFallDamage, errorMessage: "Couldn't find second occurence of fall damage when using jetpack");
            Tools.FindInteger(ref index, ref codes, findValue: 30, addCode: ReduceFallDamage, errorMessage: "Couldn't find third occurence of fall damage when using jetpack");
            Tools.FindField(ref index, ref codes, findField: climbSpeed, addCode: additionalClimbSpeed, errorMessage: "Couldn't find occurence of climb speed field");
            return codes;
        }

        [HarmonyPatch(nameof(PlayerControllerB.BeginGrabObject))]
        [HarmonyPatch(nameof(PlayerControllerB.SetHoverTipAndCurrentInteractTrigger))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> SetGrabDistanceTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo grabDistance = typeof(PlayerControllerB).GetField(nameof(PlayerControllerB.grabDistance));
            MethodInfo getIncreasedRange = typeof(MechanicalArms).GetMethod(nameof(MechanicalArms.GetIncreasedGrabDistance));
            List<CodeInstruction> codes = new(instructions);
            int index = 0;
            Tools.FindField(ref index, ref codes, findField: grabDistance, addCode: getIncreasedRange, errorMessage: "Couldn't find the grab distance field");
            return codes;
        }

        [HarmonyPatch(nameof(PlayerControllerB.SetFaceUnderwaterFilters))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> SetFaceUnderwaterFiltersTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo ReduceOxygenConsumption = typeof(OxygenCanisters).GetMethod(nameof(OxygenCanisters.ReduceOxygenConsumption));
            List<CodeInstruction> codes = new(instructions);
            int index = 0;
            Tools.FindDiv(ref index, ref codes, addCode: ReduceOxygenConsumption, errorMessage: "Couldn't find the value used to decrease the drowning timer");
            return codes;
        }

        [HarmonyPatch(nameof(PlayerControllerB.ClickHoldInteraction))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> ClickHoldInteractionTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo timeToHoldSpeedMultiplier = typeof(InteractTrigger).GetField(nameof(InteractTrigger.timeToHoldSpeedMultiplier));
            MethodInfo increaseInteractionSpeed = typeof(QuickHands).GetMethod(nameof(QuickHands.IncreaseInteractionSpeed));

            List<CodeInstruction> codes = new(instructions);
            int index = 0;
            Tools.FindField(ref index, ref codes, findField: timeToHoldSpeedMultiplier, addCode: increaseInteractionSpeed, errorMessage: "Couldn't find the interaction speed multiplier saved in the interact trigger");

            return codes;
        }

        [HarmonyPatch(nameof(PlayerControllerB.DropAllHeldItems))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> DropAllHeldItemsTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo CanHoldItem = typeof(FusionMatter).GetMethod(nameof(FusionMatter.CanHoldItem));
            MethodInfo RemainIsHoldingObjectIfTeleporting = typeof(PlayerControllerBExtensions).GetMethod(nameof(PlayerControllerBExtensions.RemainIsHoldingObjectIfTeleporting));
            MethodInfo RemainActivatingItemIfTeleporting = typeof(PlayerControllerBExtensions).GetMethod(nameof(PlayerControllerBExtensions.RemainActivatingItemIfTeleporting));
            MethodInfo RemainTwoHandedIfTeleporting = typeof(PlayerControllerBExtensions).GetMethod(nameof(PlayerControllerBExtensions.RemainTwoHandedIfTeleporting));
            MethodInfo RemainCarryWeightIfTeleporting = typeof(PlayerControllerBExtensions).GetMethod(nameof(PlayerControllerBExtensions.RemainCarryWeightIfTeleporting));
            MethodInfo RemainCurrentlyHeldObjectServerIfTeleporting = typeof(PlayerControllerBExtensions).GetMethod(nameof(PlayerControllerBExtensions.RemainCurrentlyHeldObjectServerIfTeleporting));

            FieldInfo isHoldingObject = typeof(PlayerControllerB).GetField(nameof(PlayerControllerB.isHoldingObject));
            FieldInfo playerBodyAnimator = typeof(PlayerControllerB).GetField(nameof(PlayerControllerB.playerBodyAnimator));

            List<CodeInstruction> codes = new(instructions);
            int index = 0;
            Tools.FindNull(ref index, ref codes, skip: true);
            index++;
            codes.Insert(index, new CodeInstruction(OpCodes.And));
            codes.Insert(index, new CodeInstruction(OpCodes.Not));
            codes.Insert(index, new CodeInstruction(OpCodes.Call, CanHoldItem));
            codes.Insert(index, new CodeInstruction(OpCodes.Ldarg_0));
            codes.Insert(index, new CodeInstruction(OpCodes.Ldloc_0));
            Tools.FindField(ref index, ref codes, findField: isHoldingObject, addCode: RemainIsHoldingObjectIfTeleporting, andInstruction: true, notInstruction: true, requireInstance: true);
            Tools.FindField(ref index, ref codes, findField: playerBodyAnimator, skip: true);
            Tools.FindInteger(ref index, ref codes, findValue: 0, addCode: RemainActivatingItemIfTeleporting, orInstruction: true, requireInstance: true);
            Tools.FindInteger(ref index, ref codes, findValue: 0, addCode: RemainTwoHandedIfTeleporting, orInstruction: true, requireInstance: true);
            Tools.FindFloat(ref index, ref codes, findValue: 1f, addCode: RemainCarryWeightIfTeleporting, requireInstance: true, instanceBefore: true);
            Tools.FindNull(ref index, ref codes, addCode: RemainCurrentlyHeldObjectServerIfTeleporting, requireInstance: true, instanceBefore: true);
            return codes;
        }
    }
}
