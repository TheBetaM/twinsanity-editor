using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Forms;
using Twinsanity;

namespace TwinsanityEditor
{
    public partial class SceneryEditor : Form
    {
        private const string angleFormat = "{0:0.000}º";
        private SceneryDataController controller;
        private SceneryData.SceneryModel ins;
        private SceneryData.LightAmbient li_am;
        private SceneryData.LightDirectional li_di;
        private SceneryData.LightPoint li_po;
        private SceneryData.LightNegative li_ne;

        private enum ScenTabSel
        {
            Scenery = 0,
            Submodels = 1,
            LightsAmbient = 2,
            LightsDirectional = 3,
            LightsPoint = 4,
            LightsNegative = 5,
        }
        
        private FileController File { get; set; }
        private TwinsFile FileData { get => File.Data; }

        private bool ignore_value_change;

        public SceneryEditor(SceneryDataController c)
        {
            File = c.MainFile;
            controller = c;
            InitializeComponent();
            Text = $"Scenery Data Editor";
            PopulateList();
            comboBox1.TextChanged += comboBox1_TextChanged;
            tabControl1.SelectedIndexChanged += tabControl1_SelectedIndexChanged;
            FormClosed += SceneryEditor_FormClosed;
        }

        private void SceneryEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            File.SelectItem(null);
        }

        private void PopulateList()
        {
            listBox1.BeginUpdate();
            listBox1.Items.Clear();
            for (int i = 0; i < controller.Data.sceneryModels.Count; i++)
            {
                for (int d = 0; d < controller.Data.sceneryModels[i].Models.Count; d++)
                {
                    listBox1.Items.Add($"Model {i} Submodel {d}");
                }
            }
            listBox1.EndUpdate();

            listBox2.BeginUpdate();
            listBox2.Items.Clear();
            for (int d = 0; d < controller.Data.sceneryModels[0].Models.Count; d++)
            {
                listBox2.Items.Add($"Submodel {d}");
            }
            listBox2.EndUpdate();

            listBox3.BeginUpdate();
            listBox3.Items.Clear();
            for (int d = 0; d < controller.Data.LightsAmbient.Count; d++)
            {
                listBox3.Items.Add($"Ambient Light {d}");
            }
            listBox3.EndUpdate();

            listBox4.BeginUpdate();
            listBox4.Items.Clear();
            for (int d = 0; d < controller.Data.LightsDirectional.Count; d++)
            {
                listBox4.Items.Add($"Directional Light {d}");
            }
            listBox4.EndUpdate();

            listBox5.BeginUpdate();
            listBox5.Items.Clear();
            for (int d = 0; d < controller.Data.LightsPoint.Count; d++)
            {
                listBox5.Items.Add($"Point Light {d}");
            }
            listBox5.EndUpdate();

            listBox6.BeginUpdate();
            listBox6.Items.Clear();
            for (int d = 0; d < controller.Data.LightsNegative.Count; d++)
            {
                listBox6.Items.Add($"Negative Light {d}");
            }
            listBox6.EndUpdate();

            comboBox1.Items.Clear();
            
        }

        private void listBox1_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (listBox1.SelectedIndex == -1) return;

            this.SuspendDrawing();

            int target_id = 0;
            int target_scenery = 0;
            int target_model = 0;
            for (int i = 0; i < controller.Data.sceneryModels.Count; i++)
            {
                for (int d = 0; d < controller.Data.sceneryModels[i].Models.Count; d++)
                {
                    if (target_id == listBox1.SelectedIndex)
                    {
                        target_scenery = i;
                        target_model = d;
                        break;
                    }
                    else
                    {
                        target_id++;
                    }
                    
                }
                target_id++;
            }

            ins = controller.Data.sceneryModels[target_scenery].Models[target_model];
            File.RMViewer_CustomTeleport(ins.ModelMatrix[3].X, ins.ModelMatrix[3].Y, ins.ModelMatrix[3].Z);
            tabControl1.Enabled = true;
            tabControl1.Tag = 0x00;
            ignore_value_change = true;
            if (tabControl1.SelectedIndex == 0)
            {
                UpdateTab1();
            }
            else if (tabControl1.SelectedIndex == 1)
            {
                UpdateTab2();
            }

            ignore_value_change = false;

            this.ResumeDrawing();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0 && ((int)tabControl1.Tag & 0x01) == 0)
            {
                UpdateTab1();
            }
            else if (tabControl1.SelectedIndex == 1 && ((int)tabControl1.Tag & 0x02) == 0)
            {
                UpdateTab2();
            }
        }

        private void UpdateTab1()
        {
            comboBox1.Text = "";

            ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;

            if (SelType == ScenTabSel.Scenery)
            {
                numericUpDown12.Value = listBox1.SelectedIndex;
                numericUpDown2.Value = (decimal)ins.ModelMatrix[3].X;
                numericUpDown3.Value = (decimal)ins.ModelMatrix[3].Y;
                numericUpDown4.Value = (decimal)ins.ModelMatrix[3].Z;
                numericUpDown5.Value = (decimal)ins.ModelMatrix[3].W;
            }
            else if (SelType == ScenTabSel.Submodels)
            {
                numericUpDown12.Value = listBox2.SelectedIndex;
                numericUpDown2.Value = (decimal)ins.ModelMatrix[3].X;
                numericUpDown3.Value = (decimal)ins.ModelMatrix[3].Y;
                numericUpDown4.Value = (decimal)ins.ModelMatrix[3].Z;
                numericUpDown5.Value = (decimal)ins.ModelMatrix[3].W;
            }
            else if (SelType == ScenTabSel.LightsAmbient)
            {
                numericUpDown12.Value = listBox3.SelectedIndex;
                numericUpDown2.Value = (decimal)li_am.Position.X;
                numericUpDown3.Value = (decimal)li_am.Position.Y;
                numericUpDown4.Value = (decimal)li_am.Position.Z;
                numericUpDown5.Value = (decimal)li_am.Position.W;
            }
            else if (SelType == ScenTabSel.LightsDirectional)
            {
                numericUpDown12.Value = listBox4.SelectedIndex;
                numericUpDown2.Value = (decimal)li_di.Position.X;
                numericUpDown3.Value = (decimal)li_di.Position.Y;
                numericUpDown4.Value = (decimal)li_di.Position.Z;
                numericUpDown5.Value = (decimal)li_di.Position.W;
            }
            else if (SelType == ScenTabSel.LightsPoint)
            {
                numericUpDown12.Value = listBox5.SelectedIndex;
                numericUpDown2.Value = (decimal)li_po.Position.X;
                numericUpDown3.Value = (decimal)li_po.Position.Y;
                numericUpDown4.Value = (decimal)li_po.Position.Z;
                numericUpDown5.Value = (decimal)li_po.Position.W;
            }
            else if (SelType == ScenTabSel.LightsNegative)
            {
                numericUpDown12.Value = listBox6.SelectedIndex;
                numericUpDown2.Value = (decimal)li_ne.Position.X;
                numericUpDown3.Value = (decimal)li_ne.Position.Y;
                numericUpDown4.Value = (decimal)li_ne.Position.Z;
                numericUpDown5.Value = (decimal)li_ne.Position.W;
            }
            
            textBox1.Text = "";
            tabControl1.Tag = (int)tabControl1.Tag | 0x01;
            GetXRot(true, true); GetYRot(true, true); GetZRot(true, true);
        }

        private void UpdateTab2()
        {
            ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;

            if (SelType == ScenTabSel.Scenery || SelType == ScenTabSel.Submodels)
            {

            }
            else if (SelType == ScenTabSel.LightsAmbient)
            {
                numericUpDown13_colorR.Value = (decimal)li_am.Color_R;
                numericUpDown11_colorG.Value = (decimal)li_am.Color_G;
                numericUpDown10_colorB.Value = (decimal)li_am.Color_B;
                numericUpDown20_radius.Value = (decimal)li_am.Radius;
                numericUpDown9_colorF.Value = (decimal)li_am.UnkFloat;
                numericUpDown21_v1x.Value = (decimal)li_am.Vectors[0].X;
                numericUpDown19_v1y.Value = (decimal)li_am.Vectors[0].Y;
                numericUpDown15_v1z.Value = (decimal)li_am.Vectors[0].Z;
                numericUpDown14_v1w.Value = (decimal)li_am.Vectors[0].W;
                numericUpDown26_v2x.Value = (decimal)li_am.Vectors[1].X;
                numericUpDown25_v2y.Value = (decimal)li_am.Vectors[1].Y;
                numericUpDown24_v2z.Value = (decimal)li_am.Vectors[1].Z;
                numericUpDown23_v2w.Value = (decimal)li_am.Vectors[1].W;
                numericUpDown30_v3x.Value = 0;
                numericUpDown29_v3y.Value = 0;
                numericUpDown28_v3z.Value = 0;
                numericUpDown27_v3w.Value = 0;
                numericUpDown33_nefloats_1.Value = 0;
                numericUpDown32_nefloats_2.Value = 0;
                numericUpDown31_nefloats_3.Value = 0;
                numericUpDown22_nefloats_4.Value = 0;
                numericUpDown34_nefloats_5.Value = 0;
                textBox2_flag1.Text = Convert.ToString(li_am.Flags[0], 16);
                textBox3_flag2.Text = Convert.ToString(li_am.Flags[1], 16);
                textBox4_flag3.Text = Convert.ToString(li_am.Flags[2], 16);
                textBox5_flag4.Text = Convert.ToString(li_am.Flags[3], 16);
                textBox9_flag2_1.Text = "";
                textBox8_flag2_2.Text = "";
            }
            else if (SelType == ScenTabSel.LightsDirectional)
            {
                numericUpDown13_colorR.Value = (decimal)li_di.Color_R;
                numericUpDown11_colorG.Value = (decimal)li_di.Color_G;
                numericUpDown10_colorB.Value = (decimal)li_di.Color_B;
                numericUpDown20_radius.Value = (decimal)li_di.Radius;
                numericUpDown9_colorF.Value = (decimal)li_di.UnkFloat;
                numericUpDown21_v1x.Value = (decimal)li_di.Vectors[0].X;
                numericUpDown19_v1y.Value = (decimal)li_di.Vectors[0].Y;
                numericUpDown15_v1z.Value = (decimal)li_di.Vectors[0].Z;
                numericUpDown14_v1w.Value = (decimal)li_di.Vectors[0].W;
                numericUpDown26_v2x.Value = (decimal)li_di.Vectors[1].X;
                numericUpDown25_v2y.Value = (decimal)li_di.Vectors[1].Y;
                numericUpDown24_v2z.Value = (decimal)li_di.Vectors[1].Z;
                numericUpDown23_v2w.Value = (decimal)li_di.Vectors[1].W;
                numericUpDown30_v3x.Value = (decimal)li_di.Vectors[2].X;
                numericUpDown29_v3y.Value = (decimal)li_di.Vectors[2].Y;
                numericUpDown28_v3z.Value = (decimal)li_di.Vectors[2].Z;
                numericUpDown27_v3w.Value = (decimal)li_di.Vectors[2].W;
                numericUpDown33_nefloats_1.Value = 0;
                numericUpDown32_nefloats_2.Value = 0;
                numericUpDown31_nefloats_3.Value = 0;
                numericUpDown22_nefloats_4.Value = 0;
                numericUpDown34_nefloats_5.Value = 0;
                textBox2_flag1.Text = Convert.ToString(li_di.Flags[0], 16);
                textBox3_flag2.Text = Convert.ToString(li_di.Flags[1], 16);
                textBox4_flag3.Text = Convert.ToString(li_di.Flags[2], 16);
                textBox5_flag4.Text = Convert.ToString(li_di.Flags[3], 16);
                textBox9_flag2_1.Text = Convert.ToString(li_di.Flags2[0], 16);
                textBox8_flag2_2.Text = Convert.ToString(li_di.Flags2[1], 16);
            }
            else if (SelType == ScenTabSel.LightsPoint)
            {
                numericUpDown13_colorR.Value = (decimal)li_po.Color_R;
                numericUpDown11_colorG.Value = (decimal)li_po.Color_G;
                numericUpDown10_colorB.Value = (decimal)li_po.Color_B;
                numericUpDown20_radius.Value = (decimal)li_po.Radius;
                numericUpDown9_colorF.Value = (decimal)li_po.UnkFloat;
                numericUpDown21_v1x.Value = (decimal)li_po.Vectors[0].X;
                numericUpDown19_v1y.Value = (decimal)li_po.Vectors[0].Y;
                numericUpDown15_v1z.Value = (decimal)li_po.Vectors[0].Z;
                numericUpDown14_v1w.Value = (decimal)li_po.Vectors[0].W;
                numericUpDown26_v2x.Value = (decimal)li_po.Vectors[1].X;
                numericUpDown25_v2y.Value = (decimal)li_po.Vectors[1].Y;
                numericUpDown24_v2z.Value = (decimal)li_po.Vectors[1].Z;
                numericUpDown23_v2w.Value = (decimal)li_po.Vectors[1].W;
                numericUpDown30_v3x.Value = 0;
                numericUpDown29_v3y.Value = 0;
                numericUpDown28_v3z.Value = 0;
                numericUpDown27_v3w.Value = 0;
                numericUpDown33_nefloats_1.Value = 0;
                numericUpDown32_nefloats_2.Value = 0;
                numericUpDown31_nefloats_3.Value = 0;
                numericUpDown22_nefloats_4.Value = 0;
                numericUpDown34_nefloats_5.Value = 0;
                textBox2_flag1.Text = Convert.ToString(li_po.Flags[0], 16);
                textBox3_flag2.Text = Convert.ToString(li_po.Flags[1], 16);
                textBox4_flag3.Text = Convert.ToString(li_po.Flags[2], 16);
                textBox5_flag4.Text = Convert.ToString(li_po.Flags[3], 16);
                textBox9_flag2_1.Text = Convert.ToString(li_po.Flags2[0], 16);
                textBox8_flag2_2.Text = Convert.ToString(li_po.Flags2[1], 16);
            }
            else if (SelType == ScenTabSel.LightsNegative)
            {
                numericUpDown13_colorR.Value = (decimal)li_ne.Color_R;
                numericUpDown11_colorG.Value = (decimal)li_ne.Color_G;
                numericUpDown10_colorB.Value = (decimal)li_ne.Color_B;
                numericUpDown20_radius.Value = (decimal)li_ne.Radius;
                numericUpDown9_colorF.Value = (decimal)li_ne.UnkFloat;
                numericUpDown21_v1x.Value = (decimal)li_ne.Vectors[0].X;
                numericUpDown19_v1y.Value = (decimal)li_ne.Vectors[0].Y;
                numericUpDown15_v1z.Value = (decimal)li_ne.Vectors[0].Z;
                numericUpDown14_v1w.Value = (decimal)li_ne.Vectors[0].W;
                numericUpDown26_v2x.Value = (decimal)li_ne.Vectors[1].X;
                numericUpDown25_v2y.Value = (decimal)li_ne.Vectors[1].Y;
                numericUpDown24_v2z.Value = (decimal)li_ne.Vectors[1].Z;
                numericUpDown23_v2w.Value = (decimal)li_ne.Vectors[1].W;
                numericUpDown30_v3x.Value = (decimal)li_ne.Vectors[2].X;
                numericUpDown29_v3y.Value = (decimal)li_ne.Vectors[2].Y;
                numericUpDown28_v3z.Value = (decimal)li_ne.Vectors[2].Z;
                numericUpDown27_v3w.Value = (decimal)li_ne.Vectors[2].W;
                numericUpDown33_nefloats_1.Value = (decimal)li_ne.Floats[0];
                numericUpDown32_nefloats_2.Value = (decimal)li_ne.Floats[1];
                numericUpDown31_nefloats_3.Value = (decimal)li_ne.Floats[2];
                numericUpDown22_nefloats_4.Value = (decimal)li_ne.Floats[3];
                numericUpDown34_nefloats_5.Value = (decimal)li_ne.Floats[4];
                textBox2_flag1.Text = Convert.ToString(li_ne.Flags[0], 16);
                textBox3_flag2.Text = Convert.ToString(li_ne.Flags[1], 16);
                textBox4_flag3.Text = Convert.ToString(li_ne.Flags[2], 16);
                textBox5_flag4.Text = Convert.ToString(li_ne.Flags[3], 16);
                textBox9_flag2_1.Text = "";
                textBox8_flag2_2.Text = "";
            }
        }

        private void GetXRot(bool slider, bool num)
        {
            ignore_value_change = true;
            /*
            if (slider)
                trackBar1.Value = ins.RotX;
            if (num)
                numericUpDown6.Value = ins.RotX;
            label6.Text = string.Format(angleFormat, ins.RotX / (float)(ushort.MaxValue + 1) * 360f);
            */
            ignore_value_change = false;
        }

        private void GetYRot(bool slider, bool num)
        {
            ignore_value_change = true;
            /*
            if (slider)
                trackBar2.Value = ins.RotY;
            if (num)
                numericUpDown7.Value = ins.RotY;
            label7.Text = string.Format(angleFormat, ins.RotY / (float)(ushort.MaxValue + 1) * 360f);
            */
            ignore_value_change = false;
        }

        private void GetZRot(bool slider, bool num)
        {
            ignore_value_change = true;
            /*
            if (slider)
                trackBar3.Value = ins.RotZ;
            if (num)
                numericUpDown8.Value = ins.RotZ;
            label9.Text = string.Format(angleFormat, ins.RotZ / (float)(ushort.MaxValue + 1) * 360f);
            */
            ignore_value_change = false;
        }

        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            /*
            if (ignore_value_change) return;
            if (ushort.TryParse(comboBox1.Text.Split(new char[] { ' ' }, 2)[0], out ushort oid))
            {
                ins.ObjectID = oid;
                listBox1.Items[listBox1.SelectedIndex] = GenTextForList(ins);
            }
            controller.UpdateText();
            */
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            /*
            if (ignore_value_change) return;
            if (uint.TryParse(textBox1.Text, System.Globalization.NumberStyles.HexNumber, null, out uint o))
            {
                ins.UnkI32 = o;
            }
            ((Controller)controller.Node.Nodes[controller.Data.RecordIDs[ins.ID]].Tag).UpdateText();
            */
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;

            ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;

            if (SelType == ScenTabSel.Scenery || SelType == ScenTabSel.Submodels)
            {
                ins.ModelMatrix[3].X = (float)numericUpDown2.Value;
                File.RMViewer_UpdateScenery();
            }
            else if (SelType == ScenTabSel.LightsAmbient)
            {
                li_am.Position.X = (float)numericUpDown2.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsDirectional)
            {
                li_di.Position.X = (float)numericUpDown2.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsPoint)
            {
                li_po.Position.X = (float)numericUpDown2.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsNegative)
            {
                li_ne.Position.X = (float)numericUpDown2.Value;
                File.RMViewer_UpdateLights();
            }

            controller.UpdateText();
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;

            if (SelType == ScenTabSel.Scenery || SelType == ScenTabSel.Submodels)
            {
                ins.ModelMatrix[3].Y = (float)numericUpDown3.Value;
                File.RMViewer_UpdateScenery();
            }
            else if (SelType == ScenTabSel.LightsAmbient)
            {
                li_am.Position.Y = (float)numericUpDown3.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsDirectional)
            {
                li_di.Position.Y = (float)numericUpDown3.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsPoint)
            {
                li_po.Position.Y = (float)numericUpDown3.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsNegative)
            {
                li_ne.Position.Y = (float)numericUpDown3.Value;
                File.RMViewer_UpdateLights();
            }

            controller.UpdateText();
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;

            if (SelType == ScenTabSel.Scenery || SelType == ScenTabSel.Submodels)
            {
                ins.ModelMatrix[3].Z = (float)numericUpDown4.Value;
                File.RMViewer_UpdateScenery();
            }
            else if (SelType == ScenTabSel.LightsAmbient)
            {
                li_am.Position.Z = (float)numericUpDown4.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsDirectional)
            {
                li_di.Position.Z = (float)numericUpDown4.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsPoint)
            {
                li_po.Position.Z = (float)numericUpDown4.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsNegative)
            {
                li_ne.Position.Z = (float)numericUpDown4.Value;
                File.RMViewer_UpdateLights();
            }

            controller.UpdateText();
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;

            if (SelType == ScenTabSel.Scenery || SelType == ScenTabSel.Submodels)
            {
                ins.ModelMatrix[3].W = (float)numericUpDown5.Value;
            }
            else if (SelType == ScenTabSel.LightsAmbient)
            {
                li_am.Position.W = (float)numericUpDown5.Value;
            }
            else if (SelType == ScenTabSel.LightsDirectional)
            {
                li_di.Position.W = (float)numericUpDown5.Value;
            }
            else if (SelType == ScenTabSel.LightsPoint)
            {
                li_po.Position.W = (float)numericUpDown5.Value;
            }
            else if (SelType == ScenTabSel.LightsNegative)
            {
                li_ne.Position.W = (float)numericUpDown5.Value;
            }

            controller.UpdateText();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            //ins.RotX = (ushort)trackBar1.Value;
            GetXRot(false, true);
            controller.UpdateText();
            File.RMViewer_UpdateScenery();
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            //ins.RotY = (ushort)trackBar2.Value;
            GetYRot(false, true);
            controller.UpdateText();
            File.RMViewer_UpdateScenery();
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            //ins.RotZ = (ushort)trackBar3.Value;
            GetZRot(false, true);
            controller.UpdateText();
            File.RMViewer_UpdateScenery();
        }

        private void numericUpDown6_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            //ins.RotX = (ushort)numericUpDown6.Value;
            GetXRot(true, false);
            controller.UpdateText();
            File.RMViewer_UpdateScenery();
        }

        private void numericUpDown7_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            //ins.RotY = (ushort)numericUpDown7.Value;
            GetYRot(true, false);
            controller.UpdateText();
            File.RMViewer_UpdateScenery();
        }

        private void numericUpDown8_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            //ins.RotZ = (ushort)numericUpDown8.Value;
            GetZRot(true, false);
            controller.UpdateText();
            File.RMViewer_UpdateScenery();
        }

        private void button_PosFromCam_Click(object sender, EventArgs e)
        {
            ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;

            if (SelType == ScenTabSel.Scenery || SelType == ScenTabSel.Submodels)
            {
                Pos currentPos = File.RMViewer_GetPos(new Pos(ins.ModelMatrix[3].X, ins.ModelMatrix[3].Y, ins.ModelMatrix[3].Z, ins.ModelMatrix[3].W));
                ins.ModelMatrix[3].X = -currentPos.X;
                ins.ModelMatrix[3].Y = currentPos.Y;
                ins.ModelMatrix[3].Z = currentPos.Z;
                numericUpDown2.Value = (decimal)ins.ModelMatrix[3].X;
                numericUpDown3.Value = (decimal)ins.ModelMatrix[3].Y;
                numericUpDown4.Value = (decimal)ins.ModelMatrix[3].Z;
                numericUpDown5.Value = (decimal)ins.ModelMatrix[3].W;
                File.RMViewer_UpdateScenery();
            }
            else if (SelType == ScenTabSel.LightsAmbient)
            {
                Pos currentPos = File.RMViewer_GetPos(new Pos(li_am.Position.X, li_am.Position.Y, li_am.Position.Z, li_am.Position.W));
                li_am.Position.X = -currentPos.X;
                li_am.Position.Y = currentPos.Y;
                li_am.Position.Z = currentPos.Z;
                numericUpDown2.Value = (decimal)li_am.Position.X;
                numericUpDown3.Value = (decimal)li_am.Position.Y;
                numericUpDown4.Value = (decimal)li_am.Position.Z;
                numericUpDown5.Value = (decimal)li_am.Position.W;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsDirectional)
            {
                Pos currentPos = File.RMViewer_GetPos(new Pos(li_di.Position.X, li_di.Position.Y, li_di.Position.Z, li_di.Position.W));
                li_di.Position.X = -currentPos.X;
                li_di.Position.Y = currentPos.Y;
                li_di.Position.Z = currentPos.Z;
                numericUpDown2.Value = (decimal)li_di.Position.X;
                numericUpDown3.Value = (decimal)li_di.Position.Y;
                numericUpDown4.Value = (decimal)li_di.Position.Z;
                numericUpDown5.Value = (decimal)li_di.Position.W;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsPoint)
            {
                Pos currentPos = File.RMViewer_GetPos(new Pos(li_po.Position.X, li_po.Position.Y, li_po.Position.Z, li_po.Position.W));
                li_po.Position.X = -currentPos.X;
                li_po.Position.Y = currentPos.Y;
                li_po.Position.Z = currentPos.Z;
                numericUpDown2.Value = (decimal)li_po.Position.X;
                numericUpDown3.Value = (decimal)li_po.Position.Y;
                numericUpDown4.Value = (decimal)li_po.Position.Z;
                numericUpDown5.Value = (decimal)li_po.Position.W;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsNegative)
            {
                Pos currentPos = File.RMViewer_GetPos(new Pos(li_ne.Position.X, li_ne.Position.Y, li_ne.Position.Z, li_ne.Position.W));
                li_ne.Position.X = -currentPos.X;
                li_ne.Position.Y = currentPos.Y;
                li_ne.Position.Z = currentPos.Z;
                numericUpDown2.Value = (decimal)li_ne.Position.X;
                numericUpDown3.Value = (decimal)li_ne.Position.Y;
                numericUpDown4.Value = (decimal)li_ne.Position.Z;
                numericUpDown5.Value = (decimal)li_ne.Position.W;
                File.RMViewer_UpdateLights();
            }
            
            controller.UpdateText();
            
        }

        private void numericUpDown9_ValueChanged(object sender, EventArgs e)
        {
            /*
            if (ignore_value_change) return;
            ins.SomeNum1 = (int)numericUpDown9.Value;
            ((Controller)controller.Node.Nodes[controller.Data.RecordIDs[ins.ID]].Tag).UpdateText();
            */
        }

        private void numericUpDown10_ValueChanged(object sender, EventArgs e)
        {
            /*
            if (ignore_value_change) return;
            ins.SomeNum2 = (int)numericUpDown10.Value;
            ((Controller)controller.Node.Nodes[controller.Data.RecordIDs[ins.ID]].Tag).UpdateText();
            */
        }

        private void numericUpDown11_ValueChanged(object sender, EventArgs e)
        {
            /*
            if (ignore_value_change) return;
            ins.SomeNum3 = (int)numericUpDown11.Value;
            ((Controller)controller.Node.Nodes[controller.Data.RecordIDs[ins.ID]].Tag).UpdateText();
            */
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            /*
            if (ignore_value_change) return;
            ins.InstanceIDs.Clear();
            for (int i = 0; i < textBox2.Lines.Length; ++i)
            {
                if (ushort.TryParse(textBox2.Lines[i], out ushort v))
                    ins.InstanceIDs.Add(v);
            }
            ((Controller)controller.Node.Nodes[controller.Data.RecordIDs[ins.ID]].Tag).UpdateText();
            */
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            /*
            if (ignore_value_change) return;
            ins.PositionIDs.Clear();
            for (int i = 0; i < textBox3.Lines.Length; ++i)
            {
                if (ushort.TryParse(textBox3.Lines[i], out ushort v))
                    ins.PositionIDs.Add(v);
            }
            ((Controller)controller.Node.Nodes[controller.Data.RecordIDs[ins.ID]].Tag).UpdateText();
            */
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            /*
            if (ignore_value_change) return;
            ins.PathIDs.Clear();
            for (int i = 0; i < textBox4.Lines.Length; ++i)
            {
                if (ushort.TryParse(textBox4.Lines[i], out ushort v))
                    ins.PathIDs.Add(v);
            }
            ((Controller)controller.Node.Nodes[controller.Data.RecordIDs[ins.ID]].Tag).UpdateText();
            */
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            /*
            if (ignore_value_change) return;
            ins.UnkI321.Clear();
            for (int i = 0; i < textBox7.Lines.Length; ++i)
            {
                if (uint.TryParse(textBox7.Lines[i], out uint v))
                    ins.UnkI321.Add(v);
            }
            ((Controller)controller.Node.Nodes[controller.Data.RecordIDs[ins.ID]].Tag).UpdateText();
            */
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            /*
            if (ignore_value_change) return;
            ins.UnkI322.Clear();
            for (int i = 0; i < textBox6.Lines.Length; ++i)
            {
                if (float.TryParse(textBox6.Lines[i], out float v))
                    ins.UnkI322.Add(v);
            }
            ((Controller)controller.Node.Nodes[controller.Data.RecordIDs[ins.ID]].Tag).UpdateText();
            */
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            /*
            if (ignore_value_change) return;
            ins.UnkI323.Clear();
            for (int i = 0; i < textBox5.Lines.Length; ++i)
            {
                if (uint.TryParse(textBox5.Lines[i], out uint v))
                    ins.UnkI323.Add(v);
            }
            controller.UpdateText();
            */
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;

            if (tabControl2.SelectedIndex > 1)
            {
                if (controller.Data.LightsAmbient.Count + controller.Data.LightsDirectional.Count + controller.Data.LightsNegative.Count + controller.Data.LightsPoint.Count > 254)
                {
                    return;
                }
            }

            if (SelType == ScenTabSel.LightsAmbient)
            {
                int id = controller.Data.LightsAmbient.Count;
                SceneryData.LightAmbient light = new SceneryData.LightAmbient()
                {
                    Color_R = 1f,
                    Color_G = 1f,
                    Color_B = 1f,
                    Position = new Pos(0, 0, 0, 1),
                    Radius = 3f,
                    UnkFloat = 0,
                    Vectors = new Pos[2] { new Pos(0, 0, 0, 1), new Pos(0, 0, 0, 1) },
                    Flags = new byte[4] { 0x00, 0x01, 0x00, 0x00 },
                };
                controller.Data.LightsAmbient.Add(light);
                li_am = controller.Data.LightsAmbient[id];
                listBox3.Items.Add($"Ambient Light {id}");

                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsDirectional)
            {
                int id = controller.Data.LightsDirectional.Count;
                SceneryData.LightDirectional light = new SceneryData.LightDirectional()
                {
                    Color_R = 1f,
                    Color_G = 1f,
                    Color_B = 1f,
                    Position = new Pos(0, 0, 0, 1),
                    Radius = 3f,
                    UnkFloat = 0,
                    Vectors = new Pos[3] { new Pos(0, 0, 0, 1), new Pos(0, 0, 0, 1), new Pos(0, 0, 0, 0) },
                    Flags = new byte[4] { 0x01, 0x01, 0x00, 0x00 },
                    Flags2 = new byte[2] { 0x00, 0x00 },
                };
                controller.Data.LightsDirectional.Add(light);
                li_di = controller.Data.LightsDirectional[id];
                listBox4.Items.Add($"Directional Light {id}");

                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsPoint)
            {
                int id = controller.Data.LightsPoint.Count;
                SceneryData.LightPoint light = new SceneryData.LightPoint()
                {
                    Color_R = 1f,
                    Color_G = 1f,
                    Color_B = 1f,
                    Position = new Pos(0, 0, 0, 1),
                    Radius = 3f,
                    UnkFloat = 0,
                    Vectors = new Pos[2] { new Pos(0, 0, 0, 1), new Pos(0, 0, 0, 1), },
                    Flags = new byte[4] { 0x02, 0x01, 0x00, 0x00 },
                    Flags2 = new byte[2] { 0x00, 0x00 },
                };
                controller.Data.LightsPoint.Add(light);
                li_po = controller.Data.LightsPoint[id];
                listBox5.Items.Add($"Point Light {id}");

                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsNegative)
            {
                int id = controller.Data.LightsNegative.Count;
                SceneryData.LightNegative light = new SceneryData.LightNegative()
                {
                    Color_R = 1f,
                    Color_G = 1f,
                    Color_B = 1f,
                    Position = new Pos(0, 0, 0, 1),
                    Radius = 3f,
                    UnkFloat = 0,
                    Vectors = new Pos[3] { new Pos(0, 0, 0, 1), new Pos(0, 0, 0, 1), new Pos(0, 0, 0, 0), },
                    Flags = new byte[4] { 0x03, 0x01, 0x00, 0x00 },
                    Floats = new float[5] { 0f, 0f, 0f, 0f, 0f },
                };
                controller.Data.LightsNegative.Add(light);
                li_ne = controller.Data.LightsNegative[id];
                listBox6.Items.Add($"Negative Light {id}");

                File.RMViewer_UpdateLights();
            }
            controller.UpdateText();
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*
            var sel_i = listBox1.SelectedIndex;
            if (sel_i == -1)
                return;
            controller.RemoveItem(ins.ID);
            listBox1.BeginUpdate();
            listBox1.Items.RemoveAt(sel_i);
            for (int i = 0; i < controller.Data.Records.Count; ++i)
            {
                bool update_text = false;
                Instance new_ins = (Instance)controller.Data.Records[i];
                if (new_ins.ID != i)
                {
                    controller.ChangeID(new_ins.ID, (uint)i);
                    listBox1.Items[i] = GenTextForList(new_ins);
                    ((Controller)controller.Node.Nodes[i].Tag).UpdateName();
                    update_text = true;
                }
                for (int j = 0; j < new_ins.InstanceIDs.Count; ++j)
                {
                    if (new_ins.InstanceIDs[j] > sel_i)
                    {
                        update_text = true;
                        --new_ins.InstanceIDs[j];
                    }
                    else if (new_ins.InstanceIDs[j] == sel_i)
                    {
                        update_text = true;
                        new_ins.InstanceIDs.RemoveAt(j);
                        --j;
                    }
                }
                if (update_text)
                    ((Controller)controller.Node.Nodes[i].Tag).UpdateTextBox();
                update_text = false;
            }

            var trig_sec_c = (SectionController)controller.Node.Parent.Nodes[7].Tag;

            foreach (TreeNode node in trig_sec_c.Node.Nodes)
            {
                bool update_text = false;
                Trigger tr = ((TriggerController)node.Tag).Data;
                for (int j = 0; j < tr.Instances.Count; ++j)
                {
                    if (tr.Instances[j] > sel_i)
                    {
                        update_text = true;
                        tr.Instances[j] -= 1;
                    }
                    else if (tr.Instances[j] == sel_i)
                    {
                        update_text = true;
                        tr.Instances.RemoveAt(j);
                        --j;
                    }
                    else
                        continue;
                }
                if (update_text)
                    ((Controller)node.Tag).UpdateTextBox();
            }
            trig_sec_c.UpdateTextBox();
            if (sel_i >= listBox1.Items.Count) sel_i = listBox1.Items.Count - 1;
            listBox1.SelectedIndex = sel_i;
            listBox1.EndUpdate();
            if (listBox1.Items.Count == 0)
                tabControl1.Enabled = false;
            controller.UpdateTextBox();
            */
        }

        private void duplicateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*
            var sel_i = listBox1.SelectedIndex;
            if (sel_i == -1)
                return;

            Instance last_inst = ins;

            if (controller.Data.RecordIDs.Count >= ushort.MaxValue) return;
            uint id;
            for (id = 0; id < uint.MaxValue; ++id)
            {
                if (!controller.Data.ContainsItem(id))
                    break;
            }
            Instance new_ins = new Instance
            {
                ID = id,
                AfterOID = 0xFFFFFFFF,
                Pos = new Pos(last_inst.Pos.X,last_inst.Pos.Y + 1f,last_inst.Pos.Z, 1),
                RotX = last_inst.RotX,
                RotY = last_inst.RotY,
                RotZ = last_inst.RotZ,
                COMRotX = last_inst.COMRotX,
                COMRotY = last_inst.COMRotY,
                COMRotZ = last_inst.COMRotZ,
                SomeNum1 = last_inst.SomeNum1,
                SomeNum2 = last_inst.SomeNum2,
                SomeNum3 = last_inst.SomeNum3,
                UnkI32 = last_inst.UnkI32,
                UnkI321 = last_inst.UnkI321,
                UnkI322 = last_inst.UnkI322,
                UnkI323 = last_inst.UnkI323,
                ObjectID = last_inst.ObjectID,
                InstanceIDs = last_inst.InstanceIDs,
                PathIDs = last_inst.PathIDs,
                PositionIDs = last_inst.PositionIDs,
            };
            controller.Data.AddItem(id, new_ins);
            ((MainForm)Tag).GenTreeNode(new_ins, controller, controller.MainFile);
            ins = new_ins;
            listBox1.Items.Add(GenTextForList(ins));
            controller.UpdateText();
            ((Controller)controller.Node.Nodes[controller.Data.RecordIDs[ins.ID]].Tag).UpdateText();
            */
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Submodels
        }

        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Ambient Lights
            if (listBox3.SelectedIndex == -1) return;

            this.SuspendDrawing();

            li_am = controller.Data.LightsAmbient[listBox3.SelectedIndex];
            File.RMViewer_CustomTeleport(li_am.Position.X, li_am.Position.Y, li_am.Position.Z);
            tabControl1.Enabled = true;
            tabControl1.Tag = 0x00;
            ignore_value_change = true;
            if (tabControl1.SelectedIndex == 0)
            {
                UpdateTab1();
            }
            else if (tabControl1.SelectedIndex == 1)
            {
                UpdateTab2();
            }

            ignore_value_change = false;

            this.ResumeDrawing();
        }

        private void listBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Directional Lights
            if (listBox4.SelectedIndex == -1) return;

            this.SuspendDrawing();

            li_di = controller.Data.LightsDirectional[listBox4.SelectedIndex];
            File.RMViewer_CustomTeleport(li_di.Position.X, li_di.Position.Y, li_di.Position.Z);
            tabControl1.Enabled = true;
            tabControl1.Tag = 0x00;
            ignore_value_change = true;
            if (tabControl1.SelectedIndex == 0)
            {
                UpdateTab1();
            }
            else if (tabControl1.SelectedIndex == 1)
            {
                UpdateTab2();
            }

            ignore_value_change = false;

            this.ResumeDrawing();
        }

        private void listBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Point Lights
            if (listBox5.SelectedIndex == -1) return;

            this.SuspendDrawing();

            li_po = controller.Data.LightsPoint[listBox5.SelectedIndex];
            File.RMViewer_CustomTeleport(li_po.Position.X, li_po.Position.Y, li_po.Position.Z);
            tabControl1.Enabled = true;
            tabControl1.Tag = 0x00;
            ignore_value_change = true;
            if (tabControl1.SelectedIndex == 0)
            {
                UpdateTab1();
            }
            else if (tabControl1.SelectedIndex == 1)
            {
                UpdateTab2();
            }

            ignore_value_change = false;

            this.ResumeDrawing();
        }

        private void listBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Negative Lights
            if (listBox6.SelectedIndex == -1) return;

            this.SuspendDrawing();

            li_ne = controller.Data.LightsNegative[listBox6.SelectedIndex];
            File.RMViewer_CustomTeleport(li_ne.Position.X, li_ne.Position.Y, li_ne.Position.Z);
            tabControl1.Enabled = true;
            tabControl1.Tag = 0x00;
            ignore_value_change = true;
            if (tabControl1.SelectedIndex == 0)
            {
                UpdateTab1();
            }
            else if (tabControl1.SelectedIndex == 1)
            {
                UpdateTab2();
            }

            ignore_value_change = false;

            this.ResumeDrawing();
        }

        private void numericUpDown13_colorR_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;

            ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;

            if (SelType == ScenTabSel.LightsAmbient)
            {
                li_am.Color_R = (float)numericUpDown13_colorR.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsDirectional)
            {
                li_di.Color_R = (float)numericUpDown13_colorR.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsPoint)
            {
                li_po.Color_R = (float)numericUpDown13_colorR.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsNegative)
            {
                li_ne.Color_R = (float)numericUpDown13_colorR.Value;
                File.RMViewer_UpdateLights();
            }

            controller.UpdateText();
        }

        private void numericUpDown11_colorG_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;

            ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;

            if (SelType == ScenTabSel.LightsAmbient)
            {
                li_am.Color_G = (float)numericUpDown11_colorG.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsDirectional)
            {
                li_di.Color_G = (float)numericUpDown11_colorG.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsPoint)
            {
                li_po.Color_G = (float)numericUpDown11_colorG.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsNegative)
            {
                li_ne.Color_G = (float)numericUpDown11_colorG.Value;
                File.RMViewer_UpdateLights();
            }

            controller.UpdateText();
        }

        private void numericUpDown10_colorB_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;

            ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;

            if (SelType == ScenTabSel.LightsAmbient)
            {
                li_am.Color_B = (float)numericUpDown10_colorB.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsDirectional)
            {
                li_di.Color_B = (float)numericUpDown10_colorB.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsPoint)
            {
                li_po.Color_B = (float)numericUpDown10_colorB.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsNegative)
            {
                li_ne.Color_B = (float)numericUpDown10_colorB.Value;
                File.RMViewer_UpdateLights();
            }

            controller.UpdateText();
        }

        private void numericUpDown9_colorF_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;

            ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;

            if (SelType == ScenTabSel.LightsAmbient)
            {
                li_am.UnkFloat = (float)numericUpDown9_colorF.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsDirectional)
            {
                li_di.UnkFloat = (float)numericUpDown9_colorF.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsPoint)
            {
                li_po.UnkFloat = (float)numericUpDown9_colorF.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsNegative)
            {
                li_ne.UnkFloat = (float)numericUpDown9_colorF.Value;
                File.RMViewer_UpdateLights();
            }

            controller.UpdateText();
        }

        private void numericUpDown20_radius_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;

            ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;

            if (SelType == ScenTabSel.LightsAmbient)
            {
                li_am.Radius = (float)numericUpDown20_radius.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsDirectional)
            {
                li_di.Radius = (float)numericUpDown20_radius.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsPoint)
            {
                li_po.Radius = (float)numericUpDown20_radius.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsNegative)
            {
                li_ne.Radius = (float)numericUpDown20_radius.Value;
                File.RMViewer_UpdateLights();
            }

            controller.UpdateText();
        }

        private void textBox2_flag1_TextChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            if (uint.TryParse(textBox2_flag1.Text, System.Globalization.NumberStyles.HexNumber, null, out uint o))
            {
                ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;
                byte outByte = (byte)o;

                if (SelType == ScenTabSel.LightsAmbient)
                {
                    li_am.Flags[0] = outByte;
                    File.RMViewer_UpdateLights();
                }
                else if (SelType == ScenTabSel.LightsDirectional)
                {
                    li_di.Flags[0] = outByte;
                    File.RMViewer_UpdateLights();
                }
                else if (SelType == ScenTabSel.LightsPoint)
                {
                    li_po.Flags[0] = outByte;
                    File.RMViewer_UpdateLights();
                }
                else if (SelType == ScenTabSel.LightsNegative)
                {
                    li_ne.Flags[0] = outByte;
                    File.RMViewer_UpdateLights();
                }
            }
            controller.UpdateText();
        }

        private void textBox3_flag2_TextChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            if (uint.TryParse(textBox3_flag2.Text, System.Globalization.NumberStyles.HexNumber, null, out uint o))
            {
                ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;
                byte outByte = (byte)o;

                if (SelType == ScenTabSel.LightsAmbient)
                {
                    li_am.Flags[1] = outByte;
                    File.RMViewer_UpdateLights();
                }
                else if (SelType == ScenTabSel.LightsDirectional)
                {
                    li_di.Flags[1] = outByte;
                    File.RMViewer_UpdateLights();
                }
                else if (SelType == ScenTabSel.LightsPoint)
                {
                    li_po.Flags[1] = outByte;
                    File.RMViewer_UpdateLights();
                }
                else if (SelType == ScenTabSel.LightsNegative)
                {
                    li_ne.Flags[1] = outByte;
                    File.RMViewer_UpdateLights();
                }
            }
            controller.UpdateText();
        }

        private void textBox4_flag3_TextChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            if (uint.TryParse(textBox4_flag3.Text, System.Globalization.NumberStyles.HexNumber, null, out uint o))
            {
                ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;
                byte outByte = (byte)o;

                if (SelType == ScenTabSel.LightsAmbient)
                {
                    li_am.Flags[2] = outByte;
                    File.RMViewer_UpdateLights();
                }
                else if (SelType == ScenTabSel.LightsDirectional)
                {
                    li_di.Flags[2] = outByte;
                    File.RMViewer_UpdateLights();
                }
                else if (SelType == ScenTabSel.LightsPoint)
                {
                    li_po.Flags[2] = outByte;
                    File.RMViewer_UpdateLights();
                }
                else if (SelType == ScenTabSel.LightsNegative)
                {
                    li_ne.Flags[2] = outByte;
                    File.RMViewer_UpdateLights();
                }
            }
            controller.UpdateText();
        }

        private void textBox5_flag4_TextChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            if (uint.TryParse(textBox5_flag4.Text, System.Globalization.NumberStyles.HexNumber, null, out uint o))
            {
                ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;
                byte outByte = (byte)o;

                if (SelType == ScenTabSel.LightsAmbient)
                {
                    li_am.Flags[3] = outByte;
                    File.RMViewer_UpdateLights();
                }
                else if (SelType == ScenTabSel.LightsDirectional)
                {
                    li_di.Flags[3] = outByte;
                    File.RMViewer_UpdateLights();
                }
                else if (SelType == ScenTabSel.LightsPoint)
                {
                    li_po.Flags[3] = outByte;
                    File.RMViewer_UpdateLights();
                }
                else if (SelType == ScenTabSel.LightsNegative)
                {
                    li_ne.Flags[3] = outByte;
                    File.RMViewer_UpdateLights();
                }
            }
            controller.UpdateText();
        }

        private void textBox9_flag2_1_TextChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            if (uint.TryParse(textBox9_flag2_1.Text, System.Globalization.NumberStyles.HexNumber, null, out uint o))
            {
                ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;
                byte outByte = (byte)o;

                if (SelType == ScenTabSel.LightsDirectional)
                {
                    li_di.Flags2[0] = outByte;
                    File.RMViewer_UpdateLights();
                }
                else if (SelType == ScenTabSel.LightsPoint)
                {
                    li_po.Flags2[0] = outByte;
                    File.RMViewer_UpdateLights();
                }

            }
            controller.UpdateText();
        }

        private void textBox8_flag2_2_TextChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            if (uint.TryParse(textBox9_flag2_1.Text, System.Globalization.NumberStyles.HexNumber, null, out uint o))
            {
                ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;
                byte outByte = (byte)o;

                if (SelType == ScenTabSel.LightsDirectional)
                {
                    li_di.Flags2[1] = outByte;
                    File.RMViewer_UpdateLights();
                }
                else if (SelType == ScenTabSel.LightsPoint)
                {
                    li_po.Flags2[1] = outByte;
                    File.RMViewer_UpdateLights();
                }

            }
            controller.UpdateText();
        }

        private void numericUpDown21_v1x_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;

            ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;

            if (SelType == ScenTabSel.LightsAmbient)
            {
                li_am.Vectors[0].X = (float)numericUpDown21_v1x.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsDirectional)
            {
                li_di.Vectors[0].X = (float)numericUpDown21_v1x.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsPoint)
            {
                li_po.Vectors[0].X = (float)numericUpDown21_v1x.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsNegative)
            {
                li_ne.Vectors[0].X = (float)numericUpDown21_v1x.Value;
                File.RMViewer_UpdateLights();
            }

            controller.UpdateText();
        }

        private void numericUpDown19_v1y_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;

            ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;

            if (SelType == ScenTabSel.LightsAmbient)
            {
                li_am.Vectors[0].Y = (float)numericUpDown19_v1y.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsDirectional)
            {
                li_di.Vectors[0].Y = (float)numericUpDown19_v1y.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsPoint)
            {
                li_po.Vectors[0].Y = (float)numericUpDown19_v1y.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsNegative)
            {
                li_ne.Vectors[0].Y = (float)numericUpDown19_v1y.Value;
                File.RMViewer_UpdateLights();
            }

            controller.UpdateText();
        }

        private void numericUpDown15_v1z_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;

            ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;

            if (SelType == ScenTabSel.LightsAmbient)
            {
                li_am.Vectors[0].Z = (float)numericUpDown15_v1z.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsDirectional)
            {
                li_di.Vectors[0].Z = (float)numericUpDown15_v1z.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsPoint)
            {
                li_po.Vectors[0].Z = (float)numericUpDown15_v1z.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsNegative)
            {
                li_ne.Vectors[0].Z = (float)numericUpDown15_v1z.Value;
                File.RMViewer_UpdateLights();
            }

            controller.UpdateText();
        }

        private void numericUpDown14_v1w_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;

            ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;

            if (SelType == ScenTabSel.LightsAmbient)
            {
                li_am.Vectors[0].W = (float)numericUpDown14_v1w.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsDirectional)
            {
                li_di.Vectors[0].W = (float)numericUpDown14_v1w.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsPoint)
            {
                li_po.Vectors[0].W = (float)numericUpDown14_v1w.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsNegative)
            {
                li_ne.Vectors[0].W = (float)numericUpDown14_v1w.Value;
                File.RMViewer_UpdateLights();
            }

            controller.UpdateText();
        }

        private void numericUpDown26_v2x_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;

            ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;

            if (SelType == ScenTabSel.LightsAmbient)
            {
                li_am.Vectors[1].X = (float)numericUpDown26_v2x.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsDirectional)
            {
                li_di.Vectors[1].X = (float)numericUpDown26_v2x.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsPoint)
            {
                li_po.Vectors[1].X = (float)numericUpDown26_v2x.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsNegative)
            {
                li_ne.Vectors[1].X = (float)numericUpDown26_v2x.Value;
                File.RMViewer_UpdateLights();
            }

            controller.UpdateText();
        }

        private void numericUpDown25_v2y_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;

            ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;

            if (SelType == ScenTabSel.LightsAmbient)
            {
                li_am.Vectors[1].Y = (float)numericUpDown25_v2y.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsDirectional)
            {
                li_di.Vectors[1].Y = (float)numericUpDown25_v2y.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsPoint)
            {
                li_po.Vectors[1].Y = (float)numericUpDown25_v2y.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsNegative)
            {
                li_ne.Vectors[1].Y = (float)numericUpDown25_v2y.Value;
                File.RMViewer_UpdateLights();
            }

            controller.UpdateText();
        }

        private void numericUpDown24_v2z_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;

            ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;

            if (SelType == ScenTabSel.LightsAmbient)
            {
                li_am.Vectors[1].Z = (float)numericUpDown24_v2z.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsDirectional)
            {
                li_di.Vectors[1].Z = (float)numericUpDown24_v2z.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsPoint)
            {
                li_po.Vectors[1].Z = (float)numericUpDown24_v2z.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsNegative)
            {
                li_ne.Vectors[1].Z = (float)numericUpDown24_v2z.Value;
                File.RMViewer_UpdateLights();
            }

            controller.UpdateText();
        }

        private void numericUpDown23_v2w_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;

            ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;

            if (SelType == ScenTabSel.LightsAmbient)
            {
                li_am.Vectors[1].W = (float)numericUpDown23_v2w.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsDirectional)
            {
                li_di.Vectors[1].W = (float)numericUpDown23_v2w.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsPoint)
            {
                li_po.Vectors[1].W = (float)numericUpDown23_v2w.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsNegative)
            {
                li_ne.Vectors[1].W = (float)numericUpDown23_v2w.Value;
                File.RMViewer_UpdateLights();
            }

            controller.UpdateText();
        }

        private void numericUpDown30_v3x_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;

            ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;

            if (SelType == ScenTabSel.LightsDirectional)
            {
                li_di.Vectors[2].X = (float)numericUpDown30_v3x.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsNegative)
            {
                li_ne.Vectors[2].X = (float)numericUpDown30_v3x.Value;
                File.RMViewer_UpdateLights();
            }

            controller.UpdateText();
        }

        private void numericUpDown29_v3y_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;

            ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;

            if (SelType == ScenTabSel.LightsDirectional)
            {
                li_di.Vectors[2].Y = (float)numericUpDown29_v3y.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsNegative)
            {
                li_ne.Vectors[2].Y = (float)numericUpDown29_v3y.Value;
                File.RMViewer_UpdateLights();
            }

            controller.UpdateText();
        }

        private void numericUpDown28_v3z_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;

            ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;

            if (SelType == ScenTabSel.LightsDirectional)
            {
                li_di.Vectors[2].Z = (float)numericUpDown28_v3z.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsNegative)
            {
                li_ne.Vectors[2].Z = (float)numericUpDown28_v3z.Value;
                File.RMViewer_UpdateLights();
            }

            controller.UpdateText();
        }

        private void numericUpDown27_v3w_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;

            ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;

            if (SelType == ScenTabSel.LightsDirectional)
            {
                li_di.Vectors[2].W = (float)numericUpDown27_v3w.Value;
                File.RMViewer_UpdateLights();
            }
            else if (SelType == ScenTabSel.LightsNegative)
            {
                li_ne.Vectors[2].W = (float)numericUpDown27_v3w.Value;
                File.RMViewer_UpdateLights();
            }

            controller.UpdateText();
        }

        private void numericUpDown33_nefloats_1_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;

            ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;

            if (SelType == ScenTabSel.LightsNegative)
            {
                li_ne.Floats[0] = (float)numericUpDown33_nefloats_1.Value;
                File.RMViewer_UpdateLights();
            }

            controller.UpdateText();
        }

        private void numericUpDown32_nefloats_2_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;

            ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;

            if (SelType == ScenTabSel.LightsNegative)
            {
                li_ne.Floats[1] = (float)numericUpDown32_nefloats_2.Value;
                File.RMViewer_UpdateLights();
            }

            controller.UpdateText();
        }

        private void numericUpDown31_nefloats_3_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;

            ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;

            if (SelType == ScenTabSel.LightsNegative)
            {
                li_ne.Floats[2] = (float)numericUpDown31_nefloats_3.Value;
                File.RMViewer_UpdateLights();
            }

            controller.UpdateText();
        }

        private void numericUpDown22_nefloats_4_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;

            ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;

            if (SelType == ScenTabSel.LightsNegative)
            {
                li_ne.Floats[3] = (float)numericUpDown22_nefloats_4.Value;
                File.RMViewer_UpdateLights();
            }

            controller.UpdateText();
        }

        private void numericUpDown34_nefloats_5_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;

            ScenTabSel SelType = (ScenTabSel)tabControl2.SelectedIndex;

            if (SelType == ScenTabSel.LightsNegative)
            {
                li_ne.Floats[4] = (float)numericUpDown34_nefloats_5.Value;
                File.RMViewer_UpdateLights();
            }

            controller.UpdateText();
        }
    }
}
