using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using GameClient.Values;
using HarmonyLib;
using Shared;

namespace GameClient.Patches
{
    public static class TranspilerHelper
    {
        private static readonly FieldInfo NetworkState = AccessTools.Field(typeof(SessionValues), nameof(SessionValues.CurrentNetworkState));
        /// <summary>
        /// Checks if the player is online, with an if - else statement
        /// </summary>
        /// <param name="generator"> IlGenerate provided by Harmony</param>
        /// <param name="baseMethod"> The original's method Opcodes</param>
        /// <param name="codeToExecuteIfConnected"> The CodeInstructions to be executed if Connected</param>
        /// <param name="index"> Current index, where the IF will begin</param>
        /// <param name="codeToSkipIfConnected"> Amount of instructions to skip after the IF branch</param>
        public static void CheckIfConnected(ILGenerator generator, List<CodeInstruction> baseMethod, CodeInstruction[] codeToExecuteIfConnected, ref int index, int codeToSkipIfConnected = 0)
        {
            Label skipLabel = generator.DefineLabel();
            
            List<CodeInstruction> instructions = new List<CodeInstruction>
            {
                new(OpCodes.Ldsfld, NetworkState),
                new(OpCodes.Ldc_I4, (int)CommonEnumerators.ClientNetworkState.Connected),
                new(OpCodes.Ceq),
                new(OpCodes.Brfalse_S, skipLabel),
            };
            instructions.AddRange(codeToExecuteIfConnected);
            if (codeToSkipIfConnected > 0)
            {
                Label elseLabel = generator.DefineLabel();
                instructions.Add(new CodeInstruction(OpCodes.Br, elseLabel));
                index += codeToSkipIfConnected;
                baseMethod[index].WithLabels(elseLabel);
                index -= codeToSkipIfConnected;
            }
            baseMethod.InsertRange(index, instructions);
            index += instructions.Count;
            baseMethod[index].WithLabels(skipLabel);
        }
    }
}