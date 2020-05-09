﻿using Twinsanity;
using System;

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
            TextPrev = new string[8 + Data.Instances.Count];
            TextPrev[0] = $"ID: {Data.ID}";
            TextPrev[1] = $"Offset: {Data.Offset} Size: {Data.Size}";
            TextPrev[2] = $"Other ({Data.Coords[0].X}, {Data.Coords[0].Y}, {Data.Coords[0].Z}, {Data.Coords[0].W})";
            TextPrev[3] = $"Position ({Data.Coords[1].X}, {Data.Coords[1].Y}, {Data.Coords[1].Z}, {Data.Coords[1].W})";
            TextPrev[4] = $"Size ({Data.Coords[2].X}, {Data.Coords[2].Y}, {Data.Coords[2].Z}, {Data.Coords[2].W})";
            TextPrev[5] = $"SomeFloat: {Data.SomeFloat}";

            TextPrev[6] = $"Instances: {Data.Instances.Count}";
            for (int i = 0; i < Data.Instances.Count; ++i)
            {
                string obj_name = MainFile.GetObjectName(MainFile.GetInstance(Data.Parent.Parent.ID, Data.Instances[i]).ObjectID);
                obj_name = Utils.TextUtils.TruncateObjectName(obj_name, MainFile.GetInstance(Data.Parent.Parent.ID, Data.Instances[i]).ObjectID, "", " (Not in Objects)");

                TextPrev[7 + i] = $"Instance {Data.Instances[i]} {(obj_name != string.Empty ? $" - {obj_name}" : string.Empty)}";
            }

            TextPrev[7 + Data.Instances.Count] = $"Arguments: {Data.SomeUInt161} {Data.SomeUInt162} {Data.SomeUInt163} {Data.SomeUInt164}";
        }

        private void Menu_OpenEditor()
        {
            MainFile.OpenEditor((SectionController)Node.Parent.Tag);
        }
    }
}
