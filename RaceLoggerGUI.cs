using System;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using ComponentFactory.Krypton.Toolkit;
using Phidgets.Events;
using System.Data;
using System.IO;

namespace RaceLogger
{
    public partial class RaceLoggerGUI : KryptonForm
    {
        private RaceLoggerDO m_dataObject;
        RFIDReader m_rfidReader1 = null;
        RFIDReader m_rfidReader2 = null;

        public RFIDReader RFIDReader1
        {
            get
            {
                return m_rfidReader1;
            }
            set
            {
                m_rfidReader1 = value;
            }
        }

        public RFIDReader RFIDReader2
        {
            get
            {
                return m_rfidReader2;
            }
            set
            {
                m_rfidReader2 = value;
            }
        }

        public RaceLoggerGUI(RaceLoggerDO dataObject)
        {
            m_dataObject = dataObject;
            InitializeComponent();
            
        }

        public void RFID1ButtonOn(int serial)
        {
            toolStripRFIDReader1.Checked = true;
            toolStripRFIDReader1.Image = Properties.Resources.green_box;
            toolStripRFIDReader1.ToolTipText = toolStripRFIDReader1.Text + " On: " + serial.ToString();
        }

        public void RFID2ButtonOn(int serial)
        {
            toolStripRFIDReader2.Checked = true;
            toolStripRFIDReader2.Image = Properties.Resources.green_box;
            toolStripRFIDReader2.ToolTipText = toolStripRFIDReader2.Text + " On: " + serial.ToString();
        }

        public void RFID1ButtonOff()
        {
            toolStripRFIDReader1.Checked = false;
            toolStripRFIDReader1.Image = Properties.Resources.red_box;
            toolStripRFIDReader1.ToolTipText = toolStripRFIDReader1.Text + " Off";
        }

        public void RFID2ButtonOff()
        {
            toolStripRFIDReader2.Checked = false;
            toolStripRFIDReader2.Image = Properties.Resources.red_box;
            toolStripRFIDReader2.ToolTipText = toolStripRFIDReader2.Text + " Off";
        }

        public void DisplayInReadingLabel(string text)
        {
            kryptonReadingLabel.Values.Text = text;
        }

        #region Private Members

        private void RaceLoggerGUI_Load(object sender, EventArgs e)
        {
            // Use the filtered view of the data table
            kryptonDataGridView.DataMember = string.Empty;
            kryptonDataGridView.DataSource = m_dataObject.DataTable;

            // Expand all the nodes to show entire tree structure
            foreach(TreeNode n in treeView.Nodes)
                n.ExpandAll();

            // Hook into the up and down buttons on the details heading
            kryptonHeaderGroupDetails.ButtonSpecs[0].Click += new EventHandler(OnPrevious);
            kryptonHeaderGroupDetails.ButtonSpecs[1].Click += new EventHandler(OnNext);

            // Set column colors: Info columns one color, Time data columns another
            for(int x = 0; x < 8; x++ )
            {
                kryptonDataGridView.Columns[x].HeaderCell.Style.BackColor = Color.Gray;
                kryptonDataGridView.Columns[x].DefaultCellStyle.BackColor = Color.LightGray;
            }
            for (int x = 8; x < 13; x++)
            {
                kryptonDataGridView.Columns[x].HeaderCell.Style.BackColor = Color.Yellow;
                kryptonDataGridView.Columns[x].DefaultCellStyle.BackColor = Color.LightYellow;
            }

            // Select parent node of tree
            treeView.SelectedNode = treeView.Nodes[0];
        }

        private void RaceLoggerGUI_SystemColorsChanged(object sender, EventArgs e)
        {
            // If the system colors change that might change the palette background
            // if the palette is calculating it from the system colors and so update
            // the control colors just in case.
            UpdateOnPaletteChanged();
        }

        private void toolStripReadingPane_CheckedChanged(object sender, EventArgs e)
        {
            // Show/Hide the reading pane depending on new setting
            kryptonSplitContainerDetails.Panel2Collapsed = toolStripReadingPane.Checked;
            readingPaneToolStripMenuItem.Checked = toolStripReadingPane.Checked;
        }

        private void readingPaneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripReadingPane.Checked = !toolStripReadingPane.Checked;
        }

        private void toolStripPosition_CheckedChanged(object sender, EventArgs e)
        {
            // Vertical/Horizontal alignment depending on new setting
            kryptonSplitContainerDetails.Orientation = (toolStripPosition.Checked ? Orientation.Vertical : Orientation.Horizontal);
            panePositonToolStripMenuItem.Checked = toolStripPosition.Checked;
        }

        private void panePositonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripPosition.Checked = !toolStripPosition.Checked;
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node != null)
            {
                // Update the details header with selected node values
                kryptonHeaderGroupDetails.ValuesPrimary.Heading = e.Node.Text;
                //kryptonHeaderGroupDetails.ValuesPrimary.Image = imageList.Images[e.Node.ImageIndex];

                // Change list of displayed items based on the new tree selection
                FilterDataTable(e.Node);
            }
            else
            {
                // Should never happen, but just in case!
                kryptonHeaderGroupDetails.ValuesPrimary.Heading = "Details";
                kryptonHeaderGroupDetails.ValuesPrimary.Image = null;
            }
        }

        private void kryptonDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (kryptonDataGridView.SelectedRows.Count == 1)
            {
                //string details = (string)kryptonDataGridView.SelectedRows[0].Cells[2].Value;
                //kryptonReadingLabel.Values.Text = details;
            }
            else
            {
                //kryptonReadingLabel.Values.Text = string.Empty;
            }
        }

        private void OnNext(object sender, EventArgs e)
        {
            // If nothing is selected
            if (kryptonDataGridView.SelectedRows.Count == 0)
            {
                // If there are rows in the grid
                if (kryptonDataGridView.Rows.Count > 0)
                {
                    // Select the first row
                    kryptonDataGridView.Rows[0].Selected = true;
                }
            }
            else
            {
                // Find index of next row
                int index = kryptonDataGridView.SelectedRows[0].Index + 1;

                // If past end of list then go back to the start
                if (index >= kryptonDataGridView.Rows.Count)
                    index = 0;

                // Select the row
                kryptonDataGridView.Rows[index].Selected = true;
            }

            kryptonDataGridView.Refresh();
        }

        private void OnPrevious(object sender, EventArgs e)
        {
            // If nothing is selected
            if (kryptonDataGridView.SelectedRows.Count == 0)
            {
                // If there are rows in the grid
                if (kryptonDataGridView.Rows.Count > 0)
                {
                    // Select the last row
                    kryptonDataGridView.Rows[kryptonDataGridView.Rows.Count - 1].Selected = true;
                }
            }
            else
            {
                // Find index of previous row
                int index = kryptonDataGridView.SelectedRows[0].Index - 1;

                // If past start of list then go back to the end
                if (index < 0)
                    index = kryptonDataGridView.Rows.Count - 1;

                // Select the row
                kryptonDataGridView.Rows[index].Selected = true;
            }

            kryptonDataGridView.Refresh();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FilterDataTable(TreeNode node)
        {
            string nodeTag = node.Tag as string;

            // Check if node has a tag
            if (!string.IsNullOrEmpty(nodeTag))
            {
                // Check if node is a top-level node
                if (node.Level == 0)
                {
                    switch (nodeTag)
                    {
                        case RaceType.FiveK:
                        case RaceType.AquaRun:
                        case RaceType.AquaRunTeam:
                            m_dataObject.DataTable.DefaultView.RowFilter = DataFields.RaceType + " = '" + nodeTag + "'";
                            break;
                        default:
                            m_dataObject.DataTable.DefaultView.RowFilter = "";
                            break;
                    }
                }
                // Check if node is a 2nd-level node
                else if (node.Level == 1)
                {
                    switch (nodeTag)
                    {
                        case RaceType.FiveK:
                        case RaceType.AquaRun:
                        case RaceType.AquaRunTeam:
                            m_dataObject.DataTable.DefaultView.RowFilter = DataFields.RaceType + " = '" + nodeTag + "'";
                            break;
                        case BestTimeType.BestTime5k:
                            m_dataObject.DataTable.DefaultView.RowFilter = DataFields.RaceType + " = '" + RaceType.FiveK + "'";
                            m_dataObject.DataTable.DefaultView.Sort = DataFields.RunDuration;
                            break;
                        case BestTimeType.BestTimeAquaRun:
                            m_dataObject.DataTable.DefaultView.RowFilter = DataFields.RaceType + " = '" + RaceType.AquaRun + "'";
                            m_dataObject.DataTable.DefaultView.Sort = DataFields.TotalDuration;
                            break;
                        case BestTimeType.BestTimeAquaRunTeam:
                            m_dataObject.DataTable.DefaultView.RowFilter = DataFields.RaceType + " = '" + RaceType.AquaRunTeam + "'";
                            m_dataObject.DataTable.DefaultView.Sort = DataFields.TotalDuration;
                            break;
                        case HeatGroup.Heat1:
                        case HeatGroup.Heat2:
                        case HeatGroup.Heat3:
                            m_dataObject.DataTable.DefaultView.RowFilter = DataFields.Heat + " = '" + nodeTag + "'";
                            break;
                        default:
                            m_dataObject.DataTable.DefaultView.RowFilter = "";
                            break;
                    }
                }
                // Check if node is a 3rd-level node
                else if (node.Level == 2)
                {
                    switch (nodeTag)
                    {
                        case AgeGroup.Group1:
                        case AgeGroup.Group2:
                        case AgeGroup.Group3:
                        case AgeGroup.Group4:
                        case AgeGroup.Group5:
                        case AgeGroup.Group6:
                        case AgeGroup.Group7:
                        case AgeGroup.Group8:
                            m_dataObject.DataTable.DefaultView.RowFilter = DataFields.AgeGroup + " = '" + nodeTag + 
                                "' AND " + DataFields.RaceType + " = '" + (string)node.Parent.Tag + "'";
                            break;
                        default:
                            m_dataObject.DataTable.DefaultView.RowFilter = "";
                            break;
                    }
                }

                //show number of filtered entries
                kryptonHeaderGroupNavigation.ValuesSecondary.Heading = m_dataObject.DataTable.DefaultView.Count + " participants";
            }
        }

        private void UpdateOnPaletteChanged()
        {
            // Get the new control background color
            Color backColor = kryptonManager.GlobalPalette.GetBackColor1(PaletteBackStyle.ControlClient,
                                                                         PaletteState.Normal);

            // Update the tree and listview controls with new color
            treeView.BackColor = backColor;
        }

        private void toolStripRFIDReader1_Click(object sender, EventArgs e)
        {
            if (toolStripRFIDReader1.Checked)
            {
                m_rfidReader1.Initialize();
            }
            else
            {
                m_rfidReader1.Shutdown();
                RFID1ButtonOff();
            }
        }

        private void toolStripRFIDReader2_Click(object sender, EventArgs e)
        {
            if (toolStripRFIDReader2.Checked)
            {
                m_rfidReader2.Initialize();
            }
            else
            {
                m_rfidReader2.Shutdown();
                RFID2ButtonOff();
            }
        }

        #endregion Private Members

        private void kryptonDataGridView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            m_dataObject.DataTable.DefaultView[e.RowIndex].BeginEdit();
        }

        private void kryptonDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            m_dataObject.DataTable.DefaultView[e.RowIndex].EndEdit();
            m_dataObject.SaveToXML();
        }

        private void toolStripLogStart_Click(object sender, EventArgs e)
        {
            if (toolStripLogStart.Checked == true)
            {
                toolStripLogStop.Checked = false;
                m_dataObject.LogMode = LogModeType.LogStartTime;
            }
        }

        private void toolStripLogStop_Click(object sender, EventArgs e)
        {
            if (toolStripLogStop.Checked == true)
            {
                toolStripLogStart.Checked = false;
                m_dataObject.LogMode = LogModeType.LogEndTime;
            }
        }

        private void clearStartTimesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.DialogResult response = System.Windows.Forms.MessageBox.Show("Are you sure you want to clear ALL start times? There is NO undo!", "Warning!", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (response == System.Windows.Forms.DialogResult.OK)
            {
                m_dataObject.ClearStartTimes();
            }
        }

        private void clearAllEndTimesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.DialogResult response = System.Windows.Forms.MessageBox.Show("Are you sure you want to clear ALL end times? There is NO undo!", "Warning!", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (response == System.Windows.Forms.DialogResult.OK)
            {
                m_dataObject.ClearEndTimes();
            }
        }

        private void ImportFile(string fileName)
        {
            DataTable importedTable = m_dataObject.ParseCSVFile(fileName);

            //Overwrite current table with imported table
            m_dataObject.ReplaceDataTable(importedTable);

            // bind the refreshed DataTable to kryptonDataGridView
            //kryptonDataGridView.DataSource = m_dataObject.DataTable;
        }

        private void ExportFile()
        {
            SaveFileDialog DialogSave = new SaveFileDialog();
            DialogSave.DefaultExt = "csv";

            // Available file extensions
            DialogSave.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";

            // Adds a extension if the user does not
            DialogSave.AddExtension = true;

            // Restores the selected directory, next time
            DialogSave.RestoreDirectory = true;

            // Dialog title
            DialogSave.Title = "Export Data";

            // Startup directory
            DialogSave.InitialDirectory = @"C:/";

            // Show the dialog and process the result
            if (DialogSave.ShowDialog() == DialogResult.OK)
            {
                m_dataObject.ExportToCSV(m_dataObject.DataTable,DialogSave.FileName,",");
            }

            DialogSave.Dispose();
            DialogSave = null;
        }

        private void ImportStartTimes(string fileName)
        {
            DataTable importedTable = m_dataObject.ParseCSVFile(fileName);

            m_dataObject.ImportStartTimes(importedTable);
        }

        private void ImportEndTimes(string fileName)
        {
            DataTable importedTable = m_dataObject.ParseCSVFile(fileName);

            m_dataObject.ImportEndTimes(importedTable);
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            string fileName = null;

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter =
               "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            dialog.Title = "Select a CSV file";
            dialog.RestoreDirectory = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if (dialog.FileName != null)
                    {
                        fileName = dialog.FileName;
                        ImportFile(fileName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void menuItemImport_Click(object sender, EventArgs e)
        {
            string fileName = null;

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter =
               "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            dialog.Title = "Select a CSV file";
            dialog.RestoreDirectory = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if (dialog.FileName != null)
                    {
                        fileName = dialog.FileName;
                        ImportFile(fileName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            ExportFile();
        }

        private void menuItemExport_Click(object sender, EventArgs e)
        {
            ExportFile();
        }

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            m_dataObject.CalculateRunDuration();
        }

        private void btnCalculateTotal_Click(object sender, EventArgs e)
        {
            m_dataObject.CalculateTotalDuration();
        }

        private void clearRaceDurationTimesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.DialogResult response = System.Windows.Forms.MessageBox.Show("Are you sure you want to clear ALL duration times? There is NO undo!", "Warning!", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (response == System.Windows.Forms.DialogResult.OK)
            {
                m_dataObject.ClearDurationTimes();
            }
        }

        private void setSwimStartTimeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_dataObject.SetCurrentViewSwimStartTime();
        }

        private void toolStripLogSwim_Click(object sender, EventArgs e)
        {
            m_dataObject.SetCurrentViewSwimStartTime();
        }

        private void menuItemImportStartTimes_Click(object sender, EventArgs e)
        {
            string fileName = null;

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter =
               "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            dialog.Title = "Select a CSV file";
            dialog.RestoreDirectory = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if (dialog.FileName != null)
                    {
                        fileName = dialog.FileName;
                        ImportStartTimes(fileName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void menuItemImportEndTimes_Click(object sender, EventArgs e)
        {
            string fileName = null;

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter =
               "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            dialog.Title = "Select a CSV file";
            dialog.RestoreDirectory = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if (dialog.FileName != null)
                    {
                        fileName = dialog.FileName;
                        ImportEndTimes(fileName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }
    }
}
