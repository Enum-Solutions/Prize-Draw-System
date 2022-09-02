using ClosedXML.Excel;
using Microsoft.SharePoint;
using PrizeDrawSystem.Models;
using System;
using System.Configuration;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace PrizeDrawSystem.ImportBillsExcelDataFile
{
    public partial class ImportBillsExcelDataFileUserControl : UserControl
    {
        public string errorMsg = "";
        public string successMsg = "";
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

        protected void UploadBtn_Click(object sender, EventArgs e)
        {
            if (!customFile.HasFile || !customFile.FileName.Contains(".xlsx") || !customFile.FileName.Contains(".xls"))
            {
                errorMsg = "Please select Excel File";
                return;
            }
            else
            {
                try
                {
                    byte[] fileData = customFile.FileBytes;
                    using (MemoryStream memStream = new MemoryStream(fileData))
                    {
                        memStream.Flush();
                        ImportDataFromExcel(memStream);

                        //enabling draw button
                        new CommonOperations().UpdateDrawButtonStatus(SiteURL, true, "Bills");
                    }

                    successMsg = "Data Successfully Uploaded...";
                }
                catch (Exception Ex1)
                {
                    errorMsg = "Some Error Occured. Please Try Again! ";
                }
            }
        }

        public void ImportDataFromExcel(MemoryStream ms)
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

            SPList oList = SPContext.Current.Web.Lists.TryGetList("Bills Data");

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
                        categoryRow = categoryRow.RowBelow();
                    }
                    catch (Exception ex)
                    {
                        categoryRow = categoryRow.RowBelow();
                    }

                }
            }




        }
    }
}
