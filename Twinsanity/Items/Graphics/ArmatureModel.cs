using System.IO;
using System;

namespace Twinsanity
{
    public class ArmatureModel : TwinsItem
    {

        public long ItemSize { get; set; }

        public uint SubModelCount { get; set; }
        public SubModel[] SubModels { get; set; }

        public override void Save(BinaryWriter writer)
        {
            writer.Write(Data);
        }

        public override void Load(BinaryReader reader, int size)
        {
            long pre_pos = reader.BaseStream.Position;

            SubModelCount = reader.ReadUInt32();

            SubModels = new SubModel[SubModelCount];
            if (SubModelCount > 0)
            {
                for (int m = 0; m < SubModels.Length; m++)
                {
                    SubModel Model = new SubModel();
                    Model.MaterialID = reader.ReadUInt32();
                    Model.BlockSize = reader.ReadUInt32();
                    Model.Vertexes = reader.ReadInt32();
                    Model.k = reader.ReadUInt16();
                    Model.c = reader.ReadUInt16();
                    Model.Null1 = reader.ReadUInt32();
                    Model.Something = reader.ReadUInt32();
                    Model.Null2 = reader.ReadUInt32();

                    reader.BaseStream.Position = reader.BaseStream.Position + Model.BlockSize - 16;

                    //todo: groups, etc.

                    SubModels[m] = Model;
                }
            }

            ItemSize = size;
            reader.BaseStream.Position = pre_pos;
            Data = reader.ReadBytes(size);
        }

        protected override int GetSize()
        {
            return (int)ItemSize;
        }

        public struct SubModel
        {
            // Primary Header
            public uint MaterialID;
            public uint BlockSize;
            public int Vertexes;
            public ushort k;
            public ushort c;
            public uint Null1;
            public uint Something;
            public uint Null2;
            public Group[] Group;
        }
        public struct Group
        {
            public uint SomeNum1;
            public byte Vertexes;
            public byte Some80h;
            public ushort Null1;
            public uint SomeNum2;
            public uint SomeNum3;
            public uint Null2;
            public uint Signature1;
            public uint SomeGroup1Head;
            public uint[] Group1Stuff;
            public uint SomeGroup2Head;
            public uint[] Group2Stuff;
            public uint Signature2;
            public uint Struct3Head;
            public Position2[] Struct3;
            public uint[] Struct3End;
            public uint Struct4Head;
            public Position2[] Struct4;
            public uint Struct4End;
            public uint Struct6Head;
            public uint[] Struct6;
            public uint Struct5Head;
            public ArmatureVertex[] Struct5;
            public uint EndSignature1;
            public uint EndSignature2;
            public byte[] leftovers;
        }
        public struct Position2
        {
            public uint ID1, ID2;
        }
        public struct ArmatureVertex
        {
            public float Float;
            public uint a, b;
            public byte ID;
            public byte CONN;
            public short Null;
        }
        public struct Weight
        {
            public float X, Y, Z;
            public byte SomeByte;
            public byte CONN;
            public ushort Null1;
        }
    }
    
}
