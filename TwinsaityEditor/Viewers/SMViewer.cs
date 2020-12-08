using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.Windows.Forms;
using Twinsanity;

namespace TwinsanityEditor
{
    // obsolete
    public partial class SMViewer : ThreeDViewer
    {
        private FileController file;
        private ChunkLinks links;

        public SMViewer(FileController file, ref Form pform)
        {
            this.file = file;
            if (file.Data.ContainsItem(5))
            {
                links = file.Data.GetItem<ChunkLinks>(5);
            }
            zFar = 2000F;
        }

        //protected override void RenderHUD()
        //{
        //    return;
        //}

        protected override void RenderObjects()
        {
            //put all object rendering code here
            
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
