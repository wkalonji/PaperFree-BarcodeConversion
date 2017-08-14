<%@ Page Title="Settings" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Settings.aspx.cs" Inherits="BarcodeConversion.Contact" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <script>
        function FadeOut3() {
            $("span[id$='success']").fadeOut(4000);
        } 
    </script>
    <asp:Panel ID="SettingsPanel" Visible="false" runat="server">
        <div style="margin-top:45px; margin-bottom:40px; height:50px; border-bottom:solid 1px green;width:899px;">
            <table style="width:899px;">
                <tr>
                    <td><h2 style="display:inline; padding-top:25px;">Settings</h2></td>
                    <td style="text-align:right;"> 
                        <%-- COLLAPSE ALL--%>
                        <div style="display:block;margin-left:710px;padding-top:15px;">
                            <asp:ImageButton ID="collapseIcon" ImageUrl="Content/collapse_all.png" Width="30" Height="20" BackColor="White" Visible="true" runat="server" OnClick="collapseAll_Click" />
                            <%--<asp:Button ID="collapseAll" Visible="false" Width="87px" runat="server" Text="Collapse All" OnClick="collapseAll_Click"/> --%>
                        </div>
                    </td>
                </tr>
            </table>
        </div>

        <table>
            <tr>
                <td style="width:615px;">
                     <%-- JOB SECTION --%>
                    <div style="width:284px; border:solid 2px black; border-radius:3px; background-color:lightgray; display:inline-block;">
                        <asp:Button ID="newJobBtn" Visible="true" Width="330px" runat="server" Text="Job Section" OnClick="newJobShow_Click" />
                    </div>
                </td>
                <td style="width:324px;">
                     <%--USER & PERMISSION SECTION --%>
                    <div style="width:284px; border: solid 2px black; border-radius:3px;">
                        <asp:Button ID="newUserBtn" Visible="true" runat="server" Text="User & Permission Section" Width="310px" OnClick="permissionsShow_Click" />
                    </div>
                </td>
            </tr>   
            <tr>
                <td style="width: 615px; vertical-align:top;">
                    <%-- JOB SECTION BODY --%>
                    <div style="display:block; width: 26%;" class="auto-style5">
                        <asp:Panel ID="jobSection" Visible="false" runat="server" Width="408px" > 
                            <table style="margin-top:25px;background-color:aliceblue;width:76%;">
                                <tr>
                                    <td><asp:Label runat="server">
                                        <h4 >&nbsp;Create/Edit Jobs</h4></asp:Label></td>
                                    <td style="text-align:right;padding-right:5px;"><asp:Button Text="?" Height="23" 
                                        OnClientClick="return alert('Notes:\n*   You can make a new job accessible to an operator right away. If operator entered does not exist, a new job is created anyway.\n*    Operators accessing jobs here will see them, but cannot generate Indexes until those jobs are configured in the Index Config section below.\n*   While editing a job, red colored dropdown jobs are Active jobs.')" runat="server"></asp:Button></td>
                                </tr>
                            </table>
                            <table  style="margin-top:25px; width: 76%; margin-right: 36px; height: 149px;"  class=auto-style3 > 
                                <tr>
                                    <td style="padding-bottom:15px;"><asp:Label Text="Choose your Action: " runat="server"></asp:Label></td>
                                    <td style="padding-bottom:15px;">
                                        <asp:DropDownList ID="selectAction" AutoPostBack="True" runat="server" OnSelectedIndexChanged="actionChange">
                                            <asp:ListItem Selected="true" Value="create">Create New Job</asp:ListItem>
                                            <asp:ListItem Value="edit">Edit Existing Job</asp:ListItem>
                                            <%--<asp:ListItem Value="delete">Delete Existing Job</asp:ListItem>--%>
                                        </asp:DropDownList>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="auto-style2" style="height: 35px; width: 286px;"><asp:Label runat="server">Job Abbreviation: </asp:Label></td>
                                    <td style="height: 35px">
                                        <asp:TextBox ID="jobAbb" placeholder=" Required" runat="server"></asp:TextBox>
                                        <asp:DropDownList ID="selectJobList" runat="server">
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
                                        <asp:DropDownList ID="jobActiveBtn" style="margin-top:5px;" AutoPostBack="True" OnSelectedIndexChanged="onActiveSelect" runat="server">
                                            <asp:ListItem Selected="True" Value="1">True</asp:ListItem>
                                            <asp:ListItem Value="0">False</asp:ListItem>
                                        </asp:DropDownList>
                                    </td>
                                </tr>
                                 <tr>
                                    <td class="auto-style2" style="padding-top:25px; width: 286px;"><asp:Label ID="jobAssignedToLabel" runat="server">Accessible To: </asp:Label></td>
                                    <td><asp:TextBox ID="jobAssignedTo" style="margin-top:15px;" placeholder=" Optional" runat="server"></asp:TextBox></td>
                                </tr>
                
                            </table>
                            <table style="margin-top:20px; " class="auto-style4">
                                <tr style="height:15px;">
                                   <td style="height: 15px; text-align:right;">
                                        <asp:Button ID="deleteJobBtn" Visible="false" runat="server" Text="Delete " 
                                            OnClientClick="return confirm('ATTENTION!\n\nDeleting this job will also delete its configuration, all indexes associated to it, and any other related records in other entities. Unless it is a must, we advise to Deactivate job instead.\n\nDo you still want to procede with Delete?');" OnClick="deleteJob_Click"/> </td>
                                </tr>
                                <tr>
                                     <td style="height: 15px; text-align:right;">
                                        <asp:Button ID="editJobBtn" Visible="false" runat="server" Text="Edit " OnClick="editJob_Click" /></td>
                                </tr>
                                <tr>
                                     <td style="height: 10px;text-align:right;">
                                        <asp:Button ID="createJobBtn"  Visible="true" runat="server" Text="Create" OnClick="createJob_Click"/></td>
                                </tr>
                                <tr>
                                     <td style="height: 10px;text-align:left;">
                                        <asp:Label ID="success"  Visible="false" runat="server" Text="" OnClick="createJob_Click"/></td>
                                </tr>
                            </table>
           
                        </asp:Panel>
                    </div>
                </td>
                <td style="width: 324px; vertical-align:top;">
                    <%--USER & PERMISSION SECTION BODY--%>
                    <div style="display:inline-block; width: 26%;" class="auto-style5">
                        <asp:Panel ID="newUserSection" Visible="false" runat="server" Width="322px" Height="250px" style="margin-top: 0px" >
                            <table style="margin-top:25px;background-color:aliceblue;width:97%;">
                                <tr>
                                    <td><asp:Label runat="server">
                                        <h4 >&nbsp;Add Operators & Admins</h4></asp:Label></td>
                                    <td style="text-align:right;padding-right:5px;"><asp:Button Text="?" Height="23" 
                                        OnClientClick="return alert('Notes:\n*  Anyone accessing the site for the 1st time is automatically added as operator.\nAn operator can still be added prior to him/her accessing the site.\nTo add, just type in operator\'s username, set Permissions & submit.\n*    You can also change existing operator\'s permissions.')" runat="server"></asp:Button></td>
                                </tr>
                            </table>
                            <table  style="margin-top:25px; height: 72px;"  class=auto-style3 >
                                <tr>
                                    <td class="auto-style2" style="height: 31px; margin-left: 200px;"><asp:Label runat="server">Operator: </asp:Label></td>
                                    <td style="height: 31px"><asp:TextBox ID="user" placeholder=" Required" runat="server"></asp:TextBox>
                                    </td>
                                </tr> 
                                <tr>
                                    <td class="auto-style2"><asp:Label runat="server">Permissions: </asp:Label></td>
                                    <td>
                                        <asp:DropDownList ID="permissions" runat="server">
                                            <asp:ListItem Selected="true" Value="0">Operator</asp:ListItem>
                                            <asp:ListItem Value="1">Admin</asp:ListItem>
                                        </asp:DropDownList>
                                    </td>
                                </tr>
                            </table>
                            <div style="text-align:right; margin-top:15px;" class="auto-style4" id="abc">
                                <asp:Button ID="createBtn2" Visible="true" runat="server" Text="Submit" OnClick="setPermissions_Click" />
                            </div>
                        </asp:Panel>
                    </div>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                     <div id="line" visible="false" style=" height:50px; border-bottom:solid 1px green;width:899px;" runat="server"></div>
                </td>
            </tr>
            <tr>
                <td style="width: 615px">
                    <%--JOB ACCESS SECTION --%>
                    <div style="width:284px; border: solid 2px black; border-radius:3px; margin-top:50px;">
                        <asp:Button ID="assignBtn" Visible="true" runat="server" Text="Job Access Section" Width="310px" OnClick="assignShow_Click" />
                    </div>
                </td>
                <td style="width: 324px">
                    <%--JOB INDEX CONFIG SECTION --%>
                    <div style="width:284px; border: solid 2px black; border-radius:3px; margin-top:50px;">
                        <asp:Button ID="jobIndexEditingBtn" Visible="true" runat="server" Text="Job Index Configuration Section" Width="310px" OnClick="jobIndexEditingShow_Click" />
                    </div>
                </td>
            </tr>
            <tr>
                <td style="width: 615px; vertical-align:top;">
                    <%--JOB ACCESS SECTION BODY --%>
                    <asp:Panel ID="assignPanel" Visible="false" runat="server">
                        <table style="margin-top:25px;background-color:aliceblue;width:51.5%;">
                            <tr>
                                <td><asp:Label runat="server">
                                    <h4 >&nbsp;Assign Jobs to Operators</h4></asp:Label></td>
                                <td style="text-align:right;padding-right:5px;"><asp:Button Text="?" Height="23" 
                                    OnClientClick="return alert('Notes:\n*  Operators accessing jobs here will see them, but cannot generate indexes until those jobs are configured in the Index Config section below.')" runat="server"></asp:Button></td>
                            </tr>
                         </table>
                        <table  style="margin-top:10px; height: 72px; width: 51.5%;"  class=auto-style3 >
                            <tr>
                                <td class="auto-style2" style="height: 31px"><asp:Label runat="server">Operator: </asp:Label></td>
                                <td style="height: 25px"><asp:TextBox ID="assignee" placeholder=" Required" onfocus="this.select()" runat="server"></asp:TextBox></td>
                            </tr> 
                        </table>
                        <table style="margin-top:20px; margin-bottom:20px; width: 316px;">
                            <tr style="background-color:aliceblue; height:40px;">
                                <td style="height: 10px; text-align:left;"><asp:Button ID="assignedBtn"  Visible="true" runat="server" Text="Accessible" OnClick="assignedJob_Click" /></td>
                                <td style="height: 10px; text-align:center;padding-right:8px;"><asp:Button ID="inaccessibleBtn"  Visible="true" runat="server" Text="Inaccessible" OnClick="unassignedJob_Click" /></td>
                                <td style="height: 10px; text-align:right;"><asp:Button ID="unassignedBtn" Visible="true" runat="server" Text="Active " OnClick="unassignedJob_Click"/></td>
                            </tr> 
                        </table>
                        <div> 
                            <asp:Label ID="jobsLabel" Text="Active Jobs" runat="server"></asp:Label>
                            <asp:GridView ID="jobAccessGridView" Width="318px" runat="server" style="margin-top:8px" CssClass="mydatagrid" PagerStyle-CssClass="pager"
                                        PageSize="10" HeaderStyle-CssClass="header" RowStyle-CssClass="rows" AllowPaging="true" OnPageIndexChanging="pageChange_Click" > 
                                <columns>             
                                    <asp:templatefield HeaderText="Select">
                                        <HeaderTemplate>
                                            &nbsp;<asp:checkbox ID="selectAll" AutoPostBack="true" OnCheckedChanged="selectAll_changed" runat="server"></asp:checkbox>
                                        </HeaderTemplate>
                                        <ItemTemplate>
                                            <asp:checkbox ID="cbSelect"  runat="server"></asp:checkbox>
                                        </ItemTemplate>
                                    </asp:templatefield>

                                    <asp:templatefield HeaderText ="&nbsp;N&#176;" ShowHeader="true">
                                        <ItemTemplate >
                                            <%# Container.DataItemIndex + 1 %>
                                        </ItemTemplate>
                                    </asp:templatefield>
                                </columns>       
                            </asp:GridView>     
                        </div>
                        <div style="display:block; width: 535px;" >
                            <table class = table style="margin-top:25px; width: 320px;">
                                <tr>
                                    <td style="text-align:left"><asp:Button ID="deleteAssignedBtn" Visible="true" runat="server" Text="Deny" OnClick="deleteAssigned_Click"/></td>
                                    <td style="text-align:right;width: 100%;"><asp:Button ID="jobAccessBtn" Visible="true" runat="server" Text="Grant" OnClick="jobAccess_Click" Width="59px"/></td>                 
                                </tr>                  
                            </table>
                        </div>   
                    </asp:Panel>
                </td>
                <td style="width: 324px; vertical-align:top;">
                    <%--JOB INDEX CONFIG BODY --%>
                    <asp:Panel ID="jobIndexEditingPanel" Visible="false" runat="server">
                        <table style="margin-top:25px;background-color:aliceblue;width:99%;">
                            <tr>
                                <td><asp:Label runat="server">
                                    <h4 >&nbsp;Create Form Controls for Jobs</h4></asp:Label></td>
                                <td style="text-align:right;padding-right:5px;"><asp:Button Text="?" Height="23" 
                                    OnClientClick="return alert('Notes:\n*  Only jobs configured here can be processed by operators.\n*  Red colored dropdown jobs are Configured jobs.\n*  Type in your regex, then space, then any message you would like operators to see if their entry is not valid (e.g: \\w{4} Four characters word only, no special characters. )')" runat="server"></asp:Button></td>
                            </tr>
                        </table>
                        <table class = table style="width:320px;">
                            <tr>
                                <td style="width: 121px"><asp:Label ID="selectJobLabel" runat="server">Job Abbreviation:</asp:Label></td>
                                <td style="text-align:left;"> 
                                    <asp:DropDownList ID="selectJob" runat="server">
                                        <asp:ListItem Value="Select">Select</asp:ListItem>
                                    </asp:DropDownList>
                                </td>
                            </tr>
                        </table>


                        <table  style="margin-top:20px; width:99%;"  class=auto-style3>
                            <tr>
                                <td style="width: 80px;padding-top:8px;"><asp:Label ID="lab1" Visible="true" Height="25" Text="LABEL1:" runat="server"></asp:Label></td>
                                <td style="text-align:right;"><asp:TextBox ID="label1" Visible="true" ReadOnly="true" placeholder=" Required only for Set" onfocus="this.select()" runat="server" Width="200px"></asp:TextBox></td>
                                <td style="padding-left:5px;padding-top:4px;"><asp:ImageButton ID="edit1" Visible="true" ImageUrl="Content/edit.png" runat="server" Height="16px" Width="16px" OnClick="processRequest" /></td>
                            </tr>
                            <tr visible="false" runat="server">
                                <td style="width: 80px"><asp:Label Text="REGEX1:" runat="server"></asp:Label></td>
                                <td style="text-align:right;"><asp:TextBox ID="regex1" placeholder=" Optional" onfocus="this.select()" runat="server"></asp:TextBox></td>
                            </tr>



                            <tr>
                                <td style="width: 80px;padding-top:8px;"><asp:Label ID="lab2" Visible="true" Height="25" Text="LABEL2:" runat="server"></asp:Label></td>
                                <td style="text-align:right;"><asp:TextBox ID="label2" Visible="true" ReadOnly="true" placeholder=" Optional" onfocus="this.select()" runat="server" Width="200px"></asp:TextBox></td>
                                <td style="padding-left:5px;padding-top:4px;"><asp:ImageButton ID="edit2" Visible="true" ImageUrl="Content/edit.png" runat="server" Height="16px" Width="16px" OnClick="processRequest" /></td>
                            </tr>
                            <tr visible="false" runat="server">
                                <td style="width: 80px"><asp:Label Text="REGEX2:" runat="server"></asp:Label></td>
                                <td style="text-align:right;"><asp:TextBox ID="regex2" placeholder=" Optional" onfocus="this.select()" runat="server"></asp:TextBox></td>
                            </tr>



                            <tr>
                                <td style="width: 80px;padding-top:8px;"><asp:Label ID="lab3" Visible="true"  Height="25" Text="LABEL3:" runat="server"></asp:Label></td>
                                <td style="text-align:right;"><asp:TextBox ID="label3" Visible="true" ReadOnly="true" placeholder=" Optional" onfocus="this.select()" runat="server" Width="200px"></asp:TextBox></td>
                                <td style="padding-left:5px;padding-top:4px;"><asp:ImageButton ID="edit3" Visible="true" ImageUrl="Content/edit.png" runat="server" Height="16px" Width="16px" OnClick="processRequest" /></td>
                            </tr>
                            <tr visible="false" runat="server">
                                <td style="width: 80px"><asp:Label Text="REGEX3:" runat="server"></asp:Label></td>
                                <td style="text-align:right;"><asp:TextBox ID="regex3" placeholder=" Optional" onfocus="this.select()" runat="server"></asp:TextBox></td>
                            </tr>



                            <tr >
                                <td style="width: 80px;padding-top:8px;"><asp:Label ID="lab4" Visible="true"  Height="25" Text="LABEL4:" runat="server"></asp:Label></td>
                                <td style="text-align:right;"><asp:TextBox ID="label4" Visible="true" ReadOnly="true" placeholder=" Optional" onfocus="this.select()" runat="server" Width="200px"></asp:TextBox></td>
                                <td style="padding-left:5px;padding-top:4px;"><asp:ImageButton ID="edit4" Visible="true" ImageUrl="Content/edit.png" runat="server" Height="16px" Width="16px" OnClick="processRequest" /></td>
                            </tr>
                            <tr visible="false" runat="server">
                                <td style="width: 80px"><asp:Label Text="REGEX4:" runat="server"></asp:Label></td>
                                <td style="text-align:right;"><asp:TextBox ID="regex4" placeholder=" Optional" onfocus="this.select()" runat="server"></asp:TextBox></td>
                            </tr>


                            <tr>
                                <td style="width:80px; padding-top:8px;"><asp:Label ID="lab5" Visible="true"  Height="25" Text="LABEL5:" runat="server"></asp:Label></td>
                                <td style="text-align:right;"><asp:TextBox ID="label5" Visible="true" ReadOnly="true" placeholder=" Optional" onfocus="this.select()" runat="server" Width="200px"></asp:TextBox></td>
                                <td style="padding-left:5px;padding-top:4px;"><asp:ImageButton ID="edit5" Visible="true" ImageUrl="Content/edit.png" runat="server" Height="16px" Width="16px" OnClick="processRequest" /></td>
                            </tr>
                            <tr visible="false" runat="server">
                                <td style="width: 80px"><asp:Label Text="REGEX5:" runat="server"></asp:Label></td>
                                <td style="text-align:right;"><asp:TextBox ID="regex5" placeholder=" Optional" onfocus="this.select()" runat="server"></asp:TextBox></td>
                            </tr>
                        </table>

                        <table id="labelControlsTable" visible="false" style="width: 98%;"  class=auto-style3 runat="server" >
                            <tr style="height:33px;">
                                <td style="width: 80px"><asp:Label Text="LABEL:" runat="server"></asp:Label></td>
                                <td style="text-align:right;"><asp:TextBox ID="labelTextBox" placeholder=" Required only for Set" onfocus="this.select()" runat="server" Width="221px"></asp:TextBox></td>
                            </tr>
                            <tr style="vertical-align:top;">
                                <td style="width: 80px"><asp:Label Text="REGEX:" runat="server"></asp:Label></td>
                                <td style="text-align:right;"><asp:TextBox ID="regexTextBox" placeholder=" Optional" TextMode="MultiLine" runat="server" Width="221px" Height="60px"></asp:TextBox></td>
                            </tr>
                            <tr style="vertical-align:top;">
                                <td style="width: 80px;"><asp:Label Text="MESSAGE:" runat="server"></asp:Label></td>
                                <td style="text-align:right;"><asp:TextBox ID="msgTextBox" placeholder=" Popup message if entry not valid. &nbsp;Required only if Regex is set" TextMode="MultiLine" onfocus="this.select()" runat="server" Height="60px" Width="221px"></asp:TextBox></td>
                            </tr>
                            <tr>
                                <td colspan="2" style="text-align:right;"><asp:ImageButton ID="labelContents" ImageUrl="Content/submit.png" Font-Size="Small" runat="server" Height="22px" Width="21px" OnClick="labelContents_Click" /></td>
                            </tr>
                        </table>


                        <table style="margin-top:20px; margin-bottom:20px; width: 316px;">
                            <tr style="background-color:aliceblue; height:40px;">
                                <td style="height: 10px; text-align:left;">
                                    <asp:Button ID="unsetRules" Visible="true" runat="server" Text="Unset"
                                        OnClientClick="return confirm('ATTENTION!\n\nRemoving or changing configuration will affect the Details section of still unprinted indexes related to this job. We suggest that you make sure that there are no more unprinted indexes related to this job accross all operators prior unsetting config.\nDo you still want to procede with reconfiguration?');"
                                        OnClick="unsetRules_Click" /></td>
                                <td style="height: 10px; text-align:right;"><asp:Button ID="setRules" style="margin-left:25px;" Visible="true" runat="server" Text="Set " OnClick="setRules_Click" /></td>
                            </tr> 
                        </table>     
                    </asp:Panel>
                </td>
            </tr>
        </table>
    </asp:Panel>
        

</asp:Content>
