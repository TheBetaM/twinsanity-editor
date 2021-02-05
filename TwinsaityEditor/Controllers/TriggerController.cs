﻿using Twinsanity;
using System;
using System.Collections.Generic;

namespace TwinsanityEditor
{
    public class TriggerController : ItemController
    {
        public new Trigger Data { get; set; }

        public TriggerController(MainForm topform, Trigger item, FileController targetFile) : base(topform, item, targetFile)
        {
            Data = item;
            AddMenu("Open editor", Menu_OpenEditor);
        }

        protected override string GetName()
        {
            return $"Trigger [ID {Data.ID}]";
        }

        protected override void GenText()
        {
            List<string> text = new List<string>();
            text.Add($"ID: {Data.ID}");
            text.Add($"Offset: {Data.Offset} Size: {Data.Size}");
            text.Add($"Other ({Data.Coords[0].X}, {Data.Coords[0].Y}, {Data.Coords[0].Z}, {Data.Coords[0].W})");
            text.Add($"Position ({Data.Coords[1].X}, {Data.Coords[1].Y}, {Data.Coords[1].Z}, {Data.Coords[1].W})");
            text.Add($"Size ({Data.Coords[2].X}, {Data.Coords[2].Y}, {Data.Coords[2].Z}, {Data.Coords[2].W})");
            text.Add($"Emabled: {Data.Enabled} SomeFloat: {Data.SomeFloat} SectionHead: {Data.SectionHead}");
            text.Add($"Argument 1: {Data.Arg1 & 0x3FF} Context: {(Data.Arg1 >> 10)}");
            text.Add($"Argument 2: {Data.Arg2 & 0x3FF} Context: {(Data.Arg2 >> 10)}");
            text.Add($"Argument 3: {Data.Arg3 & 0x3FF} Context: {(Data.Arg3 >> 10)}");
            text.Add($"Argument 4: {Data.Arg4 & 0x3FF} Context: {(Data.Arg4 >> 10)}");

            text.Add($"Instances: {Data.Instances.Count}");
            for (int i = 0; i < Data.Instances.Count; ++i)
            {
                string obj_name = MainFile.GetObjectName((ushort)MainFile.GetInstanceID(Data.Parent.Parent.ID, Data.Instances[i]));
                obj_name = Utils.TextUtils.TruncateObjectName(obj_name, (ushort)MainFile.GetInstanceID(Data.Parent.Parent.ID, Data.Instances[i]), "", " (Not in Objects)");

                text.Add($"Instance {Data.Instances[i]} {(obj_name != string.Empty ? $" - {obj_name}" : string.Empty)}");
            }

            TextPrev = text.ToArray();
        }

        private void Menu_OpenEditor()
        {
            MainFile.OpenEditor((SectionController)Node.Parent.Tag);
        }
    }
}
