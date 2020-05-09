﻿using Twinsanity;
using System.Collections.Generic;

namespace TwinsanityEditor
{
    public class SpecialModelController : ItemController
    {
        public new SpecialModel Data { get; set; }

        public SpecialModelController(MainForm topform, SpecialModel item, FileController targetFile) : base(topform, item, targetFile)
        {
            Data = item;
            AddMenu("Open model viewer", Menu_OpenViewer);
        }

        protected override string GetName()
        {
            return string.Format("Special Model [ID {0:X8}]", Data.ID);
        }

        protected override void GenText()
        {
            List<string> text = new List<string>();

            text.Add(string.Format("ID: {0:X8}", Data.ID));
            text.Add($"Offset: {Data.Offset} Size: {Data.Size}");
            text.Add($"LOD Count: {Data.K_Count - 1} ");
            int cur_LOD = 0;
            if (Data.K_Count > 1)
            {
                text.Add(string.Format("LOD " + cur_LOD + " Model ID: {0:X8}", Data.LODModelIDs[2]));
                text.Add("Draw Distance Value " + cur_LOD + ": " + Data.K_Val[cur_LOD + 1]);
                cur_LOD++;
            }
            if (Data.K_Count > 2)
            {
                text.Add(string.Format("LOD " + cur_LOD + " Model ID: {0:X8}", Data.LODModelIDs[1]));
                text.Add("Draw Distance Value " + cur_LOD + ": " + Data.K_Val[cur_LOD + 1]);
                cur_LOD++;
            }
            if (Data.K_Count > 3)
            {
                text.Add(string.Format("LOD " + cur_LOD + " Model ID: {0:X8}", Data.LODModelIDs[0]));
                text.Add("Draw Distance Value " + cur_LOD + ": " + Data.K_Val[cur_LOD + 1]);
                cur_LOD++;
            }
            text.Add(string.Format("LOD " + cur_LOD + " Model ID: {0:X8}", Data.LODModelIDs[3]));
            text.Add("Draw Distance Value " + cur_LOD + ": " + Data.K_Val[0]);

            TextPrev = text.ToArray();
            
        }

        private void Menu_OpenViewer()
        {
            MainFile.OpenModelViewer(this);
        }
    }
}
