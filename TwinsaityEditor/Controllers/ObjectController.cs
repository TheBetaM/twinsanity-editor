﻿using System;
using System.Collections.Generic;
using Twinsanity;

namespace TwinsanityEditor
{
    public class ObjectController : ItemController
    {
        public new GameObject Data { get; set; }

        public ObjectController(MainForm topform, GameObject item, FileController targetFile) : base(topform, item, targetFile)
        {
            Data = item;
        }

        protected override string GetName()
        {
            string obj_name = Data.Name;
            obj_name = Utils.TextUtils.TruncateObjectName(obj_name, (ushort)Data.ID, "*", "");
            return $"{obj_name} [ID {Data.ID}]";
        }

        protected override void GenText()
        {
            List<string> text = new List<string>();
            text.Add($"ID: {Data.ID}");
            text.Add($"Offset: {Data.Offset} Size: {Data.Size}");
            text.Add($"Name: {Data.Name}");
            text.Add($"Unknown bitfield: 0x{Data.UnkBitfield.ToString("X")}");
            for (int i = 0; i < Data.ScriptSlots.Length; ++i)
            {
                var slotName = "Reserved";
                var slotAmt = Data.ScriptSlots[i];
                switch(i)
                {
                    case 0:
                        {
                            slotName = "Pairs(OGI/Animation)";
                        }
                        break;
                    case 1:
                        {
                            slotName = "Scripts";
                        }
                        break;
                    case 2:
                        {
                            slotName = "Objects";
                        }
                        break;
                    case 3:
                        {
                            slotName = "UInt32";
                        }
                        break;
                    case 4:
                        {
                            slotName = "Sounds";
                        }
                        break;
                }
                text.Add($"{slotName} script slots: {slotAmt}");
            }

            text.Add($"");
            text.Add($"Reference Data");
            text.Add($"");

            text.Add($"UnknownInt32Count: {Data.UI32.Length}");
            for (int i = 0; i < Data.UI32.Length; ++i)
                text.Add(Data.UI32[i].ToString("X"));

            text.Add($"OGICount: {Data.OGIs.Length}");
            for (int i = 0; i < Data.OGIs.Length; ++i)
                text.Add(Data.OGIs[i].ToString());

            text.Add($"AnimCount: {Data.Anims.Length}");
            for (int i = 0; i < Data.Anims.Length; ++i)
                text.Add(Data.Anims[i].ToString());

            text.Add($"ScriptCount: {Data.Scripts.Length}");
            for (int i = 0; i < Data.Scripts.Length; ++i)
            {
                string script_line = "#" + i.ToString() + ": ";
                if (Enum.IsDefined(typeof(DefaultEnums.GameObjectScriptOrder), i))
                {
                    script_line += "(" + (DefaultEnums.GameObjectScriptOrder)i + ") ";
                }
                if (Enum.IsDefined(typeof(DefaultEnums.ScriptID), Data.Scripts[i]))
                {
                    script_line += (DefaultEnums.ScriptID)Data.Scripts[i];
                }
                else
                {
                    script_line += Data.Scripts[i].ToString();
                }
                text.Add(script_line);
            }

            text.Add($"ObjectCount: {Data.Objects.Length}");
            for (int i = 0; i < Data.Objects.Length; ++i)
                text.Add(Data.Objects[i].ToString());

            text.Add($"SoundCount: {Data.Sounds.Length}");
            for (int i = 0; i < Data.Sounds.Length; ++i)
                text.Add(Data.Sounds[i].ToString());

            text.Add($"");
            text.Add($"Preload Data");
            text.Add($"");

            text.Add($"ObjectCount: {Data.cObjects.Length}");
            for (int i = 0; i < Data.cObjects.Length; ++i)
                text.Add(Data.cObjects[i].ToString());

            text.Add($"OGICount: {Data.cOGIs.Length}");
            for (int i = 0; i < Data.cOGIs.Length; ++i)
                text.Add(Data.cOGIs[i].ToString());

            text.Add($"AnimCount: {Data.cAnims.Length}");
            for (int i = 0; i < Data.cAnims.Length; ++i)
                text.Add(Data.cAnims[i].ToString());

            text.Add($"CMCount: {Data.cCM.Length}");
            for (int i = 0; i < Data.cCM.Length; ++i)
                text.Add(Data.cCM[i].ToString());

            text.Add($"ScriptCount: {Data.cScripts.Length}");
            for (int i = 0; i < Data.cScripts.Length; ++i)
            {
                string script_line = "#" + i.ToString() + ": ";
                if (Enum.IsDefined(typeof(DefaultEnums.ScriptID), Data.cScripts[i]))
                {
                    script_line += (DefaultEnums.ScriptID)Data.cScripts[i];
                }
                else
                {
                    script_line += Data.cScripts[i].ToString();
                }
                text.Add(script_line);
            }

            text.Add($"UnkCount: {Data.cUnk.Length}");
            for (int i = 0; i < Data.cUnk.Length; ++i)
                text.Add(Data.cUnk[i].ToString());

            text.Add($"SoundCount: {Data.cSounds.Length}");
            for (int i = 0; i < Data.cSounds.Length; ++i)
                text.Add(Data.cSounds[i].ToString());

            TextPrev = text.ToArray();
        }
    }
}
