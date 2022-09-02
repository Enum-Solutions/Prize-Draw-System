using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrizeDrawSystem.Models
{
    public class CommonOperations
    {
        public List<double> LoadConfigurations(string siteURL, string drawName)
        {
            using (SPSite site = new SPSite(siteURL))
            {
                using (SPWeb spWeb = site.OpenWeb())
                {
                    List<double> values = new List<double>();
                    SPList spList = spWeb.Lists.TryGetList("Draw Configurations");
                    if (spList != null)
                    {
                        SPQuery qry = new SPQuery();
                        qry.Query =
                        @"   <Where>
                        <Eq>
                            <FieldRef Name='Draw' />
                            <Value Type='Choice'>" + drawName + @"</Value>
                        </Eq>
                    </Where>";
                        SPListItemCollection listItems = spList.GetItems(qry);

                        if (listItems != null && listItems.Count > 0)
                        {
                            SPItem item = listItems[0];

                            if (item != null)
                            {
                                values.Add(Convert.ToDouble(item["Winning_x0020_Amount"]));
                                values.Add(Convert.ToDouble(item["Total_x0020_Winners"]));
                                values.Add(Convert.ToDouble(item["Criteria_x0020__x0028_OMR_x0029_"]));
                            }
                        }
                    }

                    return values;
                }
            }

        }

        public bool GetDrawButtonStatus(string siteURL, string drawCategory)
        {
            bool status = false;

            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                using (SPSite site = new SPSite(siteURL))
                {
                    using (SPWeb spWeb = site.OpenWeb())
                    {
                        SPList spList = spWeb.Lists.TryGetList("Draw Settings");
                        if (spList != null)
                        {
                            SPQuery qry = new SPQuery();

                            if (drawCategory == "New Salary Account")
                            {
                                qry.Query =
                                @"   <Where>
                                        <Eq>
                                            <FieldRef Name='Title' />
                                            <Value Type='Text'>Show New Salary Account Draw Button</Value>
                                        </Eq>
                                     </Where>";
                            }
                            else if (drawCategory == "POS Machine")
                            {
                                qry.Query =
                                @"   <Where>
                                        <Eq>
                                            <FieldRef Name='Title' />
                                            <Value Type='Text'>Show POS Machine Draw Button</Value>
                                        </Eq>
                                     </Where>";
                            }
                            else if (drawCategory == "Bills")
                            {
                                qry.Query =
                                @"   <Where>
                                        <Eq>
                                            <FieldRef Name='Title' />
                                            <Value Type='Text'>Show Bills Draw Button</Value>
                                        </Eq>
                                     </Where>";
                            }

                            
                            qry.ViewFields = @"<FieldRef Name='Value' />";
                            SPListItemCollection listItems = spList.GetItems(qry);

                            if (listItems != null && listItems.Count > 0)
                            {
                                status = Convert.ToBoolean(listItems[0]["Value"]);
                            }
                        }
                    }
                }
            });

            return status;
        }

        public bool GetSliderStatus(string siteURL)
        {
            bool status = false;

            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                using (SPSite site = new SPSite(siteURL))
                {
                    using (SPWeb spWeb = site.OpenWeb())
                    {
                        SPList spList = spWeb.Lists.TryGetList("Draw Settings");
                        if (spList != null)
                        {
                            SPQuery qry = new SPQuery();

                            qry.Query =
                                @"  <Where>
                                      <Eq>
                                         <FieldRef Name='SliderShowDate' />
                                         <Value Type='DateTime'>
                                            <Today />
                                         </Value>
                                      </Eq>
                                   </Where>";


                            qry.ViewFields = @"<FieldRef Name='Value' />";
                            SPListItemCollection listItems = spList.GetItems(qry);

                            if (listItems != null && listItems.Count == 0)
                            {
                                status = true;

                                // Update status to hide slider
                                SPQuery qry2 = new SPQuery();
                                qry2.Query =
                                @"   <Where>
                                        <Eq>
                                            <FieldRef Name='Title' />
                                            <Value Type='Text'>Show Slider</Value>
                                        </Eq>
                                     </Where>";


                                qry2.ViewFields = @"<FieldRef Name='Value' />";
                                listItems = spList.GetItems(qry2);

                                if (listItems != null && listItems.Count > 0)
                                {
                                    spWeb.AllowUnsafeUpdates = true;
                                    SPListItem item = listItems[0];
                                    item["SliderShowDate"] = DateTime.Now;
                                    item.Update();
                                    spWeb.AllowUnsafeUpdates = false;
                                }
                            }

                        }
                    }
                }
            });

            return status;
        }

        public void UpdateDrawButtonStatus(string siteURL, bool status, string drawCategory)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                using (SPSite site = new SPSite(siteURL))
                {
                    using (SPWeb spWeb = site.OpenWeb())
                    {
                        spWeb.AllowUnsafeUpdates = true;
                        SPList spList = spWeb.Lists.TryGetList("Draw Settings");
                        if (spList != null)
                        {
                            SPQuery qry = new SPQuery();

                            if (drawCategory == "New Salary Account")
                            {
                                qry.Query =
                                @"   <Where>
                                        <Eq>
                                            <FieldRef Name='Title' />
                                            <Value Type='Text'>Show New Salary Account Draw Button</Value>
                                        </Eq>
                                     </Where>";
                            }
                            else if (drawCategory == "POS Machine")
                            {
                                qry.Query =
                                @"   <Where>
                                        <Eq>
                                            <FieldRef Name='Title' />
                                            <Value Type='Text'>Show POS Machine Draw Button</Value>
                                        </Eq>
                                     </Where>";
                            }
                            else if (drawCategory == "Bills")
                            {
                                qry.Query =
                                @"   <Where>
                                        <Eq>
                                            <FieldRef Name='Title' />
                                            <Value Type='Text'>Show Bills Draw Button</Value>
                                        </Eq>
                                     </Where>";
                            }

                            qry.ViewFields = @"<FieldRef Name='Value' />";
                            SPListItemCollection listItems = spList.GetItems(qry);

                            if (listItems != null && listItems.Count > 0)
                            {
                                SPListItem item = listItems[0];
                                item["Value"] = status;
                                item.Update();
                            }
                        }
                        spWeb.AllowUnsafeUpdates = false;
                    }
                }
            });
        }

        public void ArchiveAndDeleteAllListItems(string siteURL, string listFrom, string listTo)
        {
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    using (SPSite spSite = new SPSite(siteURL))
                    {
                        using (SPWeb spWeb = spSite.OpenWeb())
                        {
                            spWeb.AllowUnsafeUpdates = true;
                            SPList spListFrom = spWeb.Lists[listFrom];

                            if (spListFrom != null)
                            {
                                string listID = spListFrom.ID.ToString();
                                SPListItemCollection listFromItems = spListFrom.Items;

                                if (listFromItems != null && listFromItems.Count > 0)
                                {
                                    // Deleting all items
                                    int chunkSize = 500;

                                    int count = spListFrom.Items.Count;
                                    int loop = (int)Math.Ceiling(count / (float)chunkSize);

                                    int start = 0;
                                    int end = 0;

                                    for (int i = 1; i <= loop; i++)
                                    {
                                        if (i == loop)
                                        {
                                            end = count - 1;
                                        }
                                        else
                                        {
                                            end = chunkSize * i - 1;
                                        }

                                        StringBuilder deletebuilder = BatchCommand(listID, listFromItems, start, end);
                                        spSite.RootWeb.ProcessBatchData(deletebuilder.ToString());

                                        start += chunkSize;
                                    }


                                    // Moving all data to Archive
                                    SPList spListTo = spWeb.Lists[listTo];

                                    if (spListTo != null)
                                    {
                                        if (listFrom == "POS Machine Data")
                                        {
                                            foreach (SPListItem fromItem in listFromItems)
                                            {
                                                SPListItem archiveItem = spListTo.AddItem();
                                                archiveItem["Title"] = fromItem["Title"];
                                                archiveItem["BranchCode"] = fromItem["BranchCode"];
                                                archiveItem["Credit_x0020_Card_x0020_Number"] = fromItem["Credit_x0020_Card_x0020_Number"];
                                                archiveItem["POS_x0020_Transaction_x0020_Amou"] = fromItem["POS_x0020_Transaction_x0020_Amou"];
                                                archiveItem["Transaction_x0020_Date"] = fromItem["Transaction_x0020_Date"];
                                                archiveItem["Account_x0020_Number"] = fromItem["Account_x0020_Number"];
                                                archiveItem.Update();
                                            }
                                        }
                                        else if (listFrom == "New Salary Account Data")
                                        {
                                            foreach (SPListItem fromItem in listFromItems)
                                            {
                                                SPListItem archiveItem = spListTo.AddItem();
                                                archiveItem["Title"] = fromItem["Title"];
                                                archiveItem["BranchCode"] = fromItem["BranchCode"];
                                                archiveItem["Account_x0020_Type"] = fromItem["Account_x0020_Type"];
                                                archiveItem["Account_x0020_Opening_x0020_Date"] = fromItem["Account_x0020_Opening_x0020_Date"];
                                                archiveItem["Last_x0020_Salary_x0020_Transfer"] = fromItem["Last_x0020_Salary_x0020_Transfer"];
                                                archiveItem["Account_x0020_Number"] = fromItem["Account_x0020_Number"];
                                                archiveItem.Update();
                                            }
                                        }
                                        else if (listFrom == "Bills Data")
                                        {
                                            foreach (SPListItem fromItem in listFromItems)
                                            {
                                                SPListItem archiveItem = spListTo.AddItem();
                                                archiveItem["Title"] = fromItem["Title"];
                                                archiveItem["Transaction_x0020_Category"] = fromItem["Transaction_x0020_Category"];
                                                archiveItem["Transaction_x0020_Amount"] = fromItem["Transaction_x0020_Amount"];
                                                archiveItem["Transaction_x0020_Date"] = fromItem["Transaction_x0020_Date"];
                                                archiveItem["Account_x0020_Number"] = fromItem["Account_x0020_Number"];
                                                archiveItem["Customer"] = fromItem["Customer"];
                                                archiveItem.Update();
                                            }
                                        }
                                    }

                                    
                                }
                            }
                            spWeb.AllowUnsafeUpdates = false;
                        }
                    }
                });
            }
            catch (Exception)
            {


            }

        }

        private StringBuilder BatchCommand(string listID, SPListItemCollection listItems, int start, int end)
        {
            StringBuilder deletebuilder = new StringBuilder();
            deletebuilder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><Batch>");
            string command = "<Method><SetList Scope=\"Request\">" + listID +
                "</SetList><SetVar Name=\"ID\">{0}</SetVar><SetVar Name=\"owsfileref\">{1}</SetVar><SetVar Name=\"Cmd\">Delete</SetVar></Method>";

            for (int i = start; i <= end; i++)
            {
                SPListItem item = listItems[i];
                deletebuilder.Append(string.Format(command, item.ID.ToString(), item["FileRef"].ToString()));
            }

            //foreach (SPListItem item in spList.Items)
            //{
            //    deletebuilder.Append(string.Format(command, item.ID.ToString(), item["FileRef"].ToString()));
            //}
            deletebuilder.Append("</Batch>");
            return deletebuilder;
        }



    }
}
