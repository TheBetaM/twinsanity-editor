﻿using System.Windows.Forms;
using System.IO;
using Twinsanity;

namespace TwinsanityEditor
{
    public class ItemController : Controller
    {
        public TwinsItem Data { get; set; }
        public FileController MainFile { get; private set; }

        public ItemController(MainForm topform, TwinsItem item, FileController targetFile) : base(topform)
        {
            MainFile = targetFile;
            Data = item;
            AddMenu("Extract raw data to file", Menu_ExtractItem);
            AddMenu("Replace raw data with new file", Menu_ReplaceItem);
        }

        protected override string GetName()
        {
            return $"Item [ID {Data.ID}]";
        }

        protected override void GenText()
        {
            TextPrev = new string[2];
            TextPrev[0] = $"ID: {Data.ID}";
            TextPrev[1] = $"Offset: {Data.Offset} Size: {Data.Size}";
        }

        private void Menu_ExtractItem()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = GetName().Replace(":", string.Empty);
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                FileStream file = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write);
                BinaryWriter writer = new BinaryWriter(file);
                Data.Save(writer);
                writer.Close();
            }
        }

        private void Menu_ReplaceItem()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                BinaryReader reader = new BinaryReader(new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read));
                if (Data is ChunkLinks links)
                {
                    MainFile.CloseEditor(Editors.ChunkLinks);
                    links.Load(reader, (int)reader.BaseStream.Length);
                }
                else if (Data is Instance instance)
                {
                    MainFile.CloseEditor(Editors.Instance, (int)Data.Parent.Parent.ID);
                    instance.Load(reader, (int)reader.BaseStream.Length);
                }
                else if (Data is Position position)
                {
                    MainFile.CloseEditor(Editors.Position, (int)Data.Parent.Parent.ID);
                    position.Load(reader, (int)reader.BaseStream.Length);
                }
                else if (Data is Twinsanity.Path path)
                {
                    MainFile.CloseEditor(Editors.Path, (int)Data.Parent.Parent.ID);
                    path.Load(reader, (int)reader.BaseStream.Length);
                }
                else if (Data is Trigger trigger)
                {
                    MainFile.CloseEditor(Editors.Trigger, (int)Data.Parent.Parent.ID);
                    trigger.Load(reader, (int)reader.BaseStream.Length);
                }
                else if (Data is ColData col)
                {
                    MainFile.CloseEditor(Editors.ColData);
                    col.Load(reader, (int)reader.BaseStream.Length);
                }
                else
                    Data.Load(reader, (int)reader.BaseStream.Length);
                reader.Close();
                UpdateText();
            }
        }
    }
}
