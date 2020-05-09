using Twinsanity;

namespace TwinsanityEditor
{
    public class ArmatureModelController : ItemController
    {
        public new ArmatureModel Data { get; set; }

        public ArmatureModelController(MainForm topform, ArmatureModel item, FileController targetFile) : base(topform, item, targetFile)
        {
            Data = item;
        }

        protected override string GetName()
        {
            return string.Format("Armature Model [ID {0:X8}]", Data.ID);
        }

        protected override void GenText()
        {
            TextPrev = new string[2 + (Data.SubModels.Length * 4)];
            TextPrev[0] = string.Format("ID: {0:X8}", Data.ID);
            TextPrev[1] = $"SubModels {Data.SubModels.Length}";
            int line = 2;
            for (int i = 0; i < Data.SubModels.Length; i++)
            {
                TextPrev[line + 0] = $"SubModel {i}";
                TextPrev[line + 1] = string.Format("MaterialID: {0:X8}", Data.SubModels[i].MaterialID);
                TextPrev[line + 2] = $"Block Size {Data.SubModels[i].BlockSize}";
                TextPrev[line + 3] = $"Vertexes {Data.SubModels[i].Vertexes}";
                line += 4;
            }
        }
    }
}