using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Drawing;

namespace MCMHelper_MenuDesigner
{
    public partial class formMain : Form
    {
        // Containers
        Root jsonConfig;
        RootKeybind jsonKeybind;
        MenuData cMenu;
        ItemData WorkingItem;
        List<SettingData> sSettings;
        List<TranslationData> tTranslation;

        // Working Values
        int PageIndex = 0;
        string sSaveFileLocation = "";
        bool bContentChanges = false;

        public formMain()
        {
            InitializeComponent();
            cMenu = new MenuData();

            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            // Create the ToolTip and associate with the Form container.
            ToolTip toolTip1 = new ToolTip();

            // Set up the delays for the ToolTip.
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 500;
            toolTip1.ReshowDelay = 500;
            // Force the ToolTip text to be displayed whether or not the form is active.
            toolTip1.ShowAlways = true;

            // Plugins
            toolTip1.SetToolTip(this.btPluginAdd, "Add plugin");
            toolTip1.SetToolTip(this.btPluginRemove, "Remove plugin");

            // Page tooltips
            toolTip1.SetToolTip(this.btPageAdd, "Add new page");
            toolTip1.SetToolTip(this.btPageRemove, "Remove page");
            toolTip1.SetToolTip(this.btExportPage, "Save page");
            toolTip1.SetToolTip(this.btImportPage, "Import page");
            toolTip1.SetToolTip(this.btPageUp, "Move page up");
            toolTip1.SetToolTip(this.btPageDown, "Move page down");

            // Item pane tooltips
            toolTip1.SetToolTip(this.btLeftAdd, "Add new item");
            toolTip1.SetToolTip(this.btRightAdd, "Add new item");
            toolTip1.SetToolTip(this.btLeftRemove, "Remove item");
            toolTip1.SetToolTip(this.btRightRemove, "Remove item");
            toolTip1.SetToolTip(this.btLeftUp, "Move item up");
            toolTip1.SetToolTip(this.btRightUp, "Move item up");
            toolTip1.SetToolTip(this.btLeftDown, "Move item down");
            toolTip1.SetToolTip(this.btRightDown, "Move item down");

            // Item tooltips
            toolTip1.SetToolTip(this.btMenuEnumAdd, "Add new option");
            toolTip1.SetToolTip(this.btMenuEnumRemove, "Remove option");
            toolTip1.SetToolTip(this.btMenuEnumUp, "Move option up");
            toolTip1.SetToolTip(this.btMenuEnumDown, "Move option down");

            toolTip1.SetToolTip(this.btActionAdd, "Add new parameter");
            toolTip1.SetToolTip(this.btMenuEnumRemove, "Remove parameter");
            toolTip1.SetToolTip(this.btActionUp, "Move parameter up");
            toolTip1.SetToolTip(this.btActionDown, "Move parameter down");
        }

        private void ChangedContent(bool bStatus)
        {
            bContentChanges = bStatus;
        }

        #region Plugin Objects
        private void txtPluginName_TextChanged(object sender, EventArgs e)
        {
            cMenu.PluginName = txtPluginName.Text;
            ChangedContent(true);

        }

        private void txtDisplayName_TextChanged(object sender, EventArgs e)
        {
            cMenu.DisplayName = txtDisplayName.Text;
            cMenu.DisplayNameTranslated = TranslateString(txtDisplayName.Text, "MenuName");
            ChangedContent(true);
        }

        private void txtCustomContent_TextChanged(object sender, EventArgs e)
        {
            cMenu.CustomContent = txtCustomContent.Text;
            ChangedContent(true);
        }

        private void txtX_TextChanged(object sender, EventArgs e)
        {
            cMenu.X = txtX.Text;
            ChangedContent(true);
        }

        private void txtY_TextChanged(object sender, EventArgs e)
        {
            cMenu.Y = txtY.Text;
            ChangedContent(true);
        }

        private void txtMCMVersion_TextChanged(object sender, EventArgs e)
        {
            cMenu.MCMVersion = txtMCMVersion.Text;
            ChangedContent(true);
        }

        private void ckMultiplePages_CheckedChanged(object sender, EventArgs e)
        {
            // Remove controls
            txtPageDisplayName.TextChanged -= txtPageDisplayName_TextChanged;

            // Check Data
            cMenu.MultiplePages = chkMultiplePages.Checked;
            groupPage.Enabled = !chkMultiplePages.Checked;

            // Adjust Left-Right Items
            if (chkMultiplePages.Checked)
                SetItems(cMenu.Pages.Find(x => x.ID == 0));
            else
            {
                if (lvPages.Items.Count > 1)
                {
                    lvPages.SelectedItems.Clear();
                    lvPages.Items[0].Selected = true;
                }
                else
                    ClearPageControls();
            }

            // Add controls back
            txtPageDisplayName.TextChanged -= txtPageDisplayName_TextChanged;
            ChangedContent(true);
        }

        private void btAddTranslations_Click(object sender, EventArgs e)
        {
            // Create the translation list if it doesnt exist
            if (cMenu.ExtraTranslations == null)
                cMenu.ExtraTranslations = new List<TranslationData>();

            PopupWindowTranslations popup = new PopupWindowTranslations();
            popup.Location = Cursor.Position;
            popup.Left = popup.Left - 32;
            popup.Top = popup.Top - 32;
            popup.StartPosition = FormStartPosition.Manual;
            popup.EnteredTranslations = cMenu.TranslationList();
            popup.ShowDialog();

            if (popup.EnteredTranslations != null)
            {
                cMenu.ExtraTranslations.Clear();

                //iterate the array
                for (int i = 0; i < popup.EnteredTranslations.Length; i++)
                {
                    cMenu.ExtraTranslations.Add(new TranslationData() { tTitle = popup.EnteredTranslations[i].Text, tValue = popup.EnteredTranslations[i].SubItems[1].Text });
                }
            }

            popup.Dispose();
        }

        private void btPluginAdd_Click(object sender, EventArgs e)
        {
            PopupWindow popup = new PopupWindow();
            popup.Location = Cursor.Position;
            popup.Left = popup.Left - 32;
            popup.Top = popup.Top - 32;
            popup.StartPosition = FormStartPosition.Manual;
            popup.ShowDialog();

            if (popup.EnteredText != "")
            {
                int NewID = cMenu.GetNewPlugin();

                cMenu.RequiredPlugins.Add(new ItemListData() { ID = NewID, PluginName = popup.EnteredText });
                ListViewItem NewPage = new ListViewItem();
                NewPage.Text = NewID.ToString();
                NewPage.SubItems.Add(popup.EnteredText);
                lvRequiredPlugins.Items.Add(NewPage);
                ChangedContent(true);
            }

            popup.Dispose();
        }

        private void btPluginRemove_Click(object sender, EventArgs e)
        {
            if (lvRequiredPlugins.SelectedItems.Count <= 0)
                return;

            cMenu.RemovePlugin(Int32.Parse(lvRequiredPlugins.SelectedItems[0].Text));
            lvRequiredPlugins.Items.RemoveAt(lvRequiredPlugins.SelectedItems[0].Index);
            ChangedContent(true);
        }
        #endregion

        #region Menu Pages
        private void btPageAdd_Click(object sender, EventArgs e)
        {
            PopupWindow popup = new PopupWindow();
            popup.Location = Cursor.Position;
            popup.Left = popup.Left - 32;
            popup.Top = popup.Top - 32;
            popup.StartPosition = FormStartPosition.Manual;
            popup.ShowDialog();

            if (popup.EnteredText != "")
            {
                int NewID = cMenu.GetNewPage();

                cMenu.Pages.Add(new PageData() { DisplayName = popup.EnteredText, DisplayNameTranslated = TranslateString(popup.EnteredText, "PageName"), FillMode = "topToBottom", ID = NewID, DisplayOrder = lvPages.Items.Count });
                ListViewItem NewPage = new ListViewItem();
                NewPage.Text = NewID.ToString();
                NewPage.SubItems.Add(popup.EnteredText);
                lvPages.Items.Add(NewPage);

                // Select the newest option after it gets created
                lvPages.Items[lvPages.Items.Count - 1].Selected = true;
            }

            popup.Dispose();
        }

        private void btPageRemove_Click(object sender, EventArgs e)
        {
            if (lvPages.SelectedItems.Count <= 0)
                return;

            cMenu.RemovePage(Int32.Parse(lvPages.SelectedItems[0].Text));
            lvPages.Items.RemoveAt(lvPages.SelectedItems[0].Index);
        }

        private void btExportPage_Click(object sender, EventArgs e)
        {
            if (lvPages.SelectedItems.Count <= 0)
                return;

            PageData pPage = cMenu.Pages.Find(x => x.ID == Int32.Parse(lvPages.SelectedItems[0].Text));

            // Displays a SaveFileDialog so the user can save the MCM Designer file
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Designer Page files|*.mcmpage";
            saveFileDialog1.Title = "Save an MCM Designer Page File";
            saveFileDialog1.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.FileName != "")
            {
                // Stream the menu file
                FileStream fs = new FileStream(saveFileDialog1.FileName, FileMode.Create);

                // Construct a BinaryFormatter and use it to serialize the data to the stream.
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    formatter.Serialize(fs, pPage);
                }
                catch (SerializationException f)
                {
                    Console.WriteLine("Failed to serialize. Reason: " + f.Message);
                    throw;
                }
                finally
                {
                    fs.Close();
                }
            }
        }

        private void btImportPage_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "Designer Page files (*.mcmpage)|*.mcmpage";

                // If the dialog is not OK we return
                if (dlg.ShowDialog() != DialogResult.OK)
                    return;

                PageData pPage;

                // Open the file containing the data that you want to deserialize.
                FileStream fs = new FileStream(dlg.FileName, FileMode.Open);
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();

                    // Deserialize the hashtable from the file and
                    // assign the reference to the local variable.
                    pPage = (PageData)formatter.Deserialize(fs);
                }
                catch (SerializationException f)
                {
                    Console.WriteLine("Failed to deserialize. Reason: " + f.Message);
                    throw;
                }
                finally
                {
                    fs.Close();
                }

                // Add New Page
                pPage.ID = cMenu.GetNewPage();
                cMenu.Pages.Add(pPage);
                lvPages.Items.Clear();
                lvPages.Items.AddRange(cMenu.PageList());
                lvPages.Items[lvPages.Items.Count - 1].Selected = true;
            }
            catch
            {
                MessageBox.Show("Unable to load the selected file", "MCM Helper Designer");
            }
        }

        private void btPageUp_Click(object sender, EventArgs e)
        {
            if (lvPages.SelectedItems.Count <= 0 || lvPages.SelectedItems[0].Index <= 0)
                return;

            // Modify the location of the selected item
            ListViewItem WorkingItem = lvPages.SelectedItems[0];
            int CurrentIndex = lvPages.SelectedItems[0].Index;
            int index = lvPages.SelectedItems[0].Index - 1;
            lvPages.Items.RemoveAt(CurrentIndex);
            lvPages.Items.Insert(index, WorkingItem);

            // Adjust Page Display Order
            cMenu.PageOrder(lvPages.Items);
        }

        private void btPageDown_Click(object sender, EventArgs e)
        {
            if (lvPages.SelectedItems.Count <= 0 || lvPages.SelectedItems[0].Index + 1 == lvPages.Items.Count)
                return;

            // Modify the location of the selected item
            ListViewItem WorkingItem = lvPages.SelectedItems[0];
            int CurrentIndex = lvPages.SelectedItems[0].Index;
            int index = lvPages.SelectedItems[0].Index + 1;
            lvPages.Items.RemoveAt(CurrentIndex);
            lvPages.Items.Insert(index, WorkingItem);

            // Adjust Page Display Order
            cMenu.PageOrder(lvPages.Items);
        }

        private void lvPages_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvPages.SelectedItems.Count <= 0)
            {
                txtPageDisplayName.Text = "";
                return;
            }

            SaveItemChanges();
            WorkingItem = null;
            SetItems(cMenu.Pages.Find(x => x.ID == Int32.Parse(lvPages.SelectedItems[0].Text)));


        }

        private void txtPageDisplayName_TextChanged(object sender, EventArgs e)
        {
            if (lvPages.SelectedItems.Count <= 0)
                return;

            lvPages.SelectedItems[0].SubItems[1].Text = txtPageDisplayName.Text;
            cMenu.Pages.Find(x => x.ID == Int32.Parse(lvPages.SelectedItems[0].Text)).DisplayName = txtPageDisplayName.Text;
            cMenu.Pages.Find(x => x.ID == Int32.Parse(lvPages.SelectedItems[0].Text)).DisplayNameTranslated = TranslateString(txtPageDisplayName.Text, "PageName");
        }
        #endregion

        #region Left Item List
        private void btLeftAdd_Click(object sender, EventArgs e)
        {
            if (PageIndex <= 0 && !cMenu.MultiplePages)
                return;

            PopupWindowValue popup = new PopupWindowValue();
            popup.Location = Cursor.Position;
            popup.Left = popup.Left - 32;
            popup.Top = popup.Top - 32;
            popup.StartPosition = FormStartPosition.Manual;
            popup.ShowDialog();

            if (popup.EnteredText != "")
            {
                cMenu.Pages.Find(x => x.ID == PageIndex).AddNewItem("Left", popup.EnteredText);
                lvLeftSide.Items.Clear();
                lvLeftSide.Items.AddRange(cMenu.Pages.Find(x => x.ID == PageIndex).ItemList("Left"));

                // Select the newest option after it gets created
                lvLeftSide.Items[lvLeftSide.Items.Count - 1].Selected = true;
            }

            popup.Dispose();
        }

        private void btLeftRemove_Click(object sender, EventArgs e)
        {
            // Null the working item
            WorkingItem = null;

            if (lvLeftSide.SelectedItems.Count <= 0)
                return;

            int FoundID = Int32.Parse(lvLeftSide.SelectedItems[0].Text);
            cMenu.Pages.Find(x => x.ID == PageIndex).RemoveItem(FoundID);

            int CurrentLocation = lvLeftSide.SelectedItems[0].Index;
            lvLeftSide.Items.RemoveAt(CurrentLocation);
        }

        private void btLeftUp_Click(object sender, EventArgs e)
        {
            if (lvLeftSide.SelectedItems.Count <= 0 || lvLeftSide.SelectedItems[0].Index <= 0)
                return;

            // Modify the location of the selected item
            ListViewItem WorkingItem = lvLeftSide.SelectedItems[0];
            int CurrentIndex = lvLeftSide.SelectedItems[0].Index;
            int index = lvLeftSide.SelectedItems[0].Index - 1;
            lvLeftSide.Items.RemoveAt(CurrentIndex);
            lvLeftSide.Items.Insert(index, WorkingItem);

            // Adjust Page Display Order
            cMenu.Pages.Find(x => x.ID == PageIndex).ItemOrder(lvLeftSide.Items);
        }

        private void btLeftDown_Click(object sender, EventArgs e)
        {
            if (lvLeftSide.SelectedItems.Count <= 0 || lvLeftSide.SelectedItems[0].Index + 1 == lvLeftSide.Items.Count)
                return;

            // Modify the location of the selected item
            ListViewItem WorkingItem = lvLeftSide.SelectedItems[0];
            int CurrentIndex = lvLeftSide.SelectedItems[0].Index;
            int index = lvLeftSide.SelectedItems[0].Index + 1;
            lvLeftSide.Items.RemoveAt(CurrentIndex);
            lvLeftSide.Items.Insert(index, WorkingItem);

            // Adjust Page Display Order
            cMenu.Pages.Find(x => x.ID == PageIndex).ItemOrder(lvLeftSide.Items);
        }

        private void lvLeftSide_SelectedIndexChanged(object sender, EventArgs e)
        {
            // If no selected clear it out
            if (lvLeftSide.SelectedItems.Count <= 0)
                return;

            // Save previous changes if necessary
            SaveItemChanges();
            WorkingItem = cMenu.Pages.Find(x => x.ID == PageIndex).GetItem(Int32.Parse(lvLeftSide.SelectedItems[0].Text));

            // Clear the left side
            if (lvRightSide.SelectedItems.Count > 0)
            {
                lvRightSide.SelectedIndexChanged -= lvRightSide_SelectedIndexChanged;
                lvRightSide.SelectedItems.Clear();
                lvRightSide.SelectedIndexChanged += lvRightSide_SelectedIndexChanged;
            }

            ClearItemControls();
            SetControls();
        }
        #endregion

        #region Right Item List
        private void btRightAdd_Click(object sender, EventArgs e)
        {
            if (PageIndex <= 0 && !cMenu.MultiplePages)
                return;

            PopupWindowValue popup = new PopupWindowValue();
            popup.Location = Cursor.Position;
            popup.Left = popup.Left - 32;
            popup.Top = popup.Top - 32;
            popup.StartPosition = FormStartPosition.Manual;
            popup.ShowDialog();

            if (popup.EnteredText != "")
            {
                cMenu.Pages.Find(x => x.ID == PageIndex).AddNewItem("Right", popup.EnteredText);
                lvRightSide.Items.Clear();
                lvRightSide.Items.AddRange(cMenu.Pages.Find(x => x.ID == PageIndex).ItemList("Right"));

                // Select the newest option after it gets created
                lvRightSide.Items[lvRightSide.Items.Count - 1].Selected = true;
            }

            popup.Dispose();
        }

        private void btRightRemove_Click(object sender, EventArgs e)
        {
            // Null the working item
            WorkingItem = null;

            if (lvRightSide.SelectedItems.Count <= 0)
                return;

            int FoundID = Int32.Parse(lvRightSide.SelectedItems[0].Text);
            cMenu.Pages.Find(x => x.ID == PageIndex).RemoveItem(FoundID);

            int CurrentLocation = lvRightSide.SelectedItems[0].Index;
            lvRightSide.Items.RemoveAt(CurrentLocation);
        }

        private void btRightUp_Click(object sender, EventArgs e)
        {
            if (lvRightSide.SelectedItems.Count <= 0 || lvRightSide.SelectedItems[0].Index <= 0)
                return;

            // Modify the location of the selected item
            ListViewItem WorkingItem = lvRightSide.SelectedItems[0];
            int CurrentIndex = lvRightSide.SelectedItems[0].Index;
            int index = lvRightSide.SelectedItems[0].Index - 1;
            lvRightSide.Items.RemoveAt(CurrentIndex);
            lvRightSide.Items.Insert(index, WorkingItem);

            // Adjust Page Display Order
            cMenu.Pages.Find(x => x.ID == PageIndex).ItemOrder(lvRightSide.Items);
        }

        private void btRightDown_Click(object sender, EventArgs e)
        {
            if (lvRightSide.SelectedItems.Count <= 0 || lvRightSide.SelectedItems[0].Index + 1 == lvRightSide.Items.Count)
                return;

            // Modify the location of the selected item
            ListViewItem WorkingItem = lvRightSide.SelectedItems[0];
            int CurrentIndex = lvRightSide.SelectedItems[0].Index;
            int index = lvRightSide.SelectedItems[0].Index + 1;
            lvRightSide.Items.RemoveAt(CurrentIndex);
            lvRightSide.Items.Insert(index, WorkingItem);

            // Adjust Page Display Order
            cMenu.Pages.Find(x => x.ID == PageIndex).ItemOrder(lvRightSide.Items);
        }

        private void lvRightSide_SelectedIndexChanged(object sender, EventArgs e)
        {
            // If no selected clear it out
            if (lvRightSide.SelectedItems.Count <= 0)
                return;

            // Save previous changes if necessary
            SaveItemChanges();
            WorkingItem = cMenu.Pages.Find(x => x.ID == PageIndex).GetItem(Int32.Parse(lvRightSide.SelectedItems[0].Text));

            // Clear the left side
            if (lvLeftSide.SelectedItems.Count > 0)
            {
                lvLeftSide.SelectedIndexChanged -= lvLeftSide_SelectedIndexChanged;
                lvLeftSide.SelectedItems.Clear();
                lvLeftSide.SelectedIndexChanged += lvLeftSide_SelectedIndexChanged;
            }

           

            ClearItemControls();
            SetControls();
        }
        #endregion

        #region Item Controls
        private void CheckGroupCondition()
        {
            foreach (string s in WorkingItem.GroupCondition)
            {
                ListViewItem lFoundItem = lvGroupCondition.FindItemWithText(s);
                if (lFoundItem == null)
                    return;

                lFoundItem.Checked = true;
            }
        }

        private void SaveItemChanges()
        {
            // No Option
            if (WorkingItem == null) return;

            // Primary Options
            WorkingItem.ItemType = cbMenuType.Text;
            WorkingItem.ItemColor = (btTextColor.BackColor.ToArgb() & 0x00FFFFFF).ToString("X6");
            WorkingItem.ItemText = txtItemText.Text;
            WorkingItem.ItemTextTranslated = TranslateString(txtItemText.Text, "ItemName", cMenu.Pages.Find(x => x.ID == PageIndex).DisplayName);
            WorkingItem.ItemID = txtItemID.Text;
            WorkingItem.HelpText = txtHelpText.Text;
            WorkingItem.HelpTextTranslated = TranslateString(txtItemText.Text, "HelpName", cMenu.Pages.Find(x => x.ID == PageIndex).DisplayName);

            // Value Options
            WorkingItem.SourceType = cbSourceType.Text;
            WorkingItem.DefaultValue = txtDefaultValue.Text;
            WorkingItem.PropertyName = txtPropertyName.Text;
            WorkingItem.SourceForm = txtSourceForm.Text;
            WorkingItem.SourceScript = txtSourceScript.Text;

            // Enum
            int iMenuDisplay = 1;
            WorkingItem.MenuEnumStep.Clear();
            foreach (ListViewItem lItem in lvMenuEnum.Items)
            {
                string sShortName = null;
                string sShortNameTranslated = null;
                string sOptionTranslated = null;
                string sBackColor = (lItem.BackColor.ToArgb() & 0x00FFFFFF).ToString("X6");

                // Populate and Translate the option or Short Name
                if (lItem.SubItems.Count > 1 && !String.IsNullOrEmpty(lItem.SubItems[1].Text))
                {
                    sShortName = lItem.SubItems[1].Text;
                    sShortNameTranslated = TranslateString(lItem.SubItems[1].Text, "OptionName");
                }
                else
                    sOptionTranslated = TranslateString(lItem.Text, "OptionName");

                ItemListData NewMenuData = new ItemListData();
                NewMenuData.ID = WorkingItem.GetNewID("MenuList");
                NewMenuData.DisplayOrder = iMenuDisplay;
                NewMenuData.Options = lItem.Text;
                NewMenuData.OptionsTranslated = sOptionTranslated;
                NewMenuData.ShortName = sShortName;
                NewMenuData.ShortNameTranslated = sShortNameTranslated;
                NewMenuData.OptionColor = sBackColor;

                WorkingItem.MenuEnumStep.Add(NewMenuData);
                iMenuDisplay++;
            }

            // Slider
            WorkingItem.Min = txtMin.Text;
            WorkingItem.Max = txtMax.Text;
            WorkingItem.Step = txtStep.Text;
            WorkingItem.Format = txtFormat.Text;

            // Hotkey
            WorkingItem.HotkeyDescription = txtHotkeyDescription.Text;
            WorkingItem.IgnoreConflict = chkIgnoreConflict.Checked;

            // If no check boxes are checked, we check true by default
            if (lvGroupCondition.CheckedItems.Count >= 1 && !chkTrue.Checked && !chkFalse.Checked && !chkTrueAND.Checked && !chkTrueOR.Checked && !chkTrueONLY.Checked)
                chkTrue.Checked = true;

            // Group Conditions
            WorkingItem.GroupControl = txtGroupControl.Text;
            WorkingItem.GroupBehavior = cbGroupBehavior.Text;
            WorkingItem.GroupTrue = chkTrue.Checked;
            WorkingItem.GroupFalse = chkFalse.Checked;
            WorkingItem.GroupTrueONLY = chkTrueONLY.Checked;
            WorkingItem.GroupTrueOR = chkTrueOR.Checked;
            WorkingItem.GroupTrueAND = chkTrueAND.Checked;

            // Group Condition Numbers
            WorkingItem.GroupCondition.Clear();
            foreach (ListViewItem lChecked in lvGroupCondition.CheckedItems)
                WorkingItem.GroupCondition.Add(lChecked.Text);

            // Action
            WorkingItem.ActionType = cbActionType.Text;
            WorkingItem.ActionForm = txtActionForm.Text;
            WorkingItem.ActionScript = txtActionScript.Text;
            WorkingItem.ActionFunction = txtActionFunction.Text;
            WorkingItem.ActionCommand = txtActionCommand.Text;

            WorkingItem.ActionParameters.Clear();
            int iActionDisplay = 1;
            foreach (ListViewItem lItem in lvActionParameters.Items)
            {
                WorkingItem.ActionParameters.Add(new ItemListData() { ID = WorkingItem.GetNewID("ActionParameters"), DisplayOrder = iActionDisplay, Parameter = lItem.Text });
                iActionDisplay++;
            }

            // Set the text on the List Views
            if (PageIndex == Int32.Parse(lvPages.SelectedItems[0].Text))
            {
                ListViewItem WorkingList = new ListViewItem();
                if (WorkingItem.ItemSide == "Left")
                    WorkingList = lvLeftSide.FindItemWithText(WorkingItem.ID.ToString(), false, 0);
                else
                    WorkingList = lvRightSide.FindItemWithText(WorkingItem.ID.ToString(), false, 0);

                WorkingList.SubItems[1].Text = String.IsNullOrEmpty(WorkingItem.ItemText) ? "" : WorkingItem.ItemText;
                WorkingList.SubItems[2].Text = WorkingItem.ItemType;
                WorkingList.SubItems[3].Text = String.IsNullOrEmpty(WorkingItem.GroupControl) ? "" : WorkingItem.GroupControl;
                WorkingList.SubItems[4].Text = WorkingItem.GetConditionList();
            }
            // Item changes where made
            ChangedContent(true);
        }

        private void cbMenuType_SelectedIndexChanged(object sender, EventArgs e)
        {
            // If the source type is null, return
            if (cbMenuType.SelectedItem == null)
                return;

            // Enable all boxes
            txtItemID.Enabled = true;
            txtHelpText.Enabled = true;
            txtItemText.Enabled = true;
            groupValueOptions.Enabled = true;
            groupMenu.Enabled = true;
            groupSlider.Enabled = true;
            groupControl.Enabled = true;
            groupAction.Enabled = true;
            groupHotkey.Enabled = true;
            label17.Text = "Default Value";

            // Save current sourceType Value
            string sTempValue = cbSourceType.Text;
            cbSourceType.Items.Clear();

            switch (cbMenuType.SelectedItem.ToString())
            {
                case "empty":
                    txtItemID.Enabled = false;
                    txtHelpText.Enabled = false;
                    txtItemText.Enabled = false;
                    groupValueOptions.Enabled = false;
                    groupMenu.Enabled = false;
                    groupSlider.Enabled = false;
                    groupControl.Enabled = false;
                    groupAction.Enabled = false;
                    groupHotkey.Enabled = false;
                    break;
                case "header":
                    txtItemID.Enabled = false;
                    txtHelpText.Enabled = false;
                    groupValueOptions.Enabled = false;
                    groupMenu.Enabled = false;
                    groupSlider.Enabled = false;
                    groupAction.Enabled = false;
                    groupHotkey.Enabled = false;
                    break;
                case "text":
                    label17.Text = "Value";
                    groupMenu.Enabled = false;
                    groupSlider.Enabled = false;
                    groupHotkey.Enabled = false;
                    cbSourceType.Items.AddRange(new string[] { "", "PropertyValueString", "ModSettingString", "GlobalValue" });
                    break;
                case "toggle":
                    groupMenu.Enabled = false;
                    groupSlider.Enabled = false;
                    groupAction.Enabled = false;
                    groupHotkey.Enabled = false;
                    cbSourceType.Items.AddRange(new string[] { "", "PropertyValueBool", "PropertyValueInt", "ModSettingBool", "ModSettingInt", "GlobalValue" });
                    break;
                case "hiddenToggle":
                    txtItemText.Enabled = false;
                    txtHelpText.Enabled = false;
                    groupMenu.Enabled = false;
                    groupSlider.Enabled = false;
                    groupAction.Enabled = false;
                    groupHotkey.Enabled = false;
                    cbSourceType.Items.AddRange(new string[] { "", "PropertyValueBool", "PropertyValueInt", "ModSettingBool", "ModSettingInt", "GlobalValue" });
                    break;
                case "slider":
                    groupMenu.Enabled = false;
                    groupAction.Enabled = false;
                    groupHotkey.Enabled = false;
                    cbSourceType.Items.AddRange(new string[] { "", "PropertyValueFloat", "PropertyValueInt", "ModSettingFloat", "ModSettingInt", "GlobalValue" });
                    break;
                case "stepper":
                    groupSlider.Enabled = false;
                    groupAction.Enabled = false;
                    groupHotkey.Enabled = false;
                    cbSourceType.Items.AddRange(new string[] { "", "PropertyValueInt", "ModSettingInt", "GlobalValue" });
                    break;
                case "menu":
                    groupSlider.Enabled = false;
                    groupAction.Enabled = false;
                    groupHotkey.Enabled = false;
                    cbSourceType.Items.AddRange(new string[] { "", "PropertyValueString", "ModSettingString", "GlobalValue" });
                    break;
                case "enum":
                    groupSlider.Enabled = false;
                    groupAction.Enabled = false;
                    groupHotkey.Enabled = false;
                    cbSourceType.Items.AddRange(new string[] { "", "PropertyValueInt", "ModSettingInt", "GlobalValue" });
                    break;
                case "color":
                    groupMenu.Enabled = false;
                    groupSlider.Enabled = false;
                    groupAction.Enabled = false;
                    groupHotkey.Enabled = false;
                    cbSourceType.Items.AddRange(new string[] { "", "PropertyValueInt", "ModSettingInt", "GlobalValue" });
                    break;
                case "keymap":
                    groupMenu.Enabled = false;
                    groupSlider.Enabled = false;
                    cbSourceType.Items.AddRange(new string[] { "", "PropertyValueInt", "ModSettingInt", "GlobalValue" });
                    break;
                case "input":
                    groupMenu.Enabled = false;
                    groupSlider.Enabled = false;
                    groupAction.Enabled = false;
                    groupHotkey.Enabled = false;
                    cbSourceType.Items.AddRange(new string[] { "", "PropertyValueString", "ModSettingString", "GlobalValue" });
                    break;
            }

            // Reset Source Type
            cbSourceType.Text = sTempValue;
        }

        private void cbSourceType_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (cbSourceType.SelectedItem == null)
            //{
            //    txtDefaultValue.Enabled = false;
            //    txtPropertyName.Enabled = false;
            //    txtSourceForm.Enabled = false;
            //    txtSourceScript.Enabled = false;
            //    return;
            //}

            //if (cbSourceType.SelectedItem.ToString().Contains("Property"))
            //{
            //    txtDefaultValue.Enabled = false;
            //    txtPropertyName.Enabled = true;
            //    txtSourceForm.Enabled = true;
            //    txtSourceScript.Enabled = true;
            //}
            //else if (cbSourceType.SelectedItem.ToString().Contains("Setting"))
            //{
            //    txtDefaultValue.Enabled = true;
            //    txtPropertyName.Enabled = false;
            //    txtSourceForm.Enabled = false;
            //    txtSourceScript.Enabled = false;
            //}
            //else if (cbSourceType.SelectedItem.ToString().Contains("Global"))
            //{
            //    txtDefaultValue.Enabled = true;
            //    txtPropertyName.Enabled = false;
            //    txtSourceForm.Enabled = true;
            //    txtSourceScript.Enabled = false;
            //}

            //if (txtItemID.Text != "")
            //{

            //}
        }

        private void btTextColor_Click(object sender, EventArgs e)
        {

            frmColorPicker ColorDialog = new frmColorPicker(btTextColor.BackColor);
            ColorDialog.DrawStyle = frmColorPicker.eDrawStyle.Hue;
            if (ColorDialog.ShowDialog() == DialogResult.OK)
                btTextColor.BackColor = ColorDialog.PrimaryColor;

            ColorDialog.Dispose();
        }

        private void lvMenuEnum_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo SelectedInfo = lvMenuEnum.HitTest(e.X, e.Y);
            ListViewItem SelectedItem = SelectedInfo.Item;

            if (SelectedItem != null)
            {
                PopupWindowMenu popup = new PopupWindowMenu();
                popup.Location = Cursor.Position;
                popup.Left = popup.Left - 32;
                popup.Top = popup.Top - 32;
                popup.StartPosition = FormStartPosition.Manual;

                // Set the Textbox
                popup.EnteredOption = SelectedItem.Text;
                popup.EnteredShortName = SelectedItem.SubItems[1].Text;

                popup.ShowDialog();

                if (popup.EnteredOption != "")
                {
                    SelectedItem.Text = popup.EnteredOption;
                    SelectedItem.SubItems[1].Text = popup.EnteredShortName;
                    SelectedItem.BackColor = ColorTranslator.FromHtml("#" + popup.SelectedColor);
                    lvMenuEnum.Refresh();
                }

                popup.Dispose();
            }
        }

        private void btMenuEnumAdd_Click(object sender, EventArgs e)
        {
            PopupWindowMenu popup = new PopupWindowMenu();
            popup.Location = Cursor.Position;
            popup.Left = popup.Left - 32;
            popup.Top = popup.Top - 32;
            popup.StartPosition = FormStartPosition.Manual;
            popup.ShowDialog();

            if (popup.EnteredOption != "")
            {
                ListViewItem NewPage = new ListViewItem();
                NewPage.Text = popup.EnteredOption;
                NewPage.SubItems.Add(popup.EnteredShortName);
                NewPage.BackColor = ColorTranslator.FromHtml("#" + popup.SelectedColor);
                lvMenuEnum.Items.Add(NewPage);
            }

            popup.Dispose();
        }

        private void btMenuEnumRemove_Click(object sender, EventArgs e)
        {
            if (lvMenuEnum.SelectedItems.Count > 0)
                lvMenuEnum.Items.RemoveAt(lvMenuEnum.SelectedItems[0].Index);
        }

        private void btMenuEnumUp_Click(object sender, EventArgs e)
        {
            if (lvMenuEnum.SelectedItems.Count <= 0 || lvMenuEnum.SelectedItems[0].Index <= 0)
                return;

            // Modify the location of the selected item
            ListViewItem WorkingItem = lvMenuEnum.SelectedItems[0];
            int CurrentIndex = lvMenuEnum.SelectedItems[0].Index;
            int index = lvMenuEnum.SelectedItems[0].Index - 1;
            lvMenuEnum.Items.RemoveAt(CurrentIndex);
            lvMenuEnum.Items.Insert(index, WorkingItem);
        }

        private void btMenuEnumDown_Click(object sender, EventArgs e)
        {
            if (lvMenuEnum.SelectedItems.Count <= 0 || lvMenuEnum.SelectedItems[0].Index + 1 == lvMenuEnum.Items.Count)
                return;

            // Modify the location of the selected item
            ListViewItem WorkingItem = lvMenuEnum.SelectedItems[0];
            int CurrentIndex = lvMenuEnum.SelectedItems[0].Index;
            int index = lvMenuEnum.SelectedItems[0].Index + 1;
            lvMenuEnum.Items.RemoveAt(CurrentIndex);
            lvMenuEnum.Items.Insert(index, WorkingItem);
        }

        private void btExportMenu_Click(object sender, EventArgs e)
        {
            if (lvMenuEnum.Items.Count <= 0) return;

            // Generate a menu item list
            int iMenuDisplay = 1;
            MenuItems SavedItem = new MenuItems();
            foreach (ListViewItem lItem in lvMenuEnum.Items)
            {
                string sShortName = null;
                string sShortNameTranslated = null;
                string sOptionTranslated = null;
                string sBackColor = (lItem.BackColor.ToArgb() & 0x00FFFFFF).ToString("X6");

                // Populate and Translate the option or Short Name
                if (lItem.SubItems.Count > 1 && !String.IsNullOrEmpty(lItem.SubItems[1].Text))
                {
                    sShortName = lItem.SubItems[1].Text;
                    sShortNameTranslated = TranslateString(lItem.SubItems[1].Text, "OptionName");
                }
                else
                    sOptionTranslated = TranslateString(lItem.Text, "OptionName");

                ItemListData NewMenuData = new ItemListData();
                NewMenuData.ID = WorkingItem.GetNewID("MenuList");
                NewMenuData.DisplayOrder = iMenuDisplay;
                NewMenuData.Options = lItem.Text;
                NewMenuData.OptionsTranslated = sOptionTranslated;
                NewMenuData.ShortName = sShortName;
                NewMenuData.ShortNameTranslated = sShortNameTranslated;
                NewMenuData.OptionColor = sBackColor;

                SavedItem.MenuData.Add(NewMenuData);
                iMenuDisplay++;
            }

            // Displays a SaveFileDialog so the user can save the MCM Designer file
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Designer Page files|*.mcmdata";
            saveFileDialog1.Title = "Save MCM Menu Designer Data";
            saveFileDialog1.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.FileName != "")
            {
                // Stream the menu file
                FileStream fs = new FileStream(saveFileDialog1.FileName, FileMode.Create);

                // Construct a BinaryFormatter and use it to serialize the data to the stream.
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    formatter.Serialize(fs, SavedItem);
                }
                catch (SerializationException f)
                {
                    Console.WriteLine("Failed to serialize. Reason: " + f.Message);
                    throw;
                }
                finally
                {
                    fs.Close();
                }
            }
        }

        private void btImportMenu_Click(object sender, EventArgs e)
        {
            if (WorkingItem == null) return;

            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "Designer Page files (*.mcmdata)|*.mcmdata";

                // If the dialog is not OK we return
                if (dlg.ShowDialog() != DialogResult.OK)
                    return;

                MenuItems mData;

                // Open the file containing the data that you want to deserialize.
                FileStream fs = new FileStream(dlg.FileName, FileMode.Open);
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();

                    // Deserialize the hashtable from the file and
                    // assign the reference to the local variable.
                    mData = (MenuItems)formatter.Deserialize(fs);
                }
                catch (SerializationException f)
                {
                    Console.WriteLine("Failed to deserialize. Reason: " + f.Message);
                    throw;
                }
                finally
                {
                    fs.Close();
                }

                // Add New Page
                WorkingItem.MenuEnumStep = mData.MenuData;
                lvMenuEnum.Items.Clear();
                lvMenuEnum.Items.AddRange(WorkingItem.ItemList("MenuList"));
            }
            catch
            {
                MessageBox.Show("Unable to load the selected file", "MCM Helper Designer");
            }
        }

        private void btActionAdd_Click(object sender, EventArgs e)
        {
            PopupWindow popup = new PopupWindow();
            popup.Location = Cursor.Position;
            popup.Left = popup.Left - 32;
            popup.Top = popup.Top - 32;
            popup.StartPosition = FormStartPosition.Manual;
            popup.ShowDialog();

            if (popup.EnteredText != "")
            {
                ListViewItem NewPage = new ListViewItem();
                NewPage.Text = popup.EnteredText;
                lvActionParameters.Items.Add(NewPage);
            }

            popup.Dispose();
        }

        private void btActionRemove_Click(object sender, EventArgs e)
        {
            if (lvActionParameters.SelectedItems.Count > 0)
                lvActionParameters.Items.RemoveAt(lvActionParameters.SelectedItems[0].Index);
        }

        private void btActionUp_Click(object sender, EventArgs e)
        {
            if (lvActionParameters.SelectedItems.Count <= 0 || lvActionParameters.SelectedItems[0].Index <= 0)
                return;

            // Modify the location of the selected item
            ListViewItem WorkingItem = lvActionParameters.SelectedItems[0];
            int CurrentIndex = lvActionParameters.SelectedItems[0].Index;
            int index = lvActionParameters.SelectedItems[0].Index - 1;
            lvActionParameters.Items.RemoveAt(CurrentIndex);
            lvActionParameters.Items.Insert(index, WorkingItem);
        }

        private void btActionDown_click(object sender, EventArgs e)
        {
            if (lvActionParameters.SelectedItems.Count <= 0 || lvActionParameters.SelectedItems[0].Index + 1 == lvActionParameters.Items.Count)
                return;

            // Modify the location of the selected item
            ListViewItem WorkingItem = lvActionParameters.SelectedItems[0];
            int CurrentIndex = lvActionParameters.SelectedItems[0].Index;
            int index = lvActionParameters.SelectedItems[0].Index + 1;
            lvActionParameters.Items.RemoveAt(CurrentIndex);
            lvActionParameters.Items.Insert(index, WorkingItem);
        }

        private void chkTrue_CheckedChanged(object sender, EventArgs e)
        {
            if (chkTrue.Checked)
            {
                chkFalse.Checked = false;
                chkTrueAND.Checked = false;
                chkTrueOR.Checked = false;
                chkTrueONLY.Checked = false;
            }
        }

        private void chkFalse_CheckedChanged(object sender, EventArgs e)
        {
            if (chkFalse.Checked)
            {
                chkTrue.Checked = false;
                chkTrueAND.Checked = false;
                chkTrueOR.Checked = false;
                chkTrueONLY.Checked = false;
            }
        }

        private void chkTrueAND_CheckedChanged(object sender, EventArgs e)
        {
            if (chkTrueAND.Checked)
            {
                chkTrue.Checked = false;
                chkFalse.Checked = false;
                chkTrueOR.Checked = false;
                chkTrueONLY.Checked = false;
            }
        }

        private void chkTrueOR_CheckedChanged(object sender, EventArgs e)
        {
            if (chkTrueOR.Checked)
            {
                chkTrue.Checked = false;
                chkFalse.Checked = false;
                chkTrueAND.Checked = false;
                chkTrueONLY.Checked = false;
            }
        }

        private void chkTrueONLY_CheckedChanged(object sender, EventArgs e)
        {
            if (chkTrueONLY.Checked)
            {
                chkTrue.Checked = false;
                chkFalse.Checked = false;
                chkTrueAND.Checked = false;
                chkTrueOR.Checked = false;
            }
        }
        #endregion

        #region Toolbar Options
        private void newMenuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check for changes
            CheckForChanges();

            // Reset the save location
            ChangeSaveLocation("");

            // Clear controls
            ClearItemControls();
            ClearPageControls();
            ClearPluginControls();

            // Set the new menu
            cMenu = new MenuData();

            // Clear saved
            ChangedContent(false);
        }

        private void openMenuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "Designer files (*.mcm)|*.mcm";

                // If the dialog is not OK we return
                if (dlg.ShowDialog() != DialogResult.OK)
                    return;

                // Check for changes
                CheckForChanges();

                // Open the file containing the data that you want to deserialize.
                FileStream fs = new FileStream(dlg.FileName, FileMode.Open);
                BinaryFormatter formatter = new BinaryFormatter();

                // Deserialize the Menu file
                cMenu = (MenuData)formatter.Deserialize(fs);

                // Close the file
                fs.Close();

                // Save filename
                ChangeSaveLocation(dlg.FileName);

                // Clear Menu
                ClearItemControls();
                ClearPageControls();
                ClearPluginControls();

                // Set Menu
                SetMenuData();
                if (!cMenu.MultiplePages && cMenu.Pages.Count > 1)
                {
                    lvPages.Items.AddRange(cMenu.PageList());
                    lvPages.Items[0].Selected = true;
                }

                // Clear saved flag
                ChangedContent(false);
            }
            catch
            {
                MessageBox.Show("Unable to load the menu file", "MCM Helper Designer");
            }
        }

        private void importMenuConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "Menu Config Files (*.json)|*.json";

                // If the dialog is not OK we return
                if (dlg.ShowDialog() != DialogResult.OK)
                    return;

                // Attempt to Deserialize the JSON
                string data = File.ReadAllText(dlg.FileName);
                Root loadJson = JsonConvert.DeserializeObject<Root>(data);
                DeserializeJSON(loadJson);

                // Save filename
                ChangeSaveLocation(dlg.FileName);

                // Clear Menu
                ClearItemControls();
                ClearPageControls();
                ClearPluginControls();

                // Set Menu
                SetMenuData();
                if (!cMenu.MultiplePages && cMenu.Pages.Count > 1)
                {
                    lvPages.Items.AddRange(cMenu.PageList());
                    lvPages.Items[0].Selected = true;
                }
            }
            catch
            {
                MessageBox.Show("Unable to load the selected file", "MCM Helper Designer");
            }
        }

        private void saveMenuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Save Item Changes
            SaveItemChanges();
            SaveFileMessage(SaveFile("Designer file|*.mcm"));
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Save Item Changes
            SaveItemChanges();

            // Return if there is no save locaiton
            if (!sSaveFileLocation.Contains(".mcm"))
                SaveFileMessage(SaveFile("Designer file|*.mcm"));
            else
            {
                FileStream fs = new FileStream(sSaveFileLocation, FileMode.Create);
                BinaryFormatter formatter = new BinaryFormatter();
                try
                {
                    formatter.Serialize(fs, cMenu);
                    ChangedContent(false);
                }
                catch (SerializationException f)
                {
                    Console.WriteLine("Failed to serialize. Reason: " + f.Message);
                    throw;
                }
                finally
                {
                    fs.Close();
                }
            }
        }

        private void configJSONToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PrepareJSON();

            // Serialize and show
            JSONWindow jWindow = new JSONWindow(JsonConvert.SerializeObject(jsonConfig, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
            jWindow.ShowDialog();
            jWindow.Dispose();
        }

        private void settingFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Process the settings file
            ProcessSettings();

            // Show Settings
            JSONWindow jWindow = new JSONWindow(ReturnSettings());
            jWindow.ShowDialog();
            jWindow.Dispose();
        }

        private void hotkeyFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Process the settings file
            PrepareKeybindJSON();

            // Create the string
            JSONWindow jWindow = new JSONWindow(JsonConvert.SerializeObject(jsonKeybind, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
            jWindow.ShowDialog();
            jWindow.Dispose();
        }

        private void translationFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Translate the menu
            PrepareTranslation();

            JSONWindow jWindow = new JSONWindow(TranslationFile());
            jWindow.ShowDialog();
            jWindow.Dispose();
        }

        private void fullMenuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Show the FolderBrowserDialog.
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                // Folder paths
                string mcmFolderPath = folderBrowserDialog1.SelectedPath + @"\MCM\Config\" + cMenu.PluginName + @"\";
                string translationFolderPath = folderBrowserDialog1.SelectedPath + @"\interface\Translations\";

                FileInfo file = new System.IO.FileInfo(mcmFolderPath);
                file.Directory.Create();
                file = new System.IO.FileInfo(translationFolderPath);
                file.Directory.Create();

                // Save config gile
                PrepareJSON();
                File.WriteAllText(mcmFolderPath + "config.json", JsonConvert.SerializeObject(jsonConfig, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

                // Save settings file
                ProcessSettings();
                if (sSettings.Count > 0)
                    File.WriteAllText(mcmFolderPath + "settings.ini", ReturnSettings());

                // Save keybind file
                PrepareKeybindJSON();
                if (jsonKeybind != null)
                    File.WriteAllText(mcmFolderPath + "keybinds.json", JsonConvert.SerializeObject(jsonKeybind, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

                // Save Translation file
                PrepareTranslation();
                File.WriteAllText(translationFolderPath + cMenu.PluginName + "_english.txt", TranslationFile(), new UnicodeEncoding(false, true));

                // Final Message
                MessageBox.Show("Your file(s) have been saved.", "MCM Helper Designer");
            }
        }

        private void mCMConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Call the save dialog
            PrepareJSON();
            SaveFileMessage(SaveFile("MCM Config file|*.json"));
        }

        private void mCMSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Call the save dialog
            ProcessSettings();
            SaveFileMessage(SaveFile("MCM Settings file|*.ini"));
        }

        private void keybindsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Call the save dialog
            PrepareKeybindJSON();
            SaveFileMessage(SaveFile("MCM Keybinds file|*.json"));
        }

        private void generateTranslationToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // Call the save dialog
            PrepareTranslation();
            SaveFileMessage(SaveFile("MCM Translation file|*.txt"));
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check for saved changes
            CheckForChanges();

            // Quit the application
            Application.Exit();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PopupWindowAbout popup = new PopupWindowAbout();
            popup.Location = Cursor.Position;
            popup.Left = popup.Left - 32;
            popup.Top = popup.Top - 32;
            popup.StartPosition = FormStartPosition.Manual;
            popup.ShowDialog();
            popup.Dispose();
        }

        private string SaveFile(string sFilter)
        {
            // Displays a SaveFileDialog so the user can save the MCM Designer file
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = sFilter;
            saveFileDialog1.Title = "Save file as...";

            if (sFilter.Contains("MCM Settings file") && sSettings.Count == 0)
                return "nofile";
            if (sFilter.Contains("MCM Keybinds file") && jsonKeybind == null)
                return "nofile";
            else
                saveFileDialog1.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.FileName != "")
            {
                // MCM Designer Files
                if (sFilter.Contains("Designer file"))
                {
                    // Stream the menu file
                    FileStream fs = new FileStream(saveFileDialog1.FileName, FileMode.Create);
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(fs, cMenu);
                    fs.Close();

                    // Save filename
                    ChangeSaveLocation(saveFileDialog1.FileName);
                    ChangedContent(false);
                    return "success";
                }

                // MCM Config Files
                else if (sFilter.Contains("MCM Config file"))
                {
                    // Process the config file
                    File.WriteAllText(saveFileDialog1.FileName, JsonConvert.SerializeObject(jsonConfig, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
                    ChangedContent(false);
                    return "success";
                }

                // Settings File
                else if (sFilter.Contains("MCM Settings file"))
                {
                    File.WriteAllText(saveFileDialog1.FileName, ReturnSettings());
                    ChangedContent(false);
                    return "success";
                }

                // Keybinds Files
                else if (sFilter.Contains("MCM Keybinds file"))
                {
                    File.WriteAllText(saveFileDialog1.FileName, JsonConvert.SerializeObject(jsonKeybind, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
                    ChangedContent(false);
                    return "success";
                }

                // Translation File
                else if (sFilter.Contains("MCM Translation file"))
                {
                    File.WriteAllText(saveFileDialog1.FileName, TranslationFile(), new UnicodeEncoding(false, true));
                    ChangedContent(false);
                    return "success";
                }
                else
                    return "fail";
            }
            else
                return "nothing";
        }

        private void SaveFileMessage(string sSuccess)
        {
            if (sSuccess == "success")
                MessageBox.Show("Your file have been saved.", "MCM Helper Designer");
            else if (sSuccess == "fail")
                MessageBox.Show("There was a problem saving your file", "MCM Helper Designer");
            else if (sSuccess == "nofile")
                MessageBox.Show("No file generation required.", "MCM Helper Designer");
        }

        private void ChangeSaveLocation(string sFileName)
        {
            // Save name and change title
            sSaveFileLocation = sFileName;

            // Change the title of the form
            if (String.IsNullOrEmpty(sFileName))
                this.Text = "MCM Helper Designer";
            else
                this.Text = "MCM Helper Designer (" + cMenu.DisplayName + ")";
        }

        private void CheckForChanges()
        {
            // Save item Changes just to be sure
            SaveItemChanges();

            if (bContentChanges)
            {
                DialogResult dialogResult = MessageBox.Show("Save changes to your menu?", "MCM Helper Designer", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                    saveToolStripMenuItem_Click(null, null);
            }
        }
        #endregion

        #region Cleanup Functions
        private void ClearItemControls()
        {
            cbMenuType.SelectedIndex = -1;
            txtItemID.Text = "";
            txtItemText.Text = "";
            cbSourceType.SelectedIndex = -1;
            txtDefaultValue.Text = "";

            // Group Boxes
            txtGroupControl.Text = "";
            cbGroupBehavior.SelectedIndex = -1;
            foreach (ListViewItem listItem in lvGroupCondition.Items)
                listItem.Checked = false;
            chkTrue.Checked = false;
            chkTrueAND.Checked = false;
            chkTrueONLY.Checked = false;
            chkTrueOR.Checked = false;
            chkFalse.Checked = false;

            // Script Date
            cbSourceType.SelectedIndex = -1;
            txtSourceForm.Text = "";
            txtPropertyName.Text = "";

            // Menu - Enum
            lvMenuEnum.Items.Clear();

            // Slider
            txtMin.Text = "";
            txtMax.Text = "";
            txtStep.Text = "";
            txtFormat.Text = "";

            // Action
            cbActionType.SelectedIndex = -1;
            txtActionForm.Text = "";
            txtActionScript.Text = "";
            txtActionFunction.Text = "";
            txtActionCommand.Text = "";
            lvActionParameters.Items.Clear();

            // Hotkey
            txtHotkeyDescription.Text = "";
            chkIgnoreConflict.Checked = false;
        }

        private void ClearPageControls()
        {
            // Disable Event Handlers
            txtPageDisplayName.TextChanged -= txtPageDisplayName_TextChanged;

            lvRightSide.Items.Clear();
            lvLeftSide.Items.Clear();
            lvPages.Items.Clear();
            txtPageDisplayName.Text = "";

            // Enable Event Handlers
            txtPageDisplayName.TextChanged += txtPageDisplayName_TextChanged;
        }

        private void ClearPluginControls()
        {
            // Disable Text Handler
            txtPluginName.TextChanged -= txtPluginName_TextChanged;
            txtDisplayName.TextChanged -= txtDisplayName_TextChanged;
            txtCustomContent.TextChanged -= txtCustomContent_TextChanged;
            txtX.TextChanged -= txtX_TextChanged;
            txtY.TextChanged -= txtY_TextChanged;
            txtMCMVersion.TextChanged -= txtMCMVersion_TextChanged;
            chkMultiplePages.CheckedChanged -= ckMultiplePages_CheckedChanged;

            // Clear textboxes
            txtPluginName.Text = "";
            txtDisplayName.Text = "";
            txtCustomContent.Text = "";
            txtX.Text = "";
            txtY.Text = "";
            txtMCMVersion.Text = "";
            chkMultiplePages.Checked = false;
            lvRequiredPlugins.Items.Clear();

            // Add Text Handler
            txtPluginName.TextChanged += txtPluginName_TextChanged;
            txtDisplayName.TextChanged += txtDisplayName_TextChanged;
            txtCustomContent.TextChanged += txtCustomContent_TextChanged;
            txtX.TextChanged += txtX_TextChanged;
            txtY.TextChanged += txtY_TextChanged;
            txtMCMVersion.TextChanged += txtMCMVersion_TextChanged; ;
            chkMultiplePages.CheckedChanged += ckMultiplePages_CheckedChanged;
        }
        #endregion

        #region Load Functions
        private void SetMenuData()
        {
            // Main Menu Data
            txtPluginName.Text = cMenu.PluginName == null ? null : cMenu.PluginName;
            txtDisplayName.Text = cMenu.DisplayName == null ? null : cMenu.DisplayName;
            txtCustomContent.Text = cMenu.CustomContent == null ? null : cMenu.CustomContent;
            txtX.Text = cMenu.X == null ? null : cMenu.X;
            txtY.Text = cMenu.Y == null ? null : cMenu.Y;
            txtMCMVersion.Text = cMenu.MCMVersion == null ? null : cMenu.MCMVersion;
            chkMultiplePages.Checked = cMenu.MultiplePages;

            // Required Plugins
            foreach (ItemListData ildPlugin in cMenu.RequiredPlugins)
            {
                ListViewItem NewPage = new ListViewItem();
                NewPage.Text = ildPlugin.ID.ToString();
                NewPage.SubItems.Add(ildPlugin.PluginName);
                lvRequiredPlugins.Items.Add(NewPage);
            }
        }

        private void SetItems(PageData pPage)
        {
            // Remove Action
            txtPageDisplayName.TextChanged -= txtPageDisplayName_TextChanged;

            // If there is no valid page, return
            if (pPage == null)
                return;

            // Set the selected page ID
            PageIndex = pPage.ID;

            // Page Data
            txtPageDisplayName.Text = pPage.DisplayName;

            // Item Data Related to Selected Page
            lvLeftSide.Items.Clear();
            lvLeftSide.Items.AddRange(pPage.ItemList("Left"));
            lvRightSide.Items.Clear();
            lvRightSide.Items.AddRange(pPage.ItemList("Right"));

            // Clear Item Controls
            ClearItemControls();

            // Add Action
            txtPageDisplayName.TextChanged += txtPageDisplayName_TextChanged;
        }

        private void SetControls()
        {
            // If the working item is null
            if (WorkingItem == null)
                return;

            // Value Option Data
            cbMenuType.Text = WorkingItem.ItemType;
            txtItemID.Text = WorkingItem.ItemID;
            txtItemText.Text = WorkingItem.ItemText;
            btTextColor.BackColor = String.IsNullOrEmpty(WorkingItem.ItemColor) ? Color.White : ColorTranslator.FromHtml("#" + WorkingItem.ItemColor);
            txtHelpText.Text = WorkingItem.HelpText;
            cbSourceType.Text = WorkingItem.SourceType;
            txtDefaultValue.Text = WorkingItem.DefaultValue;

            // Group Boxes
            txtGroupControl.Text = WorkingItem.GroupControl;
            cbGroupBehavior.Text = WorkingItem.GroupBehavior;
            lvGroupCondition.SelectedItems.Clear();
            CheckGroupCondition();
            chkTrue.Checked = WorkingItem.GroupTrue;
            chkTrueAND.Checked = WorkingItem.GroupTrueAND;
            chkTrueONLY.Checked = WorkingItem.GroupTrueONLY;
            chkTrueOR.Checked = WorkingItem.GroupTrueOR;
            chkFalse.Checked = WorkingItem.GroupFalse;

            // Script Date
            cbSourceType.Text = WorkingItem.SourceType;
            txtSourceForm.Text = WorkingItem.SourceForm;
            txtPropertyName.Text = WorkingItem.PropertyName;

            // Menu - Enum
            lvMenuEnum.Items.Clear();
            lvMenuEnum.Items.AddRange(WorkingItem.ItemList("MenuList"));

            // Slider
            txtMin.Text = WorkingItem.Min;
            txtMax.Text = WorkingItem.Max;
            txtStep.Text = WorkingItem.Step;
            txtFormat.Text = WorkingItem.Format;

            // Action
            cbActionType.Text = WorkingItem.ActionType;
            txtActionForm.Text = WorkingItem.ActionForm;
            txtActionScript.Text = WorkingItem.ActionScript;
            txtActionFunction.Text = WorkingItem.ActionFunction;
            txtActionCommand.Text = WorkingItem.ActionCommand;
            lvActionParameters.Items.Clear();
            lvActionParameters.Items.AddRange(WorkingItem.ItemList("ActionParameters"));

            // Hotkey
            txtHotkeyDescription.Text = WorkingItem.HotkeyDescription;
            chkIgnoreConflict.Checked = WorkingItem.IgnoreConflict;
        }
        #endregion

        #region Serialize JSON
        private void PrepareJSON()
        {
            jsonConfig = new Root();

            // Primary Meny Data
            jsonConfig.modName = !String.IsNullOrEmpty(cMenu.PluginName) ? cMenu.PluginName : null;
            jsonConfig.displayName = !String.IsNullOrEmpty(cMenu.DisplayName) ? cMenu.DisplayNameTranslated : null;
            jsonConfig.minMcmVersion = Int32.TryParse(cMenu.MCMVersion, out int x) ? Int32.Parse(cMenu.MCMVersion) : default(int?);
            jsonConfig.pluginRequirements = cMenu.GetStringList("RequiredPlugin").Count != 0 ? cMenu.GetStringList("RequiredPlugin") : null;

            // Is there custom content
            if (!String.IsNullOrEmpty(cMenu.CustomContent))
            {
                CustomContent NewContent = new CustomContent();
                NewContent.source = cMenu.CustomContent;
                NewContent.x = Int32.TryParse(cMenu.X, out int i) ? Int32.Parse(cMenu.X) : default(int?);
                NewContent.y = Int32.TryParse(cMenu.Y, out int j) ? Int32.Parse(cMenu.Y) : default(int?);
                jsonConfig.customContent = NewContent;
            }

            // Build out a single page, or multiple pages
            if (cMenu.MultiplePages)
            {
                jsonConfig.cursorFillMode = "topToBottom"; //cMenu.FillMode != "" ? cMenu.FillMode : "topToBottom";
                List<Content> ItemList = new List<Content>();
                ItemList = ProcessSide(ItemList, 0, "Left");
                ItemList = ProcessSide(ItemList, 0, "Right");
                jsonConfig.content = ItemList;
            }
            else
            {
                // Build out Pages
                List<Page> jsonPages = new List<Page>();
                foreach (PageData pdPage in cMenu.Pages)
                {
                    // If index is 0, pass
                    if (pdPage.ID == 0)
                        continue;

                    // New Page
                    Page NewPage = new Page();
                    NewPage.pageDisplayName = !String.IsNullOrEmpty(pdPage.DisplayName) ? pdPage.DisplayNameTranslated : null;
                    NewPage.cursorFillMode = "topToBottom"; //pdPage.FillMode != "" ? pdPage.FillMode : "topToBottom";

                    // Build Item Menus
                    List<Content> ItemList = new List<Content>();
                    ItemList = ProcessSide(ItemList, pdPage.ID, "Left");
                    ItemList = ProcessSide(ItemList, pdPage.ID, "Right");
                    NewPage.content = ItemList;

                    // Add New Pages
                    jsonPages.Add(NewPage);
                }
                jsonConfig.pages = jsonPages;
            }
        }

        private void PrepareKeybindJSON()
        {
            jsonKeybind = new RootKeybind();

            // Primary Meny Data
            jsonKeybind.modName = !String.IsNullOrEmpty(cMenu.PluginName) ? cMenu.PluginName : null;

            List<Keybinds> NewKeybindList = new List<Keybinds>();
            foreach (PageData pPage in cMenu.Pages)
            {
                foreach (ItemData iData in pPage.Items)
                {
                    if (iData.ItemType != "keymap" || String.IsNullOrEmpty(iData.ActionType))
                        continue;

                    Keybinds NewKeybind = new Keybinds();
                    NewKeybind.id = iData.ItemID;
                    NewKeybind.desc = iData.HotkeyDescription;

                    // Action Data
                    KeybindAction NewAction = new KeybindAction();
                    NewAction.type = !String.IsNullOrEmpty(iData.ActionType) ? iData.ActionType : null;
                    NewAction.form = !String.IsNullOrEmpty(iData.ActionForm) ? iData.ActionForm : null;
                    NewAction.scriptName = !String.IsNullOrEmpty(iData.ActionScript) ? iData.ActionScript : null;
                    NewAction.function = !String.IsNullOrEmpty(iData.ActionFunction) ? iData.ActionFunction : null;
                    NewAction.command = !String.IsNullOrEmpty(iData.ActionCommand) ? iData.ActionCommand : null;

                    // Action Parameters
                    if (iData.ActionParameters.Count > 0)
                    {
                        List<object> NewActionParameters = new List<object>();
                        foreach (ItemListData acti in iData.ActionParameters)
                        {
                            if (Int32.TryParse(acti.Parameter, out int k))
                                NewActionParameters.Add(Int32.Parse(acti.Parameter));

                            else if (Double.TryParse(acti.Parameter, out double l))
                                NewActionParameters.Add(Double.Parse(acti.Parameter));

                            else if (Boolean.TryParse(acti.Parameter, out bool m))
                                NewActionParameters.Add(Boolean.Parse(acti.Parameter));

                            else
                                NewActionParameters.Add(acti.Parameter);
                        }
                        NewAction.parameters = NewActionParameters;
                    }

                    // Add the action of there is one
                    NewKeybind.action = NewAction;

                    // Add the Keybind
                    NewKeybindList.Add(NewKeybind);
                }
            }

            // If we have keybind to add we will, otherwise null out the list
            if (NewKeybindList.Count > 0)
                jsonKeybind.keybinds = NewKeybindList;
            else
                jsonKeybind = null;
        }

        private List<Content> ProcessSide(List<Content> WorkingList, int iID, string sSide)
        {
            int iIndex = 0;
            foreach (ItemData idData in cMenu.Pages.Find(x => x.ID == iID).GetMenuSide(sSide))
            {
                //============================================================================
                // Primary Item Data
                //============================================================================
                Content NewContent = new Content();
                NewContent.type = idData.ItemType;
                NewContent.text = !String.IsNullOrEmpty(idData.ItemText) ? idData.ItemTextTranslated : null;
                NewContent.id = !String.IsNullOrEmpty(idData.ItemID) ? idData.ItemID : null;
                NewContent.help = !String.IsNullOrEmpty(idData.HelpText) ? idData.HelpTextTranslated : null;
                NewContent.position = sSide == "Right" && iIndex == 0 ? 1 : default(int?);

                //============================================================================
                // Hotkey
                //============================================================================
                if (idData.ItemType == "keymap")
                    NewContent.ignoreConflicts = idData.IgnoreConflict;

                //============================================================================
                // Group Control
                //============================================================================
                NewContent.groupControl = Int32.TryParse(idData.GroupControl, out int j) ? Int32.Parse(idData.GroupControl) : default(int?);
                NewContent.groupBehavior = idData.GroupBehavior != "" ? idData.GroupBehavior : null;

                // Group Condition based on Check Boxes
                if (idData.GroupCondition.Count > 0)
                {
                    // Get list of values
                    List<int> iGroupCondition = new List<int>();
                    foreach (string sCondition in idData.GroupCondition)
                        iGroupCondition.Add(Int32.Parse(sCondition));

                    // Add group depending on what options are selected
                    GroupCondition NewCondition = new GroupCondition();
                    if (idData.GroupTrue || idData.GroupTrueOR)
                        NewContent.groupCondition = iGroupCondition;
                    else
                    {
                        if (idData.GroupFalse)
                            NewCondition.NOTParameters = iGroupCondition;
                        else if (idData.GroupTrueAND)
                            NewCondition.ANDParameters = iGroupCondition;
                        else if (idData.GroupTrueONLY)
                            NewCondition.ONLYParameters = iGroupCondition;

                        // Add the new condition
                        NewContent.groupCondition = NewCondition;
                    }
                }

                //============================================================================
                // Action
                //============================================================================
                if (idData.ItemType != "keymap")
                {
                    Action NewAction = new Action();
                    NewAction.type = !String.IsNullOrEmpty(idData.ActionType) ? idData.ActionType : null;
                    NewAction.form = !String.IsNullOrEmpty(idData.ActionForm) ? idData.ActionForm : null;
                    NewAction.scriptName = !String.IsNullOrEmpty(idData.ActionScript) ? idData.ActionScript : null;
                    NewAction.function = !String.IsNullOrEmpty(idData.ActionFunction) ? idData.ActionFunction : null;

                    // Action Parameters
                    if (idData.ActionParameters.Count > 0)
                    {
                        List<object> NewActionParameters = new List<object>();
                        foreach (ItemListData acti in idData.ActionParameters)
                        {
                            if (Int32.TryParse(acti.Parameter, out int k))
                                NewActionParameters.Add(Int32.Parse(acti.Parameter));

                            else if (Double.TryParse(acti.Parameter, out double l))
                                NewActionParameters.Add(Double.Parse(acti.Parameter));

                            else if (Boolean.TryParse(acti.Parameter, out bool m))
                                NewActionParameters.Add(Boolean.Parse(acti.Parameter));

                            else
                                NewActionParameters.Add(acti.Parameter);
                        }
                        NewAction.parameters = NewActionParameters;
                    }

                    // Add the action of there is one
                    if (!String.IsNullOrEmpty(idData.ActionType))
                        NewContent.action = NewAction;
                }
                //============================================================================
                // Value Options
                //============================================================================
                ValueOptions NewValues = new ValueOptions();
                NewValues.sourceType = !String.IsNullOrEmpty(idData.SourceType) ? idData.SourceType : null;

                // Default Value
                if (!String.IsNullOrEmpty(idData.DefaultValue))
                {
                    if ((idData.SourceType.Contains("Int") || idData.ItemID.StartsWith("i") || idData.ItemID.StartsWith("u")) && Int32.TryParse(idData.DefaultValue, out int n))
                        NewValues.defaultValue = Int32.Parse(idData.DefaultValue);
                    else if ((idData.SourceType.Contains("Float") || idData.ItemID.StartsWith("f")) && Double.TryParse(idData.DefaultValue, out double o))
                        NewValues.defaultValue = Double.Parse(idData.DefaultValue);
                    else if ((idData.SourceType.Contains("Bool") || idData.ItemID.StartsWith("b")) && Boolean.TryParse(idData.DefaultValue, out bool p))
                        NewValues.defaultValue = Boolean.Parse(idData.DefaultValue);
                    else
                        NewValues.defaultValue = idData.DefaultValue.ToString();
                }

                // Property Info
                NewValues.propertyName = !String.IsNullOrEmpty(idData.PropertyName) ? idData.PropertyName : null;
                NewValues.sourceForm = !String.IsNullOrEmpty(idData.SourceForm) ? idData.SourceForm : null;
                NewValues.scriptName = !String.IsNullOrEmpty(idData.SourceScript) ? idData.SourceScript : null;

                // Slider Values
                NewValues.min = Double.TryParse(idData.Min, out double x) ? Double.Parse(idData.Min) : default(double?);
                NewValues.max = Double.TryParse(idData.Max, out double y) ? Double.Parse(idData.Max) : default(double?);
                NewValues.step = Double.TryParse(idData.Step, out double z) ? Double.Parse(idData.Step) : default(double?);
                NewValues.formatString = !String.IsNullOrEmpty(idData.Format) ? idData.Format : null;

                // Menu - Enumerator - Stepper
                NewValues.options = idData.GetListNullable("Options") != null ? idData.GetList("Options") : null;
                NewValues.shortNames = idData.GetListNullable("Names") != null ? idData.GetList("Names") : null;

                // Add value options for those that need it
                if (NewValues.sourceType == null && NewValues.defaultValue == null && NewValues.min == null && NewValues.options == null && NewValues.value == null)
                    NewContent.valueOptions = null;
                else if (idData.ItemType == "keymap" && !String.IsNullOrEmpty(idData.ActionType))
                    NewContent.valueOptions = null;
                else
                    NewContent.valueOptions = NewValues;

                //============================================================================
                // Clean-up
                //============================================================================
                iIndex++;
                WorkingList.Add(NewContent);
            }

            return WorkingList;
        }
        #endregion

        #region Deserialize JSON
        private void DeserializeJSON(Root rJSON)
        {
            cMenu = new MenuData();

            // Primary Meny Data
            cMenu.PluginName = rJSON.modName != null ? rJSON.modName : null; ;
            cMenu.DisplayName = rJSON.displayName != null ? rJSON.displayName : null; ;
            cMenu.MCMVersion = rJSON.minMcmVersion != null ? rJSON.minMcmVersion.ToString() : null;

            // Custom Content
            if (rJSON.customContent != null)
            {
                cMenu.CustomContent = rJSON.customContent.source != null ? rJSON.customContent.source : null;
                cMenu.X = rJSON.customContent.x != null ? rJSON.customContent.x.ToString() : null;
                cMenu.Y = rJSON.customContent.y != null ? rJSON.customContent.y.ToString() : null;
            }

            // Required Plugins
            if (rJSON.pluginRequirements != null)
            {
                foreach (string sPlugin in rJSON.pluginRequirements)
                    cMenu.RequiredPlugins.Add(new ItemListData() { ID = cMenu.GetNewPlugin(), PluginName = sPlugin });
            }

            // Process single or multiple pages
            if (rJSON.content != null && rJSON.content.Count > 0)
            {
                // Content Data
                cMenu.MultiplePages = true;
                cMenu.FillMode = "topToBottom"; //rJSON.cursorFillMode != null ? rJSON.cursorFillMode : "topToBottom";

                // New Page
                PageData pData = new PageData() { ID = 0 };

                // Item Variables
                List<ItemData> ItemList = new List<ItemData>();
                int iContentIndex = 1;
                bool bNextSide = false;

                // Item Data
                if (rJSON.content != null && rJSON.content.Count > 0)
                {
                    foreach (Content cContent in rJSON.content)
                    {
                        ItemList.Add(DeserializeAddItem(cContent, iContentIndex, ref bNextSide));
                        iContentIndex++;
                    }
                }

                // Add the page data
                pData.Items = ItemList;
                cMenu.Pages.Add(pData);
            }
            else if (rJSON.pages != null && rJSON.pages.Count > 0)
            {
                // Page and Item Variables
                List<PageData> PageList = new List<PageData>();

                int iPageIndex = 1;
                int iContentIndex = 1;
                bool bNextSide = false;

                // Loop through each page
                foreach (Page pPage in rJSON.pages)
                {
                    PageData NewPage = new PageData();
                    NewPage.ID = iPageIndex;
                    NewPage.DisplayName = pPage.pageDisplayName;
                    NewPage.FillMode = "topToBottom"; //pPage.cursorFillMode;
                    List<ItemData> ItemList = new List<ItemData>();

                    // Item Data
                    if (pPage.content != null && pPage.content.Count > 0)
                    {
                        foreach (Content cContent in pPage.content)
                        {
                            ItemList.Add(DeserializeAddItem(cContent, iContentIndex, ref bNextSide));
                            iContentIndex++;
                        }
                    }
                    NewPage.Items = ItemList;

                    // Add the Page
                    bNextSide = false;
                    PageList.Add(NewPage);
                    iPageIndex++;
                }
                cMenu.Pages = PageList;
            }


        }

        private ItemData DeserializeAddItem(Content cContent, int iContentIndex, ref bool bNextSide)
        {
            ItemData NewItem = new ItemData();
            // Page Data
            if (cContent.position == 1) bNextSide = true;
            NewItem.ItemSide = bNextSide ? "Right" : "Left";

            // Item Data
            NewItem.ID = iContentIndex;
            NewItem.ItemType = cContent.type != null ? cContent.type : null;
            NewItem.ItemText = cContent.text != null ? cContent.text : null;
            NewItem.ItemID = cContent.id != null ? cContent.id : null;
            NewItem.HelpText = cContent.help != null ? cContent.help : null;

            // Group Control/Behavior
            NewItem.GroupControl = cContent.groupControl != null ? cContent.groupControl.ToString() : null;
            NewItem.GroupBehavior = cContent.groupBehavior != null ? cContent.groupBehavior.ToString() : null;

            // Null Item
            if (cContent.groupCondition == null) { }

            // Int Condition
            else if (Int32.TryParse(cContent.groupCondition.ToString(), out int n))
            {
                NewItem.GroupTrue = true;
                NewItem.GroupCondition.Add(cContent.groupCondition.ToString());
            }

            // Int List Condition
            else if (cContent.groupCondition is List<int>)
            {
                List<int> ConditionList = (List<int>)cContent.groupCondition;
                if (ConditionList.Count == 1)
                    NewItem.GroupTrue = true;
                else
                    NewItem.GroupTrueOR = true;

                // Add conditions
                foreach (int i in ConditionList)
                    NewItem.GroupCondition.Add(i.ToString());
            }

            // Complex Condition
            else if (cContent.groupCondition is GroupCondition)
            {
                GroupCondition GetCondition = (GroupCondition)cContent.groupCondition;

                if (GetCondition.NOTParameters.Count > 0)
                {
                    NewItem.GroupFalse = true;
                    foreach (int i in GetCondition.NOTParameters)
                        NewItem.GroupCondition.Add(i.ToString());
                }
                else if (GetCondition.ANDParameters.Count > 0)
                {
                    NewItem.GroupTrueAND = true;
                    foreach (int i in GetCondition.ANDParameters)
                        NewItem.GroupCondition.Add(i.ToString());
                }
                else if (GetCondition.ONLYParameters.Count > 0)
                {
                    NewItem.GroupTrueONLY = true;
                    foreach (int i in GetCondition.ONLYParameters)
                        NewItem.GroupCondition.Add(i.ToString());
                }
                else if (GetCondition.ORParameters.Count > 0)
                {
                    NewItem.GroupTrueOR = true;
                    foreach (int i in GetCondition.ORParameters)
                        NewItem.GroupCondition.Add(i.ToString());
                }
            }

            // Value Options
            if (cContent.valueOptions != null)
            {
                NewItem.SourceType = cContent.valueOptions.sourceType != null ? cContent.valueOptions.sourceType : null;
                NewItem.DefaultValue = cContent.valueOptions.defaultValue != null ? cContent.valueOptions.defaultValue.ToString() : null;
                NewItem.Min = cContent.valueOptions.min != null ? cContent.valueOptions.min.ToString() : null;
                NewItem.Max = cContent.valueOptions.max != null ? cContent.valueOptions.max.ToString() : null;
                NewItem.Step = cContent.valueOptions.step != null ? cContent.valueOptions.step.ToString() : null;
                NewItem.Format = cContent.valueOptions.formatString != null ? cContent.valueOptions.formatString : null;
                NewItem.SourceForm = cContent.valueOptions.sourceForm != null ? cContent.valueOptions.sourceForm : null;
                NewItem.SourceScript = cContent.valueOptions.scriptName != null ? cContent.valueOptions.scriptName : null;
                NewItem.PropertyName = cContent.valueOptions.propertyName != null ? cContent.valueOptions.propertyName : null;
                NewItem.IgnoreConflict = cContent.ignoreConflicts != null ? Boolean.Parse(cContent.ignoreConflicts.ToString()) : default(bool);

                // Menu - Enum - Step
                if (cContent.valueOptions.options != null)
                {
                    for (int i = 0; i < cContent.valueOptions.options.Count; i++)
                    {
                        ItemListData idMenuData = new ItemListData();
                        idMenuData.ID = i;
                        idMenuData.DisplayOrder = i;
                        idMenuData.Options = cContent.valueOptions.options[i] != null ? cContent.valueOptions.options[i] : null;

                        if (cContent.valueOptions.shortNames != null)
                            idMenuData.ShortName = cContent.valueOptions.shortNames[i] != null ? cContent.valueOptions.shortNames[i] : null;

                        NewItem.MenuEnumStep.Add(idMenuData);
                    }
                }
            }

            // Action
            if (cContent.action != null)
            {
                NewItem.ActionType = cContent.action.type != null ? cContent.action.type : null;
                NewItem.ActionForm = cContent.action.form != null ? cContent.action.form : null;
                NewItem.ActionScript = cContent.action.scriptName != null ? cContent.action.scriptName : null;
                NewItem.ActionFunction = cContent.action.function != null ? cContent.action.function : null;

                // Action Parameters
                if (cContent.action.parameters != null)
                {
                    for (int i = 0; i < cContent.action.parameters.Count; i++)
                    {
                        ItemListData idActionData = new ItemListData();
                        idActionData.ID = i;
                        idActionData.DisplayOrder = i;
                        idActionData.Parameter = cContent.action.parameters[i].ToString() != "" ? cContent.valueOptions.options[i].ToString() : null;
                        NewItem.ActionParameters.Add(idActionData);
                    }
                }
            }

            // Return the packaged item
            return NewItem;
        }
        #endregion

        #region Settings File
        private void ProcessSettings()
        {
            sSettings = new List<SettingData>();

            // Process setting file
            foreach (PageData pPage in cMenu.Pages)
            {
                foreach (ItemData item in pPage.Items)
                {
                    if (item.ItemID is null || item.ItemID == "")
                        continue;

                    if (item.ItemID.Contains(":"))
                    {
                        SettingData NewSetting = new SettingData();

                        string[] sSplitID = item.ItemID.Split(':');

                        if (sSettings.FindIndex(x => x.SettingCategory == sSplitID[1]) < 0)
                        {
                            SettingValues NewValues = new SettingValues();
                            NewValues.SettingName = sSplitID[0];
                            NewValues.SettingValue = item.DefaultValue;
                            NewSetting.SettingCategory = sSplitID[1];
                            NewSetting.ValueSettings.Add(NewValues);

                            sSettings.Add(NewSetting);
                        }
                        else
                        {
                            SettingValues NewValues = new SettingValues();
                            NewValues.SettingName = sSplitID[0];
                            NewValues.SettingValue = item.DefaultValue;
                            sSettings.Find(x => x.SettingCategory == sSplitID[1]).ValueSettings.Add(NewValues);
                        }
                    }
                }
            }
        }

        private string ReturnSettings()
        {
            // Create initial Settings file
            string sINIFile = @";This file contains default settings" + Environment.NewLine;
            sINIFile += @";Do not make changes to this file. Your changes will be lost if this file is updated" + Environment.NewLine;
            sINIFile += @";To make changes, save a copy of this file here: Data\MCM\Settings\" + Environment.NewLine;
            sINIFile += Environment.NewLine;

            // Loop through all the settings
            foreach (SettingData sData in sSettings)
            {
                sINIFile += "[" + sData.SettingCategory + "]";
                sINIFile += Environment.NewLine;
                foreach (SettingValues sValue in sData.ValueSettings)
                {
                    sINIFile += sValue.SettingName + "=" + sValue.SettingValue;
                    sINIFile += Environment.NewLine;
                }
                sINIFile += Environment.NewLine;
            }

            return sINIFile;
        }
        #endregion

        #region Translation File
        private void PrepareTranslation()
        {
            tTranslation = new List<TranslationData>();
            string sTranslationValue = "";

            if (!String.IsNullOrEmpty(cMenu.DisplayNameTranslated) && !String.IsNullOrEmpty(cMenu.DisplayName) && !tTranslation.Exists(x => x.tTitle == cMenu.DisplayNameTranslated))  
                tTranslation.Add(new TranslationData() { tTitle = cMenu.DisplayNameTranslated, tValue = cMenu.DisplayName });

            foreach (PageData pPage in cMenu.Pages)
            {
                if (!String.IsNullOrEmpty(pPage.DisplayNameTranslated) && !String.IsNullOrEmpty(pPage.DisplayName) && !tTranslation.Exists(x => x.tTitle == pPage.DisplayNameTranslated))
                    tTranslation.Add(new TranslationData() { tTitle = pPage.DisplayNameTranslated, tValue = pPage.DisplayName });

                foreach (ItemData iData in pPage.Items)
                {

                    if (!String.IsNullOrEmpty(iData.ItemTextTranslated) && !String.IsNullOrEmpty(iData.ItemText) && !tTranslation.Exists(x => x.tTitle == iData.ItemTextTranslated))
                    {
                        // Do we have color?
                        if (!String.IsNullOrEmpty(iData.ItemColor) && iData.ItemColor != "FFFFFF")
                            sTranslationValue = "<font color='#" + iData.ItemColor + "'>" + iData.ItemText + "</font>";
                        else
                            sTranslationValue = iData.ItemText;

                        // Insert the record
                        tTranslation.Add(new TranslationData() { tTitle = iData.ItemTextTranslated, tValue = sTranslationValue });
                    }
                    if (!String.IsNullOrEmpty(iData.HelpTextTranslated) && !String.IsNullOrEmpty(iData.HelpText) && !tTranslation.Exists(x => x.tTitle == iData.HelpTextTranslated))
                        tTranslation.Add(new TranslationData() { tTitle = iData.HelpTextTranslated, tValue = iData.HelpText });

                    foreach (ItemListData ilData in iData.MenuEnumStep)
                    {
                        if (!String.IsNullOrEmpty(ilData.ShortNameTranslated) && !String.IsNullOrEmpty(ilData.ShortName) && !tTranslation.Exists(x => x.tTitle == ilData.ShortNameTranslated))
                        {
                            // Do we have color?
                            if (!String.IsNullOrEmpty(ilData.OptionColor) && ilData.OptionColor != "FFFFFF")
                                sTranslationValue = "<font color='#" + ilData.OptionColor + "'>" + ilData.ShortName + "</font>";
                            else
                                sTranslationValue = ilData.ShortName;

                            // Insert the record
                            tTranslation.Add(new TranslationData() { tTitle = ilData.ShortNameTranslated, tValue = sTranslationValue });
                        }
                        else if (!String.IsNullOrEmpty(ilData.OptionsTranslated) && !String.IsNullOrEmpty(ilData.Options) && !tTranslation.Exists(x => x.tTitle == ilData.OptionsTranslated))
                        {
                            // Do we have color?
                            if (!String.IsNullOrEmpty(ilData.OptionColor) && ilData.OptionColor != "FFFFFF")
                                sTranslationValue = "<font color='#" + ilData.OptionColor + "'>" + ilData.Options + "</font>";
                            else
                                sTranslationValue = ilData.Options;

                            // Insert the record
                            tTranslation.Add(new TranslationData() { tTitle = ilData.OptionsTranslated, tValue = sTranslationValue });
                        }
                    }
                }
            }

            // Add additional translations
            foreach(TranslationData td in cMenu.ExtraTranslations)
                tTranslation.Add(new TranslationData() { tTitle = td.tTitle, tValue = td.tValue });
        }

        private string TranslationFile()
        {
            string sTranslation = "";
            foreach (TranslationData t in tTranslation)
                sTranslation += t.tTitle + "\t" + t.tValue + Environment.NewLine;

            return sTranslation;
        }

        private string TranslateString(string sToTranslate, string sTranslationType, string sExtraText = "")
        {
            // If there is no text to translate or its already a translation string
            if (String.IsNullOrEmpty(sToTranslate) || sToTranslate.StartsWith("$"))
                return "";

            // If there is no dash, add it
            if (!String.IsNullOrEmpty(sExtraText))
                sExtraText = "_" + sExtraText.Replace(" ", "");

            // Setup translation text
            string sTranslatedTitle = "$" + sToTranslate.Replace(" ", "") + sExtraText;

            // Add Suffix
            if (sTranslationType == "PageName")
                sTranslatedTitle += "_Page";
            else if (sTranslationType == "HelpName")
                sTranslatedTitle += "_Help";
            else if (sTranslationType == "OptionName")
                sTranslatedTitle += "_Option";

            // Return Translation
            return sTranslatedTitle;
        }

        #endregion
    }
}
