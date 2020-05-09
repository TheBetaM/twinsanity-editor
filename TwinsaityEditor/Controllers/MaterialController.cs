using Twinsanity;

namespace TwinsanityEditor
{
    public class MaterialController : ItemController
    {
        public new Material Data { get; set; }

        public MaterialController(MainForm topform, Material item, FileController targetFile) : base(topform, item, targetFile)
        {
            Data = item;
        }

        protected override string GetName()
        {
            return string.Format("{1} [ID {0:X8}]", Data.ID, Data.Name);
        }

        protected override void GenText()
        {
            TextPrev = new string[5];
            TextPrev[0] = string.Format("ID: {0:X8}", Data.ID);
            TextPrev[1] = $"Offset: {Data.Offset} Size: {Data.Size}";
            TextPrev[2] = string.Format("Name: {0:X8} Texture ID: {1:X8}", Data.Name, Data.Tex);
            TextPrev[3] = $"Integers: {Data.ValuesI[0]} {Data.ValuesI[1]} {Data.ValuesI[2]} {Data.ValuesI[3]}";
            TextPrev[4] = $"Floats: {Data.ValuesF[0]} {Data.ValuesF[1]} {Data.ValuesF[2]} {Data.ValuesF[3]}";
        }
    }
}
