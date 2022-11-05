using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FaceDown
{
	[StaticConstructorOnStartup]
	internal static class HarmonyInit
	{
		static HarmonyInit()
		{
			new Harmony("FaceDown.Mod").PatchAll();
		}
	}

    [HarmonyPatch(typeof(PawnRenderer), "LayingFacing")]
    public class LayingFacing_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            var get_North = AccessTools.Method(typeof(Rot4), "get_North");
            var get_South = AccessTools.Method(typeof(Rot4), "get_South");
            var get_West = AccessTools.Method(typeof(Rot4), "get_West");
            var codes = instructions.ToList();
            bool firstPatch = false;
            bool secondPatch = false;
            for (var i = 0; i < codes.Count; i++)
            {
                var instr = codes[i];
                if (!firstPatch && i > 2 && codes[i - 1].opcode == OpCodes.Br_S && codes[i].Calls(get_South))
                {
                    firstPatch = true;
                    yield return new CodeInstruction(OpCodes.Call, get_North).MoveLabelsFrom(codes[i]);
                }
                else if (firstPatch && !secondPatch && codes[i - 7].opcode == OpCodes.Br_S && codes[i].Calls(get_West))
                {
                    secondPatch = true;
                    yield return new CodeInstruction(OpCodes.Call, get_North).MoveLabelsFrom(codes[i]);
                }
                else
                {
                    yield return instr;
                }
            }

            if (!firstPatch || !secondPatch)
            {
                Log.Error("FACE DOWN: Failed to patch LayingFacing");
            }
        }
    }
}
