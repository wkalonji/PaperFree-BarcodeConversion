<%@ Page Title="Create Indexes" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="BarcodeConversion._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <script>
        // FADEOUT INDEX-SAVED MSG. FUNCTION
        function FadeOut() {
            $("span[id$='indexSetPrintedMsg']").hide();
            $("span[id$='indexSavedMsg']").fadeOut(5000);
        } 
        function FadeOut2() {
            $("span[id$='indexSavedMsg']").hide();
            $("span[id$='indexSetPrintedMsg']").fadeOut(5000);
        }
        // PRINTING INDEX SHEETS. FUNCTION
        function printing() {
            $.when($.ajax(function1())).then(function () {
                setTimeout(function () {
                    function2();
                }, 1000);
            });
        }
        function function1() {
            window.print();
        }
        function function2() {
            var answer = confirm("Are you satisfied?\n\n" +
                "Click OK If you did print and are satisfied with the Index Sheets.\n" +
                "Click CANCEL if you did not print or are not satisfied with the Index Sheets.");
            if (answer == true) {
                // Set the just printed index as PRINTED
                document.getElementById("pageToPrint").style.display = "none";
                document.getElementById('<%=setAsPrinted.ClientID%>').click();
            } else {
                document.getElementById("pageToPrint").style.display = "none";
                document.getElementById('<%=backToForm.ClientID%>').click(); 
            }
        }
    </script>

    <asp:Panel ID="formPanel" runat="server">
        <asp:Panel ID="formPanelJobSelection" runat="server">
            <div style="margin-top:45px; margin-bottom:40px; height:50px; border-bottom:solid 1px green;width:899px;">
                <h2 style="margin-top:45px;color:#595959;">Index Setup</h2>
            </div>
            <asp:Button ID="selectJobBtn" Visible="false" runat="server" Text="Generate Jobs" onclick="selectJob_Click" />

            <table class = table>
                <tr> <th colspan="2" style="font-family:Arial;">Select Job: </th></tr>
                <tr>
                    <td style="width: 186px"><asp:Label ID="selectJobLabel" runat="server">Job Abbreviation:</asp:Label></td>
                    <td> 
                        <asp:DropDownList ID="selectJob"  OnSelectedIndexChanged="onJobSelect" AutoPostBack="true" runat="server">
                            <asp:ListItem Value="Select">Select</asp:ListItem>
                        </asp:DropDownList>
                    </td>
                </tr>
            </table> 
            <asp:Panel ID="noJobsFound" Visible="false" runat="server"><h3> You currently have no accessible jobs.</h3> </asp:Panel>
        </asp:Panel>
        
        <asp:panel ID="indexCreationSection" Visible="false" runat="server" style="width:auto; margin:auto">  
            <table style="width:470px;">
                <tr style="height:50px;"> <th colspan="2" style="font-family:Arial;">&nbsp;Upload Index Data File: </th></tr>
                <tr style="background-color:aliceblue;height:40px;margin-top:10px;">
                    <td style="width:315px; padding-left:5px;">
                       <INPUT style="width:300px;" type=file id=File1 name=File1 runat="server" /></td>
                    <td style="text-align:right; font-size:13px;padding-right:7px;"><asp:Button ID="upload" Text="Upload" OnClick="upload_Click" runat="server"/></td>
                </tr>
            </table>    

            <table id="uploadedFileMenu" Visible="false" style="margin-top:12px; width:470px;" runat="server">
                <tr>
                    <td style="padding-right:15px;width:330px;"><asp:Label ID ="uploadSuccess" Text="File Uploaded successfully!" runat="server"></asp:Label></td>
                    <td style="text-align:right; padding-right:5px;"><asp:Button ID="viewContentBtn" Text="View" Font-Size="8" OnClick="viewContent_Click" runat="server"/></td>
                    <td style="text-align:right; padding-right:5px;">
                        <asp:Button ID="saveIndexesBtn" Text="Save" Font-Size="8" OnClientClick="return confirm('ATTENTION!\n\nMake sure the uploaded file corresponds to the currently selected Job.\nWish to proceed and save indexes?');" OnClick="saveIndexes_Click" runat="server"/></td>
                    <td style="text-align:right;">
                        <asp:Button ID="printIndexesBtn" Text="Save & Print" Font-Size="8" OnClientClick="return confirm('ATTENTION!\n\nMake sure the uploaded file corresponds to the currently selected Job.\nWish to proceed and print barcodes?');" OnClick="printIndexes_Click" runat="server"/></td>
                </tr>
                <tr>
                    <td style="padding-right:15px;"><asp:Label ID ="uploadHidden" Text="" Visible="false" runat="server"></asp:Label></td>
                </tr>
            </table>

            <asp:GridView ID="GridView1" CssClass="mGrid" Font-Size="10" OnRowDataBound="rowDataBound" style="margin-top:20px;" Width=470 runat="server"></asp:GridView>

            <asp:table id="fileEntryMsg" runat="server"></asp:table>
                  
            <h2 style="margin-top:40px;color:#666666;">Index Creation</h2>

            <table id="jobControls" class = table runat="server">
                <tr> <th colspan="3" style="font-family:Arial;">Index Data Information: </th></tr>
                <tr>
                    <td style="vertical-align:middle;width:185px;"><asp:Label ID="LABEL1" Text="LABEL1" Visible="false" runat="server"></asp:Label></td>
                    <td>
                        <asp:TextBox ID="label1Box" Visible="false" placeholder=" Required" onfocus="this.select()" runat="server"></asp:TextBox>
                        <asp:DropDownList ID="label1Dropdown" onmousedown="this.focus()" Visible="false" runat="server">
                            <asp:ListItem Value=""></asp:ListItem>
                        </asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td style="vertical-align:middle;"><asp:Label ID="LABEL2" Text="LABEL2"  Visible="false" runat="server"></asp:Label></td>
                    <td>
                        <asp:TextBox ID="label2Box" Visible="false" onfocus="this.select()" runat="server"></asp:TextBox>
                        <asp:DropDownList ID="label2Dropdown" onmousedown="this.focus()" Visible="false" runat="server">
                            <asp:ListItem Value=""></asp:ListItem>
                        </asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td style="vertical-align:middle;"><asp:Label ID="LABEL3" Text="LABEL3"  Visible="false" runat="server"></asp:Label></td>
                    <td>
                        <asp:TextBox ID="label3Box" Visible="false" onfocus="this.select()" runat="server"></asp:TextBox>
                        <asp:DropDownList ID="label3Dropdown" onmousedown="this.focus()" Visible="false" runat="server">
                            <asp:ListItem Value=""></asp:ListItem>
                        </asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td style="vertical-align:middle;"><asp:Label ID="LABEL4" Text="LABEL4"  Visible="false" runat="server"></asp:Label></td>
                    <td>
                        <asp:TextBox ID="label4Box" Visible="false" onfocus="this.select()" runat="server"></asp:TextBox>
                        <asp:DropDownList ID="label4Dropdown" onmousedown="this.focus()" Visible="false" runat="server">
                            <asp:ListItem Value=""></asp:ListItem>
                        </asp:DropDownList>
                    </td>
                </tr>
                <tr>
                    <td style="vertical-align:middle;"><asp:Label ID="LABEL5" Text="LABEL5"  Visible="false" runat="server"></asp:Label></td>
                    <td>
                        <asp:TextBox ID="label5Box" Visible="false" onfocus="this.select()" runat="server"></asp:TextBox>
                        <asp:DropDownList ID="label5Dropdown" onmousedown="this.focus()" Visible="false" runat="server">
                            <asp:ListItem Value=""></asp:ListItem>
                        </asp:DropDownList>
                    </td>
                </tr>
            </table>

     <%--
        Not utilized (yet).
        <p>
            Barcode thickness:
            <asp:DropDownList ID="ddlBarcodeThickness" runat="server">
                <asp:ListItem Value="1">Thin</asp:ListItem>
                <asp:ListItem Value="2">Medium</asp:ListItem>
                <asp:ListItem Value="3">Thick</asp:ListItem>    
            </asp:DropDownList>
        </p>
           
     --%>   <asp:Panel ID="generateIndexSection" Visible="false" runat="server">
                <table class = tableFull style="margin-top:25px; width:470px;">
                    <tr style="background-color:#e6f3ff;height:40px;margin-top:10px;">
                        <td style="padding-left:5px;"><asp:Button ID="saveIndex" runat="server" Text="Save Index" Font-Size="10" onclick="saveIndex_Click" /></td>
                        <td style="text-align:right; padding-right:5px;"><asp:Button ID="saveAndPrint" runat="server" Text="Save & Print" Font-Size="10" onclick="printIndexes_Click" /></td>
                    </tr>               
                </table>
            </asp:Panel>
        </asp:panel>

        <div style="margin-top:50px;"></div>
        <%--Link to Print Indexes page --%>     
        <asp:HyperLink ID="HyperLink1" Font-Underline="true" runat="server" NavigateUrl="~/Indexes">
            <span style="font-size:medium;">View all of your unprinted indexes</span>
        </asp:HyperLink>
        <input id="indexString" type="hidden"  runat="server"  value=""/>
    </asp:Panel>

    <%-- Helper hidden buttons to help set as PRINTED the just-printed index And help get back to blank form--%>
    <div style="display:none;">
        <asp:Button ID="setAsPrinted" runat="server" Text="ShowPanel" onclick="setAsPrinted_Click"/>
    </div>
    <div style="display:none;">
        <asp:Button ID="backToForm" runat="server" Text="ShowPanel" onclick="backToForm_Click"/>
    </div>
  
    <%-- Msgs showing up when successful save or saveAndPrint operations happens.--%>
    <div style="margin-top:10px;"><asp:table id="manualEntryMsg" runat="server"></asp:table></div>
</asp:Content>
