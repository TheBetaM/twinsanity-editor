using Twinsanity;

namespace TwinsanityEditor
{
    public class ArmatureModelXController : ItemController
    {
        public new ArmatureModelX Data { get; set; }

        public ArmatureModelXController(MainForm topform, ArmatureModelX item, FileController targetFile) : base(topform, item, targetFile)
        {
            Data = item;
        }

        protected override string GetName()
        {
            return string.Format("Armature Model X [ID {0:X8}]", Data.ID);
        }

        protected override void GenText()
        {
            TextPrev = new string[2 + (Data.SubModels * 4)];
            TextPrev[0] = string.Format("ID: {0:X8}", Data.ID);
            TextPrev[1] = $"SubModels {Data.SubModels}";
            int line = 2;
            for (int i = 0; i < Data.SubModels; i++)
            {
                TextPrev[line + 0] = $"SubModel {i}";
                TextPrev[line + 1] = $"MaterialID {Data.MaterialIDs[i]}";
                TextPrev[line + 2] = $"Block Size {Data.BlockSize[i]}";
                TextPrev[line + 3] = $"Vertexes {Data.Vertexes[i]}";
                line += 4;
            }
        }
    }
}