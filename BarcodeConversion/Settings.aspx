<%@ Page Title="Settings" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Settings.aspx.cs" Inherits="BarcodeConversion.Contact" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <script>
        function FadeOut3() {
            $("span[id$='success']").fadeOut(4000);
        } 
    </script>
    <asp:Panel ID="SettingsPanel" Visible="false" runat="server">
        <div style="margin-top:45px; margin-bottom:30px; height:50px; border-bottom:solid 1px green;width:860px;">
            <table style="width:860px;">
                <tr>
                    <td><h2 style="display:inline; padding-top:25px; color:#595959;">Settings</h2></td>
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
                <td style="width:480px;">
                     <%-- JOB SECTION --%>
                    <div style="width:284px; border:solid 2px #808080; border-radius:3px; background-color:lightgray; display:inline-block;">
                        <asp:Button ID="newJobBtn" Visible="true" Width="330px" runat="server" Text="Job Section" OnClick="newJobShow_Click" />
                    </div>
                </td>
                <td style="width:324px;">
                     <%--USER & PERMISSION SECTION --%>
                    <div style="width:284px; border: solid 2px #808080; border-radius:3px;">
                        <asp:Button ID="newUserBtn" Visible="true" runat="server" Text="User & Permission Section" Width="310px" OnClick="permissionsShow_Click" />
                    </div>
                </td>
            </tr>   
            <tr>
                <td style="width: 315px; vertical-align:top;">
                    <%-- JOB SECTION BODY --%>
                    <div style="display:block; class="auto-style5">
                        <asp:Panel ID="jobSection" Visible="false" runat="server" Width="408px" > 
                            <table style="margin-top:25px;background-color:aliceblue;width:76%;">
                                <tr style="height:35px;">
                                    <th style="color:#737373;font-family:Arial;font-size:15px">&nbsp;Create / Edit Jobs</th>
                                    <td style="text-align:right;padding-right:5px;">
                                        <asp:linkButton ID="LinkButton1" runat="server" ForeColor="#737373" 
                                            OnClientClick="return alert('Notes:\n*   You can make a new job accessible to an operator right away.\n*    Jobs made accessible in this section will be visible to the specified operator but can not be processed until configured in the Index Config section below.\n*   When selecting a job to edit, Active jobs are in red.')">
                                            <i class="fa fa-info-circle" style="font-size:20px;" BackColor="#e6f3ff" runat="server" ></i>
                                        </asp:linkButton></td>
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
                                    <td class="auto-style2" style="padding-top:22px; width: 286px;"><asp:Label ID="jobAssignedToLabel" runat="server">Grant Access To: </asp:Label></td>
                                    <td>
                                        <asp:DropDownList ID="jobAssignedTo" style="margin-top:22px;" runat="server">
                                            <asp:ListItem Selected="true" Value="Select">Select</asp:ListItem>
                                        </asp:DropDownList>
                                    </td>
                                </tr>
                
                            </table>
                            <table class="auto-style4">
                                <tr>
                                   <td style="text-align:right;">
                                        <asp:Button ID="deleteJobBtn" Visible="false" runat="server" Text="Delete " 
                                            OnClientClick="return confirm('ATTENTION!\n\nDeleting this job will also delete its configuration, all indexes associated to it, and any other related records in other entities. Unless it is a must, we advise to Deactivate job instead.\n\nDo you still want to procede with Delete?');" OnClick="deleteJob_Click"/> </td>
                                </tr>
                                <tr>
                                     <td style="height: 15px; text-align:right;">
                                        <asp:Button ID="editJobBtn" Visible="false" Font-Size="10" runat="server" Text="Edit " OnClick="editJob_Click" /></td>
                                </tr>
                                <tr>
                                     <td style="height: 10px;text-align:right;">
                                        <asp:Button ID="createJobBtn"  Visible="true" Font-Size="10" runat="server" Text="Create" OnClick="createJob_Click"/></td>
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
                                <tr style="height:35px;">
                                    <th style="color:#737373;font-family:Arial;font-size:15px">&nbsp;Add Operators & Admins</th>
                                    <td style="text-align:right;padding-right:5px;">
                                        <asp:linkButton ID="LinkButton2" runat="server" ForeColor="#737373" 
                                            OnClientClick="return alert('Notes:\n*  Anyone accessing the site for the 1st time is automatically added as an operator.\n*  An operator can also be added prior accessing the site.\nTo add, just type in the operator\'s username, set Permissions & submit.\n*    You can also change existing operator\'s permissions.')">
                                            <i class="fa fa-info-circle" style="font-size:20px;" BackColor="#e6f3ff" runat="server" ></i>
                                        </asp:linkButton></td>
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
                                <asp:Button ID="createBtn2" Visible="true" Font-Size="10" runat="server" Text="Submit" OnClick="setPermissions_Click" />
                            </div>
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
                <td style="width: 480px">
                    <%--JOB ACCESS SECTION --%>
                    <div style="width:284px; border: solid 2px #808080; border-radius:3px; margin-top:30px;">
                        <asp:Button ID="assignBtn" Visible="true" runat="server" Text="Job Access Section" Width="310px" OnClick="assignShow_Click" />
                    </div>
                </td>
                <td style="width: 324px">
                    <%--JOB INDEX CONFIG SECTION --%>
                    <div style="width:284px; border: solid 2px #808080; border-radius:3px; margin-top:30px;">
                        <asp:Button ID="jobIndexEditingBtn" Visible="true" runat="server" Text="Job Index Configuration Section" Width="310px" OnClick="jobIndexEditingShow_Click" />
                    </div>
                </td>
            </tr>
            <tr>
                <td style="width: 315px; vertical-align:top;">
                    <%--JOB ACCESS SECTION BODY --%>
                    <asp:Panel ID="assignPanel" Visible="false" runat="server">
                        <table style="margin-top:25px;background-color:aliceblue;width:315px;">
                            <tr style="height: 35px;">
                                <th style="color:#737373;font-family:Arial;font-size:15px">&nbsp;Assign Jobs to Operators</th>
                                <td style="text-align:right;padding-right:5px;">
                                    <asp:linkButton ID="LinkButton3" runat="server" ForeColor="#737373" 
                                        OnClientClick="return alert('Notes:\n*   Jobs made accessible in this section will be visible to the specified operator but can not be processed until configured in the Index Config section.')">
                                        <i class="fa fa-info-circle" style="font-size:20px;" BackColor="#e6f3ff" runat="server" ></i>
                                    </asp:linkButton></td>
                            </tr>
                         </table>
                        <table  style="height: 42px; margin-bottom:10px; margin-top:10px; width: 315px;"  class=auto-style3 >
                            <tr>
                                <td class="auto-style2" style="height:31px;width:125px;"><asp:Label runat="server">Operator: </asp:Label></td>
                                <td style="height:25px;text-align:left;">
                                    <asp:DropDownList ID="assignee" runat="server">
                                        <asp:ListItem Selected="true" Value="Select">Select</asp:ListItem>
                                    </asp:DropDownList>
                                </td>
                            </tr> 
                        </table>
                        <table style="margin-bottom:10px; width: 315px;">
                            <tr style="background-color:aliceblue; height:40px;">
                                <td style="height:10px; text-align:left; padding-left:5px;"><asp:Button ID="assignedBtn"  Visible="true" Font-Size="10" runat="server" Text="Accessible" OnClick="assignedJob_Click" /></td>
                                <td style="height:10px; text-align:center;padding-right:8px;"><asp:Button ID="inaccessibleBtn" Font-Size="10" Visible="true" runat="server" Text="Inaccessible" OnClick="unassignedJob_Click" /></td>
                                <td style="height:10px; text-align:right;padding-right:5px;"><asp:Button ID="unassignedBtn" Visible="true" runat="server" Font-Size="10" Text="Active " OnClick="unassignedJob_Click"/></td>
                            </tr> 
                        </table>
                        
                            <asp:Label ID="jobsLabel" Text="Active Jobs" runat="server"></asp:Label>
                            <asp:GridView ID="jobAccessGridView" runat="server" style="margin-top:8px;" CssClass="settingsGridview" PagerStyle-CssClass="pager"
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
                        
                        <div style="display:block; width: 480px;" >
                            <table style="margin-top:8px; width:316px;">
                                <tr>
                                    <td style="text-align:right;width: 320px;">
                                        <asp:Button ID="deleteAssignedBtn" Visible="true" runat="server" Font-Size="10" Text="Deny" OnClick="deleteAssigned_Click"/>
                                        <asp:Button ID="jobAccessBtn" Visible="true" runat="server" Font-Size="10" Text="Grant" OnClick="jobAccess_Click" Width="59px"/>
                                    </td>                 
                                </tr>                  
                            </table>
                        </div>   
                    </asp:Panel>
                </td>
                <td style="width: 324px; vertical-align:top;">
                    <%--JOB INDEX CONFIG BODY --%>
                    <asp:Panel ID="jobIndexEditingPanel" Visible="false" runat="server">
                        <table style="margin-top:25px;background-color:aliceblue;width:315px;">
                            <tr style="height:35px;">
                                <th style="color:#737373;font-family:Arial;font-size:15px">&nbsp;Create Index Data Controls for Jobs</th>
                                <td style="text-align:right;padding-right:5px;">
                                    <asp:linkButton ID="LinkButton4" runat="server" ForeColor="#737373" 
                                        OnClientClick="return alert('Notes:\n*  Only jobs configured in this section can be processed by operators.\n*  Already configured jobs are in red.\n*  Regex is optional. But if specified, an alert message must also be specified to let operator know what a valid entry should be.\n*  Example: Regex:  .*\\S.*    Alert: Field can not be empty!\n*  Unspecified regex means textbox can be left blank when creating index.\n*  You can make use of \'regexr.com\' to test your regular expressions.')">
                                        <i class="fa fa-info-circle" style="font-size:20px;" BackColor="#e6f3ff" runat="server" ></i>
                                    </asp:linkButton></td>
                            </tr>
                        </table>
                        <table class = table style="width:297px;">
                            <tr>
                                <td style="width:150px;white-space:nowrap;"><asp:Label ID="selectJobLabel" Text ="Job Abbreviation: " runat="server"></asp:Label></td>
                                <td style="text-align:left;padding-left:20px;"> 
                                    <asp:DropDownList ID="selectJob" runat="server" AutoPostBack="true" OnSelectedIndexChanged="JobAbbSelect">
                                        <asp:ListItem Value="Select">Select</asp:ListItem>
                                    </asp:DropDownList>
                                </td>
                            </tr>
                        </table>

                        <table id="labelsTable" visible="false" style="margin-top:20px; width:315px;"  class=auto-style3 runat="server">
                            <tr>
                                <td style="width: 80px;padding-top:8px;"><asp:Label ID="lab1" Visible="true" Height="25" Text="LABEL1:" runat="server"></asp:Label></td>
                                <td style="text-align:right;"><asp:TextBox ID="label1" Visible="true" ReadOnly="true" placeholder=" Required only for Set" onfocus="this.select()" runat="server" Width="200px"></asp:TextBox></td>
                                <td style="padding-left:5px;padding-top:4px;">
                                    <asp:linkButton ID="edit1" Visible="true" runat="server" ForeColor="#737373" OnClick="processRequest" >
                                        <i class="fa fa-pencil-square-o" style="font-size:22px;" BackColor="#e6f3ff" runat="server" ></i>
                                    </asp:linkButton>
                                </td>
                            </tr>
                            <tr>
                                <td style="width: 80px;padding-top:8px;"><asp:Label ID="lab2" Visible="true" Height="25" Text="LABEL2:" runat="server"></asp:Label></td>
                                <td style="text-align:right;"><asp:TextBox ID="label2" Visible="true" ReadOnly="true" placeholder=" Optional" onfocus="this.select()" runat="server" Width="200px"></asp:TextBox></td>
                                <td style="padding-left:5px;padding-top:4px;">
                                     <asp:linkButton ID="edit2" Visible="true" runat="server" ForeColor="#737373" OnClick="processRequest" >
                                        <i class="fa fa-pencil-square-o" style="font-size:22px;" BackColor="#e6f3ff" runat="server" ></i>
                                    </asp:linkButton>
                                </td>
                            </tr>
                            <tr>
                                <td style="width: 80px;padding-top:8px;"><asp:Label ID="lab3" Visible="true"  Height="25" Text="LABEL3:" runat="server"></asp:Label></td>
                                <td style="text-align:right;"><asp:TextBox ID="label3" Visible="true" ReadOnly="true" placeholder=" Optional" onfocus="this.select()" runat="server" Width="200px"></asp:TextBox></td>
                                <td style="padding-left:5px;padding-top:4px;">
                                     <asp:linkButton ID="edit3" Visible="true" runat="server" ForeColor="#737373" OnClick="processRequest" >
                                        <i class="fa fa-pencil-square-o" style="font-size:22px;" BackColor="#e6f3ff" runat="server" ></i>
                                    </asp:linkButton>
                                </td>
                            </tr>
                            <tr >
                                <td style="width: 80px;padding-top:8px;"><asp:Label ID="lab4" Visible="true"  Height="25" Text="LABEL4:" runat="server"></asp:Label></td>
                                <td style="text-align:right;"><asp:TextBox ID="label4" Visible="true" ReadOnly="true" placeholder=" Optional" onfocus="this.select()" runat="server" Width="200px"></asp:TextBox></td>
                                <td style="padding-left:5px;padding-top:4px;">
                                     <asp:linkButton ID="edit4" Visible="true" runat="server" ForeColor="#737373" OnClick="processRequest" >
                                        <i class="fa fa-pencil-square-o" style="font-size:22px;" BackColor="#e6f3ff" runat="server" ></i>
                                    </asp:linkButton>
                                </td>
                            </tr>
                            <tr>
                                <td style="width:80px; padding-top:8px;"><asp:Label ID="lab5" Visible="true"  Height="25" Text="LABEL5:" runat="server"></asp:Label></td>
                                <td style="text-align:right;"><asp:TextBox ID="label5" Visible="true" ReadOnly="true" placeholder=" Optional" onfocus="this.select()" runat="server" Width="200px"></asp:TextBox></td>
                                <td style="padding-left:5px;padding-top:4px;">
                                     <asp:linkButton ID="edit5" Visible="true" runat="server" ForeColor="#737373" OnClick="processRequest" >
                                        <i class="fa fa-pencil-square-o" style="font-size:22px;" BackColor="#e6f3ff" runat="server" ></i>
                                    </asp:linkButton>
                                </td>
                            </tr>
                        </table>

                        <table id="labelControlsTable" visible="false" style="width: 309px;"  class=auto-style3 runat="server" >
                            <tr style="height:30px;background-color:aliceblue">
                                <th style="font-family:Arial;width:190px;white-space:nowrap;padding-left:5px;">Control Setup </th>
                                <th style="text-align:right;padding-right:5px;">
                                    <asp:linkButton ID="trash" style="padding-left:15px;" Visible="false" runat="server" ForeColor="#737373" OnClick="hideControlInfo_Click" >
                                        <i class="fa fa-trash" style="font-size:18px;" BackColor="#e6f3ff" runat="server" ></i>
                                    </asp:linkButton>
                                    <asp:linkButton ID="hideControlInfo" style="padding-left:15px;" Visible="false" runat="server" ForeColor="#737373" OnClick="hideControlInfo_Click" >
                                        <i class="fa fa-times" style="font-size:20px;" BackColor="#e6f3ff" runat="server" ></i>
                                    </asp:linkButton>
                                </th>
                            </tr>
                            
                            <tr>
                                <td style="width: 75px"><asp:Label Text="TYPE:" runat="server"></asp:Label></td>
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
                                <td style="width:75px;padding-top:3px;"><asp:Label ID="valuesLabel" Visible="false" Text="VALUES:" runat="server"></asp:Label></td>
                                <td style="text-align:right;"><asp:TextBox ID="dropdownValues" style="margin-top:7px;" Visible="false" placeholder=" Optional.   e.g.: 10, 20, 30, Yes, No" onfocus="this.select()" TextMode="MultiLine" runat="server" Width="216px" Height="70px"></asp:TextBox></td>
                            </tr>
                            <tr style="vertical-align:top;">
                                <td style="width:75px;padding-top:3px;"><asp:Label ID="regex" Text="REGEX:" runat="server"></asp:Label></td>
                                <td style="text-align:right;"><asp:TextBox ID="regexTextBox" style="margin-top:7px;" placeholder=" Optional.     Remove delimiters //" onfocus="this.select()" TextMode="MultiLine" runat="server" Width="216px" Height="70px"></asp:TextBox></td>
                            </tr>
                            <tr style="vertical-align:top;">
                                <td style="width:75px;padding-top:3px;"><asp:Label ID="alert" Text="ALERT:" runat="server"></asp:Label></td>
                                <td style="text-align:right;"><asp:TextBox ID="msgTextBox" style="margin-top:7px;" placeholder=" Message of what a valid entry should be." TextMode="MultiLine" onfocus="this.select()" runat="server" Height="70px" Width="216px"></asp:TextBox></td>
                            </tr>
                            <tr>
                                <td colspan="2" style="text-align:right;padding-top:5px;"><asp:Button ID="labelContents" Text="Save" Font-Size="Small" runat="server" OnClick="labelContents_Click" /></td>
                            </tr>
                        </table>


                        <table style="margin-top:20px; margin-bottom:20px; width: 315px;" visible="false" runat="server">
                            <tr style="background-color:aliceblue; height:40px;">
                                <td style="height: 10px; text-align:left;padding-left:5px;">
                                    <asp:Button ID="unsetRules" Visible="true" runat="server" Text="Unset" Font-Size="10"
                                        OnClientClick="return confirm('ATTENTION!\n\nRemoving or changing configuration will affect the Index Data section of still unprinted indexes related to this job. We suggest that you make sure that there are no more unprinted indexes related to this job accross all operators prior unsetting config.\nDo you still want to procede with unsetting job configuration?');"
                                        OnClick="unsetRules_Click" /></td>
                                <td style="height: 10px; text-align:right;padding-right:5px;"><asp:Button ID="setRules" style="margin-left:25px;" Visible="true" Font-Size="10" runat="server" Text="Set " OnClick="setRules_Click" /></td>
                            </tr> 
                        </table>     
                    </asp:Panel>
                </td>
            </tr>
        </table>
    </asp:Panel>
        

</asp:Content>
