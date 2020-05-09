﻿using Twinsanity;
using System;
using System.IO;
using System.Windows.Forms;

namespace TwinsanityEditor
{
    public class ModelController : ItemController
    {
        public new Model Data { get; set; }

        public ModelController(MainForm topform, Model item, FileController targetFile) : base(topform, item, targetFile)
        {
            Data = item;
            AddMenu("Export mesh to PLY", Menu_ExportPLY);
            AddMenu("Open model viewer", Menu_OpenViewer);
        }

        protected override string GetName()
        {
            return string.Format("Model [ID {0:X8}]", Data.ID);
        }

        protected override void GenText()
        {
            TextPrev = new string[4 + Data.MaterialIDs.Length];
            TextPrev[0] = string.Format("ID: {0:X8}", Data.ID);
            TextPrev[1] = $"Offset: {Data.Offset} Size: {Data.Size}";
            TextPrev[2] = $"Header: {Data.Header} MaterialCount: {Data.MaterialIDs.Length}";
            for (int i = 0; i < Data.MaterialIDs.Length; ++i)
                TextPrev[3 + i] = MainFile.GetMaterialName(Data.MaterialIDs[i]);
            TextPrev[3 + Data.MaterialIDs.Length] = string.Format("Mesh: {0:X8}", Data.MeshID);
        }

        private void Menu_ExportPLY()
        {
            if (MessageBox.Show("PLY export is experimental, material and texture information will not be exported. Continue anyway?", "Export Warning", MessageBoxButtons.YesNo) == DialogResult.No) return;
            SaveFileDialog sfd = new SaveFileDialog { Filter = "PLY files (*.ply)|*.ply", FileName = GetName() };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllBytes(sfd.FileName, Data.Parent.Parent.GetItem<TwinsSection>(2).GetItem<Mesh>(Data.MeshID).ToPLY());
            }
        }

        private void Menu_OpenViewer()
        {
            MainFile.OpenModelViewer(this);
        }
    }
}
