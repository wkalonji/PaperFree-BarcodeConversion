
<%@ Page Title="Print Indexes" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Indexes.aspx.cs" Inherits="BarcodeConversion.About" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
	 <style type="text/css">
		.print+.print{
			page-break-before: always;
		}
	 </style>
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
			$.when($.ajax(function1())).then(function () {
				setTimeout(function () {
					function2();
				}, 1300);
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
				document.getElementById("pageToPrint").style.display = "none";
				document.getElementById('<%=setAsPrinted.ClientID%>').click();                                    
			} else {
				document.getElementById("pageToPrint").style.display = "none";
				document.getElementById('<%=getUnprinted.ClientID%>').click(); 
			}
		}
	</script>


	<asp:Panel ID="unprintedIndexesPanel" runat="server">
		 <div style="margin-top:45px; margin-bottom:30px; height:50px; border-bottom:solid 1px green;width:899px;">
			<table style="width:899px;">
				<tr>
					<td><h2 style="display:inline; padding-top:25px;color:#595959">Print Index Sheets</h2></td>
				</tr>
			</table>
		</div>

		<div style="display:inline-block;">           
			<table id="unprintedIndexTable" style="width:100%;min-width:650px;" runat="server">
				<tr style="height:40px;background-color:#e6f3ff;padding-top:5px;">
					<th style="padding-left:5px;font-family:Arial;font-size:15px;"><asp:Label ID="printTitle" Text="Unprinted Indexes" runat="server"></asp:Label></th> 
					<td style="padding:5px 8px 5px 5px;text-align:left;text-align:right;">
						<asp:Button ID="deleteBtn" Font-Size="10" runat="server" Text="Delete" 
							OnClientClick="return confirm('Selected Indexes will be permanently deleted. Delete anyway?');" 
							OnClick="deleteIndexes_Click" />
						<asp:Button ID="printBarcodeBtn" style="margin-left:15px;" Font-Size="10" runat="server" Text="Print" onclick="printBarcode_Click"/>
                        <asp:Button ID="showPrinted" style="margin-left:15px;" Font-Size="10" Visible="true" runat="server" Text="Show Printed" onclick="showPrinted_Click"/>
						<asp:linkButton style="margin-left:10px;" runat="server" ForeColor="#737373" OnClick="reset_Click" >
							<i class="fa fa-refresh" Visible="true" Width="26" Height="26" BackColor="#e6f3ff" runat="server" ></i>
						</asp:linkButton>
					</td>
				</tr>
			</table>
			<table style="width:99.8%; margin-top:20px;">
				<tr >
					<td>
						<asp:Label ID="sortOrder" Text="Sorted By : CREATION_TIME ASC" runat="server"></asp:Label>
					</td>
					<td style="text-align:right;">
						<asp:Label ID="recordsPerPageLabel" Text="Records per page " style="padding-right:5px;" runat="server"></asp:Label>
						<asp:DropDownList ID="recordsPerPage" OnSelectedIndexChanged="onSelectedRecordsPerPage" runat="server" AutoPostBack="true">
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
			<h4 style="color:#4d4dff"><asp:Label ID="description" runat="server"></asp:Label></h4>
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
		</div>
		 
	</asp:Panel>
	<div style="display:none; margin-top:15px;">
		<asp:Button ID="setAsPrinted" runat="server" Text="ShowPanel" onclick="setAsPrinted_Click"/>
	</div>
	 <div style="display:none; margin-top:15px;">
		<asp:Button ID="getUnprinted" runat="server" Text="ShowPanel" onclick="getUnprinted_Click"/>
	</div>
  
</asp:Content>
