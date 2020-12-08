﻿using Twinsanity;
using System;
using System.IO;
using System.Collections.Generic;

namespace TwinsanityEditor
{
    public class ActorModelController : ItemController
    {
        public new ActorModel Data { get; set; }

        public ActorModelController(MainForm topform, ActorModel item, FileController targetFile) : base(topform, item, targetFile)
        {
            Data = item;
            //AddMenu("Export mesh to PLY", Menu_ExportPLY);
            //AddMenu("Open model viewer", Menu_OpenViewer);
        }

        protected override string GetName()
        {
            return string.Format("Actor Model [ID {0:X8}]", Data.ID);
        }

        protected override void GenText()
        {
            List<string> text = new List<string>();
            text.Add($"ID: {Data.ID} ");
            text.Add($"Size: {Data.Size}");

            for (int i = 0; i < Data.Models.Length; i++)
            {
                text.Add($"Model {i}: Material - {MainFile.GetMaterialName(Data.Models[i].MaterialID)} [ID: {Data.Models[i].MaterialID}]");
                for (int a = 0; a < Data.Models[i].SubModels.Length; a++)
                {
                    text.Add($"SubModel {a}: Int {Data.Models[i].SubModels[a].UnkInt}, Bone Count {Data.Models[i].SubModels[a].Bones.Length}");
                    for (int b = 0; b < Data.Models[i].SubModels[a].Bones.Length; b++)
                    {
                        //text.Add($"Bone {b}: Int {Data.Models[i].SubModels[a].Bones[b].UnkInt}");
                    }
                }
            }

            TextPrev = text.ToArray();
        }

        private void Menu_OpenViewer()
        {
            //MainFile.OpenModelViewer(this);
        }
    }
}
