﻿using Twinsanity;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System;

namespace TwinsanityEditor
{
    public class SectionController : Controller
    {
        public TwinsSection Data { get; set; }
        public FileController MainFile { get; private set; }

        public SectionController(MainForm topform, TwinsSection item, FileController targetFile) : base (topform)
        {
            MainFile = targetFile;
            Data = item;
            AddMenu("Add item from raw data file", Menu_AddFromFile);
            if (item.Type != SectionType.Texture && item.Type != SectionType.TextureX
                && item.Type != SectionType.Material && item.Type != SectionType.Mesh
                && item.Type != SectionType.MeshX && item.Type != SectionType.Model
                && item.Type != SectionType.ArmatureModel && item.Type != SectionType.ActorModel && item.Type != SectionType.ArmatureModelX
                && item.Type != SectionType.StaticModel && item.Type != SectionType.SpecialModel
                && item.Type != SectionType.Skydome && !(item is TwinsFile))
            {
                AddMenu("Re-order by ID (asc.)", Menu_ReOrderByID_Asc);
                if (item.Type == SectionType.ObjectInstance
                    || item.Type == SectionType.AIPosition
                    || item.Type == SectionType.AIPath
                    || item.Type == SectionType.Position
                    || item.Type == SectionType.Path
                    || item.Type == SectionType.Trigger)
                {
                    AddMenu("Re-ID by order", Menu_ReIDByOrder);
                    AddMenu("Open editor", Menu_OpenEditor);
                }
                else if (item.Type == SectionType.Instance)
                {
                    AddMenu("Clear instance section", Menu_ClearInstanceSection);
                    AddMenu("Fill instance section", Menu_FillInstanceSection);
                }
                else if (item.Type >= SectionType.SE && item.Type <= SectionType.SE_Jpn)
                {
                    AddMenu("Extract extra data", Menu_ExtractExtraData);
                }
            }
            else if (item is TwinsFile f && f.Type == TwinsFile.FileType.RM2)
            {
                AddMenu("Add remaining instance sections", Menu_FileFillInstanceSections);
            }
            else
            {
                AddMenu("Re-order by ID (desc.)", Menu_ReOrderByID_Desc);
            }
            if (item.Type == SectionType.Mesh || item.Type == SectionType.MeshX || item.Type == SectionType.Model || item.Type == SectionType.StaticModel)
            {
                AddMenu("Export all meshes to PLY", Menu_ExportAllPLY);
            }
        }

        protected override string GetName()
        {
            return $"{Data.Type} Section [ID {Data.ID}]";
        }

        protected override void GenText()
        {
            TextPrev = new string[Data.ExtraData == null ? 3 : 4];
            TextPrev[0] = $"ID: {Data.ID}";
            TextPrev[1] = $"Offset: {Data.Offset} Size: {Data.Size}";
            TextPrev[2] = $"ContentSize: {Data.ContentSize} Element Count: {Data.Records.Count}";
            if (Data.ExtraData != null)
                TextPrev[3] = $"ExtraDataSize: {Data.ExtraData.Length}";
        }

        public void AddItem(uint id, TwinsItem item)
        {
            Data.AddItem(id, item);
            TopForm.GenTreeNode(item, this, MainFile);
            UpdateText();
            ((Controller)Node.Nodes[Data.RecordIDs[item.ID]].Tag).UpdateText();
        }

        public void RemoveItem(TwinsItem item)
        {
            RemoveItem(item.ID);
        }

        public void RemoveItem(uint id)
        {
            Node.Nodes[Data.RecordIDs[id]].Remove();
            Data.RemoveItem(id);
            UpdateText();
        }

        public void ChangeID(uint old_id, uint new_id)
        {
            if (Data.ContainsItem(new_id))
                throw new System.ArgumentException("New ID already exists.");
            var index = Data.RecordIDs[old_id];
            Data.GetItem<TwinsItem>(old_id).ID = new_id;
            Data.RecordIDs.Remove(old_id);
            Data.RecordIDs.Add(new_id, index);
        }

        private Controller GetItem(uint id)
        {
            return (Controller)Node.Nodes[Data.RecordIDs[id]].Tag;
        }

        public T GetItem<T>(uint id) where T : Controller
        {
            return Node.Nodes[Data.RecordIDs[id]].Tag as T;
        }

        private void Menu_ExtractExtraData()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = GetName().Replace(":", string.Empty);
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                FileStream file = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write);
                BinaryWriter writer = new BinaryWriter(file);
                writer.Write(Data.ExtraData);
                writer.Close();
            }
        }

        private void Menu_ClearInstanceSection()
        {
            for (uint i = 0; i <= 8; ++i)
            {
                if (Data.ContainsItem(i))
                {
                    RemoveItem(i);
                }
            }
        }

        private void Menu_FillInstanceSection()
        {
            for (uint i = 0; i <= 8; ++i)
            {
                SectionType type = SectionType.Null;
                switch (i)
                {
                    case 0: type = SectionType.InstanceTemplate; break;
                    case 1: type = SectionType.AIPosition; break;
                    case 2: type = SectionType.AIPath; break;
                    case 3: type = SectionType.Position; break;
                    case 4: type = SectionType.Path; break;
                    case 5: type = SectionType.CollisionSurface; break;
                    case 6: type = SectionType.ObjectInstance; break;
                    case 7: type = SectionType.Trigger; break;
                    case 8: type = SectionType.Camera; break;
                }
                if (!Data.ContainsItem(i))
                {
                    TwinsSection sec = new TwinsSection
                    {
                        ID = i,
                        Level = Data.Level + 1,
                        Offset = 0,
                        Type = type,
                        Parent = Data
                    };
                    AddItem(i, sec);
                }
            }
        }

        private void Menu_FileFillInstanceSections()
        {
            for (uint i = 0; i <= 7; ++i)
            {
                if (!Data.ContainsItem(i))
                {
                    TwinsSection sec = new TwinsSection
                    {
                        ID = i,
                        Level = Data.Level + 1,
                        Offset = 0,
                        Type = SectionType.Instance,
                        Parent = Data
                    };
                    AddItem(i, sec);
                }
            }
        }

        private void Menu_ReOrderByID_Asc()
        {
            if (Data.Type == SectionType.ObjectInstance)
            {
                MainFile.CloseEditor(Editors.Instance, (int)Data.Parent.ID);
            }
            else if (Data.Type == SectionType.Position)
            {
                MainFile.CloseEditor(Editors.Position, (int)Data.Parent.ID);
            }
            Node.TreeView.BeginUpdate();
            Node.Nodes.Clear();
            SortedDictionary<uint, int> sdic = new SortedDictionary<uint, int>(Data.RecordIDs);
            List<TwinsItem> slist = new List<TwinsItem>();
            foreach (var i in sdic)
            {
                slist.Add(Data.Records[i.Value]);
                TopForm.GenTreeNode(Data.Records[i.Value], this, MainFile);
            }
            Data.Records = slist;
            Node.TreeView.EndUpdate();
        }

        private void Menu_ReOrderByID_Desc()
        {
            Node.TreeView.BeginUpdate();
            Node.Nodes.Clear();
            SortedDictionary<uint, int> sdic = new SortedDictionary<uint, int>(new Utils.DescendingComparer<uint>());
            foreach (var i in Data.RecordIDs)
                sdic.Add(i.Key, i.Value);
            List<TwinsItem> slist = new List<TwinsItem>();
            foreach (var i in sdic)
            {
                slist.Add(Data.Records[i.Value]);
                TopForm.GenTreeNode(Data.Records[i.Value], this, MainFile);
            }
            Data.Records = slist;
            Node.TreeView.EndUpdate();
        }

        private void Menu_ReIDByOrder()
        {
            if (Data.Type == SectionType.ObjectInstance)
            {
                MainFile.CloseEditor(Editors.Instance, (int)Data.Parent.ID);
            }
            else if (Data.Type == SectionType.Position)
            {
                MainFile.CloseEditor(Editors.Position, (int)Data.Parent.ID);
            }
            Node.TreeView.BeginUpdate();
            Node.Nodes.Clear();
            Data.RecordIDs.Clear();
            for (int i = 0; i < Data.Records.Count; ++i)
            {
                Data.Records[i].ID = (uint)i;
                Data.RecordIDs.Add((uint)i, i);
                TopForm.GenTreeNode(Data.Records[i], this, MainFile);
            }
            Node.TreeView.EndUpdate();
        }

        private void Menu_OpenEditor()
        {
            MainFile.OpenEditor(this);
        }

        private void Menu_AddFromFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                uint id = 0;
                if (Data.Records.Count > 0)
                {
                    if (ofd.FileName.Contains("[ID ")) //terrible hack, but i'm too lazy to rework IDasker
                    {
                        int startPos = ofd.FileName.IndexOf("[ID ") + 4;
                        string textID = ofd.FileName.Substring(startPos, ((ofd.FileName.Length - 1) - startPos));
                        id = Convert.ToUInt32(textID, 16);
                        if (Data.ContainsItem(id))
                        {
                            MessageBox.Show("Item of this ID already exists!");
                            return;
                        }
                    }
                    else
                    {
                        id = Data.Records[Data.Records.Count - 1].ID + 1;
                    }
                }
                TwinsItem item = new TwinsItem() { ID = id } ;
                
                BinaryReader reader = new BinaryReader(new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read));
                if (item is Instance instance)
                {
                    MainFile.CloseEditor(Editors.Instance, (int)Data.Parent.Parent.ID);
                    instance.Load(reader, (int)reader.BaseStream.Length);
                }
                else if (item is Position position)
                {
                    MainFile.CloseEditor(Editors.Position, (int)Data.Parent.Parent.ID);
                    position.Load(reader, (int)reader.BaseStream.Length);
                }
                else if (item is Twinsanity.Path path)
                {
                    MainFile.CloseEditor(Editors.Path, (int)Data.Parent.Parent.ID);
                    path.Load(reader, (int)reader.BaseStream.Length);
                }
                else if (item is Trigger trigger)
                {
                    MainFile.CloseEditor(Editors.Trigger, (int)Data.Parent.Parent.ID);
                    trigger.Load(reader, (int)reader.BaseStream.Length);
                }
                else
                    item.Load(reader, (int)reader.BaseStream.Length);
                reader.Close();

                Data.AddItem(id, item);
                TopForm.GenTreeNode(item, this, MainFile);
                UpdateText();
                ((Controller)Node.Nodes[Data.RecordIDs[id]].Tag).UpdateText();
            }
        }

        private void Menu_ExportAllPLY()
        {
            if (Data.Type == SectionType.Model || Data.Type == SectionType.StaticModel)
                if (MessageBox.Show("PLY export is experimental, material and texture information will not be exported. Continue anyway?", "Export Warning", MessageBoxButtons.YesNo) == DialogResult.No) return;
            var fdbSave = new CommonOpenFileDialog { IsFolderPicker = true };
            if (fdbSave.ShowDialog() == CommonFileDialogResult.Ok)
            {
                foreach (TreeNode n in Node.Nodes)
                {
                    string fname = fdbSave.FileName + @"\{n.Text}.ply";
                    if (n.Tag is MeshController c)
                    {
                        File.WriteAllBytes(fname, c.Data.ToPLY());
                    }
                    else if (n.Tag is ModelController d)
                    {
                        File.WriteAllBytes(fname, Data.Parent.GetItem<TwinsSection>(2).GetItem<Mesh>(d.Data.MeshID).ToPLY());
                    }
                }
            }
        }
    }
}
