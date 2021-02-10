﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Twinsanity;

namespace TwinsanityEditor.Utils
{
    //Because C# UInt16 and others has no fucing interface like IParseable i have to do this. Please don't kill me.
    //Toughest of choices require strongest of will.
    public class ListManipulatorUInt16
    {
        List<UInt16> list;
        ListBox listBox;
        TextBox source;
        bool update;
        public ListManipulatorUInt16(Button addBtn, Button delBtn, Button setBtn, Button upBtn, Button downBtn, ListBox listBox, TextBox source)
        {

            this.source = source;
            this.listBox = listBox;
            if (listBox != null) listBox.SelectedIndexChanged += UpdateSource;
            if (addBtn != null) addBtn.Click += Add;
            if (delBtn != null) delBtn.Click += Remove;
            if (setBtn != null) setBtn.Click += Set;
            if (upBtn != null) upBtn.Click += MoveUp;
            if (downBtn != null) downBtn.Click += MoveDown;

        }
        public void SetSource(List<UInt16> list)
        {
            this.list = list;
            update = true;
        }
        public void UpdateSource(Object sender, EventArgs args)
        {
            if (update)
            {
                source.Text = list[listBox.SelectedIndex].ToString();
            }
        }
        public void PopulateList()
        {
            listBox.BeginUpdate();
            listBox.Items.Clear();
            int i = 0;
            foreach (UInt16 e in list)
            {
                listBox.Items.Add(GenerateText(i, e));
                ++i;
            }
            listBox.EndUpdate();
        }
        private string GenerateText(int i, int e)
        {
            return $"{i:000}: {e}";
        }

        public void MoveUp(Object sender, EventArgs args)
        {
            if (listBox.SelectedIndex > 0)
            {
                int index = listBox.SelectedIndex;
                UInt16 val1 = list[index];
                UInt16 val2 = list[index - 1];
                list[index - 1] = val1;
                list[index] = val2;
                DisableUpdate();
                PopulateList();
                int top = listBox.Items.Count - 1;
                --index;
                listBox.SelectedIndex = Math.Min(Math.Max(index, 0), top);
                EnableUpdate();
            }
        }
        public void MoveDown(Object sender, EventArgs args)
        {
            if (listBox.SelectedIndex < listBox.Items.Count - 1)
            {
                int index = listBox.SelectedIndex;
                UInt16 val1 = list[index];
                UInt16 val2 = list[index + 1];
                list[index + 1] = val1;
                list[index] = val2;
                DisableUpdate();
                PopulateList();
                int top = listBox.Items.Count - 1;
                ++index;
                listBox.SelectedIndex = Math.Min(Math.Max(index, 0), top);
                EnableUpdate();
            }
        }
        public void Remove(Object sender, EventArgs args)
        {
            if (listBox.SelectedIndex >= 0)
            {
                int index = listBox.SelectedIndex;
                list.RemoveAt(index);
                DisableUpdate();
                PopulateList();
                int top = listBox.Items.Count - 1;
                listBox.SelectedIndex = Math.Min(Math.Max(index, 0), top);
                EnableUpdate();
            }
        }
        public void Add(Object sender, EventArgs args)
        {
            int index = listBox.SelectedIndex;
            if (index < 0)
            {
                index = 0;
            }
            UInt16 val;
            Boolean success;
            success = UInt16.TryParse(source.Text, out val);
            if (success)
            {
                list.Insert(index, val);
                DisableUpdate();
                PopulateList();
                listBox.SelectedIndex = index;
                EnableUpdate();
            }

        }
        public void Set(Object sender, EventArgs args)
        {
            if (listBox.SelectedIndex >= 0)
            {
                UInt16 val;
                Boolean success;
                success = UInt16.TryParse(source.Text, out val);
                if (success)
                {
                    list[listBox.SelectedIndex] = val;
                    int index = listBox.SelectedIndex;
                    DisableUpdate();
                    listBox.Items[listBox.SelectedIndex] = GenerateText(index, val);
                    listBox.SelectedIndex = index;
                    EnableUpdate();
                }
            }
        }
        private void DisableUpdate()
        {
            update = false;
        }
        private void EnableUpdate()
        {
            update = true;
        }
    }

    public class ListManipulatorUInt32
    {
        List<UInt32> list;
        ListBox listBox;
        TextBox source;
        bool update;
        public ListManipulatorUInt32(Button addBtn, Button delBtn, Button setBtn, Button upBtn, Button downBtn, ListBox listBox, TextBox source)
        {

            this.source = source;
            this.listBox = listBox;
            if (listBox != null) listBox.SelectedIndexChanged += UpdateSource;
            if (addBtn != null) addBtn.Click += Add;
            if (delBtn != null) delBtn.Click += Remove;
            if (setBtn != null) setBtn.Click += Set;
            if (upBtn != null) upBtn.Click += MoveUp;
            if (downBtn != null) downBtn.Click += MoveDown;

        }
        public void SetSource(List<UInt32> list)
        {
            this.list = list;
            update = true;
        }
        public void UpdateSource(Object sender, EventArgs args)
        {
            if (update)
            {
                source.Text = list[listBox.SelectedIndex].ToString();
            }
        }
        public void PopulateList()
        {
            listBox.BeginUpdate();
            listBox.Items.Clear();
            int i = 0;
            foreach (UInt32 e in list)
            {
                listBox.Items.Add(GenerateText(i, e));
                ++i;
            }
            listBox.EndUpdate();
        }
        private string GenerateText(int i, uint e)
        {
            return $"{i:000}: {e}";
        }

        public void MoveUp(Object sender, EventArgs args)
        {
            if (listBox.SelectedIndex > 0)
            {
                int index = listBox.SelectedIndex;
                UInt32 val1 = list[index];
                UInt32 val2 = list[index - 1];
                list[index - 1] = val1;
                list[index] = val2;
                DisableUpdate();
                listBox.Items[index] = list[index].ToString();
                listBox.Items[index - 1] = list[index - 1].ToString();
                int top = listBox.Items.Count - 1;
                --index;
                listBox.SelectedIndex = Math.Min(Math.Max(index, 0), top);
                EnableUpdate();
            }
        }
        public void MoveDown(Object sender, EventArgs args)
        {
            if (listBox.SelectedIndex < listBox.Items.Count - 1)
            {
                int index = listBox.SelectedIndex;
                UInt32 val1 = list[index];
                UInt32 val2 = list[index + 1];
                list[index + 1] = val1;
                list[index] = val2;
                DisableUpdate();
                listBox.Items[index] = list[index].ToString();
                listBox.Items[index + 1] = list[index + 1].ToString();
                int top = listBox.Items.Count - 1;
                ++index;
                listBox.SelectedIndex = Math.Min(Math.Max(index, 0), top);
                EnableUpdate();
            }
        }
        public void Remove(Object sender, EventArgs args)
        {
            if (listBox.SelectedIndex >= 0)
            {
                int index = listBox.SelectedIndex;
                list.RemoveAt(index);
                DisableUpdate();
                listBox.Items.RemoveAt(index);
                int top = listBox.Items.Count - 1;
                listBox.SelectedIndex = Math.Min(Math.Max(index, 0), top);
                EnableUpdate();
            }
        }
        public void Add(Object sender, EventArgs args)
        {
            int index = listBox.SelectedIndex;
            if (index < 0)
            {
                index = 0;
            }
            UInt32 val;
            Boolean success;
            success = UInt32.TryParse(source.Text, out val);
            if (success)
            {
                list.Insert(index, val);
                DisableUpdate();
                listBox.Items.Insert(index, source.Text);
                listBox.SelectedIndex = index;
                EnableUpdate();
            }

        }
        public void Set(Object sender, EventArgs args)
        {
            if (listBox.SelectedIndex >= 0)
            {
                UInt32 val;
                Boolean success;
                success = UInt32.TryParse(source.Text, out val);
                if (success)
                {
                    list[listBox.SelectedIndex] = val;
                    int index = listBox.SelectedIndex;
                    DisableUpdate();
                    listBox.Items[listBox.SelectedIndex] = GenerateText(index, val);
                    listBox.SelectedIndex = index;
                    EnableUpdate();
                }
            }
        }
        private void DisableUpdate()
        {
            update = false;
        }
        private void EnableUpdate()
        {
            update = true;
        }
    }
    public class ListManipulatorSingle
    {
        List<Single> list;
        ListBox listBox;
        TextBox source;
        bool update;
        public ListManipulatorSingle(Button addBtn, Button delBtn, Button setBtn, Button upBtn, Button downBtn, ListBox listBox, TextBox source)
        {

            this.source = source;
            this.listBox = listBox;
            if (listBox != null) listBox.SelectedIndexChanged += UpdateSource;
            if (addBtn != null) addBtn.Click += Add;
            if (delBtn != null) delBtn.Click += Remove;
            if (setBtn != null) setBtn.Click += Set;
            if (upBtn != null) upBtn.Click += MoveUp;
            if (downBtn != null) downBtn.Click += MoveDown;

        }
        public void SetSource(List<Single> list)
        {
            this.list = list;
            update = true;
        }
        public void UpdateSource(Object sender, EventArgs args)
        {
            if (update)
            {
                source.Text = list[listBox.SelectedIndex].ToString();
            }
        }
        public void PopulateList()
        {
            listBox.BeginUpdate();
            listBox.Items.Clear();
            foreach (Single e in list)
            {
                listBox.Items.Add(e.ToString());
            }
            listBox.EndUpdate();
        }

        public void MoveUp(Object sender, EventArgs args)
        {
            if (listBox.SelectedIndex > 0)
            {
                int index = listBox.SelectedIndex;
                Single val1 = list[index];
                Single val2 = list[index - 1];
                list[index - 1] = val1;
                list[index] = val2;
                DisableUpdate();
                listBox.Items[index] = list[index].ToString();
                listBox.Items[index - 1] = list[index - 1].ToString();
                int top = listBox.Items.Count - 1;
                --index;
                listBox.SelectedIndex = Math.Min(Math.Max(index, 0), top);
                EnableUpdate();
            }
        }
        public void MoveDown(Object sender, EventArgs args)
        {
            if (listBox.SelectedIndex < listBox.Items.Count - 1)
            {
                int index = listBox.SelectedIndex;
                Single val1 = list[index];
                Single val2 = list[index + 1];
                list[index + 1] = val1;
                list[index] = val2;
                DisableUpdate();
                listBox.Items[index] = list[index].ToString();
                listBox.Items[index + 1] = list[index + 1].ToString();
                int top = listBox.Items.Count - 1;
                ++index;
                listBox.SelectedIndex = Math.Min(Math.Max(index, 0), top);
                EnableUpdate();
            }
        }
        public void Remove(Object sender, EventArgs args)
        {
            if (listBox.SelectedIndex >= 0)
            {
                int index = listBox.SelectedIndex;
                list.RemoveAt(index);
                DisableUpdate();
                listBox.Items.RemoveAt(index);
                int top = listBox.Items.Count - 1;
                listBox.SelectedIndex = Math.Min(Math.Max(index, 0), top);
                EnableUpdate();
            }
        }
        public void Add(Object sender, EventArgs args)
        {
            int index = listBox.SelectedIndex;
            if (index < 0)
            {
                index = 0;
            }
            Single val;
            Boolean success;
            success = Single.TryParse(source.Text, out val);
            if (success)
            {
                list.Insert(index, val);
                DisableUpdate();
                listBox.Items.Insert(index, source.Text);
                listBox.SelectedIndex = index;
                EnableUpdate();
            }

        }
        public void Set(Object sender, EventArgs args)
        {
            if (listBox.SelectedIndex >= 0)
            {
                Single val;
                Boolean success;
                success = Single.TryParse(source.Text, out val);
                if (success)
                {
                    list[listBox.SelectedIndex] = val;
                    int index = listBox.SelectedIndex;
                    DisableUpdate();
                    listBox.Items[listBox.SelectedIndex] = source.Text;
                    listBox.SelectedIndex = index;
                    EnableUpdate();
                }
            }
        }
        private void DisableUpdate()
        {
            update = false;
        }
        private void EnableUpdate()
        {
            update = true;
        }
    }
    public class ListManipulatorByte
    {
        List<Byte> list;
        ListBox listBox;
        TextBox source;
        bool update;
        public ListManipulatorByte(Button addBtn, Button delBtn, Button setBtn, Button upBtn, Button downBtn, ListBox listBox, TextBox source)
        {

            this.source = source;
            this.listBox = listBox;
            if (listBox != null) listBox.SelectedIndexChanged += UpdateSource;
            if (addBtn != null) addBtn.Click += Add;
            if (delBtn != null) delBtn.Click += Remove;
            if (setBtn != null) setBtn.Click += Set;
            if (upBtn != null) upBtn.Click += MoveUp;
            if (downBtn != null) downBtn.Click += MoveDown;

        }
        public void SetSource(List<Byte> list)
        {
            this.list = list;
            update = true;
        }
        public void UpdateSource(Object sender, EventArgs args)
        {
            if (update)
            {
                source.Text = list[listBox.SelectedIndex].ToString();
            }
        }
        public void PopulateList()
        {
            listBox.BeginUpdate();
            listBox.Items.Clear();
            foreach (Single e in list)
            {
                listBox.Items.Add(e.ToString());
            }
            listBox.EndUpdate();
        }

        public void MoveUp(Object sender, EventArgs args)
        {
            if (listBox.SelectedIndex > 0)
            {
                int index = listBox.SelectedIndex;
                Byte val1 = list[index];
                Byte val2 = list[index - 1];
                list[index - 1] = val1;
                list[index] = val2;
                DisableUpdate();
                listBox.Items[index] = list[index].ToString();
                listBox.Items[index - 1] = list[index - 1].ToString();
                int top = listBox.Items.Count - 1;
                --index;
                listBox.SelectedIndex = Math.Min(Math.Max(index, 0), top);
                EnableUpdate();
            }
        }
        public void MoveDown(Object sender, EventArgs args)
        {
            if (listBox.SelectedIndex < listBox.Items.Count - 1)
            {
                int index = listBox.SelectedIndex;
                Byte val1 = list[index];
                Byte val2 = list[index + 1];
                list[index + 1] = val1;
                list[index] = val2;
                DisableUpdate();
                listBox.Items[index] = list[index].ToString();
                listBox.Items[index + 1] = list[index + 1].ToString();
                int top = listBox.Items.Count - 1;
                ++index;
                listBox.SelectedIndex = Math.Min(Math.Max(index, 0), top);
                EnableUpdate();
            }
        }
        public void Remove(Object sender, EventArgs args)
        {
            if (listBox.SelectedIndex >= 0)
            {
                int index = listBox.SelectedIndex;
                list.RemoveAt(index);
                DisableUpdate();
                listBox.Items.RemoveAt(index);
                int top = listBox.Items.Count - 1;
                listBox.SelectedIndex = Math.Min(Math.Max(index, 0), top);
                EnableUpdate();
            }
        }
        public void Add(Object sender, EventArgs args)
        {
            int index = listBox.SelectedIndex;
            if (index < 0)
            {
                index = 0;
            }
            Byte val;
            Boolean success;
            success = Byte.TryParse(source.Text, out val);
            if (success)
            {
                list.Insert(index, val);
                DisableUpdate();
                listBox.Items.Insert(index, source.Text);
                listBox.SelectedIndex = index;
                EnableUpdate();
            }

        }
        public void Set(Object sender, EventArgs args)
        {
            if (listBox.SelectedIndex >= 0)
            {
                Byte val;
                Boolean success;
                success = Byte.TryParse(source.Text, out val);
                if (success)
                {
                    list[listBox.SelectedIndex] = val;
                    int index = listBox.SelectedIndex;
                    DisableUpdate();
                    listBox.Items[listBox.SelectedIndex] = source.Text;
                    listBox.SelectedIndex = index;
                    EnableUpdate();
                }
            }
        }
        private void DisableUpdate()
        {
            update = false;
        }
        private void EnableUpdate()
        {
            update = true;
        }
    }


    public class ListManipulatorEvent
    {
        List<UInt32> list;
        ListBox listBox;
        TextBox source;
        TextBox source_caller;
        TextBox source_script;
        TextBox source_argument;
        bool update;
        public ListManipulatorEvent(Button addBtn, Button delBtn, Button setBtn, Button upBtn, Button downBtn, ListBox listBox, TextBox source, TextBox caller, TextBox script, TextBox argument)
        {

            this.source = source;
            this.listBox = listBox;
            this.source_caller = caller;
            this.source_script = script;
            this.source_argument = argument;

            if (listBox != null) listBox.SelectedIndexChanged += UpdateSource;
            if (addBtn != null) addBtn.Click += Add;
            if (delBtn != null) delBtn.Click += Remove;
            if (setBtn != null) setBtn.Click += Set;
            if (upBtn != null) upBtn.Click += MoveUp;
            if (downBtn != null) downBtn.Click += MoveDown;

        }
        public void SetSource(List<UInt32> list)
        {
            this.list = list;
            update = true;
        }
        public void UpdateSource(Object sender, EventArgs args)
        {
            if (update)
            {
                uint val = list[listBox.SelectedIndex];
                source.Text = val.ToString();
                ushort script = (ushort)((val >> 0xA) & 0x3FFF);
                ushort arg = (ushort)(val & 0x3FF);
                ushort caller = (ushort)((val >> 0x18 & 0x1));
                source_caller.Text = caller.ToString();
                source_script.Text = script.ToString();
                source_argument.Text = arg.ToString();
            }
        }
        public void PopulateList()
        {
            listBox.BeginUpdate();
            listBox.Items.Clear();
            int i = 0;
            foreach (UInt32 e in list)
            {
                listBox.Items.Add(GenerateText(i));
                ++i;
            }
            listBox.EndUpdate();
        }
        private string GenerateText(int i)
        {
            uint val = list[i];
            ushort script = (ushort)((val >> 0xA) & 0x3FFF);
            ushort arg = (ushort)(val & 0x3FF);
            ushort caller = (ushort)((val >> 0x18 & 0x1));
            string scrTxt = script.ToString();

            if (Enum.IsDefined(typeof(DefaultEnums.ScriptID), script))
            {
                scrTxt += " " + (DefaultEnums.ScriptID)script;
            }

            return $"{i:000}: Arg {arg}: Script {scrTxt} (Caller {caller})";
        }

        public void MoveUp(Object sender, EventArgs args)
        {
            if (listBox.SelectedIndex > 0)
            {
                int index = listBox.SelectedIndex;
                UInt32 val1 = list[index];
                UInt32 val2 = list[index - 1];
                list[index - 1] = val1;
                list[index] = val2;
                DisableUpdate();
                listBox.Items[index] = GenerateText(index);
                listBox.Items[index - 1] = GenerateText(index - 1);
                int top = listBox.Items.Count - 1;
                --index;
                listBox.SelectedIndex = Math.Min(Math.Max(index, 0), top);
                EnableUpdate();
            }
        }
        public void MoveDown(Object sender, EventArgs args)
        {
            if (listBox.SelectedIndex < listBox.Items.Count - 1)
            {
                int index = listBox.SelectedIndex;
                UInt32 val1 = list[index];
                UInt32 val2 = list[index + 1];
                list[index + 1] = val1;
                list[index] = val2;
                DisableUpdate();
                listBox.Items[index] = GenerateText(index);
                listBox.Items[index + 1] = GenerateText(index + 1);
                int top = listBox.Items.Count - 1;
                ++index;
                listBox.SelectedIndex = Math.Min(Math.Max(index, 0), top);
                EnableUpdate();
            }
        }
        public void Remove(Object sender, EventArgs args)
        {
            if (listBox.SelectedIndex >= 0)
            {
                int index = listBox.SelectedIndex;
                list.RemoveAt(index);
                DisableUpdate();
                listBox.Items.RemoveAt(index);
                int top = listBox.Items.Count - 1;
                listBox.SelectedIndex = Math.Min(Math.Max(index, 0), top);
                EnableUpdate();
            }
        }
        public void Add(Object sender, EventArgs args)
        {
            int index = listBox.SelectedIndex;
            if (index < 0)
            {
                index = 0;
            }
            UInt32 val;
            Boolean success;
            success = UInt32.TryParse(source.Text, out val);
            if (success)
            {
                list.Insert(index, val);
                DisableUpdate();
                listBox.Items.Insert(index, GenerateText(index));
                listBox.SelectedIndex = index;
                EnableUpdate();
            }

        }
        public void Set(Object sender, EventArgs args)
        {
            if (listBox.SelectedIndex >= 0)
            {
                UInt32 val;
                ushort scr;
                ushort arg;
                ushort caller;
                Boolean success;
                success = UInt32.TryParse(source.Text, out val);
                success = UInt16.TryParse(source_script.Text, out scr);
                success = UInt16.TryParse(source_argument.Text, out arg);
                success = UInt16.TryParse(source_caller.Text, out caller);
                if (success)
                {
                    for (int i = 0; i < 0xA + 0xE + 0x1; i++)
                    {
                         val &= ~(uint)(1 << i);
                    }
                    val |= ((uint)arg & 0x3FF);
                    val |= (((uint)scr << 0xA));

                    uint mask = 1 << 0x18;
                    if (caller != 0)
                        val |= mask;

                    list[listBox.SelectedIndex] = val;
                    int index = listBox.SelectedIndex;
                    DisableUpdate();

                    source.Text = val.ToString();

                    listBox.Items[listBox.SelectedIndex] = GenerateText(index);
                    listBox.SelectedIndex = index;
                    EnableUpdate();
                }
            }
        }
        private void DisableUpdate()
        {
            update = false;
        }
        private void EnableUpdate()
        {
            update = true;
        }
    }
}
