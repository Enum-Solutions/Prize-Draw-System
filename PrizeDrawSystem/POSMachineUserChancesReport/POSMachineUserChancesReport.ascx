<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Register TagPrefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="POSMachineUserChancesReport.ascx.cs" Inherits="PrizeDrawSystem.POSMachineUserChancesReport.POSMachineUserChancesReport" %>



<div class="row">
    <div class="col-md-12">
        <h2 class="text-center mt-5">Users Chances Report (POS Machine)</h2>
        <hr class="mb-5">

        <div class="dropdown">
            <button type="button" class="btn btn-primary dropdown-toggle" data-toggle="dropdown">
                Select Report To View
            </button>
            <div class="dropdown-menu">
                <a class="dropdown-item" href="javascript:;" onclick="LoadReport('Weekly')">Weekly</a>
                <a class="dropdown-item" href="javascript:;" onclick="LoadReport('Monthly')">Monthly</a>
                <a class="dropdown-item" href="javascript:;" onclick="LoadReport('Quarterly')">Quarterly</a>
                <a class="dropdown-item" href="javascript:;" onclick="LoadReport('Half Yearly')">Half Yearly</a>
                <a class="dropdown-item" href="javascript:;" onclick="LoadReport('Yearly')">Yearly</a>
                <a class="dropdown-item" href="javascript:;" onclick="LoadReport('One Time')">One Time</a>
            </div>
        </div>

        <div id="winnersDiv" style="display: none;">
            <h2 class="winnersHeading">Users Chances
            </h2>
            <table id="reportTbl" class="display" style="width: 100%;">
                <thead>
                    <tr>
                        <th>Customer Name</th>
                        <th>Account No</th>
                        <th>Credit Card No</th>
                        <th>Branch Code</th>
                        <th>Chances To Win</th>
                    </tr>
                </thead>
            </table>

        </div>

    </div>
</div>

<script type="text/javascript">

    var winners = [];

    function LoadReport(reportType) {

        var methodUrl = [location.protocol, '//', location.host].join('') + '/_layouts/15/PrizeDrawSystem/WebAPI.aspx/GetPOSMachineUsersChancesReport';
        $('.winnersHeading').html('Users Chances (' + reportType + ')');
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
                            { "data": "CustomerName" },
                            { "data": "AccountNumber" },
                            { "data": "CreditCardNumber" },
                            { "data": "BranchCode" },
                            { "data": "Chances" }
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

        //LoadReport();
    });

</script>

