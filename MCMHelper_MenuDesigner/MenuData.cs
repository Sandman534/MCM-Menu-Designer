using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Drawing;

namespace MCMHelper_MenuDesigner
{
    [Serializable()]
    internal class MenuData
    {
        // MCM Main Menu Data
        public List<PageData> Pages;
        public string PluginName;
        public string DisplayName;
        public string DisplayNameTranslated;
        public string CustomContent;
        public string X;
        public string Y;
        public string MCMVersion;
        public string FillMode;
        public bool MultiplePages;
        public List<ItemListData> RequiredPlugins;

        // Extra Translations
        public List<TranslationData> ExtraTranslations;

        public MenuData()
        {
            RequiredPlugins = new List<ItemListData>();
            Pages = new List<PageData>();
            ExtraTranslations = new List<TranslationData>();
            Pages.Add(new PageData() { ID = 0 } );
            FillMode = "topToBottom";
        }

        public List<string> GetStringList(string sType)
        {
            List<string> ReturnList = new List<string>();
            if (sType == "RequiredPlugin")
            {
                foreach (ItemListData ild in RequiredPlugins)
                    ReturnList.Add(ild.PluginName);
            }

            return ReturnList;
        }

        public ListViewItem[] PageList()
        {
            if (Pages.Count < 0)
                return null;

            // Account for an index of 0... Just in case
            ListViewItem[] AllPageList;
            if (Pages.FindIndex(x => x.ID == 0) >= 0)
                AllPageList = new ListViewItem[Pages.Count - 1];
            else
                AllPageList = new ListViewItem[Pages.Count];

            int iIndex = 0;
            foreach (PageData pd in Pages)
            {
                // Dont process index 0
                if (pd.ID == 0)
                    continue;

                ListViewItem NewPage = new ListViewItem(pd.ID.ToString());
                NewPage.SubItems.Add(pd.DisplayName);
                AllPageList[iIndex] = NewPage;
                iIndex++;
            }

            return AllPageList;
        }

        public ListViewItem[] TranslationList()
        {
            if (ExtraTranslations.Count < 0)
                return null;

            // Account for an index of 0... Just in case
            ListViewItem[] AllTranslationList;
            AllTranslationList = new ListViewItem[ExtraTranslations.Count];

            int iIndex = 0;
            foreach (TranslationData tr in ExtraTranslations)
            {
                ListViewItem NewTranslation = new ListViewItem(tr.tTitle);
                NewTranslation.SubItems.Add(tr.tValue);
                AllTranslationList[iIndex] = NewTranslation;
                iIndex++;
            }

            return AllTranslationList;
        }

        public void PageOrder(ListView.ListViewItemCollection WorkingItem)
        {
            // Set the Display Order
            foreach (ListViewItem lvi in WorkingItem)
                Pages.Find(x => x.ID == Int32.Parse(lvi.Text)).DisplayOrder = lvi.Index;

            // Sort the List
            Pages.Sort((x, y) => x.DisplayOrder.CompareTo(y.DisplayOrder));
        }

        public void RemovePage(int iID)
        {
            Pages.RemoveAt(Pages.FindIndex(x => x.ID == iID));
        }

        public void RemovePlugin(int iID)
        {
            RequiredPlugins.RemoveAt(RequiredPlugins.FindIndex(x => x.ID == iID));
        }

        public int GetNewPlugin()
        {
            if (RequiredPlugins.Count > 0)
                return RequiredPlugins.Max(x => x.ID) + 1;
            else
                return 1;
        }

        public int GetNewPage()
        {
            if (Pages.Count > 0)
                return Pages.Max(x => x.ID) + 1;
            else
                return 1;
        }
    }

    [Serializable()]
    internal class PageData
    {
        public int ID;
        public int DisplayOrder;

        public string DisplayName;
        public string DisplayNameTranslated;
        public string FillMode;

        public List<ItemData> Items;

        public PageData()
        {
            Items = new List<ItemData>();
        }

        public ListViewItem[] ItemList(string sSide)
        {
            List<ItemData> AllSide = Items.FindAll(x => x.ItemSide == sSide);

            ListViewItem[] AllSideList = new ListViewItem[AllSide.Count];
            int iIndex = 0;
            foreach (ItemData asi in AllSide)
            {
                ListViewItem NewItem = new ListViewItem(asi.ID.ToString());
                NewItem.SubItems.Add(asi.ItemText);
                NewItem.SubItems.Add(asi.ItemType);
                NewItem.SubItems.Add(asi.GroupControl);
                NewItem.SubItems.Add(asi.GetConditionList());
                AllSideList[iIndex] = NewItem;
                iIndex++;
            }

            return AllSideList;
        }

        public int GetItemID()
        {
            if (Items.Count > 0)
                return Items.Max(x => x.ID) + 1;
            else
                return 1;
        }

        public ItemData GetItem(int ItemID)
        {
            ItemData CurrentItem = Items.Find(x => x.ID == ItemID);
            return CurrentItem;
        }

        public List<ItemData> GetMenuSide(string sSide)
        {
            return Items.FindAll(x => x.ItemSide == sSide);
        }

        public void RemoveItem(int iID)
        {
            Items.RemoveAt(Items.FindIndex(x => x.ID == iID));
        }

        public void ItemOrder(ListView.ListViewItemCollection WorkingItem)
        {
            // Reset the Display Order
            foreach (ListViewItem lvi in WorkingItem)
                Items.Find(x => x.ID == Int32.Parse(lvi.Text)).DisplayOrder = lvi.Index;

            // Sort Item List
            Items.Sort((x, y) => x.DisplayOrder.CompareTo(y.DisplayOrder));
        }

        public void AddNewItem(string sSide, string sText)
        {
            Items.Add(new ItemData() { ItemSide = sSide, ID = GetItemID(), ItemType = sText });
        }
    }

    [Serializable()]
    internal class ItemData
    {
        public int ID;
        public int DisplayOrder;

        // Page Data
        public string ItemSide;

        // Primary Item Data
        public string ItemType;
        public string ItemID;
        public string ItemText;
        public string ItemTextTranslated;
        public string ItemColor;
        public string HelpText;
        public string HelpTextTranslated;

        // Value Options
        public string SourceType;
        public string DefaultValue;
        public string PropertyName;
        public string SourceForm;
        public string SourceScript;

        // Menu - Enum - Stepper
        public List<ItemListData> MenuEnumStep;

        // Slider
        public string Min;
        public string Max;
        public string Step;
        public string Format;

        // Hotkey
        public string HotkeyDescription;
        public bool IgnoreConflict;

        // Group Control
        public string GroupControl;
        public string GroupBehavior;
        public List<string> GroupCondition;
        public bool GroupTrue;
        public bool GroupTrueAND;
        public bool GroupTrueOR;
        public bool GroupTrueONLY;
        public bool GroupFalse;

        // Action
        public string ActionType;
        public string ActionForm;
        public string ActionScript;
        public string ActionFunction;
        public string ActionCommand;
        public List<ItemListData> ActionParameters;

        public ItemData()
        {
            GroupCondition = new List<string>();
            MenuEnumStep = new List<ItemListData>();
            ActionParameters = new List<ItemListData>();
        }

        public List<string> GetListNullable(string sListType)
        {
            List<string> ReturnList = new List<string>();

            if (sListType == "Options")
            {
                foreach (ItemListData ild in MenuEnumStep)
                {
                    if (ild.Options == null)
                        ReturnList.Add("");
                    else
                        ReturnList.Add(ild.Options);
                }
            }
            else if (sListType == "Names")
            {
                foreach (ItemListData ild in MenuEnumStep)
                {
                    if (ild.ShortName == null)
                        ReturnList.Add("");
                    else
                        ReturnList.Add(ild.ShortName);
                }

            }

            // If there are no valid values, return null
            if (ReturnList.FindAll(x => x.Length > 0).Count == 0)
                return null;
            else
                return ReturnList;
        }

        public List<string> GetList(string sListType)
        {
            List<string> ReturnList = new List<string>();

            if (sListType == "Options")
            {
                foreach (ItemListData ild in MenuEnumStep)
                {
                    if (String.IsNullOrEmpty(ild.OptionsTranslated) && String.IsNullOrEmpty(ild.Options))
                        ReturnList.Add("");
                    else if (String.IsNullOrEmpty(ild.OptionsTranslated))
                        ReturnList.Add(ild.Options);
                    else
                        ReturnList.Add(ild.OptionsTranslated);
                }
            }
            else if (sListType == "Names")
            {
                foreach (ItemListData ild in MenuEnumStep)
                {
                    if (String.IsNullOrEmpty(ild.ShortNameTranslated) && String.IsNullOrEmpty(ild.ShortName))
                        ReturnList.Add("");
                    else if (String.IsNullOrEmpty(ild.ShortNameTranslated))
                        ReturnList.Add(ild.ShortName);
                    else
                        ReturnList.Add(ild.ShortNameTranslated);
                }
            }

            return ReturnList;
        }

        public int GetNewID(string sType)
        {
            // Default value to 1
            int iReturnValue = 1;

            // Get new ID depending on sType
            if (sType == "MenuList" && MenuEnumStep.Count > 0)
                iReturnValue = MenuEnumStep.Max(x => x.ID) + 1;
            else if (sType == "ActionParameters" && ActionParameters.Count > 0)
                iReturnValue = ActionParameters.Max(x => x.ID) + 1;

            // Return the New ID
            return iReturnValue;
        }

        public string GetConditionList()
        {
            if (GroupCondition.Count > 0)
                return GroupCondition.Aggregate((a, b) => a + "," + b);
            else
                return "";
        }

        public ListViewItem[] ItemList(string sType)
        {
            ListViewItem[] ItemListView = new ListViewItem[0];
            int iIndex = 0;

            if (sType == "MenuList")
            {
                ItemListView = new ListViewItem[MenuEnumStep.Count];
                foreach (ItemListData mes in MenuEnumStep)
                {
                    ListViewItem NewItem = new ListViewItem(mes.Options);
                    NewItem.SubItems.Add(mes.ShortName);
                    NewItem.BackColor = String.IsNullOrEmpty(mes.OptionColor) ? Color.White : ColorTranslator.FromHtml("#" + mes.OptionColor);
                    ItemListView[iIndex] = NewItem;
                    iIndex++;
                }
            }
            else if (sType == "ActionParameters")
            {
                ItemListView = new ListViewItem[ActionParameters.Count];
                foreach (ItemListData mes in ActionParameters)
                {
                    ListViewItem NewItem = new ListViewItem(mes.Parameter);
                    ItemListView[iIndex] = NewItem;
                    iIndex++;
                }
            }

            return ItemListView;
        }
    }

    [Serializable()]
    internal class ItemListData
    {
        public int ID;
        public int DisplayOrder;

        // Menu/Stepper/Enum
        public string Options;
        public string OptionsTranslated;
        public string ShortName;
        public string ShortNameTranslated;
        public string OptionColor;

        // Required Plugin
        public string PluginName;

        // Action Parameter
        public string Parameter;
    }

    [Serializable()]
    internal class MenuItems
    {
        public List<ItemListData> MenuData;

        public MenuItems()
        {
            MenuData = new List<ItemListData>();
        }
    }

    [Serializable()]
    internal class TranslationData
    {
        public string tTitle;
        public string tValue;
    }

    internal class SettingData
    {
        public string SettingCategory;
        public List<SettingValues> ValueSettings;

        public SettingData()
        {
            ValueSettings = new List<SettingValues>();
        }
    }

    internal class SettingValues
    {
        public string SettingName;
        public string SettingValue;
    }

    
}
