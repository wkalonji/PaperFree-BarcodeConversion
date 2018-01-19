<%@ Page Title="Settings" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Settings.aspx.cs" Inherits="BarcodeConversion.Contact" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <script>
        function FadeOut() {
            $("table[id$='jobAccessMsg2']").delay(2000).fadeOut(1000);    
        }
        function FadeOut2() {
            $("table[id$='UserSectionMsg']").delay(2000).fadeOut(1000);
        }
        function FadeOut4() {
            $("table[id$='jobSetupMsg']").delay(2000).fadeOut(1000);
        }

        function scrollToBottom() {
            $({ myScrollTop: window.pageYOffset }).animate({ myScrollTop: 600 }, {
                duration: 600,
                easing: 'swing',
                step: function (val) {
                    window.scrollTo(0, val);
                }
            });
        }
    </script>

     <asp:UpdatePanel ID="SettingsPanel" Visible="false" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
        <div id="scroll">
        <div style="margin-top:45px; margin-bottom:30px; height:50px; border-bottom:solid 1px green;width:860px;">
            <table style="width:860px;">
                <tr>
                    <td><h2 style="display:inline;padding-top:25px; color:#595959;">Settings</h2></td>
                    <td style="text-align:right;"> 
                        <%-- COLLAPSE ALL--%>
                        <div style="display:block;margin-left:710px;padding-top:15px;">
                            <asp:linkButton ID="upArrow" Visible="false" runat="server" ForeColor="#737373" OnClick="collapseAll_Click" >
                                <i class="fa fa-angle-double-up" style="font-size:30px;" BackColor="#e6f3ff" runat="server" ></i>
                            </asp:linkButton>
                            <asp:linkButton ID="downArrow" Visible="true" runat="server" ForeColor="#737373" OnClick="collapseAll_Click" >
                                <i class=" fa fa-angle-double-down" style="font-size:30px;" BackColor="#e6f3ff" runat="server" ></i>
                            </asp:linkButton>
                        </div>
                    </td>
                </tr>
            </table>
        </div>

        <table>
            <tr>
                <td style="width:511px;">
                     <%-- JOB SETUP SECTION --%>
                    <div style="width:284px;border:solid 3px #808080;border-radius:3px;text-align:center;">
                        <asp:linkButton ID="newJobBtn" style="background-color:#e6e6e6;color:#666666;padding-top:2px;" Font-Underline="false" Visible="true" Width="278px" Height="29" runat="server" OnClick="newJobShow_Click">
                        <i class="fa fa-folder-open" aria-hidden="true" style="margin-right:9px;"></i> Job Setup Section&nbsp;</asp:linkButton>
                    </div>
                </td>
                <td style="width:324px;">
                     <%--USER & PERMISSION SECTION --%>
                    <div style="width:284px;border:solid 3px #808080;border-radius:3px;text-align:center;">
                        <asp:linkButton ID="newUserBtn" style="background-color:#e6e6e6;color:#666666;padding-top:2px;" Font-Underline="false" Visible="true" Width="278px" Height="29" runat="server" OnClick="permissionsShow_Click">
                        <i class="fa fa-user" aria-hidden="true" style="margin-right:10px;"></i> User & Permission Section</asp:linkButton>
                    </div>
                </td>
            </tr>   
            <tr>
                <td style="vertical-align:top;">
                    <%-- JOB SETUP SECTION BODY --%>
                    <div style="display:block;" class="auto-style5">
                        <asp:Panel ID="jobSection" Visible="false" runat="server" Width="408px" >
                            <asp:Panel CssClass="card" style="margin-top:25px;background-color:aliceblue;width:76%;padding: 0px 10px 0px 10px;" runat="server">
                                <table style="width:100%;" >
                                    <tr style="height:35px;">
                                        <th style="color:#737373;font-family:Arial;font-size:15px">&nbsp;Create / Edit Jobs</th>
                                        <td style="text-align:right;padding-right:5px;">
                                            <asp:linkButton ID="LinkButton1" runat="server" ForeColor="#737373" 
                                                OnClientClick="return alert('Notes:\n*   You can make a new job accessible to an operator right away.\n*    Jobs made accessible in this section will be visible to the specified operator but can not be processed until configured in the Index Config section below.\n*   When selecting a job to edit, Active jobs are in green.')">
                                                <i class="fa fa-info-circle" style="font-size:20px;" BackColor="#e6f3ff" runat="server" ></i>
                                            </asp:linkButton></td>
                                    </tr>
                                </table>
                            </asp:Panel> 
                            
                            <table  style="margin-top:25px; width: 76%; margin-right: 36px; height: 149px;"  class=auto-style3 > 
                                <tr>
                                    <td style="padding-bottom:15px;"><asp:Label Text="Choose Action: " runat="server"></asp:Label></td>
                                    <td style="padding-bottom:15px;">
                                        <asp:DropDownList ID="selectAction" AutoPostBack="True" onmousedown="this.focus()" runat="server" OnSelectedIndexChanged="actionChange">
                                            <asp:ListItem Selected="true" Value="create">Create New Job</asp:ListItem>
                                            <asp:ListItem Value="edit">Edit Existing Job</asp:ListItem>
                                            <%--<asp:ListItem Value="delete">Delete Existing Job</asp:ListItem>--%>
                                        </asp:DropDownList>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="auto-style2" style="height: 35px; width: 290px;"><asp:Label runat="server">Job Abbreviation: </asp:Label></td>
                                    <td style="height: 35px">
                                        <asp:TextBox ID="jobAbb" placeholder=" Required" runat="server"></asp:TextBox>
                                        <asp:DropDownList ID="selectJobList" onmousedown="this.focus()" runat="server">
                                            <asp:ListItem Value="Select">Select</asp:ListItem>
                                        </asp:DropDownList>
                                    </td>
                                </tr> 
                                <tr>
                                    <td class="auto-style2" style="width: 286px"><asp:Label ID="jobNameLabel" runat="server">Job Name: </asp:Label></td>
                                    <td><asp:TextBox ID="jobName" placeholder=" Required" runat="server"></asp:TextBox></td>
                                </tr>
                                <tr>
                                    <td class="auto-style2" style="padding-top:5px; width: 286px;"><asp:Label ID="jobActiveLabel" runat="server">Active: </asp:Label></td>
                                    <td>
                                        <asp:DropDownList ID="jobActiveBtn" style="margin-top:5px;" AutoPostBack="True" onmousedown="this.focus()" OnSelectedIndexChanged="onActiveSelect" runat="server">
                                            <asp:ListItem Selected="True" Value="1">True</asp:ListItem>
                                            <asp:ListItem Value="0">False</asp:ListItem>
                                        </asp:DropDownList>
                                    </td>
                                </tr>
                                 <tr>
                                    <td class="auto-style2" style="padding-top:22px; width: 286px;"><asp:Label ID="jobAssignedToLabel" runat="server">Grant Access To: </asp:Label></td>
                                    <td>
                                        <asp:DropDownList ID="jobAssignedTo" style="margin-top:22px;" onmousedown="this.focus()" runat="server">
                                            <asp:ListItem Selected="true" Value="Select">Select</asp:ListItem>
                                        </asp:DropDownList>
                                    </td>
                                </tr>
                
                            </table>
                            <table class="auto-style4">
                                <tr>
                                   <td style="text-align:right;">
                                        <asp:Button ID="deleteJobBtn" CssClass="btn btn-primary" Visible="false" runat="server" Text="Delete " 
                                            OnClientClick="return confirm('ATTENTION!\n\nDeleting this job will also delete its configuration, all indexes associated to it, and any other related records in other entities. Unless it is a must, we advise to Deactivate job instead.\n\nDo you still want to procede with Delete?');" OnClick="deleteJob_Click"/> </td>
                                </tr>
                                <tr>
                                    <td><asp:Table id="jobSetupMsg" style="float:left;" runat="server"></asp:Table> </td>
                                    <td style="height: 15px; text-align:right;">
                                        <asp:Button ID="editJobBtn" CssClass="btn btn-primary" style="padding:1px 10px 0px 10px;" Visible="false" Font-Size="10" runat="server" Text="Edit " OnClick="editJob_Click" />
                                        <asp:Button ID="createJobBtn" CssClass="btn btn-primary" style="padding:1px 10px 0px 10px;" Visible="true" Font-Size="10" runat="server" Text="Create" OnClick="createJob_Click"/>
                                    </td>
                                </tr>
                            </table>
           
                        </asp:Panel>
                    </div>
                </td>
                <td style="vertical-align:top;">
                    <%--USER & PERMISSION SECTION BODY--%>
                    <div style="display:inline-block; width: 26%;" class="auto-style5">
                        <asp:Panel ID="newUserSection" Visible="false" runat="server" Width="322px" Height="250px" style="margin-top: 0px" >
                            <asp:Panel CssClass="card" style="margin-top:25px;background-color:aliceblue;width:97%;padding: 0px 10px 0px 10px;" runat="server">
                                <table style="width:100%;">
                                    <tr style="height:35px;">
                                        <th style="color:#737373;font-family:Arial;font-size:15px">&nbsp;Add Operators & Admins</th>
                                        <td style="text-align:right;padding-right:5px;">
                                            <asp:linkButton ID="LinkButton5" runat="server" ForeColor="#737373" 
                                                OnClientClick="return alert('Notes:\n*  Anyone accessing the site for the 1st time is automatically added as an operator.\n*  An operator can also be added prior accessing the site.\nTo add, just type in the operator\'s username, set Permissions & submit.\n*    You can also change existing operator\'s permissions.')">
                                                <i class="fa fa-info-circle" style="font-size:20px;" BackColor="#e6f3ff" runat="server" ></i>
                                            </asp:linkButton></td>
                                    </tr>
                                </table>
                            </asp:Panel>
                            
                            <table  style="margin-top:25px; height:72px;"  class=auto-style3 >
                                <tr>
                                    <td class="auto-style2" style="height: 31px; margin-left: 200px;"><asp:Label runat="server">Operator: </asp:Label></td>
                                    <td style="height: 31px"><asp:TextBox ID="user" placeholder=" Required" runat="server"></asp:TextBox></td>
                                </tr> 
                                <tr>
                                    <td class="auto-style2"><asp:Label runat="server">Permissions: </asp:Label></td>
                                    <td>
                                        <asp:DropDownList ID="permissions" onmousedown="this.focus()" runat="server">
                                            <asp:ListItem Selected="true" Value="0">Operator</asp:ListItem>
                                            <asp:ListItem Value="1">Admin</asp:ListItem>
                                        </asp:DropDownList>
                                    </td>
                                </tr>
                            </table>
                            <table style="width:315px;">
                                <tr>
                                    <td><asp:Table id="UserSectionMsg" style="float:left;" runat="server"></asp:Table></td>
                                    <td><asp:Button ID="submitOp" CssClass="btn btn-primary" style="padding:1px 10px 0px 10px;float:right;" Visible="true" Font-Size="10" runat="server" Text="Submit" OnClick="setPermissions_Click"/></td>
                                </tr>
                            </table>
                        </asp:Panel>
                    </div>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                     <div id="line" visible="false" style=" height:30px; border-bottom:solid 1px green;width:860px;" runat="server"></div>
                </td>
            </tr>
            <tr>
                <td>
                    <%--JOB ACCESS SECTION --%>
                    <div style="width:284px;border:solid 3px #808080;border-radius:3px;text-align:center;margin-top:30px;">
                        <asp:linkButton ID="assignBtn" style="background-color:#e6e6e6;color:#666666;padding-top:2px;" Font-Underline="false" Visible="true" Width="278px" Height="29" runat="server" OnClick="assignShow_Click">
                        <i class="fa fa-unlock-alt" aria-hidden="true" style="margin-right:10px;"></i> Job Access Section</asp:linkButton>
                    </div>
                </td>
                <td>
                    <%--JOB INDEX CONFIG SECTION --%>
                    <div style="width:284px;border:solid 3px #808080;border-radius:3px;text-align:center;margin-top:30px;">
                        <asp:linkButton ID="jobIndexEditingBtn" style="background-color:#e6e6e6;color:#666666;padding-top:2px;" Font-Underline="false" Visible="true" Width="278px" Height="29" runat="server" OnClick="jobIndexEditingShow_Click">
                        <i class="fa fa-cogs" aria-hidden="true" style="margin-right:10px;"></i> Index Configuration Section</asp:linkButton>
                    </div>
                </td>
            </tr>
            <tr>
                <td style="vertical-align:top;">
                    <%--JOB ACCESS SECTION BODY --%>
                    <asp:Panel ID="assignPanel" Visible="false" runat="server">
                        <asp:Panel CssClass="card" style="margin-top:25px;background-color:aliceblue;width:315px;padding: 0px 10px 0px 10px;" runat="server">
                            <table style="width:100%;">
                                <tr style="height: 35px;">
                                    <th style="color:#737373;font-family:Arial;font-size:15px">&nbsp;Assign Jobs to Operators</th>
                                    <td style="text-align:right;padding-right:5px;">
                                        <asp:linkButton ID="LinkButton3" runat="server" ForeColor="#737373" 
                                            OnClientClick="return alert('Notes:\n*   Jobs made accessible in this section will be visible to the specified operator but can not be processed until configured in the Index Config section.')">
                                            <i class="fa fa-info-circle" style="font-size:20px;" BackColor="#e6f3ff" runat="server" ></i>
                                        </asp:linkButton></td>
                                </tr>
                             </table>
                        </asp:Panel>
                        
                        <table  style="height: 42px; margin-bottom:10px; margin-top:10px; width: 315px;"  class=auto-style3 >
                            <tr>
                                <td class="auto-style2" style="height:31px;width:125px;"><asp:Label runat="server">Operator: </asp:Label></td>
                                <td style="height:25px;text-align:left;">
                                    <asp:DropDownList ID="assignee" onmousedown="this.focus()" AutoPostBack="true" OnSelectedIndexChanged="getInaccessibleJobs" runat="server">
                                        <asp:ListItem Selected="true" Value="Select">Select</asp:ListItem>
                                    </asp:DropDownList>
                                </td>
                                <td><asp:Table id="jobAccessMsg1" style="float:left;" runat="server"></asp:Table></td>
                            </tr> 
                        </table>
                        <asp:Panel CssClass="card" style="margin-bottom:10px;background-color:aliceblue;width:315px;padding: 0 5px 0px 12px;" runat="server">
                            <table >
                                <tr style="height:40px;">
                                    <td style="height:10px; text-align:left;padding-right:10px;">
                                        <asp:Button ID="assignedBtn" CssClass="btn btn-primary" style="padding:2px 10px 1px 10px;"  Visible="true" Font-Size="9" runat="server" Text="Accessible" OnClick="assignedJob_Click" /></td>
                                    <td style="height:10px; text-align:center;padding-right:10px;">
                                        <asp:Button ID="inaccessibleBtn" CssClass="btn btn-primary" style="padding:2px 10px 1px 10px;" Font-Size="9" Visible="true" runat="server" Text="Inaccessible" OnClick="unassignedJob_Click" /></td>
                                    <td style="height:10px; text-align:right;">
                                        <asp:Button ID="unassignedBtn" CssClass="btn btn-primary" style="padding:2px 10px 1px 10px;" Visible="true" runat="server" Font-Size="9" Text="Active " OnClick="unassignedJob_Click"/></td>
                                </tr> 
                            </table>
                        </asp:Panel>
                        
                        <table style="width:315px;" runat="server">
                            <tr>
                                <td><asp:Label ID="jobsLabel" Font-Size="14" Text="Active Jobs" runat="server"></asp:Label></td>
                                <td><asp:Table id="jobAccessMsg2" style="float:right;" runat="server"></asp:Table></td>
                            </tr>
                        </table>
                        
                        <asp:GridView ID="jobAccessGridView" runat="server" style="margin-top:4px;" CssClass="settingsGridview" PagerStyle-CssClass="pager"
                                    PageSize="10" HeaderStyle-CssClass="header" RowStyle-CssClass="rows" AllowPaging="true" OnPageIndexChanging="pageChange_Click"
                                    OnRowDataBound="rowDataBound"> 
                            <columns>             
                                <asp:templatefield HeaderText="Select">
                                    <HeaderTemplate>
                                        &nbsp;&nbsp;<asp:checkbox ID="selectAll" AutoPostBack="true" OnCheckedChanged="selectAll_changed" runat="server"></asp:checkbox>
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        &nbsp;<asp:checkbox ID="cbSelect"  runat="server"></asp:checkbox>
                                    </ItemTemplate>
                                </asp:templatefield>

                                <asp:templatefield HeaderText ="&nbsp;N&#176;" ShowHeader="true">
                                    <ItemTemplate >
                                        <%# Container.DataItemIndex + 1 %>
                                    </ItemTemplate>
                                </asp:templatefield>
                            </columns>       
                        </asp:GridView>   
                        
                        <div style="display:block; width:315px;" >
                            <table style="margin-top:11px; width:315px;">
                                <tr>
                                    <td style="text-align:right;width: 315px;">
                                        <asp:Button ID="deleteAssignedBtn" CssClass="btn btn-danger" style="padding:1px 10px 0px 10px;" Visible="false" runat="server" Font-Size="10" Text="Deny" OnClick="deleteAssigned_Click"/>
                                        <asp:Button ID="jobAccessBtn" CssClass="btn btn-primary" style="padding:1px 10px 0px 10px;" Visible="false" runat="server" Font-Size="10" Text="Grant" OnClick="jobAccess_Click" />
                                    </td>                 
                                </tr>                  
                            </table>
                        </div>   
                    </asp:Panel>
                </td>
                <td style="vertical-align:top;">
                    <%--JOB INDEX CONFIG BODY --%>
                    <asp:Panel ID="jobIndexEditingPanel" Visible="false" runat="server">
                        <asp:Panel CssClass="card" style="margin-top:25px;background-color:aliceblue;width:315px;padding: 0px 5px 0px 10px;" runat="server">
                            <table style="width:100%;">
                                <tr style="height:35px;">
                                    <th style="color:#737373;font-family:Arial;font-size:15px">&nbsp;Create Index Data Controls for Jobs</th>
                                    <td style="text-align:right;padding-right:5px;">
                                        <asp:linkButton ID="LinkButton4" runat="server" ForeColor="#737373" 
                                            OnClientClick="return alert('Notes:\n*  Only jobs configured in this section can be processed by operators.\n*  Already configured jobs are in green.\n*  Regex is optional. But if specified, an alert message must also be specified to let operator know what a valid entry should be.\n*  Example: Regex:  .*\\S.*    Alert: Field can not be empty!\n*  Unspecified regex means textbox can be left blank when creating index.\n*  One can make use of \'regexr.com\' to test regular expressions.')">
                                            <i class="fa fa-info-circle" style="font-size:20px;" BackColor="#e6f3ff" runat="server" ></i>
                                        </asp:linkButton></td>
                                </tr>
                            </table>
                        </asp:Panel>
                        
                        <table class=table style="width:297px;margin-top:15px;">
                            <tr>
                                <td style="width:150px;white-space:nowrap;"><asp:Label ID="selectJobLabel" runat="server">Job Abbreviation:</asp:Label></td>
                                <td style="text-align:left;padding-left:20px;"> 
                                    <asp:DropDownList ID="selectJob" runat="server" AutoPostBack="true" onmousedown="this.focus()" OnSelectedIndexChanged="JobAbbSelect">
                                        <asp:ListItem Value="Select">Select</asp:ListItem>
                                    </asp:DropDownList>
                                </td>
                            </tr>
                        </table>

                        <%--<table>
                          <% for (int i = 0; i < 10; i++){ %>
                            <tr><td><%= i %></td></tr>
                          <% } %>
                        </table>--%>

                        <table id="labelsTable" visible="false" style="margin-top:15px;margin-bottom:5px;width:315px;" class=auto-style3 runat="server">
                             
                            <tr style="height:35px;">
                                <td style="width: 80px;"><asp:Label ID="lab1" Visible="true" Height="30" Text="LABEL1:" runat="server"></asp:Label></td>
                                <td style="text-align:right;"><asp:TextBox ID="label1" Visible="true" ReadOnly="true" placeholder="  Required only for Set" onfocus="this.select()" runat="server" Width="200px"></asp:TextBox></td>
                                <td style="padding-left:5px;padding-top:4px;">
                                    <asp:linkButton ID="edit1" Visible="true" runat="server" ForeColor="#737373" OnClick="processRequest" >
                                        <i class="fa fa-pencil-square-o" style="font-size:22px;" BackColor="#e6f3ff" runat="server" ></i>
                                    </asp:linkButton>
                                </td>
                            </tr>

                            <tr style="height:35px;">
                                <td style="width: 80px;"><asp:Label ID="lab2" Visible="true" Height="30" Text="LABEL2:" runat="server"></asp:Label></td>
                                <td style="text-align:right;"><asp:TextBox ID="label2" Visible="true" ReadOnly="true" placeholder="Optional" onfocus="this.select()" runat="server" Width="200px"></asp:TextBox></td>
                                <td style="padding-left:5px;padding-top:4px;">
                                    <asp:linkButton ID="edit2" Visible="true" runat="server" ForeColor="#737373" OnClick="processRequest" >
                                        <i class="fa fa-pencil-square-o" style="font-size:22px;" BackColor="#e6f3ff" runat="server" ></i>
                                    </asp:linkButton>
                                </td>
                            </tr>

                            <tr style="height:35px;">
                                <td style="width: 80px;"><asp:Label ID="lab3" Visible="true"  Height="30" Text="LABEL3:" runat="server"></asp:Label></td>
                                <td style="text-align:right;"><asp:TextBox ID="label3" Visible="true" ReadOnly="true" placeholder=" Optional" onfocus="this.select()" runat="server" Width="200px"></asp:TextBox></td>
                                <td style="padding-left:5px;padding-top:4px;">
                                     <asp:linkButton ID="edit3" Visible="true" runat="server" ForeColor="#737373" OnClick="processRequest" >
                                        <i class="fa fa-pencil-square-o" style="font-size:22px;" BackColor="#e6f3ff" runat="server" ></i>
                                    </asp:linkButton>
                                </td>
                            </tr>
                            <tr style="height:35px;">
                                <td style="width: 80px;"><asp:Label ID="lab4" Visible="true"  Height="30" Text="LABEL4:" runat="server"></asp:Label></td>
                                <td style="text-align:right;"><asp:TextBox ID="label4" Visible="true" ReadOnly="true" placeholder=" Optional" onfocus="this.select()" runat="server" Width="200px"></asp:TextBox></td>
                                <td style="padding-left:5px;padding-top:4px;">
                                     <asp:linkButton ID="edit4" Visible="true" runat="server" ForeColor="#737373" OnClick="processRequest" >
                                        <i class="fa fa-pencil-square-o" style="font-size:22px;" BackColor="#e6f3ff" runat="server" ></i>
                                    </asp:linkButton>
                                </td>
                            </tr>
                            <tr style="height:35px;">
                                <td style="width:80px;"><asp:Label ID="lab5" Visible="true"  Height="30" Text="LABEL5:" runat="server"></asp:Label></td>
                                <td style="text-align:right;"><asp:TextBox ID="label5" Visible="true" ReadOnly="true" placeholder=" Optional" onfocus="this.select()" runat="server" Width="200px"></asp:TextBox></td>
                                <td style="padding-left:5px;padding-top:4px;">
                                     <asp:linkButton ID="edit5" Visible="true" runat="server" ForeColor="#737373" OnClick="processRequest" >
                                        <i class="fa fa-pencil-square-o" style="font-size:22px;" BackColor="#e6f3ff" runat="server" ></i>
                                    </asp:linkButton>
                                </td>
                            </tr>
                            <tr style="height:35px;">
                                <td style="width:80px;"><asp:Label ID="lab6" Visible="true"  Height="30" Text="LABEL6:" runat="server"></asp:Label></td>
                                <td style="text-align:right;"><asp:TextBox ID="label6" Visible="true" ReadOnly="true" placeholder=" Optional" onfocus="this.select()" runat="server" Width="200px"></asp:TextBox></td>
                                <td style="padding-left:5px;padding-top:4px;">
                                     <asp:linkButton ID="edit6" Visible="true" runat="server" ForeColor="#737373" OnClick="processRequest" >
                                        <i class="fa fa-pencil-square-o" style="font-size:22px;" BackColor="#e6f3ff" runat="server" ></i>
                                    </asp:linkButton>
                                </td>
                            </tr>
                            <tr style="height:35px;">
                                <td style="width:80px;"><asp:Label ID="lab7" Visible="true"  Height="30" Text="LABEL7:" runat="server"></asp:Label></td>
                                <td style="text-align:right;"><asp:TextBox ID="label7" Visible="true" ReadOnly="true" placeholder=" Optional" onfocus="this.select()" runat="server" Width="200px"></asp:TextBox></td>
                                <td style="padding-left:5px;padding-top:4px;">
                                     <asp:linkButton ID="edit7" Visible="true" runat="server" ForeColor="#737373" OnClick="processRequest" >
                                        <i class="fa fa-pencil-square-o" style="font-size:22px;" BackColor="#e6f3ff" runat="server" ></i>
                                    </asp:linkButton>
                                </td>
                            </tr>
                            <tr style="height:35px;">
                                <td style="width:80px;"><asp:Label ID="lab8" Visible="true"  Height="30" Text="LABEL8:" runat="server"></asp:Label></td>
                                <td style="text-align:right;"><asp:TextBox ID="label8" Visible="true" ReadOnly="true" placeholder=" Optional" onfocus="this.select()" runat="server" Width="200px"></asp:TextBox></td>
                                <td style="padding-left:5px;padding-top:4px;">
                                     <asp:linkButton ID="edit8" Visible="true" runat="server" ForeColor="#737373" OnClick="processRequest" >
                                        <i class="fa fa-pencil-square-o" style="font-size:22px;" BackColor="#e6f3ff" runat="server" ></i>
                                    </asp:linkButton>
                                </td>
                            </tr>
                            <tr style="height:35px;">
                                <td style="width:80px;"><asp:Label ID="lab9" Visible="true"  Height="30" Text="LABEL9:" runat="server"></asp:Label></td>
                                <td style="text-align:right;"><asp:TextBox ID="label9" Visible="true" ReadOnly="true" placeholder=" Optional" onfocus="this.select()" runat="server" Width="200px"></asp:TextBox></td>
                                <td style="padding-left:5px;padding-top:4px;">
                                     <asp:linkButton ID="edit9" Visible="true" runat="server" ForeColor="#737373" OnClick="processRequest" >
                                        <i class="fa fa-pencil-square-o" style="font-size:22px;" BackColor="#e6f3ff" runat="server" ></i>
                                    </asp:linkButton>
                                </td>
                            </tr>
                            <tr style="height:35px;">
                                <td style="width:80px;"><asp:Label ID="lab10" Visible="true"  Height="30" Text="LABEL10:" runat="server"></asp:Label></td>
                                <td style="text-align:right;"><asp:TextBox ID="label10" Visible="true" ReadOnly="true" placeholder=" Optional" onfocus="this.select()" runat="server" Width="200px"></asp:TextBox></td>
                                <td style="padding-left:5px;padding-top:4px;">
                                     <asp:linkButton ID="edit10" Visible="true" runat="server" ForeColor="#737373" OnClick="processRequest" >
                                        <i class="fa fa-pencil-square-o" style="font-size:22px;" BackColor="#e6f3ff" runat="server" ></i>
                                    </asp:linkButton>
                                </td>
                            </tr>
                        </table>
                        <div style="height:10px;"><asp:table id="configMsg" runat="server"></asp:table></div>
                        <asp:Panel ID="setupTitle" CssClass="card" style="height:30px;background-color:aliceblue; padding: 0px 5px 0px 10px;width:312px;margin-top:13px;" runat="server">
                            <table style="width:100%;" runat="server">
                                <tr style="height:30px;background-color:aliceblue">
                                    <td style="font-family:Arial;width:190px;white-space:nowrap;font-weight:bold;">Control Setup </td>
                                    <td style="text-align:right;padding-right:5px;">
                                        <asp:linkButton ID="trash" style="padding-left:15px;" Visible="false" runat="server" ForeColor="#737373" OnClick="hideControlInfo_Click" >
                                            <i class="fa fa-trash" style="font-size:18px;" BackColor="#e6f3ff" runat="server" ></i>
                                        </asp:linkButton>
                                        <asp:linkButton ID="hideControlInfo" style="padding-left:15px;" Visible="true" runat="server" ForeColor="#737373" OnClick="hideControlInfo_Click" >
                                            <i class="fa fa-times" style="font-size:20px;" BackColor="#e6f3ff" runat="server" ></i>
                                        </asp:linkButton>
                                    </td>
                                </tr>
                            </table>
                        </asp:Panel>

                        <table id="labelControlsTable" visible="false" style="width: 309px;"  class=auto-style3 runat="server" >
                            <tr>
                                <td style="width:95px"><asp:Label Text="TYPE:" runat="server"></asp:Label></td>
                                <td style="text-align:left;padding:15px 0px 4px 0px;">
                                    <asp:RadioButton ID="textBoxType" Text="&nbsp;Textbox" GroupName="radioGroup" Checked="true" AutoPostBack="true" OnCheckedChanged="radioBtnChanged_Click" runat="server"/>
                                    <asp:RadioButton ID="dropdownType" Text="&nbsp;Dropdown" GroupName="radioGroup" style="margin-left:30px;" AutoPostBack="true" OnCheckedChanged="radioBtnChanged_Click" runat="server"/>
                                </td>
                            </tr>
                           
                            <tr style="vertical-align:top;">
                                <td style="width:75px"><asp:Label Text="NAME:" runat="server"></asp:Label></td>
                                <td style="text-align:right;"><asp:TextBox ID="labelTextBox" placeholder=" Label name" onfocus="this.select()" runat="server" Width="216px"></asp:TextBox></td>
                            </tr>
                             <tr style="vertical-align:top;">
                                <td style="width:75px;padding-top:4px;"><asp:Label ID="valuesLabel" Visible="false" Text="VALUES:" runat="server"></asp:Label></td>
                                <td style="text-align:right;"><asp:TextBox ID="dropdownValues" style="margin-top:7px;" Visible="false" placeholder="Optional.   e.g.: 10, 20, 30, Yes, No" onfocus="this.select()" TextMode="MultiLine" runat="server" Width="216px" Height="70px"></asp:TextBox></td>
                            </tr>
                            <tr style="vertical-align:top;">
                                <td style="width:75px;padding-top:3px;"><asp:Label ID="regex" Text="REGEX:" runat="server"></asp:Label></td>
                                <td style="text-align:right;"><asp:TextBox ID="regexTextBox" style="margin-top:2px;" placeholder="Optional." onfocus="this.select()" TextMode="MultiLine" runat="server" Width="216px" Height="70px"></asp:TextBox></td>
                            </tr>
                            <tr style="vertical-align:top;">
                                <td style="width:75px;padding-top:3px;"><asp:Label ID="alert" Text="ALERT:" runat="server"></asp:Label></td>
                                <td style="text-align:right;"><asp:TextBox ID="msgTextBox" style="margin-top:7px;" placeholder="Message of what a valid entry should be." TextMode="MultiLine" onfocus="this.select()" runat="server" Height="70px" Width="216px"></asp:TextBox></td>
                            </tr>
                            <tr>
                                <td colspan="2" style="text-align:right;padding-top:5px;">
                                    <asp:Button ID="labelContents" CssClass="btn btn-primary" style="padding:1px 10px 0px 10px;" Text="Save" Font-Size="Small" runat="server" OnClick="labelContents_Click" /></td>
                            </tr>
                        </table>
                    </asp:Panel>
                </td>
            </tr>
        </table>
        </div> 
        </ContentTemplate>
    </asp:UpdatePanel>

</asp:Content>
