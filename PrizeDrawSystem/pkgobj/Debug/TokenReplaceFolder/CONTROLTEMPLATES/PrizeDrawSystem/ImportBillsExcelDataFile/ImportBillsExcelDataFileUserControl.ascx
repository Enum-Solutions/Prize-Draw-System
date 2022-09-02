<%@ Assembly Name="PrizeDrawSystem, Version=1.0.0.0, Culture=neutral, PublicKeyToken=d14854d0414e8962" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ImportBillsExcelDataFileUserControl.ascx.cs" Inherits="PrizeDrawSystem.ImportBillsExcelDataFile.ImportBillsExcelDataFileUserControl" %>


<div class="row">
    <div class="col-md-12">
        <h2 class="text-center mt-5">Import Excel Data</h2>
        <hr class="mb-5">

        <div class="row" id="loadingGifRow">

            <div class="col-md-5 col-xs-9 col-sm-9">
                <p>Please select Excel file</p>
                <div class="custom-file mb-3">
                    <asp:FileUpload CssClass="custom-file-input" ID="customFile" ClientIDMode="Static" runat="server"/>
                    <%--<input type="file" class="custom-file-input" id="customFile" name="filename">--%>
                    <label class="custom-file-label" for="customFile">Choose file</label>
                </div>
            </div>

            <div class="col-md-7 col-xs-3 col-sm-3" style="padding-top: 38px;">
                <div class="col text-left">
                    <%--<button class="btn btn-primary" onclick="return false;" id="upload">Upload</button>--%>
                    <asp:Button CssClass="btn btn-primary" ID="UploadBtn" OnClientClick="UploadFile(); return false;" ClientIDMode="Static" runat="server" Text="Upload" />
                </div>
            </div>


        </div>

        <div id="winnersDiv">
            <div class="toast" data-autohide="false">
                <div class="toast-header">
                    <strong class="mr-auto text-primary" id="msgHdr">Message</strong>
                    <button type="button" class="ml-2 mb-1 close" data-dismiss="toast">&times;</button>
                </div>
                <div class="toast-body" id="msgBody">
                    Data is saved Succesfully
                </div>
            </div>

        </div>

    </div>
</div>

<script type="text/javascript">

    function UploadFile() {


        debugger;
        $('.toast').toast('dispose');

        var filesLength = 0;
        var file = $('input[type="file"]').val();
        var exts = ['xls', 'xlsx', 'XLS', 'XLSX'];

        var pdfList = [];
        // var pdfFile = { FileName: '', B64Data: '' };

        if (file) {

            var extension = file.substring(file.lastIndexOf('.') + 1, file.length);

            if ($.inArray(extension, exts) > -1) {

                var fileUpload = $('#customFile').get(0);
                var files = fileUpload.files;

                filesLength = files.length;
                for (var i = 0; i < files.length; i++) {
                    var reader = new window.FileReader();
                    reader.myFileIndex = i;
                    reader.onloadend = function () {
                        base64data = reader.result;
                        //pdfList.push({ FileName: files[this.myFileIndex].name, B64Data: base64data.substr(base64data.indexOf(',') + 1) });
                        console.log(base64data);
                        filesLength--;
                        var methodUrl = [location.protocol, '//', location.host].join('') + '/_layouts/15/PrizeDrawSystem/WebAPI.aspx/UploadExcelBills';
                        if (filesLength === 0) {

                            $('.ajax-loader').css("visibility", "visible");
                            $.ajax({
                                url: methodUrl,
                                type: "POST",
                                //cache: false,
                                contentType: "application/json; charset=utf-8",
                                dataType: "json",
                                data: JSON.stringify({ fileBase64: base64data.substr(base64data.indexOf(',') + 1) }),
                                success: function (data) {
                                    //alert('File Uploaded Successfully!');
                                    debugger;
                                    $('#customFile').val('');
                                    $('#customFile').next('.custom-file-label').html('');

                                    if (data.d == 0) {
                                        $('.ajax-loader').css("visibility", "hidden");
                                        $('#msgHdr').html('Error Message');
                                        $('#msgHdr').attr("style", "color: red !important");
                                        $('#msgBody').html('There is not data in the file or the data is not correctly formatted. Please select file which has proper information!');
                                        $('.toast').toast('show');
                                    }
                                    else {
                                        $('.ajax-loader').css("visibility", "hidden");
                                        $('#msgHdr').html('Success Message');
                                        $('#msgHdr').attr("style", "color: #7864a2 !important");
                                        $('#msgBody').html(data.d + ' records are successfully imported');
                                        $('.toast').toast('show');
                                    }
                                },
                                error: function (data) {
                                    debugger;
                                    $('#customFile').val('');
                                    $('#customFile').next('.custom-file-label').html('');

                                    $('.ajax-loader').css("visibility", "hidden");
                                    $('#msgHdr').attr("style", "color: red !important");
                                    $('#msgHdr').html('Error Message');
                                    $('#msgBody').html('Some error occurred. Please try again!');
                                    $('.toast').toast('show');
                                }
                            });
                        }
                    }
                    reader.readAsDataURL(files[i]);
                }
            }

            else {
                $('.ajax-loader').css("visibility", "hidden");
                $('#msgHdr').attr("style", "color: red !important");
                $('#msgHdr').html('Error Message');
                $('#msgBody').html('Invalid file, Only Excel files can be uploaded!!!');
                $('.toast').toast('show');
            }
        }
        else {
            $('#msgHdr').attr("style", "color: red !important");
            $('#msgHdr').html('Error Message');
            $('#msgBody').html('Please select file!');
            $('.toast').toast('show');
        }
    }

    $(function () {

        $('#customFile').on('change', function () {
            //get the file name
            var fileName = $(this).val();
            //replace the "Choose a file" label
            $(this).next('.custom-file-label').html(fileName);
        });

    });

</script>
