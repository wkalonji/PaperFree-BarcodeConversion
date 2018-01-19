
<%@ Page Title="Print Indexes" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Indexes.aspx.cs" Inherits="BarcodeConversion.About" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

	 <script>

        function FadeOut() {
            $("table[id$='onScreenMsg']").delay(2000).fadeOut(1000);
         }

		function reprint() {
			$.when($.ajax(function1())).then(function () {
				setTimeout(function () {
					function3();
				}, 2000);
			});
            function function3() {document.getElementById('<%=getUnprinted.ClientID%>').click();}
		}

		function printing() {
			$.when($.ajax(function1())).then(function () {
				setTimeout(function () {
					function2();
				}, 2500);
			});
		    function function2() {document.getElementById('<%=goToQuestion.ClientID%>').click();}
		}

		function function1() { window.print();}
		
	</script>

    <div ID="indexPage" visible="false" runat="server"></div>

    <asp:UpdatePanel runat="server">
    <Triggers>
        <asp:PostBackTrigger ControlID="printBarcodeBtn" />
        <asp:PostBackTrigger ControlID="reprintBtn" />
    </Triggers>
    <ContentTemplate>
        <asp:Panel ID="unprintedIndexesPanel" runat="server">
            <asp:Panel ID="satisfied" Visible="false" style="margin-top:150px;margin-bottom:200px;" runat="server" Height="120"> 
			    <asp:Panel style="margin:0px auto;padding:20px;width:600px;" runat="server">
				    <table class="dataSection" style="font-size:12px;margin:10px 0 0 -100px;">
					    <tr >
						    <td style="text-align:left;">
							    <h5 style="display:inline;"><asp:Label ID="Label1" style="padding-top:25px;" Text="Satisfied with the Index Sheet(s)?" runat="server"></asp:Label></h5>
							    <asp:LinkButton ID="yesBtn" CssClass="btn btn-primary" style="margin-left:10px;" runat="server" OnClick="satisfied_Click">Yes</asp:LinkButton>
							    <asp:LinkButton ID="noBtn" CssClass="btn btn-danger" style="margin-left:10px;" runat="server" OnClick="satisfied_Click">No</asp:LinkButton>
						    </td>
					    </tr> 
					    <tr><td style="text-align:left;font-size:14px;padding-top:10px;color:#595959;">Click YES if you did print and are satisfied with the Index Sheets. </td></tr>
                        <tr><td style="text-align:left;font-size:14px;padding-top:2px;color:#595959;">Click NO if you did not print or are not satisfied with the Index Sheets.</td></tr>                
				    </table>
			    </asp:Panel>
		    </asp:Panel>

		    <div id="pageTitle" style="margin-top:45px; margin-bottom:5px; height:50px; border-bottom:solid 1px green;width:899px;" runat="server">
			    <table style="width:899px;">
				    <tr>
					    <td><h3 style="display:inline; padding-top:25px;color:#595959">Print Index Sheets</h3></td>
				    </tr>
			    </table>
		    </div>
		
		    <div id="mainContent" style="display:inline-block;margin-top:25px;" runat="server">
			    <asp:Panel CssClass="card" style="background-color:#e6f3ff;height:70px;" runat="server">          
				    <table id="unprintedIndexTable" style="width:615px;" runat="server">
					    <tr style="height:40px;padding-top:5px;">
						    <td style="padding-left:5px;font-family:Arial;font-size:15px;"><h4><asp:Label ID="printTitle" style="color:#666666" Text="Unprinted Indexes" runat="server"></asp:Label></h4></td> 
						    <td style="padding:5px 0px 5px 5px;text-align:left;text-align:right;">
							    <asp:linkButton ID="deleteBtn" CssClass="btn btn-danger" Font-Size="10" runat="server" 
								    OnClientClick="return confirm('Selected Indexes will be permanently deleted. Delete anyway?');" 
								    OnClick="deleteIndexes_Click"><i id="deleteIcon" class="fa fa-trash-o" style="font-size:14px;" aria-hidden="true" runat="server"></i>  Delete</asp:linkButton>
							    <asp:linkButton ID="printBarcodeBtn" CssClass="btn btn-primary" style="margin-left:7px;" Font-Size="10" runat="server" onclick="printBarcode_Click">
								    <i class="fa fa-print" aria-hidden="true" runat="server"></i> Print</asp:linkButton>
							    <asp:linkButton ID="reprintBtn" Visible="false" CssClass="btn btn-primary" style="margin-left:7px;" Font-Size="10" runat="server" onclick="printBarcode_Click">
								    <i class="fa fa-print" aria-hidden="true" runat="server"></i> Reprint</asp:linkButton>
							    <asp:linkButton ID="showPrinted" CssClass="btn btn-primary" style="margin-left:7px;" Font-Size="10" Visible="true" runat="server" onclick="showPrinted_Click">
								    <i class="fa fa-search-plus" aria-hidden="true" runat="server"></i> Printed</asp:linkButton>
							    <asp:linkButton ID="goBackBtn" Visible="false" CssClass="btn btn-primary" style="margin-left:7px;" Font-Size="10" runat="server" onclick="reset_Click">
								    <i class="fa fa-arrow-circle-left" style="font-size:13px;" aria-hidden="true" runat="server"></i> Go Back</asp:linkButton>
							    <%--<asp:linkButton style="margin-left:10px;" runat="server" ForeColor="#737373" OnClick="reset_Click" >
								    <i class="fa fa-refresh" Visible="true" Width="26" Height="26" BackColor="#e6f3ff" runat="server"></i>
							    </asp:linkButton>--%>
						    </td>
					    </tr>
				    </table>
			    </asp:Panel> 

			    <div style="height:25px;margin-top:10px;"><asp:Table ID="onScreenMsg" runat="server"></asp:Table></div>

			    <asp:Panel ID="gridContainer" style="display:inline-block;margin-right: 10px;" runat="server">
				    <table style="width:100%;font-size:12px;">
					    <tr >
						    <td>
							    <asp:Label ID="sortOrder" Text="Sorted By : CREATION_TIME ASC" runat="server"></asp:Label>
						    </td>
						    <td style="text-align:right;padding-right:25px;">
							    <asp:Label ID="recordsPerPageLabel" Text="Records per page " style="padding-right:5px;" runat="server"></asp:Label>
							    <asp:DropDownList ID="recordsPerPage" OnSelectedIndexChanged="onSelectedRecordsPerPage" style="padding:0px 10px 0px 5px;" onmousedown="this.focus()" runat="server" AutoPostBack="true">
								    <asp:ListItem Value="10" Selected="true">10</asp:ListItem>
								    <asp:ListItem Value="15">15</asp:ListItem>
								    <asp:ListItem Value="20">20</asp:ListItem>
								    <asp:ListItem Value="30">30</asp:ListItem>
								    <asp:ListItem Value="50">50</asp:ListItem>
								    <asp:ListItem Value="all">ALL</asp:ListItem>
							    </asp:DropDownList>
						    </td>
					    </tr>                  
				    </table>
			
				    <asp:GridView ID="indexesGridView" runat="server" style="margin-top:5px" CssClass="mydatagrid" PagerStyle-CssClass="pager" 
						      HeaderStyle-CssClass="header" RowStyle-CssClass="rows" AllowPaging="True" OnPageIndexChanging="pageChange_Click"
						     OnRowDataBound="rowDataBound" OnSorting="gridView_Sorting" Width="600" AllowSorting="True"> 
					    <columns>
						    <asp:templatefield HeaderText="Select">
							    <HeaderTemplate>
								    &nbsp;&nbsp;<asp:CheckBox ID="selectAll" runat="server" AutoPostBack="true" OnCheckedChanged="selectAll_changed" />
							    </HeaderTemplate>
							    <ItemTemplate>
								    &nbsp;<asp:CheckBox ID="cbSelect" runat="server" />
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
			    </asp:Panel>

			    <h5 style="color:#4d4dff;margin-top:25px;"><asp:Label ID="description" runat="server"></asp:Label></h5>
			
		    </div>
	    </asp:Panel>
    </ContentTemplate>
    </asp:UpdatePanel>


	<div style="display:none; margin-top:15px;">
		<asp:Button ID="goToQuestion" runat="server" Text="ShowPanel" onclick="goToQuestion_Click"/>
	</div>
    <div style="display:none; margin-top:15px;">
		<asp:Button ID="getUnprinted" runat="server" Text="ShowPanel" onclick="getUnprinted_Click"/>
	</div>
  
</asp:Content>
