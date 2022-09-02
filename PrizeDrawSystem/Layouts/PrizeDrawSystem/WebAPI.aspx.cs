using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Web.Script.Services;
using System.Web.Services;
using System.Configuration;
using System.Linq;
using System.Collections.Generic;
using PrizeDrawSystem.Models;
using System.Web;
using System.IO;
using ClosedXML.Excel;

namespace PrizeDrawSystem.Layouts.PrizeDrawSystem
{
    public partial class WebAPI : LayoutsPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        public static string SiteURL
        {
            get
            {
                return Convert.ToString(ConfigurationManager.AppSettings["PrizeDrawSiteURL"]);
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static List<POSMachineUser> GetPOSMachineAllWinners(string drawType)
        {
            try
            {
                List<POSMachineUser> winners = new POSMachineBusinessLogic().GetPOSMachineWinners(SiteURL, drawType, false);
                return winners;

                #region Commented
                //SPSecurity.RunWithElevatedPrivileges(delegate()
                //{

                //});
                #endregion

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static int GetMaxPOSMachineWinners()
        {
            try
            {
                //loading configurations
                List<double> configurationValues = new CommonOperations().LoadConfigurations(SiteURL, "POS Machine");
                int totalWinners = 0;

                if (configurationValues.Count > 0)
                {
                    totalWinners = (int)configurationValues[1];
                }

                return totalWinners;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static List<POSMachineUser> GetPOSMachineSingleWinner(string drawType, List<POSMachineUser> lastWinners)
        {
            try
            {
                List<POSMachineUser> winners = new POSMachineBusinessLogic().GetPOSMachineWinners(SiteURL, drawType, true, lastWinners);
                return winners;

                #region Commented
                //SPSecurity.RunWithElevatedPrivileges(delegate()
                //{

                //});
                #endregion

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static List<POSMachineUser> GetPOSMachineUsersChancesReport(string reportType)
        {
            try
            {
                List<POSMachineUser> usersWithApplicableChances = new POSMachineBusinessLogic().GetPOSMachineUsersWithApplicableChances(SiteURL, reportType, "Report");
                return usersWithApplicableChances;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static bool SavePOSMachineWinners(string drawType, List<POSMachineUser> allWinners)
        {
            using (SPSite site = new SPSite(SiteURL))
            {
                using (SPWeb spWeb = site.OpenWeb())
                {
                    SPUser currentUser = spWeb.CurrentUser;
                    SPList drawResultList = spWeb.Lists.TryGetList("POS Machine Draw Result");
                    SPList drawResultWinnersList = spWeb.Lists.TryGetList("POS Machine Draw Result Winners");

                    if (drawResultList != null && drawResultWinnersList != null)
                    {
                        bool userReadPermissionsDrawResultList = drawResultList.DoesUserHavePermissions(currentUser, SPBasePermissions.AddListItems);
                        bool userReadPermissionsDrawResultWinnersList = drawResultWinnersList.DoesUserHavePermissions(currentUser, SPBasePermissions.AddListItems);

                        if (userReadPermissionsDrawResultList && userReadPermissionsDrawResultWinnersList)
                        {
                            // Adding main result entry in parent list
                            spWeb.AllowUnsafeUpdates = true;
                            SPListItem parentItem = drawResultList.AddItem();

                            parentItem["Title"] = "Prize Draw " + DateTime.Now.ToString("(dd-MM-yyyy hh:mm)");
                            parentItem["Total_x0020_Winners"] = allWinners.Count;
                            parentItem["Draw_x0020_Type"] = drawType;

                            parentItem.Update();

                            // Adding winners in child list

                            if (allWinners != null && allWinners.Count > 0)
                            {
                                foreach (var winner in allWinners)
                                {
                                    SPListItem childItem = drawResultWinnersList.AddItem();

                                    childItem["Title"] = winner.CustomerName;
                                    childItem["Account_x0020_Number"] = winner.AccountNumber;
                                    childItem["BranchCode"] = winner.BranchCode;
                                    childItem["Credit_x0020_Card_x0020_Number"] = winner.CreditCardNumber;
                                    childItem["POS_x0020_Transaction_x0020_Amou"] = winner.POSTransactionAmount;
                                    childItem["Transaction_x0020_Date"] = winner.TransactionDate;
                                    childItem["WinningAmount"] = Convert.ToDouble(winner.WinningAmount);
                                    childItem["POS_x0020_Machine_x0020_Draw_x00"] = parentItem.ID;

                                    childItem.Update();
                                }
                            }
                            spWeb.AllowUnsafeUpdates = false;

                            // disabling the draw button
                            new CommonOperations().UpdateDrawButtonStatus(SiteURL, false, "POS Machine");

                            return true;
                        }
                    }
                }
            }

            return false;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static void ArchiveAndDeletePOSMachineData()
        {
            using (SPSite site = new SPSite(SiteURL))
            {
                using (SPWeb spWeb = site.OpenWeb())
                {
                    SPUser currentUser = spWeb.CurrentUser;
                    SPList posMachineDataList = spWeb.Lists.TryGetList("POS Machine Data");

                    if (posMachineDataList != null)
                    {
                        bool userReadPermissionsDrawResultList = posMachineDataList.DoesUserHavePermissions(currentUser, SPBasePermissions.DeleteListItems);

                        if (userReadPermissionsDrawResultList)
                        {
                            //Deleting existing POS Machine Data and archiving

                            new CommonOperations().ArchiveAndDeleteAllListItems(SiteURL, "POS Machine Data", "POS Machine Data Archive");
                        }

                    }

                }

            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static bool GetDrawButtonStatus(string drawCategory)
        {
            try
            {
                return new CommonOperations().GetDrawButtonStatus(SiteURL, drawCategory);
            }
            catch (Exception)
            {
                return false;
            }

        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static bool GetSliderStatus()
        {
            try
            {
                return new CommonOperations().GetSliderStatus(SiteURL);
            }
            catch (Exception)
            {
                return false;
            }

        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static List<DrawResult> GetAllPOSMachineDrawResults()
        {
            try
            {
                return new POSMachineBusinessLogic().GetAllPOSMachineDrawResults(SiteURL);
            }
            catch (Exception)
            {
                throw;
            }

        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static List<POSMachineUser> GetAllPOSMachineDrawResulWinners(int id)
        {
            try
            {
                return new POSMachineBusinessLogic().GetAllPOSMachineDrawResultWinners(SiteURL, id);
            }
            catch (Exception)
            {
                throw;
            }

        }

        // New Salary Account Data Web Services

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static int GetMaxNewSalaryAccountWinners()
        {
            try
            {
                //loading configurations
                List<double> configurationValues = new CommonOperations().LoadConfigurations(SiteURL, "New Salary Account");
                int totalWinners = 0;

                if (configurationValues.Count > 0)
                {
                    totalWinners = (int)configurationValues[1];
                }

                return totalWinners;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static List<NewSalaryAccountUser> GetNewSalaryAccountAllWinners(string drawType)
        {
            try
            {
                List<NewSalaryAccountUser> winners = new NewSalaryAccountBusinessLogic().GetNewSalaryAccountWinners(SiteURL, drawType, false);
                return winners;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static List<NewSalaryAccountUser> GetNewSalaryAccountSingleWinner(string drawType, List<NewSalaryAccountUser> lastWinners)
        {
            try
            {
                List<NewSalaryAccountUser> winners = new NewSalaryAccountBusinessLogic().GetNewSalaryAccountWinners(SiteURL, drawType, true, lastWinners);
                return winners;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static void ArchiveAndDeleteNewSalaryAccountData()
        {
            using (SPSite site = new SPSite(SiteURL))
            {
                using (SPWeb spWeb = site.OpenWeb())
                {
                    SPUser currentUser = spWeb.CurrentUser;
                    SPList posMachineDataList = spWeb.Lists.TryGetList("New Salary Account Data");

                    if (posMachineDataList != null)
                    {
                        bool userReadPermissionsDrawResultList = posMachineDataList.DoesUserHavePermissions(currentUser, SPBasePermissions.DeleteListItems);

                        if (userReadPermissionsDrawResultList)
                        {
                            //Deleting existing POS Machine Data and archiving

                            new CommonOperations().ArchiveAndDeleteAllListItems(SiteURL, "New Salary Account Data", "New Salary Account Data Archive");
                        }

                    }

                }

            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static bool SaveNewSalaryAccountWinners(string drawType, List<NewSalaryAccountUser> allWinners)
        {
            using (SPSite site = new SPSite(SiteURL))
            {
                using (SPWeb spWeb = site.OpenWeb())
                {
                    SPUser currentUser = spWeb.CurrentUser;
                    SPList drawResultList = spWeb.Lists.TryGetList("New Salary Account Draw Result");
                    SPList drawResultWinnersList = spWeb.Lists.TryGetList("New Salary Account Draw Result Winners");

                    if (drawResultList != null && drawResultWinnersList != null)
                    {
                        bool userReadPermissionsDrawResultList = drawResultList.DoesUserHavePermissions(currentUser, SPBasePermissions.AddListItems);
                        bool userReadPermissionsDrawResultWinnersList = drawResultWinnersList.DoesUserHavePermissions(currentUser, SPBasePermissions.AddListItems);

                        if (userReadPermissionsDrawResultList && userReadPermissionsDrawResultWinnersList)
                        {
                            // Adding main result entry in parent list
                            spWeb.AllowUnsafeUpdates = true;
                            SPListItem parentItem = drawResultList.AddItem();

                            parentItem["Title"] = "Prize Draw " + DateTime.Now.ToString("(dd-MM-yyyy hh:mm)");
                            parentItem["Total_x0020_Winners"] = allWinners.Count;
                            parentItem["Draw_x0020_Type"] = drawType;

                            parentItem.Update();

                            // Adding winners in child list

                            if (allWinners != null && allWinners.Count > 0)
                            {
                                foreach (var winner in allWinners)
                                {
                                    SPListItem childItem = drawResultWinnersList.AddItem();

                                    childItem["Title"] = winner.CustomerName;
                                    childItem["Account_x0020_Number"] = winner.AccountNumber;
                                    childItem["BranchCode"] = winner.BranchCode;
                                    childItem["Account_x0020_Type"] = winner.AccountType;
                                    childItem["Account_x0020_Opening_x0020_Date"] = winner.AccountOpeningDate;
                                    childItem["Last_x0020_Salary_x0020_Transfer"] = winner.LastSalaryTransferDate;
                                    childItem["WinningAmount"] = Convert.ToDouble(winner.WinningAmount);
                                    childItem["New_x0020_Salary_x0020_Account_x"] = parentItem.ID;

                                    childItem.Update();
                                }
                            }
                            spWeb.AllowUnsafeUpdates = false;

                            // disabling the draw button
                            new CommonOperations().UpdateDrawButtonStatus(SiteURL, false, "New Salary Account");

                            return true;
                        }
                    }
                }
            }

            return false;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static List<DrawResult> GetAllNewSalaryAccountDrawResults()
        {
            try
            {
                return new NewSalaryAccountBusinessLogic().GetAllNewSalaryAccountDrawResults(SiteURL);
            }
            catch (Exception)
            {
                throw;
            }

        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static List<NewSalaryAccountUser> GetAllNewSalaryAccountDrawResultWinners(int id)
        {
            try
            {
                return new NewSalaryAccountBusinessLogic().GetAllNewSalaryAccountDrawResultWinners(SiteURL, id);
            }
            catch (Exception)
            {
                throw;
            }

        }

        // Bills Data Web Services

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static int GetMaxBillsWinners()
        {
            try
            {
                //loading configurations
                List<double> configurationValues = new CommonOperations().LoadConfigurations(SiteURL, "Bills");
                int totalWinners = 0;

                if (configurationValues.Count > 0)
                {
                    totalWinners = (int)configurationValues[1];
                }

                return totalWinners;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static List<BillsUser> GetBillsAllWinners(string drawType)
        {
            try
            {
                List<BillsUser> winners = new BillsBusinessLogic().GetBillsWinners(SiteURL, drawType, false);
                return winners;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static List<BillsUser> GetBillsSingleWinner(string drawType, List<BillsUser> lastWinners)
        {
            try
            {
                List<BillsUser> winners = new BillsBusinessLogic().GetBillsWinners(SiteURL, drawType, true, lastWinners);
                return winners;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static void ArchiveAndDeleteBillsData()
        {
            using (SPSite site = new SPSite(SiteURL))
            {
                using (SPWeb spWeb = site.OpenWeb())
                {
                    SPUser currentUser = spWeb.CurrentUser;
                    SPList posMachineDataList = spWeb.Lists.TryGetList("Bills Data");

                    if (posMachineDataList != null)
                    {
                        bool userReadPermissionsDrawResultList = posMachineDataList.DoesUserHavePermissions(currentUser, SPBasePermissions.DeleteListItems);

                        if (userReadPermissionsDrawResultList)
                        {
                            //Deleting existing POS Machine Data and archiving

                            new CommonOperations().ArchiveAndDeleteAllListItems(SiteURL, "Bills Data", "Bills Data Archive");
                        }

                    }

                }

            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static bool SaveBillsWinners(string drawType, List<BillsUser> allWinners)
        {
            using (SPSite site = new SPSite(SiteURL))
            {
                using (SPWeb spWeb = site.OpenWeb())
                {
                    SPUser currentUser = spWeb.CurrentUser;
                    SPList drawResultList = spWeb.Lists.TryGetList("Bills Draw Result");
                    SPList drawResultWinnersList = spWeb.Lists.TryGetList("Bills Draw Result Winners");

                    if (drawResultList != null && drawResultWinnersList != null)
                    {
                        bool userReadPermissionsDrawResultList = drawResultList.DoesUserHavePermissions(currentUser, SPBasePermissions.AddListItems);
                        bool userReadPermissionsDrawResultWinnersList = drawResultWinnersList.DoesUserHavePermissions(currentUser, SPBasePermissions.AddListItems);

                        if (userReadPermissionsDrawResultList && userReadPermissionsDrawResultWinnersList)
                        {
                            // Adding main result entry in parent list
                            spWeb.AllowUnsafeUpdates = true;
                            SPListItem parentItem = drawResultList.AddItem();

                            parentItem["Title"] = "Prize Draw " + DateTime.Now.ToString("(dd-MM-yyyy hh:mm)");
                            parentItem["Total_x0020_Winners"] = allWinners.Count;
                            parentItem["Draw_x0020_Type"] = drawType;

                            parentItem.Update();

                            // Adding winners in child list

                            if (allWinners != null && allWinners.Count > 0)
                            {
                                foreach (var winner in allWinners)
                                {
                                    SPListItem childItem = drawResultWinnersList.AddItem();

                                    childItem["Title"] = winner.CustomerName;
                                    childItem["Account_x0020_Number"] = winner.AccountNumber;
                                    childItem["Transaction_x0020_Category"] = winner.TransactionCategory;
                                    childItem["Transaction_x0020_Date"] = winner.TransactionDate;
                                    childItem["Transaction_x0020_Amount"] = winner.TransactionAmount;
                                    childItem["Customer"] = winner.Customer;
                                    childItem["WinningAmount"] = Convert.ToDouble(winner.WinningAmount);
                                    childItem["Bills_x0020_Data_x0020_Draw_x002"] = parentItem.ID;

                                    childItem.Update();
                                }
                            }
                            spWeb.AllowUnsafeUpdates = false;

                            // disabling the draw button
                            new CommonOperations().UpdateDrawButtonStatus(SiteURL, false, "Bills");

                            return true;
                        }
                    }
                }
            }

            return false;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static List<DrawResult> GetAllBillsDrawResults()
        {
            try
            {
                return new BillsBusinessLogic().GetAllBillsDrawResults(SiteURL);
            }
            catch (Exception)
            {
                throw;
            }

        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static List<BillsUser> GetAllBillsDrawResultWinners(int id)
        {
            try
            {
                return new BillsBusinessLogic().GetAllBillsDrawResultWinners(SiteURL, id);
            }
            catch (Exception)
            {
                throw;
            }

        }


        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static int UploadExcelPOSMachine(string fileBase64)
        {
            try
            {
                byte[] fileData = Convert.FromBase64String(fileBase64);
                int totalImportedRecords = 0;
                using (MemoryStream memStream = new MemoryStream(fileData))
                {
                    memStream.Flush();
                    totalImportedRecords = ImportDataFromExcelPOSMachine(memStream);

                    //enabling draw button
                    new CommonOperations().UpdateDrawButtonStatus(SiteURL, true, "POS Machine");
                }
                return totalImportedRecords;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static int ImportDataFromExcelPOSMachine(MemoryStream ms)
        {
            // reading excel file using ClosedXML library
            var wb = new XLWorkbook(ms);
            //var ws = wb.Worksheet("Inbound Call Tegging Report-Inb");
            var ws = wb.Worksheet(1);

            // Look for the first row used
            var firstRowUsed = ws.FirstRowUsed();

            // Narrow down the row so that it only includes the used part
            var categoryRow = firstRowUsed.RowUsed();

            // Move to the next row (it now has the titles)
            categoryRow = categoryRow.RowBelow();

            int totalImportedRecords = 0;

            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                using (SPSite site = new SPSite(SiteURL))
                {
                    using (SPWeb spWeb = site.OpenWeb())
                    {
                        SPList oList = spWeb.Lists.TryGetList("POS Machine Data");

                        // adding all excel sheet rows into sharepoint list
                        spWeb.AllowUnsafeUpdates = true;
                        if (oList != null)
                        {
                            while (!categoryRow.Cell((int)POSMachineExcelColumns.Title).IsEmpty())
                            {
                                try
                                {
                                    SPListItem spItem = oList.AddItem();

                                    foreach (var item in Enum.GetValues(typeof(POSMachineExcelColumns)))
                                    {
                                        if (item.ToString() == POSMachineExcelColumns.Transaction_x0020_Date.ToString())
                                        {
                                            DateTime? dateTime = null;
                                            // checking if current DateTime column is of DateTime data type
                                            if (!categoryRow.Cell((int)item).IsEmpty())
                                            {
                                                dateTime = categoryRow.Cell((int)item).GetDateTime();
                                            }

                                            // checking null exceptions
                                            if (dateTime != null)
                                                spItem[item.ToString()] = dateTime;
                                        }
                                        else
                                        {
                                            spItem[item.ToString()] = categoryRow.Cell((int)item).GetString();
                                        }
                                    }

                                    spItem.Update();
                                    totalImportedRecords++;
                                    categoryRow = categoryRow.RowBelow();
                                }
                                catch (Exception ex)
                                {
                                    categoryRow = categoryRow.RowBelow();
                                }

                            }
                        }
                        spWeb.AllowUnsafeUpdates = false;
                    }
                }
            });

            return totalImportedRecords;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static int UploadExcelNewSalaryAccount(string fileBase64)
        {
            try
            {
                byte[] fileData = Convert.FromBase64String(fileBase64);
                int totalImportedRecords = 0;
                using (MemoryStream memStream = new MemoryStream(fileData))
                {
                    memStream.Flush();
                    totalImportedRecords = ImportDataFromExcelNewSalaryAccount(memStream);

                    //enabling draw button
                    new CommonOperations().UpdateDrawButtonStatus(SiteURL, true, "New Salary Account");
                }
                return totalImportedRecords;
            }
            catch (Exception)
            {
                
                throw;
            }
            
        }

        public static int ImportDataFromExcelNewSalaryAccount(MemoryStream ms)
        {
            // reading excel file using ClosedXML library
            var wb = new XLWorkbook(ms);
            //var ws = wb.Worksheet("Inbound Call Tegging Report-Inb");
            var ws = wb.Worksheet(1);

            // Look for the first row used
            var firstRowUsed = ws.FirstRowUsed();

            // Narrow down the row so that it only includes the used part
            var categoryRow = firstRowUsed.RowUsed();

            // Move to the next row (it now has the titles)
            categoryRow = categoryRow.RowBelow();
            int totalImportedRecords = 0;

            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                using (SPSite site = new SPSite(SiteURL))
                {
                    using (SPWeb spWeb = site.OpenWeb())
                    {
                        spWeb.AllowUnsafeUpdates = true;
                        SPList oList = spWeb.Lists.TryGetList("New Salary Account Data");

                        // adding all excel sheet rows into sharepoint list

                        if (oList != null)
                        {
                            while (!categoryRow.Cell((int)NewSalaryAccountExcelColumns.Title).IsEmpty())
                            {
                                try
                                {
                                    SPListItem spItem = oList.AddItem();

                                    foreach (var item in Enum.GetValues(typeof(NewSalaryAccountExcelColumns)))
                                    {
                                        if (item.ToString() == NewSalaryAccountExcelColumns.Account_x0020_Opening_x0020_Date.ToString() || item.ToString() == NewSalaryAccountExcelColumns.Last_x0020_Salary_x0020_Transfer.ToString())
                                        {
                                            DateTime? dateTime = null;
                                            // checking if current DateTime column is of DateTime data type
                                            if (!categoryRow.Cell((int)item).IsEmpty())
                                            {
                                                dateTime = categoryRow.Cell((int)item).GetDateTime();
                                            }

                                            // checking null exceptions
                                            if (dateTime != null)
                                                spItem[item.ToString()] = dateTime;
                                        }
                                        else
                                        {
                                            spItem[item.ToString()] = categoryRow.Cell((int)item).GetString();
                                        }
                                    }

                                    spItem.Update();
                                    totalImportedRecords++;
                                    categoryRow = categoryRow.RowBelow();
                                }
                                catch (Exception ex)
                                {
                                    categoryRow = categoryRow.RowBelow();
                                }

                            }
                        }

                        spWeb.AllowUnsafeUpdates = false;
                    }
                }
            });


            return totalImportedRecords;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static int UploadExcelBills(string fileBase64)
        {
            try
            {
                byte[] fileData = Convert.FromBase64String(fileBase64);
                int totalImportedRecords = 0;
                using (MemoryStream memStream = new MemoryStream(fileData))
                {
                    memStream.Flush();
                    totalImportedRecords = ImportDataFromExcelBills(memStream);

                    //enabling draw button
                    new CommonOperations().UpdateDrawButtonStatus(SiteURL, true, "Bills");
                }
                return totalImportedRecords;
            }
            catch (Exception)
            {
                
                throw;
            }
            
        }

        public static int ImportDataFromExcelBills(MemoryStream ms)
        {
            // reading excel file using ClosedXML library
            var wb = new XLWorkbook(ms);
            //var ws = wb.Worksheet("Inbound Call Tegging Report-Inb");
            var ws = wb.Worksheet(1);

            // Look for the first row used
            var firstRowUsed = ws.FirstRowUsed();

            // Narrow down the row so that it only includes the used part
            var categoryRow = firstRowUsed.RowUsed();

            // Move to the next row (it now has the titles)
            categoryRow = categoryRow.RowBelow();
            int totalImportedRecords = 0;

            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                using (SPSite site = new SPSite(SiteURL))
                {
                    using (SPWeb spWeb = site.OpenWeb())
                    {
                        spWeb.AllowUnsafeUpdates = true;
                        SPList oList = spWeb.Lists.TryGetList("Bills Data");

                        // adding all excel sheet rows into sharepoint list

                        if (oList != null)
                        {
                            while (!categoryRow.Cell((int)BillsExcelColumns.Title).IsEmpty())
                            {
                                try
                                {
                                    SPListItem spItem = oList.AddItem();

                                    foreach (var item in Enum.GetValues(typeof(BillsExcelColumns)))
                                    {
                                        if (item.ToString() == BillsExcelColumns.Transaction_x0020_Date.ToString())
                                        {
                                            DateTime? dateTime = null;
                                            // checking if current DateTime column is of DateTime data type
                                            if (!categoryRow.Cell((int)item).IsEmpty())
                                            {
                                                dateTime = categoryRow.Cell((int)item).GetDateTime();
                                            }

                                            // checking null exceptions
                                            if (dateTime != null)
                                                spItem[item.ToString()] = dateTime;
                                        }
                                        else
                                        {
                                            spItem[item.ToString()] = categoryRow.Cell((int)item).GetString();
                                        }
                                    }

                                    spItem.Update();
                                    totalImportedRecords++;
                                    categoryRow = categoryRow.RowBelow();
                                }
                                catch (Exception ex)
                                {
                                    categoryRow = categoryRow.RowBelow();
                                }

                            }
                        }

                        spWeb.AllowUnsafeUpdates = false;
                    }
                }
            });

            

            return totalImportedRecords;
        }

    }
}
