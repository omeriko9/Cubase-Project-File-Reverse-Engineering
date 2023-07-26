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
        string n2 = @"C:\Temp\playground\Play3\Original Chill\chill-01.cpr";
        string n3 = @"C:\Temp\playground\Play3\chill-01.cpr";
        ByteViewer bv = new ByteViewer();
        string SelectedFileName = "";
        WpfHexaEditor.HexEditor he = new WpfHexaEditor.HexEditor();
        System.Windows.Forms.ContextMenu tvCM = new ContextMenu();
        string RightClickedNode = "";
        CPR2 current = null;

        public Form1()
        {
            InitializeComponent();

            bv.Dock = DockStyle.Fill;
            bv.Font = new Font("Consolas", 20);
            bv.SetDisplayMode(DisplayMode.Hexdump);
            elementHost1.Child = he;
            he.ZoomScale = 1.3;

            tvCM.MenuItems.Add("Copy", (x, y) => { Clipboard.SetText(RightClickedNode); });

        }

        private void button1_Click(object sender, EventArgs e)
        {
            SelectedFileName = n2;
            Parse();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SelectedFileName = n3;
            Parse();
        }

        void Parse()
        {
            this.Text = "CPR Editor " + SelectedFileName;

            treeView1.Nodes.Clear();
            trvSections.Nodes.Clear();
            lstTracks.Items.Clear();

            current = new CPR2();
            current.Parse(File.ReadAllBytes(SelectedFileName));

            dataGridView1.DataSource = null;
            dataGridView1.AutoGenerateColumns = true;

            var vstMixer = current.GetSection(DataItemFactory.sVSTMixer); 

            var mixer = DataItem.ToDataTable(vstMixer.SubSections);
            mixer.Columns["Stereo Out"].SetOrdinal(mixer.Columns.Count - 1);           

            dataGridView1.DataSource = mixer;

            treeView1.ShowRootLines = trvSections.ShowRootLines = true;

            PopulateTreeview(trvSections, current.FoundSections);

            var trackList = current.GetSection(DataItemFactory.sMTrackList);
            if (trackList != null)
            {
                foreach (var t in trackList.SubSections)
                {
                    lstTracks.Items.Add(t.ToString());
                }
            }

            toolStripMenuItem1.Enabled = true;
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
            lblItemSize.Text = $"0x{di.SectionSize.ToString("x4")}";

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
            if (current != null)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                var res = sfd.ShowDialog(); 
                if (res == DialogResult.OK)
                {
                    current.Save(sfd.FileName);
                }
            }
        }
    }
}
