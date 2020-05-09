using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Forms;
using Twinsanity;

namespace TwinsanityEditor
{
    public partial class ParticleEditor : Form
    {
        private const string angleFormat = "{0:0.000}º";
        private ParticleDataController controller;
        private ParticleData.ParticleSystemInstance ins;
        
        private FileController File { get; set; }
        private TwinsFile FileData { get => File.Data; }

        private bool ignore_value_change;

        public ParticleEditor(ParticleDataController c)
        {
            File = c.MainFile;
            controller = c;
            InitializeComponent();
            Text = $"Particle Data Editor";
            PopulateList();
            comboBox1.TextChanged += comboBox1_TextChanged;
            tabControl1.SelectedIndexChanged += tabControl1_SelectedIndexChanged;
            FormClosed += ParticleEditor_FormClosed;
        }

        private void ParticleEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            File.SelectItem(null);
        }

        private void PopulateList()
        {
            listBox1.BeginUpdate();
            listBox1.Items.Clear();
            for(int i = 0; i < controller.Data.ParticleInstanceCount; i++)
            {
                listBox1.Items.Add($"#{i}: {controller.Data.ParticleInstances[i].Name}");
            }
            listBox1.EndUpdate();
            comboBox1.Items.Clear();
            for (int i = 0; i < controller.Data.ParticleTypeCount; i++)
            {
                comboBox1.Items.Add($"#{i}: {controller.Data.ParticleTypes[i].Name}");
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (listBox1.SelectedIndex == -1) return;

            this.SuspendDrawing();

            ins = controller.Data.ParticleInstances[listBox1.SelectedIndex];
            File.RMViewer_CustomTeleport(ins.Position.X, ins.Position.Y, ins.Position.Z);
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
            comboBox1.Text = ins.Name;
            numericUpDown12.Value = listBox1.SelectedIndex;
            numericUpDown2.Value = (decimal)ins.Position.X;
            numericUpDown3.Value = (decimal)ins.Position.Y;
            numericUpDown4.Value = (decimal)ins.Position.Z;
            numericUpDown5.Value = (decimal)ins.Position.W;
            textBox1.Text = "";
            tabControl1.Tag = (int)tabControl1.Tag | 0x01;
            numericUpDown13.Value = ins.Rot_X;
            numericUpDown14.Value = ins.Rot_Y;
            numericUpDown15.Value = ins.Rot_Z;
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
            if (slider)
                trackBar1.Value = ins.Rot_X;
            if (num)
                numericUpDown6.Value = ins.Rot_X;
            label6.Text = string.Format(angleFormat, ins.Rot_X / (float)(ushort.MaxValue + 1) * 360f);
            ignore_value_change = false;
        }

        private void GetYRot(bool slider, bool num)
        {
            ignore_value_change = true;
            if (slider)
                trackBar2.Value = ins.Rot_Y;
            if (num)
                numericUpDown7.Value = ins.Rot_Y;
            label7.Text = string.Format(angleFormat, ins.Rot_Y / (float)(ushort.MaxValue + 1) * 360f);
            ignore_value_change = false;
        }

        private void GetZRot(bool slider, bool num)
        {
            ignore_value_change = true;
            if (slider)
                trackBar3.Value = ins.Rot_Z;
            if (num)
                numericUpDown8.Value = ins.Rot_Z;
            label9.Text = string.Format(angleFormat, ins.Rot_Z / (float)(ushort.MaxValue + 1) * 360f);
            ignore_value_change = false;
        }

        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            ins.Name = controller.Data.ParticleTypes[comboBox1.SelectedIndex].Name;
            listBox1.Items[listBox1.SelectedIndex] = "#" + comboBox1.SelectedIndex + ": " + ins.Name;

            controller.UpdateText();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            
            controller.UpdateText();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            ins.Position.X = (float)numericUpDown2.Value;
            controller.UpdateText();
            File.RMViewer_LoadParticles();
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            ins.Position.Y = (float)numericUpDown3.Value;
            controller.UpdateText();
            File.RMViewer_LoadParticles();
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            ins.Position.Z = (float)numericUpDown4.Value;
            controller.UpdateText();
            File.RMViewer_LoadParticles();
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            ins.Position.W = (float)numericUpDown5.Value;
            controller.UpdateText();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            ins.Rot_X = (ushort)trackBar1.Value;
            GetXRot(false, true);
            controller.UpdateText();
            File.RMViewer_LoadParticles();
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            ins.Rot_Y = (ushort)trackBar2.Value;
            GetYRot(false, true);
            controller.UpdateText();
            File.RMViewer_LoadParticles();
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            ins.Rot_Z = (ushort)trackBar3.Value;
            GetZRot(false, true);
            controller.UpdateText();
            File.RMViewer_LoadParticles();
        }

        private void numericUpDown6_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            ins.Rot_X = (ushort)numericUpDown6.Value;
            GetXRot(true, false);
            controller.UpdateText();
            File.RMViewer_LoadParticles();
        }

        private void numericUpDown13_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            ins.Rot_X = (ushort)numericUpDown13.Value;
            controller.UpdateText();
            File.RMViewer_LoadParticles();
        }

        private void numericUpDown7_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            ins.Rot_Y = (ushort)numericUpDown7.Value;
            GetYRot(true, false);
            controller.UpdateText();
            File.RMViewer_LoadParticles();
        }

        private void numericUpDown14_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            ins.Rot_Y = (ushort)numericUpDown14.Value;
            controller.UpdateText();
            File.RMViewer_LoadParticles();
        }

        private void numericUpDown9_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            //ins.SomeNum1 = (int)numericUpDown9.Value;
            //((Controller)controller.Node.Nodes[controller.Data.RecordIDs[ins.ID]].Tag).UpdateText();
        }

        private void numericUpDown10_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            //ins.SomeNum2 = (int)numericUpDown10.Value;
            //((Controller)controller.Node.Nodes[controller.Data.RecordIDs[ins.ID]].Tag).UpdateText();
        }

        private void numericUpDown11_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            //ins.SomeNum3 = (int)numericUpDown11.Value;
            //((Controller)controller.Node.Nodes[controller.Data.RecordIDs[ins.ID]].Tag).UpdateText();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            /*
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
            if (ignore_value_change) return;
            /*
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
            if (ignore_value_change) return;
            /*
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
            if (ignore_value_change) return;
            /*
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
            if (ignore_value_change) return;
            /*
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
            if (ignore_value_change) return;
            /*
            ins.UnkI323.Clear();
            for (int i = 0; i < textBox5.Lines.Length; ++i)
            {
                if (uint.TryParse(textBox5.Lines[i], out uint v))
                    ins.UnkI323.Add(v);
            }
            ((Controller)controller.Node.Nodes[controller.Data.RecordIDs[ins.ID]].Tag).UpdateText();
            */
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (controller.Data.ParticleInstanceCount >= 254) return;
            uint id = controller.Data.ParticleInstanceCount;
            ParticleData.ParticleSystemInstance new_ins = new ParticleData.ParticleSystemInstance
            {
                Position = new Pos(0, 0, 0, 1),
                Rot_X = 0,
                Rot_Y = 0,
                Rot_Z = 0,
                Name = controller.Data.ParticleTypes[0].Name,
                EndZero = 0,
                UnkShorts = new ushort[12],
                UnkZero = 0
            };
            controller.Data.ParticleInstances.Add(new_ins);
            ins = new_ins;
            listBox1.Items.Add($"#{id}: {new_ins.Name}");
            controller.UpdateText();
            File.RMViewer_LoadParticles();
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sel_i = listBox1.SelectedIndex;
            if (sel_i == -1)
                return;
            controller.Data.ParticleInstances.RemoveAt(sel_i);
            listBox1.BeginUpdate();
            listBox1.Items.RemoveAt(sel_i);
            controller.Data.ParticleInstanceCount--;

            if (sel_i >= listBox1.Items.Count) sel_i = listBox1.Items.Count - 1;
            listBox1.SelectedIndex = sel_i;
            listBox1.EndUpdate();
            if (listBox1.Items.Count == 0)
                tabControl1.Enabled = false;
            controller.UpdateTextBox();
        }

        private void numericUpDown8_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            ins.Rot_Z = (ushort)numericUpDown8.Value;
            GetZRot(true, false);
            controller.UpdateText();
            File.RMViewer_LoadParticles();
        }

        private void numericUpDown15_ValueChanged(object sender, EventArgs e)
        {
            if (ignore_value_change) return;
            ins.Rot_Z = (ushort)numericUpDown15.Value;
            controller.UpdateText();
            File.RMViewer_LoadParticles();
        }

        private void button_PosFromCam_Click(object sender, EventArgs e)
        {
            Pos currentPos = File.RMViewer_GetPos(ins.Position);
            ins.Position.X = currentPos.X;
            ins.Position.Y = currentPos.Y;
            ins.Position.Z = currentPos.Z;
            numericUpDown2.Value = (decimal)ins.Position.X;
            numericUpDown3.Value = (decimal)ins.Position.Y;
            numericUpDown4.Value = (decimal)ins.Position.Z;
            numericUpDown5.Value = (decimal)ins.Position.W;
            controller.UpdateText();
            File.RMViewer_LoadParticles();
        }

        private void duplicateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sel_i = listBox1.SelectedIndex;
            if (sel_i == -1)
                return;

            ParticleData.ParticleSystemInstance last_inst = ins;

            if (controller.Data.ParticleInstanceCount >= 254) return;
            uint id = controller.Data.ParticleInstanceCount;
            ParticleData.ParticleSystemInstance new_ins = new ParticleData.ParticleSystemInstance
            {
                Position = new Pos(last_inst.Position.X,last_inst.Position.Y + 1f,last_inst.Position.Z, 1),
                Rot_X = last_inst.Rot_X,
                Rot_Y = last_inst.Rot_Y,
                Rot_Z = last_inst.Rot_Z,
                Name = last_inst.Name,
                EndZero = last_inst.EndZero,
                UnkShorts = last_inst.UnkShorts,
                UnkZero = last_inst.UnkZero
            };
            controller.Data.ParticleInstances.Add(new_ins);
            controller.Data.ParticleInstanceCount++;
            ins = new_ins;
            listBox1.Items.Add($"#{id}: {new_ins.Name}");
            controller.UpdateText();
            File.RMViewer_LoadParticles();
        }
    }
}
