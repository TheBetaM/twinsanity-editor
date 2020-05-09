using System.IO;
using System.Collections.Generic;

namespace Twinsanity
{
    public class MeshX : TwinsItem
    {
        public List<SubModel> SubModels { get; set; } = new List<SubModel>();

        public override void Load(BinaryReader reader, int size)
        {
            var sk = reader.BaseStream.Position;
            var count = reader.ReadInt32();

            SubModels.Clear();
            for (int i = 0; i < count; i++)
            {
                SubModel sub = new SubModel()
                {
                    VertexCount = reader.ReadInt32(),
                    BlockSize = reader.ReadUInt32(),
                    k = reader.ReadUInt16(),
                    c = reader.ReadUInt16(),
                    Null1 = reader.ReadUInt32(),
                    Something = reader.ReadUInt32(),
                    Null2 = reader.ReadUInt32()
                };
                SubModels.Add(sub);
            }

            reader.BaseStream.Position = sk;
            Data = reader.ReadBytes(size);
        }

        public override void Save(BinaryWriter writer)
        {
            writer.Write(Data);

            /*
            writer.Write(SubModels.Count);
            for (int i = 0; i < SubModels.Count; ++i)
            {
                var sub = SubModels[i];
                writer.Write(sub.VertexCount);
                writer.Write(sub.BlockSize);
                writer.Write(sub.k);
                writer.Write(sub.c);
                writer.Write(sub.Null1);
                writer.Write(sub.Something);
                writer.Write(sub.Null2);
                for (int a = 0; a < sub.Groups.Count; ++a)
                {
                    var group = sub.Groups[a];
                    writer.Write(group.SomeNum1);
                    writer.Write(group.VertexCount);
                    writer.Write(group.Some80h);
                    writer.Write(group.Null1);
                    writer.Write(group.SomeNum2);
                    writer.Write(group.SomeNum3);
                    writer.Write(group.Null2);
                    writer.Write(group.Signature1);
                    writer.Write(group.SomeShit1);
                    writer.Write(group.SomeShit2);
                    writer.Write(group.SomeShit3);
                    writer.Write(group.Signature2);
                    if (group.VertHead > 0) //vertex positions
                    {
                        writer.Write(group.VertHead);
                        for (int j = 0; j < group.VertexCount; ++j)
                        {
                            writer.Write(group.Vertex[j].X);
                            writer.Write(group.Vertex[j].Y);
                            writer.Write(group.Vertex[j].Z);
                        }
                    }
                    if (group.VDataHead > 0) //vertex data
                    {
                        writer.Write(group.VDataHead);
                        for (int j = 0; j < group.VertexCount; ++j)
                        {
                            writer.Write(group.VData[j].R);
                            writer.Write(group.VData[j].PX);
                            writer.Write(group.VData[j].X);
                            writer.Write(group.VData[j].G);
                            writer.Write(group.VData[j].PY);
                            writer.Write(group.VData[j].Y);
                            writer.Write(group.VData[j].B);
                            writer.Write(group.VData[j].SomeFloat);
                            writer.Write(group.VData[j].CONN);
                            writer.Write(group.VData[j].Null1);
                        }
                    }
                    if (group.UVHead > 0) //textures?
                    {
                        writer.Write(group.UVHead);
                        for (int j = 0; j < group.VertexCount; ++j)
                        {
                            writer.Write(group.UV[j].X);
                            writer.Write(group.UV[j].Y);
                            writer.Write(group.UV[j].Z);
                        }
                    }
                    if (group.ShiteHead > 0) //lighting?
                    {
                        writer.Write(group.ShiteHead);
                        for (int j = 0; j < group.VertexCount; ++j)
                            writer.Write(group.Shit[j]);
                    }
                    writer.Write(group.EndSignature1);
                    writer.Write(group.EndSignature2);
                }
                writer.Write(sub.Groups[sub.Groups.Count - 1].leftovers);
            }
            */
        }

        protected override int GetSize()
        {
            /*
            int size = 4;
            foreach (var i in SubModels)
            {
                size += 24;
                foreach (var j in i.Groups)
                {
                    size += 48;
                    if (j.VertHead > 0)
                    {
                        size += 4 + 12 * j.VertexCount;
                    }
                    if (j.VDataHead > 0)
                    {
                        size += 4 + 16 * j.VertexCount;
                    }
                    if (j.UVHead > 0)
                    {
                        size += 4 + 12 * j.VertexCount;
                    }
                    if (j.ShiteHead > 0)
                    {
                        size += 4 + 4 * j.VertexCount;
                    }
                    size += j.leftovers.Length;
                }
            }
            return size;
            */
            return Data.Length;
        }

        #region STRUCTURES
        public struct SubModel
        {
            // Primary Header
            public int VertexCount;
            public uint BlockSize;
            public ushort k, c;
            public uint Null1;
            public uint Something;
            public uint Null2;
            public List<Group> Groups;
        }
        public struct Group
        {
            public uint SomeNum1;
            public byte VertexCount;
            public byte Some80h;
            public ushort Null1;
            public uint SomeNum2;
            public uint SomeNum3;
            public uint Null2;
            public uint Signature1;
            public uint SomeShit1;
            public uint SomeShit2;
            public uint SomeShit3;
            public uint Signature2;
            public uint VertHead;
            public Position3[] Vertex;
            public uint VDataHead;
            public VertexData[] VData;
            public uint UVHead;
            public Position3[] UV;
            public uint ShiteHead;
            public uint[] Shit;
            public uint EndSignature1;
            public uint EndSignature2;
            public byte[] leftovers;
        }
        public struct Position3
        {
            public float X, Y, Z;
        }
        public struct VertexData
        {
            public byte R, G, B;
            public byte PX, PY;
            public ushort X, Y;
            public float SomeFloat;
            public byte CONN;
            public ushort Null1;
        }
        public struct RawData
        {
            public float X, Y, Z;
            public float U, V, W;
            public bool CONN;
            public uint Diffuse;
            public float Nx, Ny, Nz;
        }
        #endregion
    }
}
