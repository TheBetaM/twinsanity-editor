using System.Collections.Generic;
using Twinsanity;
using System;

namespace TwinsanityEditor
{
    public class ScriptController : ItemController
    {
        public new Script Data { get; set; }

        public ScriptController(MainForm topform, Script item, FileController targetFile) : base(topform, item, targetFile)
        {
            Data = item;
        }

        protected override string GetName()
        {
            return $"{Data.Name ?? "Script"} [ID {Data.ID}]";
        }

        protected override void GenText()
        {
            List<string> text = new List<string>();
            text.Add($"ID: {Data.ID} {(Data.Name != null ? $" Name: {Data.Name}" : string.Empty)}");
            text.Add($"Offset: {Data.Offset} Size: {Data.Size}");
            if (Data?.HeaderScript != null)
            {
                text.Add($"Pairs(LinkedScriptIndex/UnkInt): {Data.HeaderScript.unkIntPairs}");
                for (int i = 0; i < Data.HeaderScript.unkIntPairs; i++)
                {
                    ushort scriptID = (ushort)(Data.HeaderScript.pairs[i].mainScriptIndex - 1);
                    string script_line = "Pair " + scriptID.ToString() + ":";
                    if (Enum.IsDefined(typeof(DefaultEnums.ScriptID), scriptID))
                    {
                        script_line += " (" + (DefaultEnums.ScriptID)scriptID + ")";
                    }
                    script_line += " / " + Data.HeaderScript.pairs[i].unkInt2;
                    text.Add(script_line);
                    //text.Add($"Pair: {Data.HeaderScript.pairs[i].mainScriptIndex - 1} / {Data.HeaderScript.pairs[i].unkInt2}");
                }
            }
            TextPrev = text.ToArray();
        }
    }
}
