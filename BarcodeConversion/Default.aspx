﻿<%@ Page Title="Create Indexes" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="BarcodeConversion._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

	<script>
        // Hide all
	    function hideForm() {
	        //$("#formPanel").hide();
	        document.getElementById("formPanel").style.display = "none";
	    }
		// FADEOUT INDEX-SAVED MSG. FUNCTION
		function FadeOut() {
			$("table[id$='manualEntryMsg']").delay(2000).fadeOut(1000);
		}

		function FadeOut2() {
			$("table[id$='fileEntryMsg']").delay(2000).fadeOut(1000);
		}

		function FadeOut3() {
		    $.when($.ajax(hideMsg())).then(function () {
		        setTimeout(function () {
		            showMsg();
		        }, 1300);
		    });
		    function hideMsg() { $("table[id$='manualEntryMsg']").hide(); }
		    function showMsg() { $("table[id$='manualEntryMsg']").show(); }
		}

		// PRINTING INDEX SHEETS. FUNCTION
		function printing() {
		    $.when($.ajax(function1())).then(function () {
				setTimeout(function () {
				    function2();
				}, 2500);
		    });
            function function1() { window.print();}
		    function function2() { document.getElementById('<%=goToQuestion.ClientID%>').click();}
		}

	    // Manual Save Btn pause.
	    function saveBtnPause() {
	        $.when($.ajax(pauseFunction())).then(function () {
	            setTimeout(function () {
	                unpauseFunction();
	            }, 1300);
	        });
	        function pauseFunction() {
	            document.getElementById('<%=saveIndex.ClientID %>').setAttribute("disabled", "disabled");
	            document.getElementById('<%=saveAndPrint.ClientID %>').setAttribute("disabled", "disabled");
	            document.getElementById('<%=File1.ClientID %>').setAttribute("disabled", "disabled");
                document.getElementById('<%=upload.ClientID %>').setAttribute("disabled", "disabled");
	        }
	        function unpauseFunction() {
	            document.getElementById('<%=saveIndex.ClientID %>').removeAttribute("disabled");
	            document.getElementById('<%=saveAndPrint.ClientID %>').removeAttribute("disabled");
	            document.getElementById('<%=File1.ClientID %>').removeAttribute("disabled");
                document.getElementById('<%=upload.ClientID %>').removeAttribute("disabled");
	        }
	    } 
	    
		
	</script>

    <div ID="indexPage" visible="false" runat="server"></div>

    <asp:UpdatePanel runat="server">
        <Triggers>
            <asp:PostBackTrigger ControlID="upload" />
            <asp:PostBackTrigger ControlID="printIndexesBtn" />
            <asp:PostBackTrigger ControlID="saveAndPrint" />
        </Triggers>
        <ContentTemplate>
	        <div ID="formPanel" runat="server">
		        <asp:Panel ID="satisfied" Visible="false" style="margin-top:150px;margin-bottom:200px;" runat="server" Height="120"> 
			        <asp:Panel style="margin:0px auto;padding:20px;width:600px;" runat="server">
				        <table class="dataSection" style="font-size:12px;margin:10px 0 0 -100px;">
					        <tr >
						        <td style="text-align:left;">
							        <h5 style="display:inline;"><asp:Label ID="sortOrder" style="padding-top:25px;" Text="Satisfied with the Index Sheet(s)?" runat="server"></asp:Label></h5>
							        <asp:LinkButton ID="yesBtn" CssClass="btn btn-primary" style="margin-left:10px;" runat="server" OnClick="satisfied_Click">Yes</asp:LinkButton>
							        <asp:LinkButton ID="noBtn" CssClass="btn btn-danger" style="margin-left:10px;" runat="server" OnClick="satisfied_Click">No</asp:LinkButton>
						        </td>
					        </tr> 
					        <tr><td style="text-align:left;font-size:14px;padding-top:10px;color:#595959;">Click YES if you did print and are satisfied with the Index Sheets. </td></tr>
                            <tr><td style="text-align:left;font-size:14px;padding-top:2px;color:#595959;">Click NO if you did not print or are not satisfied with the Index Sheets.</td></tr>                
				        </table>
			        </asp:Panel>
		        </asp:Panel>

		        <asp:Panel ID="formPanelJobSelection" runat="server">
			        <div style="margin-top:45px; margin-bottom:40px; height:50px; border-bottom:solid 1px green;width:899px;">
				        <h3 style="margin-top:45px;color:#595959;">Index Setup</h3>
			        </div>
			        <asp:Button ID="selectJobBtn" Visible="false" runat="server" Text="Generate Jobs" onclick="selectJob_Click" />

			        <table class = table>
				        <tr> <th colspan="2" style="font-family:Arial;font-size:14px;">Select Job: </th></tr>
				        <tr style="vertical-align:central;">
					        <td style="width: 186px;"><asp:Label ID="selectJobLabel" Font-Size="11" runat="server">Job Abbreviation:</asp:Label></td>
					        <td> 
						        <asp:DropDownList  ID="selectJob" OnSelectedIndexChanged="onJobSelect" onmousedown="this.focus()" AutoPostBack="true" runat="server">
							        <asp:ListItem Value="Select">Select </asp:ListItem>
						        </asp:DropDownList>
					        </td>
				        </tr>
			        </table> 
			        <asp:Panel ID="noJobsFound" Visible="false" runat="server"><h3> You currently have no assigned jobs.</h3> </asp:Panel>
		        </asp:Panel>

		        <asp:Panel ID="indexCreationUpdatePanel" runat="server" >
                    <asp:Table ID="notConfigScreenMsg" style="margin-top:10px;" runat="server"></asp:Table>
                    <asp:Panel ID="indexCreationSection" Visible="false" style="width:auto; margin-top:20px;" runat="server">
			            <asp:Label colspan="2" style="font-family:Arial;font-weight:bold;display:block;" runat="server">&nbsp;Upload Index Data File: </asp:Label>
			            <asp:Panel CssClass="card" runat="server" style="background-color:aliceblue;margin-top:5px;">
				            <table style="width:470px;" >
					            <tr style="height:40px;margin-top:10px;">
						            <td style="width:315px; padding-left:5px;">
						                <asp:FileUpload id="File1" style="width:300px;height:30px;font-size:14px;" type=file name=File1 runat="server" /></td>
						            <td style="text-align:right; font-size:13px;padding-right:7px;">
							            <asp:linkButton ID="upload" CssClass="btn btn-primary" OnClick="upload_Click" runat="server">
							            <i class="fa fa-upload" aria-hidden="true"></i> Upload</asp:linkButton></td>
					            </tr>
				            </table>  
			            </asp:Panel>

			            <asp:Panel id="uploadedFileMenu" Visible="false" style="display:block;width:502px;margin-top:10px;" runat="server">
				            <table style="padding-top:5px;" runat="server">
					            <tr>
						            <td style="padding-right:15px;width:330px;"><asp:Label ID ="uploadSuccess" Text="File Uploaded successfully!" runat="server"></asp:Label></td>
						            <td style="text-align:right; padding-right:5px;">
							            <asp:Button ID="viewContentBtn" CssClass="btn btn-primary" style="padding:0px 6px 1px 6px;" Text="View" Font-Size="8" OnClick="viewContent_Click" runat="server"/></td>
						            <td style="text-align:right; padding-right:5px;">
							            <asp:Button ID="saveIndexesBtn" CssClass="btn btn-primary" style="padding:0px 6px 1px 6px;" Text="Save" Font-Size="8" OnClientClick="return confirm('CORRECT FILE UPLOADED?\n\nMake sure the uploaded file corresponds to the currently selected Job.\nWish to proceed and save indexes?');" OnClick="saveIndexes_Click" runat="server"/></td>
						            <td style="text-align:right;">
							            <asp:Button ID="printIndexesBtn" CssClass="btn btn-primary" style="padding:0px 6px 1px 6px;" Text="Save & Print" Font-Size="8" OnClientClick="return confirm('CORRECT FILE UPLOADED?\n\nMake sure the uploaded file corresponds to the currently selected Job.\nWish to proceed and print barcodes?');" OnClick="printIndexes_Click" runat="server"/></td>
					            </tr>
					            <tr>
						            <td style="padding-right:15px;"><asp:Label ID ="uploadHidden" Text="" Visible="false" runat="server"></asp:Label></td>
					            </tr>
				            </table>

				            <asp:GridView ID="GridView1" CssClass="mGrid" Font-Size="10" OnRowDataBound="rowDataBound" style="margin-top:20px;" Width="100%" runat="server"></asp:GridView>
			            </asp:Panel>

			            <div style="margin:10px 0 20px 0; height:30px;"><asp:table id="fileEntryMsg" runat="server"></asp:table></div>
				  
			            <h3 style="margin-top:20px;color:#666666;">Index Creation</h3>

			            <table id="jobControls" class = table runat="server">
				            <tr> <th colspan="3" style="font-family:Arial;font-size:14px;">Index Data Information: </th></tr>
				            <tr>
					            <td style="vertical-align:middle;width:185px;"><asp:Label ID="LABEL1" Font-Size="11" Text="LABEL1" Visible="false" runat="server"></asp:Label></td>
					            <td>
						            <asp:TextBox ID="label1Box" Visible="false" placeholder=" Required" onfocus="this.select()" runat="server"></asp:TextBox>
						            <asp:DropDownList ID="label1Dropdown" onmousedown="this.focus()" Visible="false" runat="server">
							            <asp:ListItem Value=""></asp:ListItem>
						            </asp:DropDownList>
					            </td>
				            </tr>
				            <tr>
					            <td style="vertical-align:middle;"><asp:Label ID="LABEL2" Font-Size="11" Text="LABEL2"  Visible="false" runat="server"></asp:Label></td>
					            <td>
						            <asp:TextBox ID="label2Box" Visible="false" onfocus="this.select()" runat="server"></asp:TextBox>
						            <asp:DropDownList ID="label2Dropdown" onmousedown="this.focus()" Visible="false" runat="server">
							            <asp:ListItem Value=""></asp:ListItem>
						            </asp:DropDownList>
					            </td>
				            </tr>
				            <tr>
					            <td style="vertical-align:middle;"><asp:Label ID="LABEL3" Font-Size="11" Text="LABEL3"  Visible="false" runat="server"></asp:Label></td>
					            <td>
						            <asp:TextBox ID="label3Box" Visible="false" onfocus="this.select()" runat="server"></asp:TextBox>
						            <asp:DropDownList ID="label3Dropdown" onmousedown="this.focus()" Visible="false" runat="server">
							            <asp:ListItem Value=""></asp:ListItem>
						            </asp:DropDownList>
					            </td>
				            </tr>
				            <tr>
					            <td style="vertical-align:middle;"><asp:Label ID="LABEL4" Font-Size="11" Text="LABEL4"  Visible="false" runat="server"></asp:Label></td>
					            <td>
						            <asp:TextBox ID="label4Box" Visible="false" onfocus="this.select()" runat="server"></asp:TextBox>
						            <asp:DropDownList ID="label4Dropdown" onmousedown="this.focus()" Visible="false" runat="server">
							            <asp:ListItem Value=""></asp:ListItem>
						            </asp:DropDownList>
					            </td>
				            </tr>
				            <tr>
					            <td style="vertical-align:middle;"><asp:Label ID="LABEL5" Font-Size="11" Text="LABEL5"  Visible="false" runat="server"></asp:Label></td>
					            <td>
						            <asp:TextBox ID="label5Box" Visible="false" onfocus="this.select()" runat="server"></asp:TextBox>
						            <asp:DropDownList ID="label5Dropdown" onmousedown="this.focus()" Visible="false" runat="server">
							            <asp:ListItem Value=""></asp:ListItem>
						            </asp:DropDownList>
					            </td>
				            </tr>
                            <tr>
					            <td style="vertical-align:middle;"><asp:Label ID="LABEL6" Font-Size="11" Text="LABEL5"  Visible="false" runat="server"></asp:Label></td>
					            <td>
						            <asp:TextBox ID="label6Box" Visible="false" onfocus="this.select()" runat="server"></asp:TextBox>
						            <asp:DropDownList ID="label6Dropdown" onmousedown="this.focus()" Visible="false" runat="server">
							            <asp:ListItem Value=""></asp:ListItem>
						            </asp:DropDownList>
					            </td>
				            </tr>
                            <tr>
					            <td style="vertical-align:middle;"><asp:Label ID="LABEL7" Font-Size="11" Text="LABEL5"  Visible="false" runat="server"></asp:Label></td>
					            <td>
						            <asp:TextBox ID="label7Box" Visible="false" onfocus="this.select()" runat="server"></asp:TextBox>
						            <asp:DropDownList ID="label7Dropdown" onmousedown="this.focus()" Visible="false" runat="server">
							            <asp:ListItem Value=""></asp:ListItem>
						            </asp:DropDownList>
					            </td>
				            </tr>
                            <tr>
					            <td style="vertical-align:middle;"><asp:Label ID="LABEL8" Font-Size="11" Text="LABEL5"  Visible="false" runat="server"></asp:Label></td>
					            <td>
						            <asp:TextBox ID="label8Box" Visible="false" onfocus="this.select()" runat="server"></asp:TextBox>
						            <asp:DropDownList ID="label8Dropdown" onmousedown="this.focus()" Visible="false" runat="server">
							            <asp:ListItem Value=""></asp:ListItem>
						            </asp:DropDownList>
					            </td>
				            </tr>
                            <tr>
					            <td style="vertical-align:middle;"><asp:Label ID="LABEL9" Font-Size="11" Text="LABEL5"  Visible="false" runat="server"></asp:Label></td>
					            <td>
						            <asp:TextBox ID="label9Box" Visible="false" onfocus="this.select()" runat="server"></asp:TextBox>
						            <asp:DropDownList ID="label9Dropdown" onmousedown="this.focus()" Visible="false" runat="server">
							            <asp:ListItem Value=""></asp:ListItem>
						            </asp:DropDownList>
					            </td>
				            </tr>
                            <tr>
					            <td style="vertical-align:middle;"><asp:Label ID="LABEL10" Font-Size="11" Text="LABEL5"  Visible="false" runat="server"></asp:Label></td>
					            <td>
						            <asp:TextBox ID="label10Box" Visible="false" onfocus="this.select()" runat="server"></asp:TextBox>
						            <asp:DropDownList ID="label10Dropdown" onmousedown="this.focus()" Visible="false" runat="server">
							            <asp:ListItem Value=""></asp:ListItem>
						            </asp:DropDownList>
					            </td>
				            </tr>
                            <tr>
                                <td colspan="2"  style="padding-top:15px;">
                                    <asp:CheckBox ID="lastValuesEntered" runat="server"/>
                                    <asp:Label style="margin-left:10px;" Font-Size="11" runat="server">Remember last value(s) entered.</asp:Label></td>
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
			                        <%-- Msgs showing up when successful save or saveAndPrint operations happens.--%>
			            <div style="margin-top:25px;height:25px;"><asp:table id="manualEntryMsg" runat="server"></asp:table></div>

			            <asp:Panel ID="generateIndexSection" CssClass="card" style="background-color:#e6f3ff;" Visible="false" runat="server">
				            <table class = tableFull style=" width:470px;">
					            <tr >
						            <td style="text-align:left;">
							            <asp:linkButton ID="saveIndex" CssClass="btn btn-primary" runat="server" Font-Size="10" OnClick="saveIndex_Click">Save Index</asp:linkButton></td>
						            <td style="text-align:right; padding-right:5px;">
							            <asp:Button ID="saveAndPrint" CssClass="btn btn-primary" runat="server" Text="Save & Print" Font-Size="10" OnClick="printIndexes_Click" /></td>
					            </tr>               
				            </table>
			            </asp:Panel>
                    </asp:Panel>
		        </asp:Panel>

		        <div style="margin-top:40px;"></div>

		        <%--Link to Print Indexes page --%>
                <div id="linkToUnprinted" runat="server">  
		            <span  style="font-size:medium;" runat="server">View your unprinted indexes</span>   
		            <asp:HyperLink ID="HyperLink1" Font-Underline="false" runat="server" NavigateUrl="~/Indexes">
			            <span style="font-size:medium;"> here.</span>
		            </asp:HyperLink>
                </div>
                <%--Some hidden fields --%>
		        <input id="indexString" type="hidden"  runat="server"  value=""/>
	        </div>
	   
        </ContentTemplate>
    </asp:UpdatePanel>

    <%-- Questions --%>
	<div style="display:none;">
		<asp:Button ID="goToQuestion" runat="server" onclick="goToQuestion_Click"/>
	</div>

</asp:Content>
