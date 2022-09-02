<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="POSMachinePrizeDraw.ascx.cs" Inherits="PrizeDrawSystem.POSMachinePrizeDraw.POSMachinePrizeDraw" %>


<div class="row">
	<div class="col-md-12">
		<h2 class="text-center mt-5">
            <%=DrawType %> Draw (Credit Card Draw)
            <br />
            السحب لمرة واحدة (سحب البطاقة الائتمانية)
		</h2>
		<hr class="mb-5">
		
		<div class="row" id="loadingGifRow">
		
			<div class="col-md-3">
				
			</div>
			
			<div class="col-md-6 text-center">
				<img src="/PublishingImages/Icons/loading.gif" id="loadingGif" style="display: none; width: 80%;"/>
				<div class="startDrawDiv">
					<button class="startDrawBtn" onclick="return false;">Start Draw بدء السحب</button>
				</div>
			</div>

			<div class="col-md-3">
				
			</div>

		
		</div>
		
		<div id="winnersDiv"  style="display: none;">
			<h2 class="winnersHeading">
				Winners
                <br />
                الفائزين
			</h2>
			<table id="winnersTbl" class="display" style="width: 100%;">
				<thead>
		            <tr>
		                <th>Customer Name <br /> إسم العميل</th>
		                <th>Account No <br /> رقم الحساب</th>
                        <th>Credit Card No <br /> رقم البطاقة الائتمانية</th>
                        <th>Branch Code <br /> رمز الفرع</th>
                        <th>Winning Amount <br />قيمة الجائزة</th>
		            </tr>
		        </thead>
			</table>
			<div class="row" style="margin-top: 25px; display: none;" id="saveResultsRow">
                <div class="col text-center">
                  <button class="btn btn-primary" onclick="SaveWinnerResults(); return false;">Save Results</button>
                </div>
            </div>
		</div>
		
	</div>
</div>

<script type="text/javascript">

    var maxWinners = 0;
    var winners = [];

    function LoadConfigurations() {

        $('.ajax-loader').css("visibility", "visible");

        var methodUrl = [location.protocol, '//', location.host].join('') + '/_layouts/15/PrizeDrawSystem/WebAPI.aspx/GetDrawButtonStatus';
        $.ajax({
            type: "POST",
            url: methodUrl,
            data: JSON.stringify({ drawCategory: "POS Machine" }),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (result) {

                $('.ajax-loader').css("visibility", "hidden");

                if (result != null && result.d != null) {

                    if (result.d == false) {

                        var html = '';
                        html += '<div class="toast" data-autohide="false" style="max-width: 100%;">';
                        html += '<div class="toast-header">';
                        html += '<strong class="mr-auto text-primary" id="msgHdr">Message</strong>';
                        html += '<button type="button" class="ml-2 mb-1 close" data-dismiss="toast">&times;</button>';
                        html += '</div>';
                        html += '<div class="toast-body" id="msgBody">';
                        html += 'Please Import Data to start draw';
                        html += '</div>';
                        html += '</div>';

                        $('.startDrawDiv').html(html);
                        $('.toast').toast('show');
                    }
                    else {
                        LoadDrawConfigurations();
                    }
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

    function LoadDrawConfigurations() {

        $('.ajax-loader').css("visibility", "visible");

        var methodUrl = [location.protocol, '//', location.host].join('') + '/_layouts/15/PrizeDrawSystem/WebAPI.aspx/GetMaxPOSMachineWinners';
        $.ajax({
            type: "POST",
            url: methodUrl,
            data: JSON.stringify({}),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (result) {

                $('.ajax-loader').css("visibility", "hidden");

                if (result != null && result.d != null) {

                    maxWinners = result.d;
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

    function DeleteAndArchiveData() {

        var methodUrl = [location.protocol, '//', location.host].join('') + '/_layouts/15/PrizeDrawSystem/WebAPI.aspx/ArchiveAndDeletePOSMachineData';
        $.ajax({
            type: "POST",
            url: methodUrl,
            data: JSON.stringify({}),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (result) {

            },
            error: function (jqXHR, textStatus, errorThrown) {
                alert('Error occured');
                if (jqXHR.status == 500) {
                    console.log('Internal error: ' + jqXHR.responseText);
                } else {
                    console.log('Unexpected error.');
                }
            }
        });
    }

    function SaveWinnerResults() {

        if (winners != null && winners.length > 0) {

            for (var i = 0; i < winners.length; i++) {

                winners[i].TransactionDate = new Date(parseInt(winners[i].TransactionDate.substr(6))).toJSON();
                delete winners[i].__type;
            }

            var methodUrl = [location.protocol, '//', location.host].join('') + '/_layouts/15/PrizeDrawSystem/WebAPI.aspx/SavePOSMachineWinners';
            $('.ajax-loader').css("visibility", "visible");

            $.ajax({
                type: "POST",
                url: methodUrl,
                data: JSON.stringify({ drawType: "<%=DrawType%>", allWinners: winners }),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (result) {

                    $('.ajax-loader').css("visibility", "hidden");

                    console.log(result);
                    if (result != null && result.d != null) {

                        DeleteAndArchiveData();

                        var html = '';
                        html += '<div class="toast" data-autohide="false" style="max-width: 100%;">';
                        html += '<div class="toast-header">';
                        html += '<strong class="mr-auto text-primary" id="msgHdr">Message</strong>';
                        html += '<button type="button" class="ml-2 mb-1 close" data-dismiss="toast">&times;</button>';
                        html += '</div>';
                        html += '<div class="toast-body" id="msgBody">';
                        html += 'Winners data is saved successfully';
                        html += '</div>';
                        html += '</div>';

                        $('.startDrawDiv').html(html);
                        $('.toast').toast('show');
                        $('#loadingGifRow').show();
                        $('#winnersDiv').html('');
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

    $(function () {

        
        LoadConfigurations();

        $('.startDrawBtn').click(function () {

            $('.startDrawBtn').hide();
            $("#loadingGif").fadeIn(2000);

            
            var methodUrl = [location.protocol, '//', location.host].join('') + '/_layouts/15/PrizeDrawSystem/WebAPI.aspx/';

            if (maxWinners > 5) {

                methodUrl += "GetPOSMachineAllWinners";
                $('.ajax-loader').css("visibility", "visible");

                $.ajax({
                    type: "POST",
                    url: methodUrl,
                    data: JSON.stringify({ drawType: "<%=DrawType%>" }),
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: function (result) {

                        $('.ajax-loader').css("visibility", "hidden");

                        if (result != null && result.d != null && result.d.length > 0) {

                            winners = result.d;

                            setTimeout(function () {

                                $('#winnersTbl').DataTable({
                                    data: winners,
                                    "bSort": false,
                                    columns: [
                                        { "data": "CustomerName" },
                                        { "data": "AccountNumber" },
                                        { "data": "CreditCardNumber" },
                                        { "data": "BranchCode" },
                                        { "data": "WinningAmount" }
                                    ]
                                });

                                $("#loadingGif").fadeOut(2000);
                                $('#loadingGifRow').hide();
                                $("#winnersDiv").fadeIn(2000);
                                $("#saveResultsRow").fadeIn(2000);

                            }, 3000);



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
            else {

                methodUrl += "GetPOSMachineSingleWinner";

                if (winners != null && winners.length > 0) {

                    for (var i = 0; i < winners.length; i++) {

                        winners[i].TransactionDate = new Date(parseInt(winners[i].TransactionDate.substr(6))).toJSON();
                        delete winners[i].__type;
                    }
                }

                $('.ajax-loader').css("visibility", "visible");

                $.ajax({
                    type: "POST",
                    url: methodUrl,
                    data: JSON.stringify({ drawType: "<%=DrawType%>", lastWinners: winners }),
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: function (result) {

                        $('.ajax-loader').css("visibility", "hidden");

                        if (result != null && result.d != null && result.d.length > 0) {

                            //result.d.forEach(function (item) {

                            //    winners.push(item);
                            //});

                            winners = result.d;

                            setTimeout(function () {

                                var table = $('#winnersTbl').DataTable();
                                table.destroy();
                                $('#winnersTbl').DataTable({
                                    data: winners,
                                    "bSort": false,
                                    columns: [
                                        { "data": "CustomerName" },
                                        { "data": "AccountNumber" },
                                        { "data": "CreditCardNumber" },
                                        { "data": "BranchCode" },
                                        { "data": "WinningAmount" }
                                    ]
                                });

                                $("#loadingGif").fadeOut(2000);
                                $("#winnersDiv").fadeIn(2000);

                                maxWinners--;

                                if (maxWinners == 0) {

                                    $('#loadingGifRow').hide();
                                    $("#saveResultsRow").fadeIn(2000);
                                }
                                else {
                                    $('.startDrawBtn').fadeIn(2000);
                                }

                            }, 3000);



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

            
        });



    });

</script>

	