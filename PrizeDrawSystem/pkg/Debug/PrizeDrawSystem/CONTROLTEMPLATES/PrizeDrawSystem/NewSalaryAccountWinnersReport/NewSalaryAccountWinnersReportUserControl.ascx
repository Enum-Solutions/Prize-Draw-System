<%@ Assembly Name="PrizeDrawSystem, Version=1.0.0.0, Culture=neutral, PublicKeyToken=d14854d0414e8962" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="NewSalaryAccountWinnersReportUserControl.ascx.cs" Inherits="PrizeDrawSystem.NewSalaryAccountWinnersReport.NewSalaryAccountWinnersReportUserControl" %>


<div class="row">
    <div class="col-md-12">
        <h2 class="text-center mt-5">Draw Winners Report (New Salary Account)</h2>
        <hr class="mb-5">

        <div id="winnersDiv" style="display: none;">
            <table id="reportTbl" class="display" style="width: 100%;">
                <thead>
                    <tr>
                        <th>Title</th>
                        <th>Total Winners</th>
                        <th>Draw Type</th>
                        <th>Draw Run By</th>
                        <th>Draw Run At</th>
                        <th>Winners</th>
                    </tr>
                </thead>
            </table>

        </div>

    </div>
</div>

<script type="text/javascript">

    var winners = [];

    function LoadReport(reportType) {

        var methodUrl = [location.protocol, '//', location.host].join('') + '/_layouts/15/PrizeDrawSystem/WebAPI.aspx/GetAllNewSalaryAccountDrawResults';
        $('.winnersHeading').html('Prize Draw Winners');
        $('.ajax-loader').css("visibility", "visible");

        $.ajax({
            type: "POST",
            url: methodUrl,
            data: JSON.stringify({ reportType: reportType }),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (result) {

                $('.ajax-loader').css("visibility", "hidden");

                if (result != null && result.d != null && result.d.length > 0) {

                    winners = result.d;

                    var table = $('#reportTbl').DataTable();
                    table.destroy();

                    $('#reportTbl').DataTable({
                        data: winners,
                        columns: [
                            { "data": "Title" },
                            { "data": "TotalWinners" },
                            { "data": "DrawType" },
                            { "data": "CreatedBy" },
                            { "data": "CreatedStr" },
                            {
                                "data": "ID",
                                render: function (data) {

                                    return "<a href='/Pages/New-Salary-Account-Draw-Winners.aspx?id=" + data + "'>View</a>"
                                }
                            }
                        ]
                    });

                    $("#winnersDiv").fadeIn(2000);
                }

            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.log('Error occured');
                $('.ajax-loader').css("visibility", "hidden");

                if (jqXHR.status == 500) {
                    console.log('Internal error: ' + jqXHR.responseText);
                } else {
                    console.log('Unexpected error.');
                }
            }
        });
    }

    $(function () {

        LoadReport();
    });

</script>

