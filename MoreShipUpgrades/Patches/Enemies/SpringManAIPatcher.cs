using HarmonyLib;
using MoreShipUpgrades.Managers;
using MoreShipUpgrades.UpgradeComponents;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using MoreShipUpgrades.Misc;
using MoreShipUpgrades.UpgradeComponents.Items;
using MoreShipUpgrades.UpgradeComponents.Items.BarbedWire;

namespace MoreShipUpgrades.Patches.Enemies
{
    [HarmonyPatch(typeof(SpringManAI))]
    internal class SpringManAIPatcher
    {
        private static LGULogger logger = new LGULogger(nameof(SpringManAIPatcher));
        [HarmonyPrefix]
        [HarmonyPatch(nameof(SpringManAI.DoAIInterval))]
        private static void DoAllIntervalPrefix(ref SpringManAI __instance)
        {
            if (Peeper.HasLineOfSightToPeepers(__instance)) __instance.currentBehaviourStateIndex = 1;
        }
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(SpringManAI.Update))]
        private static IEnumerable<CodeInstruction> Update_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            FieldInfo currentChaseSpeed = typeof(SpringManAI).GetField("currentChaseSpeed", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo peeperMethod = typeof(Peeper).GetMethod(nameof(Peeper.HasLineOfSightToPeepers));
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            int index = 0;
            index = Tools.FindField(index, ref codes, findField: currentChaseSpeed, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Couldn't find the current chase speed field");
            index = Tools.FindInteger(index, ref codes, findValue: 0, addCode: peeperMethod, requireInstance: true, orInstruction: true, errorMessage: "Couldn't find the value attributed to stop movement local variable");
            index = Tools.FindField(index, ref codes, findField: currentChaseSpeed, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Couldn't find the current chase speed field second time");
            return codes;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(SpringManAI.DoAIInterval))]
        private static IEnumerable<CodeInstruction> DoAIIntervalTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo checkForBarbedWire = typeof(BaseBarbedWire).GetMethod(nameof(BaseBarbedWire.CheckForBarbedWires));
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            int index = 0;
            index = Tools.FindFloat(index, ref codes, findValue: 6f, addCode: checkForBarbedWire, requireInstance: true, errorMessage: "Couldn't find the agent speed value when patrolling");
            return codes;
        }
    }

}
