using System;
using System.Windows.Forms;
using System.IO;
using Twinsanity;

namespace TwinsanityEditor
{
    public partial class MainForm : Form
    {
        private Form mhForm, exeForm;

        private TreeNode nodeLastSelected;

        //private List<FileController> FilesOpened { get; }
        public FileController FilesController { get => (FileController)Tag; }
        public FileController CurCont { get => FilesController; } //get currently selected file controller
        public TwinsFile CurFile { get => CurCont.Data; } //get currently selected file
        public bool UsingDefault = false;
        public FileController DefaultCont { get; private set; }
        public TwinsFile DefaultFile { get => DefaultCont.Data; }
        public bool UsingAux = false;
        public FileController AuxCont { get; private set; }
        public TwinsFile AuxFile { get => AuxCont.Data; }
        public ParticleDataController PTLCont { get; private set; }
        public bool UsingPTL = false;

        public MainForm()
        {
            InitializeComponent();
            treeView1.AfterSelect += treeView1_AfterSelect;
            treeView1.KeyDown += treeView1_KeyDown;
        }

        private void GenTree()
        {
            nodeLastSelected = null;
            treeView1.BeginUpdate();
            if (ColDataController.importer != null)
                ColDataController.importer.Close();
            treeView1.Nodes.Clear();
            CurCont.UpdateText();
            treeView1.Nodes.Add(CurCont.Node);
            treeView1.Select();
            foreach (var i in CurFile.Records)
            {
                GenTreeNode(i, CurCont, CurCont);
            }
            if (UsingAux)
            {
                AuxCont.UpdateText();
                treeView1.Nodes.Add(AuxCont.Node);
                treeView1.Select();
                foreach (var i in AuxFile.Records)
                {
                    GenTreeNode(i, AuxCont, AuxCont);
                }
                foreach (var i in ((FileController)Tag).DataAux.Records)
                {
                    GenTreeNode(i, ((FileController)Tag).AuxCont, ((FileController)Tag).AuxCont, true);
                }
            }
            if (UsingPTL)
            {
                PTLCont.UpdateText();
                treeView1.Nodes.Add(PTLCont.Node);
                treeView1.Select();
            }
            treeView1.TopNode.Expand();
            treeView1.EndUpdate();
        }
        private void Tree_LoadDefault()
        {
            treeView1.BeginUpdate();
            if (UsingDefault)
            {
                DefaultCont.IsCached = true;
                DefaultCont.UpdateText();
                treeView1.Nodes.Add(DefaultCont.Node);
                treeView1.Select();
                foreach (var i in DefaultFile.Records)
                {
                    GenTreeNode(i, DefaultCont, DefaultCont);
                }
                foreach (var i in ((FileController)Tag).DataDefault.Records)
                {
                    GenTreeNode(i, ((FileController)Tag).DefaultCont, ((FileController)Tag).DefaultCont, true);
                }
                treeView1.Nodes[1].Expand();
            }
            treeView1.TopNode.Expand();
            treeView1.EndUpdate();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (nodeLastSelected != null && nodeLastSelected.Tag is Controller c1)
                c1.Selected = false;
            if (e.Node.Tag is Controller c2)
                ControllerNodeSelect(c2);
            nodeLastSelected = e.Node;
        }

        public void ControllerNodeSelect(Controller c)
        {
            c.Selected = true;
            textBox1.Lines = c.TextPrev;
        }

        public void GenTreeNode(TwinsItem a, Controller controller, FileController targetFile, bool cached = false)
        {
            Controller c;
            if (a is TwinsSection)
            {
                c = new SectionController(this, (TwinsSection)a, targetFile);
                foreach (var i in ((TwinsSection)a).Records)
                {
                    GenTreeNode(i, c, targetFile, cached);
                }
            }
            else if (a is Texture)
                c = new TextureController(this, (Texture)a, targetFile);
            else if (a is Material)
                c = new MaterialController(this, (Material)a, targetFile);
            else if (a is Mesh)
                c = new MeshController(this, (Mesh)a, targetFile);
            else if (a is Model)
                c = new ModelController(this, (Model)a, targetFile);
            else if (a is Skydome)
                c = new SkydomeController(this, (Skydome)a, targetFile);
            else if (a is GameObject)
                c = new ObjectController(this, (GameObject)a, targetFile);
            else if (a is Script)
                c = new ScriptController(this, (Script)a, targetFile);
            else if (a is SoundEffect)
                c = new SEController(this, (SoundEffect)a, targetFile);
            else if (a is AIPosition)
                c = new AIPositionController(this, (AIPosition)a, targetFile);
            else if (a is AIPath)
                c = new AIPathController(this, (AIPath)a, targetFile);
            else if (a is Position)
                c = new PositionController(this, (Position)a, targetFile);
            else if (a is Twinsanity.Path)
                c = new PathController(this, (Twinsanity.Path)a, targetFile);
            else if (a is Instance)
                c = new InstanceController(this, (Instance)a, targetFile);
            else if (a is Trigger && CurFile.Type != TwinsFile.FileType.DemoRM2) //trigger controller assumes final instance format
                c = new TriggerController(this, (Trigger)a, targetFile);
            else if (a is ColData)
                c = new ColDataController(this, (ColData)a, targetFile);
            else if (a is ChunkLinks)
                c = new ChunkLinksController(this, (ChunkLinks)a, targetFile);
            else if (a is GraphicsInfo)
                c = new GraphicsInfoController(this, (GraphicsInfo)a, targetFile);
            else if (a is ArmatureModel)
                c = new ArmatureModelController(this, (ArmatureModel)a, targetFile);
            else if (a is ArmatureModelX)
                c = new ArmatureModelXController(this, (ArmatureModelX)a, targetFile);
            else if (a is MaterialDemo)
                c = new MaterialDController(this, (MaterialDemo)a, targetFile);
            else if (a is SceneryData)
                c = new SceneryDataController(this, (SceneryData)a, targetFile);
            else if (a is SpecialModel)
                c = new SpecialModelController(this, (SpecialModel)a, targetFile);
            else if (a is ParticleData)
                c = new ParticleDataController(this, (ParticleData)a, targetFile);
            else if (a is DynamicSceneryData)
                c = new DynamicSceneryDataController(this, (DynamicSceneryData)a, targetFile);
            else if (a is MeshX)
                c = new MeshXController(this, (MeshX)a, targetFile);
            else if (a is CollisionSurface)
                c = new CollisionSurfaceController(this, (CollisionSurface)a, targetFile);
            else if (a is Camera)
                c = new CameraController(this, (Camera)a, targetFile);
            else if (a is InstanceTemplate)
                c = new InstaceTemplateController(this, (InstanceTemplate)a, targetFile);
            else if (a is InstanceTemplateDemo)
                c = new InstaceTemplateDemoController(this, (InstanceTemplateDemo)a, targetFile);
            else if (a is InstanceDemo)
                c = new InstanceDemoController(this, (InstanceDemo)a, targetFile);
            else if (a is GameObjectDemo)
                c = new ObjectDemoController(this, (GameObjectDemo)a, targetFile);
            else if (a is Animation)
                c = new AnimationController(this, (Animation)a, targetFile);
            else if (a is CodeModel)
                c = new CodeModelController(this, (CodeModel)a, targetFile);
            else
                c = new ItemController(this, a, targetFile);

            if (!cached)
                c.UpdateText();
            controller.AddNode(c);
        }

        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            TreeView tree = (TreeView)sender;
            if (e.KeyCode == Keys.Enter && tree.SelectedNode != null && tree.SelectedNode.Tag is Controller c)
                CurCont.OpenEditor(c);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void openRM2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "RM2 files|*.rm2|RMX files|*.rmx|Demo RM2 files|*.rm2|SM2 files|*.sm2|SMX files|*.smx|Demo SM2 files|*.sm2"; //PTL files|*.ptl|BIN files|*.bin|DIR files|*.dir";
            //ofd.Filter = "PS2 files (.rm2; .sm2)|*.rm2;*.sm2|XBOX files (.rmx; .smx)|*.rmx;*.smx|Demo files (.rm2; .sm2)|*.rm2; *.sm2";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (CurCont != null)
                    CurCont.CloseFile();
                Tag = null;
                TwinsFile file = new TwinsFile();
                TwinsFile aux_file = null;
                TwinsFile default_file = null;
                ParticleDataController ptl_cont = null;
                bool IsScenery = ofd.FileName.Contains(".sm");
                UsingAux = false;
                UsingDefault = false;
                UsingPTL = false;
                switch (ofd.FilterIndex)
                {
                    case 1:
                    case 4:
                        if (IsScenery)
                            file.LoadFile(ofd.FileName, TwinsFile.FileType.SM2);
                        else
                        {
                            file.LoadFile(ofd.FileName, TwinsFile.FileType.RM2);
                            aux_file = new TwinsFile();
                            aux_file.LoadFile(ofd.FileName.Substring(0, ofd.FileName.LastIndexOf('.')) + ".sm2", TwinsFile.FileType.SM2);
                            if (aux_file.Size != 12)
                            {
                                UsingAux = true;
                            }
                        }
                        break;
                    case 2:
                    case 5:
                        if (IsScenery)
                            file.LoadFile(ofd.FileName, TwinsFile.FileType.SMX);
                        else
                        {
                            file.LoadFile(ofd.FileName, TwinsFile.FileType.RMX);
                            aux_file = new TwinsFile();
                            aux_file.LoadFile(ofd.FileName.Substring(0, ofd.FileName.LastIndexOf('.')) + ".smx", TwinsFile.FileType.SMX);
                            if (aux_file.Size != 12)
                            {
                                UsingAux = true;
                            }
                            if (System.IO.File.Exists(ofd.FileName.Substring(0, ofd.FileName.LastIndexOf('.')) + ".ptl"))
                            {
                                ParticleData PTL_data = new ParticleData();
                                BinaryReader reader = new BinaryReader(new FileStream(ofd.FileName.Substring(0, ofd.FileName.LastIndexOf('.')) + ".ptl", FileMode.Open, FileAccess.Read));
                                PTL_data.Load(reader, (int)reader.BaseStream.Length);
                                PTL_data.IsStandalone = true;
                                ptl_cont = new ParticleDataController(this, PTL_data,  null);
                                UsingPTL = true;
                            }
                        }
                        break;
                    case 3:
                    case 6:
                        if (IsScenery)
                            file.LoadFile(ofd.FileName, TwinsFile.FileType.DemoSM2);
                        else
                        {
                            file.LoadFile(ofd.FileName, TwinsFile.FileType.DemoRM2);
                            aux_file = new TwinsFile();
                            aux_file.LoadFile(ofd.FileName.Substring(0, ofd.FileName.LastIndexOf('.')) + ".sm2", TwinsFile.FileType.DemoSM2);
                            UsingAux = true;
                        }
                        break;
                    case 7:
                        {
                            file.LoadFile(ofd.FileName, TwinsFile.FileType.PTL);
                        }
                        break;
                    case 8:
                        {
                            file.LoadFile(ofd.FileName, TwinsFile.FileType.BIN);
                        }
                        break;
                    case 9:
                        {
                            file.LoadFile(ofd.FileName, TwinsFile.FileType.DIR);
                        }
                        break;
                }
                if (IsScenery)
                {
                    sMViewerToolStripMenuItem.Enabled = true;
                    rMViewerToolStripMenuItem.Enabled = false;
                }
                else
                {
                    rMViewerToolStripMenuItem.Enabled = true;
                    sMViewerToolStripMenuItem.Enabled = false;
                }
                file.SafeFileName = ofd.SafeFileName;
                Tag = new FileController(this, file, null);
                ((FileController)Tag).DataAux = aux_file;
                if (UsingAux)
                {
                    ((FileController)Tag).AuxCont = new FileController(this, aux_file, null);
                    AuxCont = new FileController(this, aux_file, null);
                }
                if (UsingPTL)
                {
                    ((FileController)Tag).AuxPTL = ptl_cont;
                    PTLCont = ptl_cont;
                }
                GenTree();
                if (UsingAux)
                {
                    string origPath = aux_file.FileName;
                    SceneryDataController scenery_sec = AuxCont.GetItem<SceneryDataController>(0);
                    string curChunkPath = scenery_sec.Data.ChunkName;
                    string adjustedPath = origPath.Substring(0, origPath.Length - curChunkPath.Length - 4);
                    string ChunkName = @"Startup\Default";
                    string Path = adjustedPath + ChunkName + ".rm";
                    if (file.Type == TwinsFile.FileType.RM2 || file.Type == TwinsFile.FileType.DemoRM2)
                    {
                        Path += "2";
                    }
                    else
                    {
                        Path += "x";
                    }
                    if (File.Exists(Path))
                    {
                        UsingDefault = true;
                        default_file = new TwinsFile();
                        if (file.Type == TwinsFile.FileType.RM2)
                        {
                            default_file.LoadFile(Path, TwinsFile.FileType.RM2);
                        }
                        else if (file.Type == TwinsFile.FileType.DemoRM2)
                        {
                            default_file.LoadFile(Path, TwinsFile.FileType.DemoRM2);
                        }
                        else
                        {
                            default_file.LoadFile(Path, TwinsFile.FileType.RMX);
                        }
                        ((FileController)Tag).DataDefault = default_file;
                        ((FileController)Tag).DefaultCont = new FileController(this, default_file, null);
                        DefaultCont = new FileController(this, default_file, null);
                        Tree_LoadDefault();
                    }
                }

                Text = $"Twinsanity Editor [{ofd.FileName}]";
            }
            ofd.Dispose();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "RM2/RMX files (and SM)|*.rm*|SM2/SMX files|*.sm*";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                if (sfd.FileName.Substring(sfd.FileName.Length - 4, 1) != ".")
                {
                    if (CurFile.Type == TwinsFile.FileType.RM2 || CurFile.Type == TwinsFile.FileType.DemoRM2)
                    {
                        sfd.FileName += ".rm2";
                    }
                    else if (CurFile.Type == TwinsFile.FileType.RMX)
                    {
                        sfd.FileName += ".rmx";
                    }
                    else if (CurFile.Type == TwinsFile.FileType.SMX)
                    {
                        sfd.FileName += ".smx";
                    }
                    else if (CurFile.Type == TwinsFile.FileType.SM2 || CurFile.Type == TwinsFile.FileType.DemoSM2)
                    {
                        sfd.FileName += ".sm2";
                    }
                }
                CurFile.SaveFile(sfd.FileName);
                CurCont.Data.FileName = sfd.FileName;
                if (UsingAux)
                {
                    string smName = sfd.FileName.Substring(0, sfd.FileName.Length - 3) + "sm" + sfd.FileName.Substring(sfd.FileName.Length - 1, 1);
                    AuxFile.SaveFile(smName);
                    AuxCont.Data.FileName = smName;
                }
                Text = $"Twinsanity Editor [{sfd.FileName}] ";
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Overwrite original file?", "Save", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                CurFile.SaveFile(CurCont.FileName);
                if (UsingAux)
                {
                    string smName = CurCont.FileName.Substring(0, CurCont.FileName.Length - 3) + "sm" + CurCont.FileName.Substring(CurCont.FileName.Length - 1, 1);
                    AuxFile.SaveFile(smName);
                }
            }
        }

        private void rMViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurCont.OpenRMViewer();
        }

        private void eLFPatcherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenEXETool();
        }

        private void sMViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurCont.OpenSMViewer();
        }

        private void mHMBToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMHTool();
        }

        public void OpenEXETool()
        {
            if (exeForm == null)
            {
                exeForm = new EXEPatcher();
                exeForm.FormClosed += delegate
                {
                    exeForm = null;
                };
            }
            else
                exeForm.Select();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("TwinsanityEditor + Twinsanity API\nDeveloped by Neo_Kesha (original author), Smartkin, ManDude, Marko, BetaM\nSource code available at: https://github.com/smartkin/twinsanity-editor", "About", MessageBoxButtons.OK);
        }

        public void OpenMHTool()
        {
            if (mhForm == null)
            {
                mhForm = new MHViewer();
                mhForm.FormClosed += delegate
                {
                    mhForm = null;
                };
            }
            else
                mhForm.Select();
        }

        public void Pref_TruncateNames_Click(object sender, EventArgs e)
        {
            Utils.TextUtils.Pref_TruncateObjectNames = !Utils.TextUtils.Pref_TruncateObjectNames;
            Pref_TruncateNames_toolStripMenuItem.Checked = Utils.TextUtils.Pref_TruncateObjectNames;
        }
        public void Pref_EnableAllNames_Click(object sender, EventArgs e)
        {
            Utils.TextUtils.Pref_EnableAnyObjectNames = !Utils.TextUtils.Pref_EnableAnyObjectNames;
            Pref_EnableAllNames_toolStripMenuItem.Checked = Utils.TextUtils.Pref_EnableAnyObjectNames;
        }

    }
}
