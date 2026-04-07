using HarmonyLib;

namespace JinMultiplier;

[HarmonyPatch]
public class Patches {
    [HarmonyPrefix, HarmonyPatch(typeof(PlayerGamePlayData), "AddGold")]
    static void AddGold(PlayerGamePlayData __instance, ref int amount, GoldSourceTag sourceTag) {
        var oldValue = amount;

        if (sourceTag == GoldSourceTag.Monster) {
            var multiplier = JinMultiplier.Instance.jinDropMultiplier.Value;
            if (multiplier == 1)
                return;
            var newValue = oldValue * multiplier;
            Log.Info($"applying {multiplier}x Jin Drop Multiplier by giving Yi {newValue} jin (instead of {oldValue})");
            amount = newValue;
        }

        if (sourceTag == GoldSourceTag.FooExplode) {
            var multiplier = JinMultiplier.Instance.tuwMultiplier.Value;
            if (multiplier == 1)
                return;
            var newValue = oldValue * multiplier;
            Log.Info($"applying {multiplier}x Transmute Unto Wealth Multiplier by giving Yi {newValue} jin (instead of {oldValue})");
            amount = newValue;
        }

        /* Ignore all other GoldSourceTags
         * - Chest appears to be unused (chest contents are Monster)
         * - Sell appears to be unused (3d printer recycling goes through a different codepath, see below)
         * - DeathPenalty does what it sounds like, but doesn't seem useful
         * - DevCheat does what it sounds like, but doesn't seem useful
         */
    }

    private static string playerGoldFlag = "0b9cad677208a48d8a2b14cd0dd2b6e8ScriptableDataFloat";

    [HarmonyPrefix, HarmonyPatch(typeof(ModifyScriptableDataIntEntry), "Receive")]
    static bool ModifyScriptableDataIntEntry_Receive(ModifyScriptableDataIntEntry __instance) {
        if (__instance.ModifyFlag?.FinalSaveID != playerGoldFlag)
            return true;

        var multiplier = JinMultiplier.Instance.recycleMultiplier.Value;
        if (multiplier == 1)
            return true;

        var oldValue = __instance.ModifyAmount;
        var newValue = oldValue * multiplier;
        Log.Info($"applying {multiplier}x Recycling Multiplier by giving Yi {newValue} jin (instead of {oldValue})");
        __instance.ModifyFlag.CurrentValue += newValue;
        return false;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(MerchandiseItemButton), "UpdateView")]
    static void MerchandiseItemButton_UpdateView_Postfix(MerchandiseItemButton __instance) {
        if (__instance.bindData?.OutcomeResult?.ModifyFlag?.FinalSaveID != playerGoldFlag)
            return;

        var multiplier = JinMultiplier.Instance.recycleMultiplier.Value;
        if (multiplier == 1)
            return;

        var oldText = __instance.priceText.text;
        if (!int.TryParse(oldText, out var amount))
            return;

        var newText = $"{multiplier}x {oldText}";
        __instance.priceText.text = newText;
        //Log.Info($"applying Recycling Multiplier to {__instance} by changing priceText from \"{oldText}\" to \"{newText}\"");
    }
}