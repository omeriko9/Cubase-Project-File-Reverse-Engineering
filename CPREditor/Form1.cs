using Parse;
using Parse.DataItems;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using ContextMenu = System.Windows.Forms.ContextMenu;
using TreeView = System.Windows.Forms.TreeView;

namespace CPREditor
{
    public partial class Form1 : Form
    {

        string SelectedFileName = "";

        WpfHexaEditor.HexEditor he = new WpfHexaEditor.HexEditor();
        System.Windows.Forms.ContextMenu tvCM = new ContextMenu();

        string RightClickedNode = "";

        CPR2 currentCPR = null;
        DataTable currentDT = null;

        public Form1()
        {
            InitializeComponent();

            elementHost1.Child = he;
            he.ZoomScale = 1.3;

            tvCM.MenuItems.Add("Copy Name", (x, y) => { Clipboard.SetText(RightClickedNode); });

        }

        void Parse()
        {
            this.Text = "CPR Editor " + SelectedFileName;

            ClearLoadedProject();

            currentCPR = new CPR2();
            currentCPR.Parse(File.ReadAllBytes(SelectedFileName));
            
            SetDataGrid(currentCPR.VSTMixer.SubSections);

            //currentDT = DataItem.ToDataTable(vstMixer.SubSections);
            //currentDT.Columns["Stereo Out"].SetOrdinal(currentDT.Columns.Count - 1);           

            //dataGridView1.DataSource = currentDT;         

            treeView1.ShowRootLines = trvSections.ShowRootLines = true;

            PopulateTreeview(trvSections, currentCPR.FoundSections);

            if (currentCPR.TrackList != null)
            {
                foreach (var t in currentCPR.TrackList.SubSections)
                {
                    lstTracks.Items.Add(t.ToString());
                }
            }

            toolStripMenuItem1.Enabled = true;
        }

        void SetDataGrid(List<DataItem> channels)
        {
            foreach (var c in channels)
            {
                dataGridView1.Columns.Add(new DataGridViewTextBoxColumn() { HeaderText = c.Name });
            }

            foreach (var effect in channels.First().SubSections)
            {
                var row = new DataGridViewRow();
                row.CreateCells(dataGridView1);
                dataGridView1.Rows.Add(row);
            }

            for (int i = 0; i < channels.Count; i++)
            {
                var channel = channels[i];
                
                dataGridView1.Columns[i].DataPropertyName = "Name";
                
                for (int j=0; j<channels.First().SubSections.Count; j++)
                {
                    dataGridView1[i, j].Value = channel.SubSections[j];
                }                
            }
        }

        void ClearLoadedProject()
        {
            treeView1.Nodes.Clear();
            trvSections.Nodes.Clear();
            lstTracks.Items.Clear();
            dataGridView1.DataSource = null;
            dataGridView1.AutoGenerateColumns = false;
        }


        void PopulateTreeview(TreeView tv, List<DataItem> items, TreeNode Parent = null)
        {
            foreach (var s in items)
            {
                TreeNode ch = new TreeNode(s.ToString());
                ch.Tag = s;
                PopulateTreeview(tv, s.SubSections, ch);

                if (Parent != null)
                {
                    Parent.Nodes.Add(ch);
                }
                else
                {
                    tv.Nodes.Add(ch);
                }
            }
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                RightClickedNode = e.Node.Text.Split('(')[0].Trim();
                tvCM.Show(trvSections, e.Location);
            }
        }



        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is DataItem)
            {
                var di = e.Node.Tag as DataItem;

                if (di.Data == null)
                    return;

                he.IsEnabled = false;
                he.Stream = new MemoryStream(di.Data);
                he.IsEnabled = true;

                SetLabels(di);


                listBox1.Items.Clear();
                foreach (var t in (di.DataStr.Split(new string[] { Environment.NewLine }, 0)))
                {
                    listBox1.Items.Add(t);
                }

            }

            treeView1.SelectedNode = e.Node;
        }

        void SetLabels(DataItem di)
        {
            lblCurrentDataItem.Text = di.Name;
            lblOffsetInFile.Text = $"0x{di.OffsetInFile.ToString("x4")}";
            lblItemSize.Text = di.Data?.Length > 0 ? $"0x{di.Data.Length.ToString("x4")}" : "0";

        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                var str = listBox1.SelectedItem.ToString();
                he.FindAll(str, true);
            }
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            var res = ofd.ShowDialog();
            if (res == DialogResult.OK)
            {
                SelectedFileName = ofd.FileName;
                Parse();
            }
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (currentCPR != null)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                var res = sfd.ShowDialog();
                if (res == DialogResult.OK)
                {



                    currentCPR.Save(sfd.FileName);
                }
            }
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value != null)
            {
                DataItem dt = e.Value as DataItem;

                if (dt != null)
                {
                    e.Value = dt.Name;
                    e.FormattingApplied = true;
                }
            }
        }

        private void dataGridView1_CellParsing(object sender, DataGridViewCellParsingEventArgs e)
        {
            var dt = currentCPR.VSTMixer.SubSections[e.ColumnIndex].SubSections[e.RowIndex] as DataItem;
            dt.Name = e.Value.ToString();

            e.Value = dt;
            e.ParsingApplied = true;
        }
    }
}
