﻿using System.Collections.Generic;
using System.IO;

namespace Twinsanity
{
    /// <summary>
    /// Enumerator that determines what type of section this TwinsSection is. Preferable to making new classes for each section since they basically all have the same format.
    /// 
    /// Please append more section types at the END of this list.
    /// </summary>
    public enum SectionType {
        Null,
        Graphics, GraphicsX,
        Code, CodeDemo,
        Instance, InstanceDemo,

        Texture, TextureX,
        Material,
        Mesh, MeshX,
        Model,
        ArmatureModel,
        ActorModel,
        StaticModel,
        SpecialModel,
        Skybox,

        Object, ObjectDemo,
        Script,
        Animation,
        OGI,
        CodeModel,
        //SE_Eng,
        //SE_Fre,
        //SE_Ger,
        //SE_Spa,
        //SE_Ita,
        //SE_Jpn,

        UnknownInstance,
        AIPosition,
        AIPath,
        Position,
        Path,
        CollisionSurface,
        ObjectInstance, ObjectInstanceDemo,
        Trigger,
        Camera
    }

    public class TwinsSection : TwinsItem
    {
        private readonly uint magic = 0x00010001;
        private readonly uint magicV2 = 0x00010003;
        private int size;

        public uint Magic { get; set; }
        public List<TwinsItem> Records;
        public Dictionary<uint, int> RecordIDs;
        public SectionType Type { get; set; }
        public int Level { get; set; }
        public int ContentSize { get => GetContentSize(); }

        /// <summary>
        /// Loads the section from a file.
        /// </summary>
        /// <param name="reader">BinaryReader already seeked to where the section begins.</param>
        /// <param name="size">Size of the section.</param>
        public override void Load(BinaryReader reader, int size)
        {
            this.size = size;
            Records = new List<TwinsItem>();
            RecordIDs = new Dictionary<uint, int>();
            if (size < 0xC || ((Magic = reader.ReadUInt32()) != magic && Magic != magicV2))
                return;
            var count = reader.ReadInt32();
            var sec_size = reader.ReadUInt32();
            for (int i = 0; i < count; i++)
            {
                TwinsSubInfo sub = new TwinsSubInfo();
                sub.Off = reader.ReadUInt32();
                sub.Size = reader.ReadInt32();
                sub.ID = reader.ReadUInt32();
                var sk = reader.BaseStream.Position;
                reader.BaseStream.Position = sk - (i + 2) * 0xC + sub.Off;
                //var m = reader.ReadUInt32(); //get magic number [obsolete?]
                //reader.BaseStream.Position -= 4;
                switch (Type)
                {
                    case SectionType.Graphics:
                    case SectionType.GraphicsX:
                        switch (sub.ID)
                        {
                            case 0:
                                if (Type == SectionType.Graphics)
                                    LoadSection(reader, sub, SectionType.Texture);
                                else
                                    LoadSection(reader, sub, SectionType.TextureX);
                                break;
                            case 1:
                                LoadSection(reader, sub, SectionType.Material);
                                break;
                            case 2:
                                if (Type == SectionType.Graphics)
                                    LoadSection(reader, sub, SectionType.Mesh);
                                else
                                    LoadSection(reader, sub, SectionType.MeshX);
                                break;
                            case 3:
                                LoadSection(reader, sub, SectionType.Model);
                                break;
                            case 4:
                                LoadSection(reader, sub, SectionType.ArmatureModel);
                                break;
                            case 5:
                                LoadSection(reader, sub, SectionType.ActorModel);
                                break;
                            case 6:
                                LoadSection(reader, sub, SectionType.StaticModel);
                                break;
                            case 7:
                                LoadSection(reader, sub, SectionType.SpecialModel);
                                break;
                            case 8:
                                LoadSection(reader, sub, SectionType.Skybox);
                                break;
                            default:
                                LoadItem<TwinsItem>(reader, sub);
                                break;
                        }
                        break;
                    case SectionType.Instance:
                    case SectionType.InstanceDemo:
                        switch (sub.ID)
                        {
                            case 0:
                                LoadSection(reader, sub, SectionType.UnknownInstance);
                                break;
                            case 1:
                                LoadSection(reader, sub, SectionType.AIPosition);
                                break;
                            case 2:
                                LoadSection(reader, sub, SectionType.AIPath);
                                break;
                            case 3:
                                LoadSection(reader, sub, SectionType.Position);
                                break;
                            case 4:
                                LoadSection(reader, sub, SectionType.Path);
                                break;
                            case 5:
                                LoadSection(reader, sub, SectionType.CollisionSurface);
                                break;
                            case 6:
                                if (Type == SectionType.InstanceDemo)
                                    LoadSection(reader, sub, SectionType.ObjectInstanceDemo);
                                else
                                    LoadSection(reader, sub, SectionType.ObjectInstance);
                                break;
                            case 7:
                                LoadSection(reader, sub, SectionType.Trigger);
                                break;
                            case 8:
                                LoadSection(reader, sub, SectionType.Camera);
                                break;
                            default:
                                LoadItem<TwinsItem>(reader, sub);
                                break;
                        }
                        break;
                    case SectionType.Code:
                    case SectionType.CodeDemo:
                        switch (sub.ID)
                        {
                            case 0:
                                if (Type == SectionType.CodeDemo)
                                    LoadSection(reader, sub, SectionType.ObjectDemo);
                                else
                                    LoadSection(reader, sub, SectionType.Object);
                                break;
                            case 1:
                                LoadSection(reader, sub, SectionType.Script);
                                break;
                            case 2:
                                LoadSection(reader, sub, SectionType.Animation);
                                break;
                            case 3:
                                LoadSection(reader, sub, SectionType.OGI);
                                break;
                            case 4:
                                LoadSection(reader, sub, SectionType.CodeModel);
                                break;
                            //case 7:
                            //    LoadSection(reader, sub, SectionType.SE_Eng);
                            //    break;
                            //case 8:
                            //    LoadSection(reader, sub, SectionType.SE_Fre);
                            //    break;
                            //case 9:
                            //    LoadSection(reader, sub, SectionType.SE_Ger);
                            //    break;
                            //case 10:
                            //    LoadSection(reader, sub, SectionType.SE_Spa);
                            //    break;
                            //case 11:
                            //    LoadSection(reader, sub, SectionType.SE_Ita);
                            //    break;
                            //case 12:
                            //    LoadSection(reader, sub, SectionType.SE_Jpn);
                            //    break;
                            default:
                                LoadItem<TwinsItem>(reader, sub);
                                break;
                        }
                        break;
                    case SectionType.Texture:
                        LoadItem<Texture>(reader, sub);
                        break;
                    case SectionType.TextureX: //XBOX textures
                        LoadItem<TwinsItem>(reader, sub);
                        break;
                    case SectionType.Material:
                        LoadItem<Material>(reader, sub);
                        break;
                    case SectionType.Mesh:
                        LoadItem<Mesh>(reader, sub);
                        break;
                    case SectionType.MeshX: //XBOX meshes
                        LoadItem<TwinsItem>(reader, sub);
                        break;
                    case SectionType.Model:
                    case SectionType.StaticModel:
                        LoadItem<Model>(reader, sub);
                        break;
                    case SectionType.Object:
                        LoadItem<GameObject>(reader, sub);
                        break;
                    case SectionType.ObjectDemo: //PS2 DEMO objects
                        LoadItem<TwinsItem>(reader, sub);
                        break;
                    case SectionType.Script:
                        LoadItem<Script>(reader, sub);
                        break;
                    case SectionType.Position:
                        LoadItem<Position>(reader, sub);
                        break;
                    case SectionType.ObjectInstance:
                        LoadItem<Instance>(reader, sub);
                        break;
                    case SectionType.ObjectInstanceDemo: //PS2 DEMO instances
                        LoadItem<TwinsItem>(reader, sub);
                        break;
                    case SectionType.Trigger:
                        LoadItem<Trigger>(reader, sub);
                        break;
                    default:
                        LoadItem<TwinsItem>(reader, sub);
                        break;
                }
                reader.BaseStream.Position = sk;
            }
        }

        private void LoadItem<T>(BinaryReader reader, TwinsSubInfo sub) where T : TwinsItem, new()
        {
            T rec = new T
            {
                ID = sub.ID,
                Offset = (uint)reader.BaseStream.Position,
                Parent = this
            };
            rec.Load(reader, sub.Size);
            RecordIDs.Add(sub.ID, Records.Count);
            Records.Add(rec);
        }

        private void LoadSection(BinaryReader reader, TwinsSubInfo sub, SectionType type)
        {
            TwinsSection sec = new TwinsSection {
                ID = sub.ID,
                Level = Level + 1,
                Offset = (uint)reader.BaseStream.Position,
                Type = type,
                Parent = this
            };
            sec.Load(reader, sub.Size);
            RecordIDs.Add(sub.ID, Records.Count);
            Records.Add(sec);
        }

        public override void Save(BinaryWriter writer)
        {
            if (size == 0)
                return;
            writer.Write(Magic);
            writer.Write(Records.Count);
            writer.Write(ContentSize);

            var sec_off = Records.Count * 12 + 12;
            foreach (var i in Records)
            {
                writer.Write(sec_off);
                writer.Write(i.Size);
                writer.Write(i.ID);
                sec_off += i.Size;
            }

            foreach (var i in Records)
            {
                i.Save(writer);
            }
        }

        protected override int GetSize()
        {
            if (size < 0xC)
                return size;
            else
                return (Records.Count + 1) * 12 + ContentSize;
        }

        private int GetContentSize()
        {
            int c_size = 0;
            foreach (var i in Records)
                c_size += i.Size;
            return c_size;
        }

        public TwinsItem GetItem(uint id)
        {
            return Records[RecordIDs[id]];
        }

        public void AddItem(uint id, TwinsItem item)
        {
            RecordIDs.Add(id, Records.Count);
            Records.Add(item);
        }

        public void RemoveItem(uint id)
        {
            var index = RecordIDs[id];
            RecordIDs.Remove(id);
            Records.RemoveAt(index);
            var new_recs = new Dictionary<uint, int>(RecordIDs);
            RecordIDs.Clear();
            foreach (var i in new_recs)
            {
                if (i.Value >= index)
                    RecordIDs.Add(i.Key, i.Value - 1);
                else
                    RecordIDs.Add(i.Key, i.Value);
            }
        }
    }
}