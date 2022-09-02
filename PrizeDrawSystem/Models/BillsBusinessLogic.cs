
using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrizeDrawSystem.Models
{
    public class BillsBusinessLogic
    {
        public List<BillsUser> GetBillsWinners(string siteURL, string drawType, bool singleWinner, List<BillsUser> lastWinners = null)
        {
            //loading configurations
            List<double> configurationValues = new CommonOperations().LoadConfigurations(siteURL, "Bills");
            string winningAmount = "";
            double criteria = 0;
            int totalWinners = 0;

            if (configurationValues.Count > 0)
            {
                winningAmount = configurationValues[0].ToString();
                totalWinners = (int)configurationValues[1];
                criteria = configurationValues[2];
            }

            List<BillsUser> winners = new List<BillsUser>();

            var userWithApplicableChances = GetBillsUsers(siteURL, drawType);

            if (userWithApplicableChances.Count > 0)
            {
                Random random = new Random();
                int count = 0;

                if (singleWinner)
                {
                    // code snippet for loading existing winners if draw is previously run by user 
                    if (lastWinners != null)
                    {
                        foreach (var lastWinner in lastWinners)
                        {
                            winners.Add(lastWinner);
                        }
                    }

                    int index = random.Next(userWithApplicableChances.Count);
                    var userAlreadyExist = winners.Where(m => m.AccountNumber == userWithApplicableChances[index].AccountNumber).FirstOrDefault();

                    if (userAlreadyExist == null)
                    {
                        winners.Add(userWithApplicableChances[index]);
                        count++;
                    }

                }
                else
                {
                    int counter = 1;
                    while (count < totalWinners)
                    {
                        int index = random.Next(userWithApplicableChances.Count);
                        var userAlreadyExist = winners.Where(m => m.AccountNumber == userWithApplicableChances[index].AccountNumber).FirstOrDefault();

                        if (userAlreadyExist == null)
                        {
                            if (counter % 2 != 0)
                            {
                                winners.Add(userWithApplicableChances[index]);
                                count++;
                            }

                            counter++;
                        }

                        
                    }
                }
            }

            return winners;
        }
        public List<BillsUser> GetBillsUsers(string siteURL, string drawType)
        {
            //loading configurations
            List<double> configurationValues = new CommonOperations().LoadConfigurations(siteURL, "Bills");
            string winningAmount = "";
            double criteria = 0;
            int totalWinners = 0;

            if (configurationValues.Count > 0)
            {
                winningAmount = configurationValues[0].ToString();
                totalWinners = (int)configurationValues[1];
                criteria = configurationValues[2];
            }

            //declaring winners empty list
            List<BillsUser> allUsers = new List<BillsUser>();

            using (SPSite site = new SPSite(siteURL))
            {
                using (SPWeb spWeb = site.OpenWeb())
                {
                    SPUser currentUser = spWeb.CurrentUser;

                    SPList spList = spWeb.Lists.TryGetList("Bills Data");
                    if (spList != null)
                    {
                        bool userReadPermissions = spList.DoesUserHavePermissions(currentUser, SPBasePermissions.ViewListItems);

                        if (userReadPermissions)
                        {
                            SPQuery qry = new SPQuery();
                            string dateTime = string.Empty;

                            if (drawType == "Weekly")
                            {
                                dateTime = DateTime.Now.AddDays(-6).ToString("yyyy-MM-ddThh:mm:ssZ");
                            }
                            else if (drawType == "Monthly")
                            {
                                dateTime = DateTime.Now.AddDays(-30).ToString("yyyy-MM-ddThh:mm:ssZ");
                            }
                            else if (drawType == "Quarterly")
                            {
                                dateTime = DateTime.Now.AddDays(-90).ToString("yyyy-MM-ddThh:mm:ssZ");
                            }
                            else if (drawType == "Half Yearly")
                            {
                                dateTime = DateTime.Now.AddDays(-180).ToString("yyyy-MM-ddThh:mm:ssZ");
                            }
                            else if (drawType == "Yearly")
                            {
                                dateTime = DateTime.Now.AddDays(-365).ToString("yyyy-MM-ddThh:mm:ssZ");
                            }

                            if (drawType != "One Time")
                            {
                                qry.Query =
                                @" <Where>
                                        <And>
                                            <Leq>
                                            <FieldRef Name='Transaction_x0020_Date' />
                                            <Value Type='DateTime'>
                                                <Today />
                                            </Value>
                                            </Leq>
                                            <Geq>
                                            <FieldRef Name='Transaction_x0020_Date' />
                                            <Value Type='DateTime'>" + dateTime + @"</Value>
                                            </Geq>
                                        </And>
                                    </Where>";
                            }

                            qry.ViewFields = @"<FieldRef Name='Title' /><FieldRef Name='Account_x0020_Number' /><FieldRef Name='Transaction_x0020_Date' /><FieldRef Name='Transaction_x0020_Category' /><FieldRef Name='Transaction_x0020_Amount' /><FieldRef Name='ID' /><FieldRef Name='Customer' />";

                            SPListItemCollection listItems = spList.GetItems(qry);

                            if (listItems != null && listItems.Count > 0)
                            {
                                foreach (SPListItem item in listItems)
                                {
                                    allUsers.Add(new BillsUser
                                    {
                                        ID = Convert.ToInt32(item["ID"]),
                                        CustomerName = Convert.ToString(item["Title"]),
                                        TransactionCategory = Convert.ToString(item["Transaction_x0020_Category"]),
                                        TransactionAmount = Convert.ToDouble(item["Transaction_x0020_Amount"]),
                                        TransactionDate = Convert.ToDateTime(item["Transaction_x0020_Date"]),
                                        TransactionDateStr = Convert.ToDateTime(item["Transaction_x0020_Date"]).ToString("dd-MMM-yyyy"),
                                        AccountNumber = Convert.ToString(item["Account_x0020_Number"]),
                                        Customer = Convert.ToString(item["Customer"]),
                                        WinningAmount = winningAmount
                                    });
                                }

                            }
                        }
                        else
                        {
                            throw new Exception("User Does not have permissions to access data");
                        }


                    }


                }
            }

            // until here

            return allUsers;
        }
        public List<DrawResult> GetAllBillsDrawResults(string siteURL)
        {
            List<DrawResult> drawResults = new List<DrawResult>();

            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                using (SPSite site = new SPSite(siteURL))
                {
                    using (SPWeb spWeb = site.OpenWeb())
                    {
                        SPList spList = spWeb.Lists.TryGetList("Bills Draw Result");
                        if (spList != null)
                        {
                            SPQuery qry = new SPQuery();
                            qry.Query =
                            @"   <OrderBy>
                                    <FieldRef Name='ID' />
                                 </OrderBy>";
                            qry.ViewFields = @"<FieldRef Name='Title' /><FieldRef Name='ID' /><FieldRef Name='Total_x0020_Winners' /><FieldRef Name='Draw_x0020_Type' /><FieldRef Name='Author' /><FieldRef Name='Created' />";
                            SPListItemCollection listItems = spList.GetItems(qry);

                            if (listItems != null && listItems.Count > 0)
                            {
                                foreach (SPListItem item in listItems)
                                {
                                    drawResults.Add(new DrawResult
                                    {
                                        Title = Convert.ToString(item["Title"]),
                                        ID = Convert.ToInt32(item["ID"]),
                                        CreatedStr = Convert.ToDateTime(item["Created"]).ToString("dd-MMM-yyyy hh:mm tt"),
                                        CreatedBy = Convert.ToString(item["Author"]).Split('#')[1],
                                        DrawType = Convert.ToString(item["Draw_x0020_Type"]),
                                        TotalWinners = Convert.ToInt32(item["Total_x0020_Winners"])
                                    });
                                }
                            }
                        }

                    }
                }
            });

            return drawResults;
        }
        public List<BillsUser> GetAllBillsDrawResultWinners(string siteURL, int billsResultID)
        {
            List<BillsUser> drawWinners = new List<BillsUser>();

            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                using (SPSite site = new SPSite(siteURL))
                {
                    using (SPWeb spWeb = site.OpenWeb())
                    {
                        SPList spList = spWeb.Lists.TryGetList("Bills Draw Result Winners");
                        if (spList != null)
                        {
                            SPQuery qry = new SPQuery();
                            qry.Query =
                            @"   <Where>
                                      <Eq>
                                         <FieldRef Name='Bills_x0020_Data_x0020_Draw_x002' LookupId='True' />
                                         <Value Type='Lookup'>" + billsResultID + @"</Value>
                                      </Eq>
                                 </Where>
                                 <OrderBy>
                                  <FieldRef Name='Title' />
                                </OrderBy>";
                            qry.ViewFields = @"<FieldRef Name='Title' /><FieldRef Name='Account_x0020_Number' /><FieldRef Name='Transaction_x0020_Date' /><FieldRef Name='Transaction_x0020_Category' /><FieldRef Name='Transaction_x0020_Amount' /><FieldRef Name='ID' /><FieldRef Name='Customer' /><FieldRef Name='WinningAmount' />";
                            SPListItemCollection listItems = spList.GetItems(qry);

                            if (listItems != null && listItems.Count > 0)
                            {
                                foreach (SPListItem item in listItems)
                                {
                                    drawWinners.Add(new BillsUser
                                    {
                                        ID = Convert.ToInt32(item["ID"]),
                                        CustomerName = Convert.ToString(item["Title"]),
                                        TransactionCategory = Convert.ToString(item["Transaction_x0020_Category"]),
                                        TransactionAmount = Convert.ToDouble(item["Transaction_x0020_Amount"]),
                                        TransactionDate = Convert.ToDateTime(item["Transaction_x0020_Date"]),
                                        TransactionDateStr = Convert.ToDateTime(item["Transaction_x0020_Date"]).ToString("dd-MMM-yyyy"),
                                        AccountNumber = Convert.ToString(item["Account_x0020_Number"]),
                                        Customer = Convert.ToString(item["Customer"]),
                                        WinningAmount = Convert.ToString(item["WinningAmount"])
                                    });
                                }
                            }
                        }

                    }
                }
            });

            return drawWinners;
        }

    }
}
