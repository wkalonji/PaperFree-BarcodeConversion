
<%@ Page Title="Print Indexes" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Indexes.aspx.cs" Inherits="BarcodeConversion.About" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

     <script>
         // FADEOUT INDEX-SAVED MSG. FUNCTION
         //function FadeOut() {
         //    $("span[id$='indexSavedMsg']").fadeOut(3000);
         //}
        // function FadeOut2() {
          //   $("span[id$='indexSetPrintedMsg']").fadeOut(3000);
         //}
        // PRINTING INDEX SHEETS. FUNCTION
        function printing() {
            window.print();
        }

        // PRINT WINDOW LISTNER. FUNCTION: ALLOW STUFF BE DONE RIGHT BFR or AFTER PRINT PREVIEW WINDOW.
        (function () {
            var beforePrint = function () {
                // Do something before printing dialogue box appears
            };
            // After printing dialogue box disappears, back to unprinted indexes gridview
            var afterPrint = function () {
                var answer = confirm("IMPORTANT!\n\nAre you satisfied?\n" +
                    "Click OK If you did print and are satisfied with the Index Sheets.\n" +
                    "Click CANCEL if you did not print or are not satisfied with the Index Sheets.");
                if (answer == true) {
                    document.getElementById("pageToPrint").style.display = "none";
                    document.getElementById('<%=setAsPrinted.ClientID%>').click();                                    
                } else {
                    document.getElementById("pageToPrint").style.display = "none";
                    document.getElementById('<%=getUnprinted.ClientID%>').click(); 
                }
            };

            if (window.matchMedia) {
                var mediaQueryList = window.matchMedia('print');
                mediaQueryList.addListener(function (mql) {
                    if (mql.matches) {
                        beforePrint();
                    } else {
                        afterPrint();
                    }
                });
            }
            window.onbeforeprint = beforePrint;
            window.onafterprint = afterPrint;
         }());
    </script>


    <asp:Panel ID="unprintedIndexesPanel" runat="server">
         <div style="margin-top:45px; margin-bottom:30px; height:50px; border-bottom:solid 1px green;width:899px;">
            <table style="width:899px;">
                <tr>
                    <td><h2 style="display:inline; padding-top:25px;">Print Index Sheets</h2></td>
                </tr>
            </table>
        </div>
        <table style="width:460px;">
            <tr style="height:50px;"> <th colspan="2">Upload Index Data: </th></tr>
            <tr style="background-color:aliceblue;height:40px;margin-top:10px;">
                <td style="width:300px; padding-left:5px;">
                   <INPUT style="width:300px;" type=file id=File1 name=File1 runat="server" /></td>
                <td style="text-align:right; font-size:12px;padding-right:10px;"><input type="submit" id="Submit1" value="Upload" OnClick="Submit1_ServerClick" runat="server" /></td>
                <td style="text-align:left;"><asp:Button ID="viewContent" Text="View Content" Font-Size="9" OnClick="Submit1_ServerClick" runat="server"/></td>
            </tr>
        </table> 

        <div style="display:inline-block;">           
            <table id="unprintedIndexTable" style="width:100%;" runat="server">
                <tr><td><h2 style="margin-top:35px">Unprinted Indexes</h2></td></tr>
                <tr style="background-color:#e6f3ff; height:40px;margin-top:10px;">
                    <td style="text-align:left; padding-left:5px;">
                        <asp:Button ID="deleteBtn" Width="105" Visible="false" runat="server" Text="Delete Indexes" 
                            OnClientClick="return confirm('Selected Indexes will be permanently deleted. Delete anyway?');" 
                            OnClick="deleteIndexes_Click" />
                    </td>
                    <td style="text-align:left;padding-right:5px;"><asp:Button ID="printBarcodeBtn" Width="105" Visible="false" runat="server" Text="Print Barcodes" onclick="printBarcode_Click"/></td>
                    <td style="text-align:right;"> 
                        <div style="display:block;padding-bottom:1px;padding-top:6px;">
                            <asp:ImageButton ID="resetBtn" ImageUrl="Content/reset.png" Width="30" Height="30"  BackColor="#e6f3ff" Visible="true" runat="server" OnClick="getUnprintedIndexes_Click" />
                        </div>
                    </td>
                </tr>
            </table>
            <table style="width:99.9%;">
                <tr >
                    <td style="padding-top:10px;">
                        <asp:Label ID="sortOrder" Text="Sorted By : CREATION_TIME ASC" runat="server"></asp:Label>
                    </td>
                    <td style="text-align:right;">
                        <asp:Label ID="recordsPerPageLabel" Text="Records per page" runat="server"></asp:Label>
                        <asp:DropDownList ID="recordsPerPage" OnSelectedIndexChanged="onSelectedRecordsPerPage" runat="server" AutoPostBack="true">
                            <asp:ListItem Value="5">5</asp:ListItem>
                            <asp:ListItem Value="10" Selected="true">10</asp:ListItem>
                            <asp:ListItem Value="15">15</asp:ListItem>
                            <asp:ListItem Value="20">20</asp:ListItem>
                            <asp:ListItem Value="30">30</asp:ListItem>
                        </asp:DropDownList>
                    </td>
                </tr>                  
            </table>
            <asp:Label ID="description" runat="server"></asp:Label>
            <asp:GridView ID="indexesGridView" runat="server" style="margin-top:5px" CssClass="mydatagrid" PagerStyle-CssClass="pager" 
                          HeaderStyle-CssClass="header" RowStyle-CssClass="rows" AllowPaging="True" OnPageIndexChanging="pageChange_Click"
                         OnRowDataBound="rowDataBound" OnSorting="gridView_Sorting" AllowSorting="True"> 
                <columns>
                    <asp:templatefield HeaderText="Select">
                        <HeaderTemplate>
                            &nbsp;<asp:CheckBox ID="selectAll" runat="server" AutoPostBack="true" OnCheckedChanged="selectAll_changed" />
                        </HeaderTemplate>
                        <ItemTemplate>
                            <asp:CheckBox ID="cbSelect" runat="server" />
                        </ItemTemplate>
                    </asp:templatefield>
                    <asp:templatefield HeaderText="&nbsp;N°" ShowHeader="true">
                        <ItemTemplate>
                            <%# Container.DataItemIndex + 1 %>
                        </ItemTemplate>
                    </asp:templatefield>
                </columns>
                <HeaderStyle CssClass="header" />
                <PagerStyle CssClass="pager" />
                <RowStyle CssClass="rows" />
            </asp:GridView>
        </div>
         
    </asp:Panel>
    <div style="display:none; margin-top:15px;">
        <asp:Button ID="setAsPrinted" runat="server" Text="ShowPanel" onclick="setAsPrinted_Click"/>
    </div>
     <div style="display:none; margin-top:15px;">
        <asp:Button ID="getUnprinted" runat="server" Text="ShowPanel" onclick="getUnprinted_Click"/>
    </div>
  
</asp:Content>
