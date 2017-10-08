<%@ Page Title="Index Status" Language="C#"  MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="IndexStatus.aspx.cs" Inherits="BarcodeConversion.IndexStatus" %>


<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
	
<script src="Scripts/jquery.dynDateTime.min.js" type="text/javascript"></script>
<script src="Scripts/calendar-en.min.js" type="text/javascript"></script>
<link href="Content/calendar-blue.css" rel="stylesheet" type="text/css" />
<script type="text/javascript">
	$(document).ready(function () {
		$("#<%=from.ClientID %>").dynDateTime({
			showsTime: true,
			ifFormat: "%Y/%m/%d %H:%M",
			daFormat: "%l;%M %p, %e %m, %Y",
			align: "BR",
			electric: false,
			singleClick: false,
			displayArea: ".siblings('.dtcDisplayArea')",
			button: ".next()"
		});
		$("#<%=to.ClientID %>").dynDateTime({
			showsTime: true,
			ifFormat: "%Y/%m/%d %H:%M",
			daFormat: "%l;%M %p, %e %m, %Y",
			align: "BR",
			electric: false,
			singleClick: false,
			displayArea: ".siblings('.dtcDisplayArea')",
			button: ".next()"
		});
	});
</script>


	 <asp:Panel ID="indexStatusPanel" runat="server">
		<div style="margin-top:45px; height:50px; border-bottom:solid 1px green;width:899px;">
			<table style="width:899px;">
				<tr>
					<td><h2 style="display:inline; padding-top:25px;color:#595959">Index Status Report</h2></td>
				</tr>
			</table>
		</div>  
		<div style="display:inline-block;">           
			<table class = "table" style="width:99%;">
				<tr style="background-color:#e6f3ff;">
					<td><asp:Label ID="filterLabel" Width="55" runat="server"><h4>Filter :</h4></asp:Label></td>
					<td style="padding-top:14px;"> 
						<asp:DropDownList ID="jobsFilter" OnSelectedIndexChanged="onSelectedChange" runat="server">
							<asp:ListItem Value="allJobs">Your Jobs</asp:ListItem>
						</asp:DropDownList>
					</td>
					<td style="padding-top:14px;"> 
						<asp:DropDownList ID="whoFilter" OnSelectedIndexChanged="onSelectedChange" runat="server" AutoPostBack="true">
							<asp:ListItem Value="meOnly">Your Indexes Only</asp:ListItem>
							<asp:ListItem Value="everyone">Indexes for all Operators</asp:ListItem>
						</asp:DropDownList>
					</td>
					<td style="padding-top:14px;"> 
						<asp:DropDownList ID="whenFilter" OnSelectedIndexChanged="onSelectWhen" runat="server" AutoPostBack="true">
							<asp:ListItem Value="allTime">For All Time</asp:ListItem>
							<asp:ListItem Value="pickRange">Pick Date/Time Range</asp:ListItem>
						</asp:DropDownList>
					</td>
					<td style="padding-top:14px;"> 
						<asp:DropDownList ID="whatFilter" OnSelectedIndexChanged="onSelectedChange" runat="server" AutoPostBack="true">
							<asp:ListItem Value="allSheets">All Sheets</asp:ListItem>
							<asp:ListItem Value="printed">Printed Only</asp:ListItem>
							<asp:ListItem Value="notPrinted">Not Printed</asp:ListItem>
						</asp:DropDownList>
					</td>
					<td style="text-align:right;"> 
						<div style="display:block;padding:10px 5px 1px 4px;" >
							<asp:linkButton runat="server" ForeColor="#737373" OnClick="reset_Click" >
								<i class="fa fa-refresh" Visible="true" Width="26" Height="26" BackColor="#e6f3ff" runat="server" ></i>
							</asp:linkButton>
						</div>
					</td>
				</tr>
			</table> 
			<asp:Panel ID="timePanel" Visible="false" runat="server">
				<table class = "table" style="width:600px;height:50px !important;">
					<tr>
						<td><asp:label runat="server">From:&nbsp;&nbsp;&nbsp;</asp:label>
							<asp:TextBox ID="from" runat="server" ></asp:TextBox>
							<img style="margin-left:2px;" src="Content/calender.png" /> 
						</td>
						<td style="padding-left:15px;"><asp:label runat="server">To:&nbsp;&nbsp;</asp:label>
							<asp:TextBox ID="to" runat="server" ></asp:TextBox>
							<img style="margin-left:2px;display:inline;s" src="Content/calender.png" /> 
						</td>
						<td style="padding-left:15px;">
							<asp:Button ID="dates" Text="Submit" runat="server" onclick="submit_Click" />
						</td>
					</tr>
				</table>
			</asp:Panel>
			<div style="display:inline-block; width:99.9%">
				<table id="gridHeader" style="width:100%;margin-top:15px;margin-bottom:-10px;" runat="server">
					<tr><td colspan="2"><h4 style="color:#4d4dff"><asp:Label ID="description" Text="" runat="server"></asp:Label></h4> </td></tr>
					<tr>
						<td><asp:Label ID="sortOrder" Text="Sorted By : CREATION_TIME ASC" runat="server"></asp:Label><asp:Label id="sortDirection" Font-Size="8" runat="server"></asp:Label></td>
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
			
				<asp:GridView ID="indexeStatusGridView" runat="server" style="margin-top:15px" CssClass="mydatagrid" PagerStyle-CssClass="pager"
							HeaderStyle-CssClass="header" RowStyle-CssClass="rows" AllowPaging="true" OnPageIndexChanging="pageChange_Click"
							OnRowDataBound="rowDataBound" OnSorting="gridView_Sorting" AllowSorting="True" Width="637">  
					<columns>
						<asp:templatefield HeaderText ="&nbsp;N°" ShowHeader="true">
							<ItemTemplate >
								<%# Container.DataItemIndex + 1 %>
							</ItemTemplate>
						</asp:templatefield>
					</columns>       
				</asp:GridView>
			</div>
		
		</div>   
	</asp:Panel>
</asp:Content>
