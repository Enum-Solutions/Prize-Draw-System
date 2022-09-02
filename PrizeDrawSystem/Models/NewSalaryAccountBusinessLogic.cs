
using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrizeDrawSystem.Models
{
    public class NewSalaryAccountBusinessLogic
    {
        public List<NewSalaryAccountUser> GetNewSalaryAccountWinners(string siteURL, string drawType, bool singleWinner, List<NewSalaryAccountUser> lastWinners = null)
        {
            //loading configurations
            List<double> configurationValues = new CommonOperations().LoadConfigurations(siteURL, "New Salary Account");
            string winningAmount = "";
            double criteria = 0;
            int totalWinners = 0;

            if (configurationValues.Count > 0)
            {
                winningAmount = configurationValues[0].ToString();
                totalWinners = (int)configurationValues[1];
                criteria = configurationValues[2];
            }

            List<NewSalaryAccountUser> winners = new List<NewSalaryAccountUser>();

            var userWithApplicableChances = GetNewSalaryAccountUsers(siteURL, drawType);

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
        public List<NewSalaryAccountUser> GetNewSalaryAccountUsers(string siteURL, string drawType)
        {
            //loading configurations
            List<double> configurationValues = new CommonOperations().LoadConfigurations(siteURL, "New Salary Account");
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
            List<NewSalaryAccountUser> allUsers = new List<NewSalaryAccountUser>();

            using (SPSite site = new SPSite(siteURL))
            {
                using (SPWeb spWeb = site.OpenWeb())
                {
                    SPUser currentUser = spWeb.CurrentUser;

                    SPList spList = spWeb.Lists.TryGetList("New Salary Account Data");
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
                                            <FieldRef Name='Account_x0020_Opening_x0020_Date' />
                                            <Value Type='DateTime'>
                                                <Today />
                                            </Value>
                                            </Leq>
                                            <Geq>
                                            <FieldRef Name='Account_x0020_Opening_x0020_Date' />
                                            <Value Type='DateTime'>" + dateTime + @"</Value>
                                            </Geq>
                                        </And>
                                    </Where>";
                            }

                            qry.ViewFields = @"<FieldRef Name='Account_x0020_Number' /><FieldRef Name='Account_x0020_Opening_x0020_Date' /><FieldRef Name='Account_x0020_Type' /><FieldRef Name='BranchCode' /><FieldRef Name='Title' /><FieldRef Name='ID' /><FieldRef Name='Last_x0020_Salary_x0020_Transfer' />";

                            SPListItemCollection listItems = spList.GetItems(qry);

                            if (listItems != null && listItems.Count > 0)
                            {
                                foreach (SPListItem item in listItems)
                                {
                                    allUsers.Add(new NewSalaryAccountUser
                                    {
                                        ID = Convert.ToInt32(item["ID"]),
                                        CustomerName = Convert.ToString(item["Title"]),
                                        BranchCode = Convert.ToString(item["BranchCode"]),
                                        AccountType = Convert.ToString(item["Account_x0020_Type"]),
                                        AccountOpeningDate = Convert.ToDateTime(item["Account_x0020_Opening_x0020_Date"]),
                                        AccountOpeningDateStr = Convert.ToDateTime(item["Account_x0020_Opening_x0020_Date"]).ToString("dd-MMM-yyyy"),
                                        LastSalaryTransferDate = Convert.ToDateTime(item["Last_x0020_Salary_x0020_Transfer"]),
                                        LastSalaryTransferDateStr = Convert.ToDateTime(item["Last_x0020_Salary_x0020_Transfer"]).ToString("dd-MMM-yyyy"),
                                        AccountNumber = Convert.ToString(item["Account_x0020_Number"]),
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

        public List<DrawResult> GetAllNewSalaryAccountDrawResults(string siteURL)
        {
            List<DrawResult> drawResults = new List<DrawResult>();

            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                using (SPSite site = new SPSite(siteURL))
                {
                    using (SPWeb spWeb = site.OpenWeb())
                    {
                        SPList spList = spWeb.Lists.TryGetList("New Salary Account Draw Result");
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
        public List<NewSalaryAccountUser> GetAllNewSalaryAccountDrawResultWinners(string siteURL, int newSalaryAccountResultID)
        {
            List<NewSalaryAccountUser> drawWinners = new List<NewSalaryAccountUser>();

            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                using (SPSite site = new SPSite(siteURL))
                {
                    using (SPWeb spWeb = site.OpenWeb())
                    {
                        SPList spList = spWeb.Lists.TryGetList("New Salary Account Draw Result Winners");
                        if (spList != null)
                        {
                            SPQuery qry = new SPQuery();
                            qry.Query =
                            @"   <Where>
                                      <Eq>
                                         <FieldRef Name='New_x0020_Salary_x0020_Account_x' LookupId='True' />
                                         <Value Type='Lookup'>" + newSalaryAccountResultID + @"</Value>
                                      </Eq>
                                 </Where>
                                 <OrderBy>
                                  <FieldRef Name='Title' />
                                </OrderBy>";
                            qry.ViewFields = @"<FieldRef Name='Account_x0020_Number' /><FieldRef Name='Account_x0020_Opening_x0020_Date' /><FieldRef Name='Account_x0020_Type' /><FieldRef Name='BranchCode' /><FieldRef Name='Title' /><FieldRef Name='ID' /><FieldRef Name='Last_x0020_Salary_x0020_Transfer' /><FieldRef Name='WinningAmount' />";
                            SPListItemCollection listItems = spList.GetItems(qry);

                            if (listItems != null && listItems.Count > 0)
                            {
                                foreach (SPListItem item in listItems)
                                {
                                    drawWinners.Add(new NewSalaryAccountUser
                                    {
                                        ID = Convert.ToInt32(item["ID"]),
                                        CustomerName = Convert.ToString(item["Title"]),
                                        BranchCode = Convert.ToString(item["BranchCode"]),
                                        AccountType = Convert.ToString(item["Account_x0020_Type"]),
                                        AccountOpeningDate = Convert.ToDateTime(item["Account_x0020_Opening_x0020_Date"]),
                                        AccountOpeningDateStr = Convert.ToDateTime(item["Account_x0020_Opening_x0020_Date"]).ToString("dd-MMM-yyyy"),
                                        LastSalaryTransferDate = Convert.ToDateTime(item["Last_x0020_Salary_x0020_Transfer"]),
                                        LastSalaryTransferDateStr = Convert.ToDateTime(item["Last_x0020_Salary_x0020_Transfer"]).ToString("dd-MMM-yyyy"),
                                        AccountNumber = Convert.ToString(item["Account_x0020_Number"]),
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
