﻿using Twinsanity;

namespace TwinsanityEditor
{
    public class PositionController : ItemController
    {
        public new Position Data { get; set; }

        public PositionController(MainForm topform, Position item, FileController targetFile) : base(topform, item, targetFile)
        {
            Data = item;
            AddMenu("Open editor", Menu_OpenEditor);
        }

        protected override string GetName()
        {
            return $"Position [ID {Data.ID}]";
        }

        protected override void GenText()
        {
            TextPrev = new string[3];
            TextPrev[0] = $"ID: {Data.ID}";
            TextPrev[1] = $"Offset: {Data.Offset} Size: {Data.Size}";
            TextPrev[2] = $"Position ({Data.Pos.X}, {Data.Pos.Y}, {Data.Pos.Z}, {Data.Pos.W})";
        }

        private void Menu_OpenEditor()
        {
            MainFile.OpenEditor((SectionController)Node.Parent.Tag);
        }
    }
}
