using EM_Common;
using System;

namespace EM_Executable
{
    public static partial class EM_Executable_Test
    {
        public static void PrintOperandInfo()
        {
            PrintHeading("Operands");
            foreach (string operandName in infoStore.operandAdmin.GetOperandNames())
            {
                RandColor.Write($"{operandName}  ", GetSwapColor());
                RandColor.Write($"{infoStore.operandAdmin.GetParType(operandName)}  ", GetSwapColor());
                RandColor.Write($"{(infoStore.operandAdmin.GetIsMonetary(operandName) ? "monetary" : "not-monetary")}  ", GetSwapColor());
                RandColor.Write($"{(infoStore.operandAdmin.GetIsGlobal(operandName) ? "global" : "not-global")}  ", GetSwapColor());
                try { RandColor.Write($"var-index:{infoStore.operandAdmin.GetIndexInPersonVarList(operandName)}  ", GetSwapColor()); } catch { }

                if (infoStore.operandAdmin.GetParType(operandName) == DefPar.PAR_TYPE.IL &&
                    infoStore.operandAdmin.GetILContent(operandName).Count > 0)
                {
                    RandColor.Write($"content:", GetSwapColor());
                    foreach (var ilComponent in infoStore.operandAdmin.GetILContent(operandName))
                    {
                        string pm = ilComponent.Value.ToString() + "*";
                        if (ilComponent.Value == 1) pm = "+"; if (ilComponent.Value == -1) pm = "-";
                        Console.Write($"{pm}{ilComponent.Key}");
                    }
                }
                Console.WriteLine();
            }
        }
    }
}
