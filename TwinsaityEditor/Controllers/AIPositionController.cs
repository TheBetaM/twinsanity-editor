﻿using Twinsanity;

namespace TwinsanityEditor
{
    public class AIPositionController : ItemController
    {
        public new AIPosition Data { get; set; }

        public AIPositionController(MainForm topform, AIPosition item, FileController targetFile) : base (topform, item, targetFile)
        {
            Data = item;
        }

        protected override string GetName()
        {
            return $"AI Position [ID {Data.ID}]";
        }

        protected override void GenText()
        {
            TextPrev = new string[4];
            TextPrev[0] = $"ID: {Data.ID}";
            TextPrev[1] = $"Offset: {Data.Offset} Size: {Data.Size}";
            TextPrev[2] = $"Position: {Data.Pos.X}, {Data.Pos.Y}, {Data.Pos.Z}, {Data.Pos.W}";
            TextPrev[3] = $"Argument: {Data.Num}";
        }
    }
}
