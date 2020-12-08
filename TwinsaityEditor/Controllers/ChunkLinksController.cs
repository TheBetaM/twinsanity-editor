﻿using System;
using System.Collections.Generic;
using Twinsanity;

namespace TwinsanityEditor
{
    public class ChunkLinksController : ItemController
    {
        public new ChunkLinks Data { get; set; }

        public ChunkLinksController(MainForm topform, ChunkLinks item, FileController targetFile) : base(topform, item, targetFile)
        {
            Data = item;
            AddMenu("Open editor", Menu_OpenEditor);
        }

        protected override string GetName()
        {
            return $"Chunk Links [ID {Data.ID}]";
        }

        protected override void GenText()
        {
            List<string> text = new List<string>();
            text.Add($"ID: {Data.ID}");
            text.Add($"Offset: {Data.Offset} Size: {Data.Size}");
            text.Add($"LinkCount: {Data.Links.Count}");
            for (int i = 0; i < Data.Links.Count; ++i)
            {
                text.Add($"Link{i}");
                text.Add($"Type: {Data.Links[i].Type}");
                text.Add($"Directory: {new string(Data.Links[i].Path)}");
                text.Add($"Flags: {Convert.ToString(Data.Links[i].Flags, 16).ToUpper()}");
                ChunkLinks.ChunkLink.LinkTree? Ptr = Data.Links[i].TreeRoot;
                string add = "";
                int depth = 0;
                while (Ptr != null)
                {
                    text.Add(add + $"Load Zone {depth}");
                    add += "     ";
                    depth++;
                    if (Ptr.Value.Ptr != null)
                    {
                        Ptr = (ChunkLinks.ChunkLink.LinkTree)Ptr.Value.Ptr;
                    }
                    else
                    {
                        Ptr = null;
                    }
                }
                text.Add("");
            }

            TextPrev = text.ToArray();
        }

        private void Menu_OpenEditor()
        {
            MainFile.OpenEditor(this);
        }
    }
}
