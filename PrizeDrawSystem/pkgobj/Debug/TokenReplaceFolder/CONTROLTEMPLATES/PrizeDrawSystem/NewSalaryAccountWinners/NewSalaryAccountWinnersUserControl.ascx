<%@ Assembly Name="PrizeDrawSystem, Version=1.0.0.0, Culture=neutral, PublicKeyToken=d14854d0414e8962" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="NewSalaryAccountWinnersUserControl.ascx.cs" Inherits="PrizeDrawSystem.NewSalaryAccountWinners.NewSalaryAccountWinnersUserControl" %>


<div class="row">
    <div class="col-md-12">
        <h2 class="text-center mt-5">Draw Winners Report (New Salary Account)</h2>
        <hr class="mb-5">

        <div id="winnersDiv" style="display: none;">
            <table id="reportTbl" class="display" style="width: 100%;">
                <thead>
                    <tr>
                        <th>Customer Name</th>
		                <th>Account No</th>
                        <th>Account Type</th>
                        <th>Branch Code</th>
                        <th>Account Opening Date</th>
                        <th>Last Salary Transfer Date</th>
                        <th>Winning Amount</th>
                    </tr>
                </thead>
            </table>

        </div>

    </div>
</div>

<script type="text/javascript">

    var winners = [];

    function LoadReport(reportType) {

        var id = getUrlVars()["id"];

        if (id != undefined) {

            var methodUrl = [location.protocol, '//', location.host].join('') + '/_layouts/15/PrizeDrawSystem/WebAPI.aspx/GetAllNewSalaryAccountDrawResultWinners';
            $('.winnersHeading').html('Users Chances (' + reportType + ')');
            $('.ajax-loader').css("visibility", "visible");

            $.ajax({
                type: "POST",
                url: methodUrl,
                data: JSON.stringify({ id: id }),
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
                                { "data": "AccountType" },
                                { "data": "BranchCode" },
                                { "data": "AccountOpeningDateStr" },
                                { "data": "LastSalaryTransferDateStr" },
                                { "data": "WinningAmount" },
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

        
    }

    // Read a page's GET URL variables and return them as an associative array.
    function getUrlVars() {
        var vars = [], hash;
        var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
        for (var i = 0; i < hashes.length; i++) {
            hash = hashes[i].split('=');
            vars.push(hash[0]);
            vars[hash[0]] = hash[1];
        }
        return vars;
    }

    $(function () {

        LoadReport();
    });

</script>
