﻿using LethalCompanyInputUtils.Api;
using MoreShipUpgrades.Misc.Util;
using UnityEngine.InputSystem;

namespace MoreShipUpgrades.Input
{
    /// <summary>
    /// Class which stores the used key binds in the mod
    /// </summary>
    internal class IngameKeybinds : LcInputActions
    {
        public static IngameKeybinds Instance;
        /// <summary>
        /// Asset used to store all the input bindings defined for our controls
        /// </summary>
        internal static InputActionAsset GetAsset()
        {
            return Instance.Asset;
        }

        /// <summary>
        /// Input binding used to trigger the toggle of Night Vision action
        /// </summary>
        [InputAction(LguConstants.TOGGLE_NIGHT_VISION_DEFAULT_KEYBIND, Name = LguConstants.TOGGLE_NIGHT_VISION_KEYBIND_NAME)]
        public InputAction NvgKey { get; set; }

        [InputAction("<Keyboard>/leftShift", Name = "Show Alternative Currency Counter")]
        public InputAction ShowAlternativeCounterKey {  get; set; }

    }
}
