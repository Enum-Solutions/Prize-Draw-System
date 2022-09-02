using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrizeDrawSystem.Models
{
    public class POSMachineBusinessLogic
    {
        public List<POSMachineUser> CalculatePOSMachineUserChances(List<POSMachineUser> allUsers, double criteriaAmount, string reportOrDraw)
        {
            List<POSMachineUser> uniqueUsersWithTotalChances = new List<POSMachineUser>();

            if (allUsers != null && allUsers.Count > 0)
            {
                if (reportOrDraw == "Report")
                {
                    foreach (var user in allUsers)
                    {
                        var userAlreadyExist = uniqueUsersWithTotalChances.Where(m => m.AccountNumber == user.AccountNumber).FirstOrDefault();

                        if (userAlreadyExist != null && user.POSTransactionAmount >= criteriaAmount)
                        {
                            userAlreadyExist.Chances += (int)Math.Floor(user.POSTransactionAmount / criteriaAmount);
                        }
                        else if (user.POSTransactionAmount >= criteriaAmount)
                        {
                            user.Chances = (int)Math.Floor(user.POSTransactionAmount / criteriaAmount);
                            uniqueUsersWithTotalChances.Add(user);
                        }
                    }
                }
                else if (reportOrDraw == "Draw")
                {
                    foreach (var user in allUsers)
                    {
                        int chances = (int)Math.Floor(user.POSTransactionAmount / criteriaAmount);

                        if (chances > 0)
                        {
                            for (int i = 0; i < chances; i++)
                            {
                                uniqueUsersWithTotalChances.Add(user);
                            }
                        }
                    }
                }



            }

            //List<POSMachineUser> usersWithAllChances = new List<POSMachineUser>();

            //if (uniqueUsersWithTotalAmount != null && uniqueUsersWithTotalAmount.Count > 0)
            //{
            //    foreach (var user in uniqueUsersWithTotalAmount)
            //    {
            //        int chances = Convert.ToInt32(Math.Floor(user.POSTransactionAmount / criteriaAmount));

            //        for (int i = 0; i < chances; i++)
            //        {
            //            usersWithAllChances.Add(user);
            //        }
            //    }
            //}

            return uniqueUsersWithTotalChances;
        }
        public List<POSMachineUser> GetPOSMachineUsersWithApplicableChances(string siteURL, string drawType, string reportOrDraw)
        {
            //loading configurations
            List<double> configurationValues = new CommonOperations().LoadConfigurations(siteURL, "POS Machine");
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
            List<POSMachineUser> userWithApplicableChances = new List<POSMachineUser>();

            using (SPSite site = new SPSite(siteURL))
            {
                using (SPWeb spWeb = site.OpenWeb())
                {
                    SPUser currentUser = spWeb.CurrentUser;
                    List<POSMachineUser> allUsers = new List<POSMachineUser>();

                    SPList spList = spWeb.Lists.TryGetList("POS Machine Data");
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

                            qry.ViewFields = @"<FieldRef Name='Account_x0020_Number' /><FieldRef Name='BranchCode' /><FieldRef Name='Credit_x0020_Card_x0020_Number' /><FieldRef Name='Title' /><FieldRef Name='POS_x0020_Transaction_x0020_Amou' /><FieldRef Name='Transaction_x0020_Date' />";

                            SPListItemCollection listItems = spList.GetItems(qry);

                            if (listItems != null && listItems.Count > 0)
                            {
                                foreach (SPListItem item in listItems)
                                {
                                    allUsers.Add(new POSMachineUser
                                    {
                                        ID = Convert.ToInt32(item["ID"]),
                                        CustomerName = Convert.ToString(item["Title"]),
                                        BranchCode = Convert.ToString(item["BranchCode"]),
                                        CreditCardNumber = Convert.ToString(item["Credit_x0020_Card_x0020_Number"]),
                                        POSTransactionAmount = Convert.ToDouble(item["POS_x0020_Transaction_x0020_Amou"]),
                                        TransactionDate = Convert.ToDateTime(item["Transaction_x0020_Date"]),
                                        AccountNumber = Convert.ToString(item["Account_x0020_Number"]),
                                        WinningAmount = winningAmount
                                    });
                                }

                                userWithApplicableChances = CalculatePOSMachineUserChances(allUsers, criteria, reportOrDraw);

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

            return userWithApplicableChances;
        }
        public List<POSMachineUser> GetPOSMachineWinners(string siteURL, string drawType, bool singleWinner, List<POSMachineUser> lastWinners = null)
        {
            //loading configurations
            List<double> configurationValues = new CommonOperations().LoadConfigurations(siteURL, "POS Machine");
            string winningAmount = "";
            double criteria = 0;
            int totalWinners = 0;

            if (configurationValues.Count > 0)
            {
                winningAmount = configurationValues[0].ToString();
                totalWinners = (int)configurationValues[1];
                criteria = configurationValues[2];
            }

            List<POSMachineUser> winners = new List<POSMachineUser>();

            var userWithApplicableChances = GetPOSMachineUsersWithApplicableChances(siteURL, drawType, "Draw");

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
                    int userTotalWinning = winners.Where(m => m.AccountNumber == userWithApplicableChances[index].AccountNumber).Count();

                    if (userTotalWinning == 0)
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

                        if (counter % 2 != 0)
                        {
                            int userTotalWinning = winners.Where(m => m.AccountNumber == userWithApplicableChances[index].AccountNumber).Count();

                            if (userTotalWinning == 0)
                            {
                                winners.Add(userWithApplicableChances[index]);
                                count++;
                            }
                        }

                        counter++;
                    }
                }
            }

            return winners;
        }
        public List<DrawResult> GetAllPOSMachineDrawResults(string siteURL)
        {
            List<DrawResult> drawResults = new List<DrawResult>();

            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                using (SPSite site = new SPSite(siteURL))
                {
                    using (SPWeb spWeb = site.OpenWeb())
                    {
                        SPList spList = spWeb.Lists.TryGetList("POS Machine Draw Result");
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
        public List<POSMachineUser> GetAllPOSMachineDrawResultWinners(string siteURL, int posMachineResultID)
        {
            List<POSMachineUser> drawWinners = new List<POSMachineUser>();

            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                using (SPSite site = new SPSite(siteURL))
                {
                    using (SPWeb spWeb = site.OpenWeb())
                    {
                        SPList spList = spWeb.Lists.TryGetList("POS Machine Draw Result Winners");
                        if (spList != null)
                        {
                            SPQuery qry = new SPQuery();
                            qry.Query =
                            @"   <Where>
                                      <Eq>
                                         <FieldRef Name='POS_x0020_Machine_x0020_Draw_x00' LookupId='True' />
                                         <Value Type='Lookup'>" + posMachineResultID + @"</Value>
                                      </Eq>
                                 </Where>
                                 <OrderBy>
                                  <FieldRef Name='Title' />
                                </OrderBy>";
                            qry.ViewFields = @"<FieldRef Name='Account_x0020_Number' /><FieldRef Name='BranchCode' /><FieldRef Name='Credit_x0020_Card_x0020_Number' /><FieldRef Name='Title' /><FieldRef Name='ID' /><FieldRef Name='POS_x0020_Transaction_x0020_Amou' /><FieldRef Name='Transaction_x0020_Date' /><FieldRef Name='WinningAmount' />";
                            SPListItemCollection listItems = spList.GetItems(qry);

                            if (listItems != null && listItems.Count > 0)
                            {
                                foreach (SPListItem item in listItems)
                                {
                                    drawWinners.Add(new POSMachineUser
                                    {
                                        ID = Convert.ToInt32(item["ID"]),
                                        CustomerName = Convert.ToString(item["Title"]),
                                        BranchCode = Convert.ToString(item["BranchCode"]),
                                        CreditCardNumber = Convert.ToString(item["Credit_x0020_Card_x0020_Number"]),
                                        POSTransactionAmount = Convert.ToDouble(item["POS_x0020_Transaction_x0020_Amou"]),
                                        TransactionDate = Convert.ToDateTime(item["Transaction_x0020_Date"]),
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
