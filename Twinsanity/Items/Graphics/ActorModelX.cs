using System;
using System.Collections.Generic;
using System.IO;

namespace Twinsanity
{
    public class ActorModelX : TwinsItem
    {
        public long ItemSize { get; set; }

        public override void Save(BinaryWriter writer)
        {
            writer.Write(Data);
        }

        public override void Load(BinaryReader reader, int size)
        {
            long pre_pos = reader.BaseStream.Position;

            ItemSize = size;
            reader.BaseStream.Position = pre_pos;
            Data = reader.ReadBytes(size);
        }

        protected override int GetSize()
        {
            return (int)ItemSize;
        }
    }
}
