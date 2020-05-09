﻿using System.Windows.Forms;

namespace TwinsanityEditor
{
    public class ModelViewer : MeshViewer
    {
        private ModelController model;
        private FileController file;

        public ModelViewer(ModelController model, ref Form pform) : base(model.MainFile.MeshSection.GetItem<MeshController>(model.Data.MeshID), ref pform)
        {
            //initialize variables here
            this.model = model;
            file = model.MainFile;
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
