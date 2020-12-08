using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Forms;
using Twinsanity;

namespace TwinsanityEditor
{
    public partial class CameraEditor : Form
    {
        private const string angleFormat = "{0:0.000}º";
        private SectionController controller;
        private Camera ins;
        
        private FileController File { get; set; }
        private TwinsFile FileData { get => File.Data; }

        private bool ignore_value_change;

        public CameraEditor(SectionController c)
        {
            File = c.MainFile;
            controller = c;
            InitializeComponent();
            Text = $"Camera Editor (Section {c.Data.Parent.ID})";
            PopulateList();
            comboBox1.TextChanged += comboBox1_TextChanged;
            tabControl1.SelectedIndexChanged += tabControl1_SelectedIndexChanged;
            FormClosed += CameraEditor_FormClosed;
        }

        private void CameraEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            File.SelectItem(null);
        }

        private void PopulateList()
        {
            listBox1.BeginUpdate();
            listBox1.Items.Clear();
            foreach (Camera i in controller.Data.Records)
            {
                listBox1.Items.Add($"ID {i.ID}");
            }
            listBox1.EndUpdate();
            comboBox1.Items.Clear();
        }

        private void listBox1_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (listBox1.SelectedIndex == -1) return;

            this.SuspendDrawing();

            File.SelectItem((Camera)controller.Data.Records[listBox1.SelectedIndex]);
            ins = (Camera)File.SelectedItem;
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
            //comboBox1.Text = ins.ObjectID.ToString();
            numericUpDown12.Value = ins.ID;
            numericUpDown2.Value = (decimal)ins.Coords[1].X;
            numericUpDown3.Value = (decimal)ins.Coords[1].Y;
            numericUpDown4.Value = (decimal)ins.Coords[1].Z;
            numericUpDown5.Value = (decimal)ins.Coords[1].W;
            textBox1.Text = "";// Convert.ToString(ins.UnkI32, 16);
            tabControl1.Tag = (int)tabControl1.Tag | 0x01;
            //numericUpDown13.Value = ins.CamRot.Pitch;
            //numericUpDown14.Value = ins.CamRot.Yaw;
            //numericUpDown15.Value = ins.CamRot.Roll;
            GetXRot(true, true); GetYRot(true, true); GetZRot(true, true);
        }

        private void UpdateTab2()
        {
            /*
            numericUpDown9.Value = ins.SomeNum1;
            numericUpDown10.Value = ins.SomeNum2;
            numericUpDown11.Value = ins.SomeNum3;

            string[] lines = new string[ins.InstanceIDs.Count];
            for (int i = 0; i < ins.InstanceIDs.Count; ++i)
                lines[i] = ins.InstanceIDs[i].ToString();
            textBox2.Lines = lines;
            lines = new string[ins.PositionIDs.Count];
            for (int i = 0; i < ins.PositionIDs.Count; ++i)
                lines[i] = ins.PositionIDs[i].ToString();
            textBox3.Lines = lines;
            lines = new string[ins.PathIDs.Count];
            for (int i = 0; i < ins.PathIDs.Count; ++i)
                lines[i] = ins.PathIDs[i].ToString();
            textBox4.Lines = lines;

            lines = new string[ins.UnkI321.Count];
            for (int i = 0; i < ins.UnkI321.Count; ++i)
                lines[i] = ins.UnkI321[i].ToString("X");
            textBox7.Lines = lines;
            lines = new string[ins.UnkI322.Count];
            for (int i = 0; i < ins.UnkI322.Count; ++i)
                lines[i] = ins.UnkI322[i].ToString();
            textBox6.Lines = lines;
            lines = new string[ins.UnkI323.Count];
            for (int i = 0; i < ins.UnkI323.Count; ++i)
                lines[i] = ins.UnkI323[i].ToString();
            textBox5.Lines = lines;
            tabControl1.Tag = (int)tabControl1.Tag | 0x02;
            */
        }

        private void GetXRot(bool slider, bool num)
        {
            ignore_value_change = true;
            //if (slider)
                //trackBar1.Value = ins.CamRot.Pitch;
            //if (num)
                //numericUpDown6.Value = ins.CamRot.Pitch;
            //label6.Text = string.Format(angleFormat, ins.CamRot.Pitch / (float)(ushort.MaxValue + 1) * 360f);
            ignore_value_change = false;
        }

        private void GetYRot(bool slider, bool num)
        {
            ignore_value_change = true;
            //if (slider)
                //trackBar2.Value = ins.CamRot.Yaw;
            //if (num)
                //numericUpDown7.Value = ins.CamRot.Yaw;
            //label7.Text = string.Format(angleFormat, ins.CamRot.Yaw / (float)(ushort.MaxValue + 1) * 360f);
            ignore_value_change = false;
        }

        private void GetZRot(bool slider, bool num)
        {
            ignore_value_change = true;
            //if (slider)
                //trackBar3.Value = ins.CamRot.Roll;
            //if (num)
            //    numericUpDown8.Value = ins.CamRot.Roll;
            //label9.Text = string.Format(angleFormat, ins.CamRot.Roll / (float)(ushort.MaxValue + 1) * 360f);
            ignore_value_change = false;
        }

        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            ins.Coords[1].X = (float)numericUpDown2.Value;
            ((Controller)controller.Node.Nodes[controller.Data.RecordIDs[ins.ID]].Tag).UpdateTextBox();
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            ins.Coords[1].Y = (float)numericUpDown3.Value;
            ((Controller)controller.Node.Nodes[controller.Data.RecordIDs[ins.ID]].Tag).UpdateTextBox();
            //File.RMViewer_LoadInstances();
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            ins.Coords[1].Z = (float)numericUpDown4.Value;
            ((Controller)controller.Node.Nodes[controller.Data.RecordIDs[ins.ID]].Tag).UpdateTextBox();
            //File.RMViewer_LoadInstances();
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            ins.Coords[1].W = (float)numericUpDown5.Value;
            ((Controller)controller.Node.Nodes[controller.Data.RecordIDs[ins.ID]].Tag).UpdateTextBox();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            //ins.CamRot.Pitch = (ushort)trackBar1.Value;
            GetXRot(false, true);
            ((Controller)controller.Node.Nodes[controller.Data.RecordIDs[ins.ID]].Tag).UpdateTextBox();
            //File.RMViewer_LoadInstances();
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            //ins.CamRot.Yaw = (ushort)trackBar2.Value;
            GetYRot(false, true);
            ((Controller)controller.Node.Nodes[controller.Data.RecordIDs[ins.ID]].Tag).UpdateTextBox();
            //File.RMViewer_LoadInstances();
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            //ins.CamRot.Roll = (ushort)trackBar3.Value;
            GetZRot(false, true);
            ((Controller)controller.Node.Nodes[controller.Data.RecordIDs[ins.ID]].Tag).UpdateTextBox();
            //File.RMViewer_LoadInstances();
        }

        private void numericUpDown6_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            //ins.CamRot.Pitch = (ushort)numericUpDown6.Value;
            GetXRot(true, false);
            ((Controller)controller.Node.Nodes[controller.Data.RecordIDs[ins.ID]].Tag).UpdateText();
            //File.RMViewer_LoadInstances();
        }

        private void numericUpDown13_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            //ins.CamRot.Pitch = (ushort)numericUpDown13.Value;
            ((Controller)controller.Node.Nodes[controller.Data.RecordIDs[ins.ID]].Tag).UpdateText();
        }

        private void numericUpDown7_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            //ins.CamRot.Yaw = (ushort)numericUpDown7.Value;
            GetYRot(true, false);
            ((Controller)controller.Node.Nodes[controller.Data.RecordIDs[ins.ID]].Tag).UpdateText();
            //File.RMViewer_LoadInstances();
        }

        private void numericUpDown14_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            //ins.CamRot.Yaw = (ushort)numericUpDown14.Value;
            ((Controller)controller.Node.Nodes[controller.Data.RecordIDs[ins.ID]].Tag).UpdateText();
        }

        private void numericUpDown8_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            //ins.CamRot.Roll = (ushort)numericUpDown8.Value;
            GetZRot(true, false);
            ((Controller)controller.Node.Nodes[controller.Data.RecordIDs[ins.ID]].Tag).UpdateText();
            //File.RMViewer_LoadInstances();
        }

        private void numericUpDown15_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            //ins.CamRot.Roll = (ushort)numericUpDown15.Value;
            ((Controller)controller.Node.Nodes[controller.Data.RecordIDs[ins.ID]].Tag).UpdateText();
        }

        private void numericUpDown9_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
        }

        private void numericUpDown10_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
        }

        private void numericUpDown11_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (controller.Data.RecordIDs.Count >= ushort.MaxValue) return;
            uint id;
            for (id = 0; id < uint.MaxValue; ++id)
            {
                if (!controller.Data.ContainsItem(id))
                    break;
            }
            Camera new_ins = new Camera { ID = id, }; //todo
            controller.Data.AddItem(id, new_ins);
            ((MainForm)Tag).GenTreeNode(new_ins, controller, controller.MainFile);
            ins = new_ins;
            listBox1.Items.Add($"ID {new_ins.ID}");
            controller.UpdateText();
            ((Controller)controller.Node.Nodes[controller.Data.RecordIDs[ins.ID]].Tag).UpdateText();
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sel_i = listBox1.SelectedIndex;
            if (sel_i == -1)
                return;
            controller.RemoveItem(ins.ID);
            listBox1.BeginUpdate();
            listBox1.Items.RemoveAt(sel_i);
            for (int i = 0; i < controller.Data.Records.Count; ++i)
            {
                bool update_text = false;
                Camera new_ins = (Camera)controller.Data.Records[i];
                if (new_ins.ID != i)
                {
                    controller.ChangeID(new_ins.ID, (uint)i);
                    listBox1.Items[i] = $"ID {new_ins.ID}";
                    ((Controller)controller.Node.Nodes[i].Tag).UpdateName();
                    update_text = true;
                }
                if (update_text)
                    ((Controller)controller.Node.Nodes[i].Tag).UpdateTextBox();
                update_text = false;
            }

            if (sel_i >= listBox1.Items.Count) sel_i = listBox1.Items.Count - 1;
            listBox1.SelectedIndex = sel_i;
            listBox1.EndUpdate();
            if (listBox1.Items.Count == 0)
                tabControl1.Enabled = false;
            controller.UpdateTextBox();
        }

        private void button_PosFromCam_Click(object sender, EventArgs e)
        {
            Pos currentPos = File.RMViewer_GetPos(ins.Coords[1]);
            ins.Coords[1].X = currentPos.X;
            ins.Coords[1].Y = currentPos.Y;
            ins.Coords[1].Z = currentPos.Z;
            numericUpDown2.Value = (decimal)ins.Coords[1].X;
            numericUpDown3.Value = (decimal)ins.Coords[1].Y;
            numericUpDown4.Value = (decimal)ins.Coords[1].Z;
            numericUpDown5.Value = (decimal)ins.Coords[1].W;
            ((Controller)controller.Node.Nodes[controller.Data.RecordIDs[ins.ID]].Tag).UpdateTextBox();
            //File.RMViewer_LoadInstances();
        }

        private void duplicateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sel_i = listBox1.SelectedIndex;
            if (sel_i == -1)
                return;

            Camera last_inst = ins;

            if (controller.Data.RecordIDs.Count >= ushort.MaxValue) return;
            uint id;
            for (id = 0; id < uint.MaxValue; ++id)
            {
                if (!controller.Data.ContainsItem(id))
                    break;
            }
            Camera new_ins = new Camera
            {
                ID = id,
                //todo
            };
            controller.Data.AddItem(id, new_ins);
            ((MainForm)Tag).GenTreeNode(new_ins, controller, controller.MainFile);
            ins = new_ins;
            listBox1.Items.Add($"ID {new_ins.ID}");
            controller.UpdateText();
            ((Controller)controller.Node.Nodes[controller.Data.RecordIDs[ins.ID]].Tag).UpdateText();
        }
    }
}
