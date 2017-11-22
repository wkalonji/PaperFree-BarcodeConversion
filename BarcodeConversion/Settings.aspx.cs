using System;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using BarcodeConversion.App_Code;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;

namespace BarcodeConversion
{
    public partial class Contact : Page
    {
        public void Page_Init(object o, EventArgs e)
        {
            Page.MaintainScrollPositionOnPostBack = true;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                // Make sure only admins can see Settings page
                if (!Page.IsPostBack) jobAbb.Focus();

                if (userStatus() == "True")
                {
                    SettingsPanel.Visible = true;
                }
                else if (userStatus() == "False")
                {
                    SettingsPanel.Visible = false;
                    Response.Redirect("~/");
                    return;
                }
                else if (userStatus() == "Failed")
                {
                    return;
                }
                else if (userStatus() == "Not Found")
                {
                    string msg = "Operator not Found.";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    return;
                }
                jobSectionDropdownColor(); // Job section
                setDropdownColor(); // Index Config section
                success.Visible = false; 
            }
            catch (Exception ex)
            {
                string msg = "Error 50: Issue occured while attempting to load page. Contact system admin.";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
                return;
            }
        }



        // CHECK WHETHER USER IS ADMIN
        private string userStatus() 
        {
            string user = Environment.UserName;
            using (SqlConnection con = Helper.ConnectionObj) 
            {
                using (SqlCommand cmd = con.CreateCommand()) 
                {
                    cmd.CommandText = "SELECT ADMIN FROM OPERATOR WHERE NAME=@userName";
                    cmd.Parameters.AddWithValue("@userName", user);
                    try 
                    {
                        con.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                            return result.ToString();
                        else return "Not Found";
                    }
                    catch (Exception ex)
                    {
                        string msg = "Error 51: Issue occured while attempting to identify operator status. Contact system admin." ;
                        ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                        // Log the exception and notify system operators
                        ExceptionUtility.LogException(ex);
                        ExceptionUtility.NotifySystemOps(ex);
                        return "Failed";
                    }
                }
            }
        }



        // 'CHOOSE YOUR ACTION' DROPDOWN: CHOOSE TO CREATE/EDIT JOB
        protected void actionChange(object sender, EventArgs e)
        {   
            try
            {
                if (selectAction.SelectedValue == "edit")
                {
                    // Fill dropdown list with jobs.
                    selectJobList.Items.Clear();
                    selectJobList.Items.Add("Select");
                    getDropdownJobItems();
                    jobAbb.Visible = false;
                    createJobBtn.Visible = false;
                    deleteJobBtn.Visible = false;
                    selectJobList.Visible = true;
                    editJobBtn.Visible = true;
                    jobNameLabel.Visible = true;
                    jobName.Visible = true;
                    jobName.Attributes["placeholder"] = " Optional";
                    jobActiveLabel.Visible = true;
                    jobActiveBtn.Visible = true;
                }
                else if (selectAction.SelectedValue == "create")
                {
                    jobSectionDefault();
                }
                else
                {
                    editJobBtn.Visible = false;
                    jobAbb.Visible = false;
                    createJobBtn.Visible = false;
                    jobNameLabel.Visible = false;
                    jobName.Visible = false;
                    jobActiveLabel.Visible = false;
                    jobActiveBtn.Visible = false;
                    jobAssignedToLabel.Visible = false;
                    jobAssignedTo.Visible = false;
                    selectJobList.Visible = true;
                    deleteJobBtn.Visible = true;
                    getDropdownJobItems();
                }
            }
            catch (Exception ex)
            {
                string msg  = "Error 52: Issue occured while attempting to choose action. Contact system admin. " ;
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
        }



        // 'CREATE' CLICKED: CREATE NEW JOB. FUNCTION
        protected void createJob_Click(object sender, EventArgs e)
        {   
            try
            {
                if (!Page.IsValid) return;
                if (this.jobAbb.Text == string.Empty || jobAbb.Text.Length > 6)
                {
                    string msg = "Job abbreviation is required (6 characters max)";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    jobAbb.Text = string.Empty;
                    jobAbb.Focus();
                    return;
                }
                else if (this.jobName.Text == string.Empty)
                {
                    string warning2 = "Job name is required!";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + warning2 + "');", true);
                    jobName.Text = string.Empty;
                    jobName.Focus();
                    return;
                }
                else
                {
                    try
                    {
                        using(SqlConnection con = Helper.ConnectionObj) 
                        {
                            // Save created job
                            using (SqlCommand cmd = con.CreateCommand()) 
                            {
                                cmd.CommandText = "INSERT INTO JOB (ABBREVIATION, NAME, ACTIVE) VALUES(@abbr, @name, @active)";
                                cmd.Parameters.AddWithValue("@abbr", this.jobAbb.Text);
                                cmd.Parameters.AddWithValue("@name", this.jobName.Text);
                                cmd.Parameters.AddWithValue("@active", this.jobActiveBtn.SelectedValue.ToUpper());
                                con.Open();
                                if (cmd.ExecuteNonQuery() == 1)
                                {
                                    // Assign job to assignee
                                    if (jobAssignedTo.SelectedValue != "Select")
                                    {
                                        string assignee = jobAssignedTo.SelectedValue;
                                        string abbr = jobAbb.Text;
                                        bool answer = AssignJob(assignee, abbr); // calling assignJob function
                                        if (answer == true)
                                        {
                                            string msg = "New job successfully saved & assigned!";
                                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                            jobFormClear();
                                            object b = this.Master.FindControl("MainContent").FindControl("unassignedBtn") as Button;
                                            getUnassignedJobs(b);
                                        }
                                        else
                                        {
                                            string msg = "Error 53: New job successfully saved, but not assigned! Contact system admin.";
                                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                            jobFormClear();
                                        }
                                    }
                                    else
                                    {
                                        success.Text = "New Job Created!";
                                        success.Visible = true;
                                        ClientScript.RegisterStartupScript(this.GetType(), "fadeoutOp", "FadeOut3();", true);
                                        jobFormClear();
                                        object b = this.Master.FindControl("MainContent").FindControl("unassignedBtn") as Button;
                                        getUnassignedJobs(b);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        jobFormClear();
                        if (ex.Message.Contains("Violation of UNIQUE KEY"))
                        {
                            string msg = "The Job Abbreviation entered already exists.";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                        }
                        else
                        {
                            string msg = "Error 54: Issue occured while attempting to save the created job. Contact system admin.";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            // Log the exception and notify system operators
                            ExceptionUtility.LogException(ex);
                            ExceptionUtility.NotifySystemOps(ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg  = "Error 55: Issue occured while attempting to save the created job. Contact system admin." ;
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
        }

        
        // 'EDIT' CLICKED: EDIT JOB NAME OR ACTIVE STATUS. FUNCTION
        protected void editJob_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            try
            {
                // Edit job ACTIVE status
                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        if (this.selectJobList.SelectedValue == "Select")
                        {
                            string msg = "Please select a Job.";
                            Page.ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            jobFormClear();
                            return;
                        }
                        if (this.jobName.Text == string.Empty)
                            cmd.CommandText = "UPDATE JOB SET ACTIVE=@active WHERE ABBREVIATION=@abbr";
                        else
                        {
                            cmd.CommandText = "UPDATE JOB SET NAME=@job, ACTIVE=@active WHERE ABBREVIATION=@abbr";
                            cmd.Parameters.AddWithValue("@job", this.jobName.Text);
                        }
                        cmd.Parameters.AddWithValue("@active", this.jobActiveBtn.SelectedValue);
                        cmd.Parameters.AddWithValue("@abbr", this.selectJobList.SelectedValue);
                        con.Open();
                        if (cmd.ExecuteNonQuery() == 1)
                        {
                            // Assign job to assignee
                            if (jobAssignedTo.Visible = true && jobAssignedTo.SelectedValue != "Select")
                            {
                                string assignee = jobAssignedTo.SelectedValue;
                                string abbr = this.selectJobList.SelectedValue;
                                bool answer = AssignJob(assignee, abbr); // calling assignJob function
                                if (answer == true)
                                {
                                    success.Text = "Job successfully updated & assigned!";
                                    success.Visible = true;
                                    ClientScript.RegisterStartupScript(this.GetType(), "fadeoutOp", "FadeOut3();", true);
                                    jobFormClear();
                                }
                                else
                                {
                                    string msg = "Error 55a: Job updated successfully, but not assigned! Contact System Admin.";
                                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                    jobFormClear();
                                }
                            }
                            else
                            {
                                success.Text = "Job updated successfully!";
                                success.Attributes["style"] = "color:green;";
                                success.Visible = true;
                                ClientScript.RegisterStartupScript(this.GetType(), "fadeoutOp", "FadeOut3();", true);
                                jobFormClear();
                            }
                            getDropdownJobItems();
                            getActiveJobs();
                            getUnassignedJobs(null);
                        }
                    }
                } 
            }
            catch (Exception ex)
            {
                string msg = "Error 56: Issue occured while attempting to update the job ACTIVE status. Contact system admin.";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
        }
        


        // 'DELETE' CLICKED: DELETE JOB. FUNCTION
        protected void deleteJob_Click(object sender, EventArgs e)
        {
            if (selectJobList.SelectedValue != "Select")
            {
                // First, Get selected job id
                int jobID = Helper.getJobId(selectJobList.SelectedValue);
                if (jobID <= 0)
                {
                    string msg = "Job selected could not be found. Contact system admin.";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    selectJob.SelectedValue = "Select";
                    return;
                }

                SqlConnection con = Helper.ConnectionObj;
                con.Open();
                if (jobID > 0)
                {
                    try 
                    {
                        // TBD: Got to delete the config 1st
                        // Then, delete job record in OPERATOR_ACCESS
                        SqlCommand cmd2 = new SqlCommand("DELETE FROM OPERATOR_ACCESS WHERE JOB_ID = @jobID", con);
                        cmd2.Parameters.AddWithValue("@jobID", jobID);
                        if (cmd2.ExecuteNonQuery() == 1)
                        {
                            // Finally, delete job record in JOB table
                            SqlCommand cmd3 = new SqlCommand("DELETE FROM JOB WHERE ABBREVIATION = @abb", con);
                            cmd3.Parameters.AddWithValue("@abb", this.selectJobList.SelectedValue);
                            if (cmd3.ExecuteNonQuery() == 1)
                            {
                                success.Text = "Job successfully deleted!";
                                success.Visible = true;
                                ClientScript.RegisterStartupScript(this.GetType(), "fadeoutOp", "FadeOut3();", true);
                                jobFormClear();
                                jobAssignedToLabel.Visible = false;
                                jobAssignedTo.Visible = false;
                                con.Close();
                                return;
                            }
                            else
                            {
                                string msg = "Error: There was an error in deleting this job in JOB.";
                                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                jobFormClear();
                                jobAssignedToLabel.Visible = false;
                                jobAssignedTo.Visible = false;
                                con.Close();
                                return;
                            }
                        }
                        else
                        {
                            // If no record in OPERATOR_ACCESS, just delete it in JOB
                            SqlCommand cmd3 = new SqlCommand("DELETE FROM JOB WHERE ABBREVIATION = @abb", con);
                            cmd3.Parameters.AddWithValue("@abb", this.selectJobList.SelectedValue);
                            if (cmd3.ExecuteNonQuery() == 1)
                            {
                                success.Text = "Job successfully deleted!";
                                success.Visible = true;
                                ClientScript.RegisterStartupScript(this.GetType(), "fadeoutOp", "FadeOut3();", true);
                                jobFormClear();
                                jobAssignedToLabel.Visible = false;
                                jobAssignedTo.Visible = false;
                                con.Close();
                                return;
                            }
                            else
                            {
                                string msg = "Error: There was an error in deleting this job in JOB.";
                                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                jobFormClear();
                                jobAssignedToLabel.Visible = false;
                                jobAssignedTo.Visible = false;
                                con.Close();
                                return;
                            }
                        }
                    } catch (Exception ex) {
                        string msg = "Error: Something went wrong while attempting to delete this job. Contact your system admin.";
                        ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                        // Log the exception and notify system operators
                        ExceptionUtility.LogException(ex);
                        ExceptionUtility.NotifySystemOps(ex);
                    }
                   
                }
                con.Close();
            }
            getDropdownJobItems();
            jobAssignedToLabel.Visible = false;
            jobAssignedTo.Visible = false;
        }

    

        // 'SUBMIT' CLICKED: SET OPERATOR ADMIN PERMISSIONS. FUNCTION
        protected void setPermissions_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            try
            {
                if (user.Text != string.Empty)
                {
                    string msg;
                    // If user exists, get ID
                    int opId = Helper.getUserId(user.Text);

                    // If user exists, set Admin status
                    using (SqlConnection con = Helper.ConnectionObj)
                    {
                        using (SqlCommand cmd = con.CreateCommand())
                        {
                            cmd.CommandText = "UPDATE OPERATOR SET ADMIN=@admin WHERE NAME=@user";
                            cmd.Parameters.AddWithValue("@admin", this.permissions.SelectedValue);
                            cmd.Parameters.AddWithValue("@user", this.user.Text);
                            con.Open();
                            if (cmd.ExecuteNonQuery() == 1)
                            {
                                msg = "Permissions set!";
                                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                getOperators(); // Get operators

                                // If operator becomes Admin, remove assigned jobs in OPERATOR_ACCESS
                                if (permissions.SelectedValue == "1")
                                {
                                    //cmd.Parameters.Clear();
                                    //cmd.CommandText = "DELETE FROM OPERATOR_ACCESS WHERE OPERATOR_ID=@opID";
                                    //cmd.Parameters.AddWithValue("@opID", opId);
                                    //cmd.ExecuteNonQuery();
                                }
                                // If admin becomes Operator
                                else
                                {   
                                    // If self demotion, hide Settings & redirect to Home page.
                                    string op = Environment.UserName;
                                    if (op == user.Text)
                                    {
                                        LinkButton l = this.Master.FindControl("settings") as LinkButton;
                                        l.Visible = false;
                                        Response.Redirect("~/");
                                    }
                                }
                                permissionsFormClear();
                                return;
                            }

                            // If user doesn't exist, register user and set Admin status.
                            cmd.CommandText = "INSERT INTO OPERATOR (NAME, ADMIN) VALUES(@user,@admin)";
                            if (cmd.ExecuteNonQuery() == 1)
                            {
                                msg = "New Operator added!";
                                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                permissionsFormClear();
                            }
                        }
                    }
                }
                else
                {
                    string msg = "Operator field is required!";
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "')", true);
                    permissionsFormClear();
                }
                getOperators(); // Get operators
            }
            catch (Exception ex)
            {   
                string msg  = "Error 58: Issue occured while attempting to set permissions. Contact system admin.";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
        }



        // CHECKBOX THAT SETS ALL THE OTHERS. HELPER FUNCTION
        protected void selectAll_changed(object sender, EventArgs e)
        {   
            try
            {
                CheckBox ChkBoxHeader = (CheckBox)jobAccessGridView.HeaderRow.FindControl("selectAll");
                foreach (GridViewRow row in jobAccessGridView.Rows)
                {
                    CheckBox ChkBoxRows = (CheckBox)row.FindControl("cbSelect");
                    if (ChkBoxHeader.Checked == true)
                        ChkBoxRows.Checked = true;
                    else ChkBoxRows.Checked = false;
                }
            }
            catch (Exception ex)
            {   
                string msg  = "Error 59: Issue occured while processing master CheckBox. Contact system admin.";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
        }



        // CLEAR JOB FORM. HELPER FUNCTION
        private void jobFormClear()
        {   
            try
            {
                jobAbb.Text = string.Empty;
                selectJobList.SelectedValue = "Select";
                jobName.Text = string.Empty;
                jobActiveBtn.SelectedValue = "1";
                jobAssignedTo.SelectedValue = "Select";
                jobAssignedToLabel.Visible = true;
                jobAssignedTo.Visible = true;
                getOperators();
                jobAbb.Focus();
            }
            catch (Exception ex)
            {   
                string msg  = "Error 60: Issue occured while attempting to clear fields. Contact system admin.";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
        }



        // CLEAR PERMISSIONS FORM. HELPER FUNCTION
        private void permissionsFormClear()
        {
            user.Text = string.Empty;
            permissions.SelectedValue = "0";
            user.Focus();
        }



        // HIDE/COLLAPSE JOB SECTION. FUNCTION
        protected void newJobShow_Click(object sender, EventArgs e)
        {
           try
           {
                if (jobSection.Visible == false)
                {
                    jobSection.Visible = true;
                    line.Visible = true;
                    jobFormClear();
                    jobSectionDefault();
                }
                else
                {
                    jobSection.Visible = false;
                    if (newUserSection.Visible == false) line.Visible = false;
                }
            }
           catch (Exception ex)
           {
                string msg  = "Error 61: Issue occured while attempting to hide or collapse panel. Contact system admin." ;
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
        }



        // 'PERMISSION SECTION' CLICKED: HIDE/SHOW USER PERMISSION SECTION
        protected void permissionsShow_Click(object sender, EventArgs e)
        {
            if(newUserSection.Visible == false)
            {
                newUserSection.Visible = true;
                line.Visible = true;
            }
            else
            {
                newUserSection.Visible = false;
                if (jobSection.Visible == false) line.Visible = false;
            }          
        }



        // 'JOB ACCESS SECTION' CLICKED: HIDE/SHOW JOB-ACCESS SECTION & PULL InnaccessibleED JOBS
        protected void assignShow_Click(object sender, EventArgs e)
        {   
            try
            {
                if (assignPanel.Visible == false)
                {
                    Page.Validate();
                    assignPanel.Visible = true;
                    assignee.SelectedValue = "Select";
                    // Get all unassigned jobs
                    getUnassignedJobs(null);
                    getOperators();
                }
                else
                {
                    assignPanel.Visible = false;
                }
            }
            catch (Exception ex)
            {
                string msg  = "Error 62: Issue occured while attempting to hide or show panel. Contact system admin.";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
        }



        // 'OPERATOR' SELECTION
        private void getOperators()
        {
            try
            {
                assignee.Items.Clear();
                jobAssignedTo.Items.Clear();
                assignee.Items.Add("Select");
                jobAssignedTo.Items.Add("Select");
                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "SELECT NAME FROM OPERATOR WHERE ADMIN=0";
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {   
                                    assignee.Items.Add(reader.GetValue(0).ToString());
                                    jobAssignedTo.Items.Add(reader.GetValue(0).ToString());
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                string msg = "Error 62a: Issue occured while retrieving operators. Contact system admin.";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
        }


        // 'ACCESSIBLE' CLICKED: GET OP'S ACCESSIBLE JOBS. FUNCTION
        protected void assignedJob_Click(object sender, EventArgs e)
        {
            jobAccessGridView.PageIndex = 0;
            getAssignedJobs();
        }



        // 'ACCESSIBLE' CLICKED: GET OP'S ACCESSIBLE JOBS. HELPER FUNCTION
        private void getAssignedJobs()
        {
            try
            {
                if (assignee.SelectedValue == "Select")
                {
                    string msg = "Operator field is required!";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    assignee.Focus();
                    return;
                }

                // If operator field not empty, get operator ID, then jobs
                int opID = 0;
                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "SELECT ID FROM OPERATOR WHERE NAME=@name";
                        cmd.Parameters.AddWithValue("@name", assignee.SelectedValue);
                        con.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null) opID = (int)result;
                        else
                        {
                            string msg = "Specified Operator could not be found!";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            assignee.Focus();
                            return;
                        }
                        jobsLabel.Text = "Accessible Jobs";

                        // Get operator's assigned jobs
                        jobAccessBtn.Visible = false;
                        cmd.Parameters.Clear();
                        cmd.CommandText =   "SELECT ABBREVIATION " +
                                            "FROM JOB " +
                                            "INNER JOIN OPERATOR_ACCESS ON JOB.ID=OPERATOR_ACCESS.JOB_ID " +
                                            "WHERE ACTIVE=1 AND OPERATOR_ACCESS.OPERATOR_ID=@ID " +
                                            "ORDER BY ABBREVIATION ASC";
                        cmd.Parameters.AddWithValue("@ID", opID);
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataSet ds = new DataSet();
                            da.Fill(ds);
                            if (ds.Tables.Count > 0)
                            {
                                jobAccessGridView.DataSource = ds.Tables[0];
                                jobAccessGridView.DataBind();
                                jobAccessGridView.Visible = true;
                                assignee.Focus();
                            }

                            // Handling of whether any JOB was returned from DB
                            if (jobAccessGridView.Rows.Count == 0)
                            {
                                jobAccessGridView.Visible = false;
                                jobAccessBtn.Visible = false;
                                deleteAssignedBtn.Visible = false;
                                jobsLabel.Text = "Operator's Accessible Jobs Not Found";
                                assignee.Focus();
                            }
                            else
                            {
                                jobAccessBtn.Visible = false;
                                deleteAssignedBtn.Visible = true;
                                assignee.Focus();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg  = "Error 65: Issue occured while attempting to get operator's accessible jobs. Contact system admin.";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
        }
        


        // 'ACTIVE' CLICKED: GET ALL ACTIVE JOBS. FUNCTION
        protected void unassignedJob_Click(object sender, EventArgs e)
        {   
            try
            {
                jobAccessGridView.PageIndex = 0;
                getUnassignedJobs(sender);
            }
            catch (Exception ex)
            {
                string msg  = "Error 66: Issue occured while attempting to retrieve active jobs. Contact system admin.";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
        }



        // 'DENY' CLICKED: REMOVE OPERATOR ACCESS TO JOBS. FUNCTION
        protected void deleteAssigned_Click(object sender, EventArgs e)
        {   
            try
            {
                int opID = 0, jobID = 0, count = 0;
                if (assignee.SelectedValue == "Select")
                {
                    string msg = "Operator field is required!";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    assignee.Focus();
                    return;
                }
                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        // If operator field not empty, get Operator ID
                        cmd.CommandText = "SELECT ID FROM OPERATOR WHERE NAME = @name";
                        cmd.Parameters.AddWithValue("@name", assignee.SelectedValue);
                        con.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null) opID = (int)result;
                        else
                        {
                            string msg = "Specified Operator could not be found!";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            assignee.Focus();
                            assignedJob_Click(new object(), new EventArgs());
                            return;
                        }

                        // For each checked job, remove access.
                        foreach (GridViewRow row in jobAccessGridView.Rows)
                        {
                            CheckBox chxBox = row.FindControl("cbSelect") as CheckBox;
                            if (chxBox.Checked)
                            {
                                // First, get ID of selectd job
                                cmd.Parameters.Clear();
                                cmd.CommandText = "SELECT ID FROM JOB WHERE ABBREVIATION = @abbr";
                                cmd.Parameters.AddWithValue("@abbr", row.Cells[2].Text);
                                object result2 = cmd.ExecuteScalar();
                                if (result2 != null) jobID = (int)result2;
                                else
                                {
                                    string msg = "Error 68: Selected job " + row.Cells[2].Text + " could not be found. Contact system admin.";
                                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                    assignee.Focus();
                                    assignedJob_Click(new object(), new EventArgs());
                                    return;
                                }

                                // Then, remove job from OPERATOR_ACCESS
                                cmd.Parameters.Clear();
                                cmd.CommandText = "DELETE FROM OPERATOR_ACCESS WHERE OPERATOR_ACCESS.JOB_ID = @job AND OPERATOR_ACCESS.OPERATOR_ID = @op";
                                cmd.Parameters.AddWithValue("@job", jobID);
                                cmd.Parameters.AddWithValue("@op", opID);
                                if (cmd.ExecuteNonQuery() == 1) count++;
                                else
                                {
                                    string msg = "Error 70: Something went wrong while removing job: " + row.Cells[2].Text + ". Contact system admin.";
                                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                    assignedJob_Click(new object(), new EventArgs());
                                    assignee.Focus();
                                }
                            }
                        }

                        // Handling whether or not any job access was denied
                        if (count == 0)
                        {
                            string msg = "Please select at least one job.";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            assignedJob_Click(new object(), new EventArgs());
                        }
                        else
                        {
                            string msg = count + " Job(s) access denied!";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            assignee.Focus();
                            assignedJob_Click(new object(), new EventArgs());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "Error 71: Issue occured while attempting to deny operator's job accesses. Contact system admin.";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
        }



        // 'GRANT' CLICKED: ASSIGN OPERATORS JOB-ACCESSES. FUNCTION
        protected void jobAccess_Click(object sender, EventArgs e)
        {   
            try
            {   
                string assigneeName = assignee.SelectedValue;
                int countGranted = 0, countChecked = 0, countError = 0;
                if (assigneeName == "Select")
                {
                    string msg = "Operator field is required!";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    assignee.Focus();
                    return;
                }

                if (jobAccessGridView.Rows.Count > 0)
                {
                    // For each checked job, assign job to assignee
                    foreach (GridViewRow row in jobAccessGridView.Rows)
                    {
                        CheckBox chxBox = row.FindControl("cbSelect") as CheckBox;
                        if (chxBox.Checked)
                        {
                            countChecked++;
                            string abbr = row.Cells[2].Text;
                            bool answer = AssignJob(assigneeName, abbr); // calling assignJob function
                            if (answer == true) countGranted++;
                            else
                            {
                                chxBox.Checked = false;
                                countError++;
                            }
                        }
                    }

                    // Handling whether any job access was granted.
                    if (countChecked == 0)
                    {
                        string msg = "Please make sure that at least 1 job is selected.";
                        ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                        return;
                    }
                    else
                    {
                        jobAccessGridView.PageIndex = 0;
                        if (countGranted > 0 && countGranted == countChecked)
                        {
                            string msg = countGranted + " job(s) granted.";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            getUnassignedJobs(sender);
                            assignee.Focus();
                            return;
                        }
                        else 
                        {
                            string msg = countGranted + " Job(s) granted. Operator already has access to " +countError+ " Job(s)";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            getUnassignedJobs(sender);
                            assignee.Focus();
                            return;
                        }
                    }
                }
                else
                {
                    string msg = "There are no jobs to be granted access to.";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    return;
                }
            }
            catch (Exception ex)
            {
                string msg  = "Error 72: Issue occured while attempting to grant jobs accesses to operator. Contact system admin.";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
        }



        // 'JOB INDEX CONFIGURATION SECTION' CLICKED: SHOW & INDEX FORM CONTROLS. FUNCTION
        protected void jobIndexEditingShow_Click(object sender, EventArgs e)
        {   
            try
            {
                // Fill Job Abb. dropdown list with jobs.
                selectJob.Items.Clear();
                selectJob.Items.Add("Select");
                if (jobIndexEditingPanel.Visible == false)
                {
                    jobIndexEditingPanel.Visible = true;
                    labelsTable.Visible = false;
                    labelControlsTable.Visible = false;
                    setupTitle.Visible = false;
                    getActiveJobs();
                }
                else
                {
                    jobIndexEditingPanel.Visible = false;
                }
            }
            catch (Exception ex)
            {
                string msg  = "Error 73: Issue occured while attempting to show or hide section. Contact system admin.";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
        }



        // 'JOB ABBREVIATION' DROPDOWN SELECT
        protected void JobAbbSelect(object sender, EventArgs e)
        {   
            try
            {

                if (selectJob.SelectedValue != "Select")
                {   
                    labelsTable.Visible = true;
                    edit1.Visible = false;
                    edit2.Visible = true;
                    edit3.Visible = true;
                    edit4.Visible = true;
                    edit5.Visible = true;
                    labelControlsTable.Visible = true;
                    setupTitle.Visible = true;

                    // Clear labelControlsTable current values
                    labelTextBox.Text = string.Empty;
                    dropdownValues.Text = string.Empty;
                    regexTextBox.Text = string.Empty;
                    msgTextBox.Text = string.Empty;

                    // Get type of 1st control 
                    int jobId = Helper.getJobId(selectJob.SelectedValue);
                    var controlInfo = isDropdownType(1, jobId);
                    
                    bool isLabel1Set = controlInfo.Item1;
                    bool isControlDropdown = controlInfo.Item2;

                    if (isControlDropdown == true)
                    {
                        dropdownType.Checked = true;
                        textBoxType.Checked = false;
                        dropdownValues.Visible = true;
                        valuesLabel.Visible = true;
                        regexTextBox.Visible = false;
                        msgTextBox.Visible = false;
                        regex.Visible = false;
                        alert.Visible = false;

                        string values = getDropdownValues(jobId, 1);
                        dropdownValues.Text = values;
                    }
                    else
                    {
                        textBoxType.Checked = true;
                        dropdownType.Checked = false;
                        dropdownValues.Visible = false;
                        valuesLabel.Visible = false;
                        regexTextBox.Visible = true;
                        msgTextBox.Visible = true;
                        regex.Visible = true;
                        alert.Visible = true;
                    }

                    // Retrieve & fill controls textboxes
                    for (int i = 1; i <= 5; i++)
                    {
                        using (SqlConnection con = Helper.ConnectionObj)
                        {
                            using (SqlCommand cmd = con.CreateCommand())
                            {
                                con.Open();
                                TextBox t = this.Master.FindControl("MainContent").FindControl("LABEL" + i) as TextBox;
                                cmd.CommandText = "SELECT LABEL" + i + " FROM JOB_CONFIG_INDEX WHERE JOB_ID=@jobID";
                                cmd.Parameters.AddWithValue("@jobID", jobId);
                                object result = cmd.ExecuteScalar();
                                if (result != null)
                                {
                                    if (result.ToString() != DBNull.Value.ToString())
                                    {
                                        t.Text = " " + (string)result;
                                        if (i == 1)
                                        {
                                            processRequest(new object(), new EventArgs());
                                            labelTextBox.Text = " " + (string)result;
                                            isLabel1Set = true;
                                        }
                                    }
                                    else
                                    {
                                        if (i == 1) t.Text = " Required";
                                        else t.Text = " Optional";
                                        if (isLabel1Set == false)
                                            labelTextBox.Attributes["placeholder"] = " Required";
                                    }
                                }
                                else
                                {
                                    if (i == 1) t.Text = " Required";
                                    else t.Text = " Optional";
                                    if (isLabel1Set == false)
                                        labelTextBox.Attributes["placeholder"] = " Required";
                                }
                            }
                        }
                    }
                    labelTextBox.Focus();
                }
                else
                {
                    labelsTable.Visible = false;
                    labelControlsTable.Visible = false;
                    setupTitle.Visible = false;
                }
            }
            catch(Exception ex)
            {
                string msg = "Error 73a: Issue occured while retrieving saved index data. Contact system admin.";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
            
        }


        // SET COLOR FOR DROPDOWN CONFIGURED JOB ITEMS. FUNCTION
        protected void onJobSelect(object sender, EventArgs e)
        {
            //setDropdownColor();
        }



        // 'ACTIVE' SELECTED: SET 'ASSIGN TO' VISIBLE OR NOT
        protected void onActiveSelect(object sender, EventArgs e)
        {
            if(jobActiveBtn.Visible && jobActiveBtn.SelectedValue == "1")
            {
                jobAssignedToLabel.Visible = true;
                jobAssignedTo.Visible = true;
                getOperators();
            }
            else
            {
                jobAssignedToLabel.Visible = false;
                jobAssignedTo.Visible = false;
            }
        }
        

        // 'X' ICON CLICKED: CLOSE CONTROL INFOs TABLE
        protected void hideControlInfo_Click(object sender, EventArgs e)
        {
            setDropdownColor();
            labelControlsTable.Visible = false;
            setupTitle.Visible = false;
            for (int i = 1; i <= 5; i++)
            {
                LinkButton btn = this.Master.FindControl("MainContent").FindControl("edit" + i) as LinkButton;
                TextBox t = this.Master.FindControl("MainContent").FindControl("label" + i) as TextBox;
                if (btn.Visible == false)
                {
                    btn.Visible = true;
                }
            }
        }


        // 'SAVE' ICON CLICKED: ADD LABEL CONTROLS
        protected void labelContents_Click(object sender, EventArgs e)
        {
            try
            {   
                if (labelTextBox.Text != string.Empty)
                {
                    // Make sure that regex & message fields are both either filled or blank
                    if ((regexTextBox.Text == string.Empty && msgTextBox.Text != string.Empty) || (regexTextBox.Text != string.Empty && msgTextBox.Text == string.Empty))
                    {
                        string msg = "Both regex and message fields must be either blank or filled.";
                        ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                        labelTextBox.Attributes["placeholder"] = " required for set";
                        if (regexTextBox.Text == string.Empty) regexTextBox.Focus();
                        else if (msgTextBox.Text == string.Empty) msgTextBox.Focus();
                        return;
                    }

                    // If Regex entered, check whether it's valid
                    if (regexTextBox.Text != string.Empty)
                    {
                        bool isValid = IsValidRegex(regexTextBox.Text.Trim());
                        if (isValid == false)
                        {
                            string msg = "The Regex pattern entered is not valid. You can test your Regex pattern at regexr.com";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            regexTextBox.Focus();
                            return;
                        }
                    }
                }

                // Get job id
                int jobId = Helper.getJobId(selectJob.SelectedValue);
                if (jobId <= 0)
                {
                    string msg = "Job selected could not be found.";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    selectJob.SelectedValue = "Select";
                    return;
                }

                // Check if job already configured
                var controlInfo = isDropdownType(1, jobId);
                bool isJobConfigured = controlInfo.Item1;
                bool isControlDropdown = controlInfo.Item2;

                // Get current control
                int controlBeingSet = 0;
                for (int i = 1; i <= 5; i++)
                {
                    LinkButton btn = this.Master.FindControl("MainContent").FindControl("edit" + i) as LinkButton;
                    if (btn.Visible == false)
                    {
                        controlBeingSet = i;
                    }
                }

                // Make sure 1st control NAME field isn't blank
                if (controlBeingSet == 1)
                {
                    if (labelTextBox.Text == string.Empty)
                    {
                        string msg = "NAME field is required!";
                        ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                        labelTextBox.Focus();
                        return;
                    }
                }

                // If job not configured
                if (!isJobConfigured)
                {
                    if (dropdownValues.Visible)
                    {
                        if (dropdownValues.Text == string.Empty)
                        {
                            // Just Save job config in JOB_CONFIG_INDEX
                            int result = saveJobConfig(jobId, 1, "dropdown", "insert");
                            handleResult(result, "B");
                            return;
                        }
                        else    // if dropdownValues = not empty
                        {
                            string[] values = dropdownValues.Text.Split(',');

                            // First Save values in INDEX_TABLE_FIELD 
                            for (int i = 1; i <= values.Length; i++)
                            {
                                int result2 = saveDropdownValues(jobId, values[i - 1].Trim(), controlBeingSet, i);
                                if (result2 <= 0)
                                {
                                    string msg = "Error 73C. Could not save dropdown values";
                                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                    selectJob.SelectedValue = "Select";
                                    return;
                                }
                            }

                            // Then Save job config in JOB_CONFIG_INDEX
                            int result = saveJobConfig(jobId, 1, "dropdown", "insert");
                            handleResult(result, "D");
                            return;
                        }
                    }
                    else // If control type = textbox
                    {
                        int result = saveJobConfig(jobId, 1, "textbox", "insert");
                        handleResult(result, "E");
                        return;
                    }
                }
                else    // If job already configured
                {
                    if (dropdownValues.Visible)
                    {
                        // First, delete current dropdown values from INDEX_TABLE_FIELD
                        deleteDropdownValues(jobId, controlBeingSet);

                        if (labelTextBox.Text == string.Empty || dropdownValues.Text == string.Empty)
                        {   
                            // Then update job config
                            int result2 = saveJobConfig(jobId, controlBeingSet, "dropdown", "update");
                            handleResult(result2, "F");
                            return;
                        }
                        string[] values = dropdownValues.Text.Split(',');

                        // Save values in INDEX_TABLE_FIELD first
                        for (int i = 1; i <= values.Length; i++)
                        {
                            int result2 = saveDropdownValues(jobId, values[i - 1].Trim(), controlBeingSet, i);
                            if (result2 <= 0)
                            {
                                string msg = "Error 73G. Could not save dropdown values";
                                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                selectJob.SelectedValue = "Select";
                                return;
                            }
                        }

                        // Then Save job config in JOB_CONFIG_INDEX
                        int result = saveJobConfig(jobId, controlBeingSet, "dropdown", "update");
                        handleResult(result, "H");
                        return;
                    }
                    else // If control type = textbox
                    {
                        if (isControlDropdown) deleteDropdownValues(jobId, controlBeingSet);
                        int result = saveJobConfig(jobId, controlBeingSet, "textbox", "update");
                        handleResult(result, "I");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "Error 74: Issue occured while attempting to add label controls. Contact system admin.";
                if (ex.Message.Contains("Cannot insert the value NULL"))
                {
                    msg = " Label1 is required! Please make sure NAME field is not blank while setting Label1.";
                    labelControlsTable.Visible = false;
                    setupTitle.Visible = false;
                    edit1.Visible = true;
                }
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
        }



        // REGEX VALIDATOR FUNCTION
        private static bool IsValidRegex(string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) return false;

            try
            {
                Regex.Match("", pattern);
            }
            catch (ArgumentException)
            {
                return false;
            }
            return true;
        }


        // 'EDIT' ICON CLICKED: EDIT LABEL CONTENTS
        protected void processRequest(object sender, EventArgs e)
        {  
            try
            {   
                // Get id of edit icon button then hide it
                string last = string.Empty;
                LinkButton b = new LinkButton();

                if (sender is LinkButton)
                {
                    b = (LinkButton)sender;
                    if (b.ID.Contains("edit"))
                    {
                        // Make sure current edit is done before starting another one
                        if (labelControlsTable.Visible == true)
                        {
                            string msg = "Current control must be set first or closed.";
                            onScreenMsg(msg, "#ff3333;", "configSection");
                            return;
                        }
                        ViewState["senderID"] = b.ID;
                        last = b.ID.Substring(b.ID.Length - 1, 1);
                    }
                }
                else
                {
                    for (int i = 1; i <= 5; i++)
                    {
                        LinkButton btn = this.Master.FindControl("MainContent").FindControl("edit" + i) as LinkButton;
                        if (btn != null)
                        {
                            if (btn.Visible == false)
                                last = i.ToString();
                        }
                        else last = "1";
                    }
                }
                
                b.Visible = false;

                // Make sure the 1st control is set 
                if (last != "1" && label1.Text == " Required")
                {
                    string msg = "LABEL1 must be set first.";
                    onScreenMsg(msg, "#ff3333;", "configSection");
                    b.Visible = true;
                    return;
                }


                // Get selected job id
                int jobId = Helper.getJobId(selectJob.SelectedValue);
                if (jobId <= 0)
                {
                    string msg = "Job selected could not be found. Contact system admin.";
                    ScriptManager.RegisterStartupScript(Page,Page.GetType(), "myalert", "alert('" + msg + "');", true);
                    selectJob.SelectedValue = "Select";
                    return;
                }

                // Clear labelControlsTable current values
                labelTextBox.Text = string.Empty;
                dropdownValues.Text = string.Empty;
                regexTextBox.Text = string.Empty;
                msgTextBox.Text = string.Empty;

                // Retrieve DB saved values
                var controlInfo = isDropdownType(Convert.ToInt32(last), jobId);
                bool isLabel1Set = controlInfo.Item1;
                bool isControlDropdown = controlInfo.Item2;

                // Check control type
                if (isControlDropdown == true)
                {
                    dropdownType.Checked = true;
                    textBoxType.Checked = false;
                    dropdownValues.Visible = true;
                    valuesLabel.Visible = true;
                    regexTextBox.Visible = false;
                    msgTextBox.Visible = false;
                    regex.Visible = false;
                    alert.Visible = false;
                    TextBox t = this.Master.FindControl("MainContent").FindControl("label" + last) as TextBox;
                    if (t.Text == string.Empty)
                    {
                        labelTextBox.Text = string.Empty;
                        string placeholder = string.Empty;
                        if (last == "1") placeholder = " Required";
                        else placeholder = " Optional";
                        labelTextBox.Attributes["placeholder"] = placeholder;
                    }
                    else labelTextBox.Text = t.Text.Trim();
                    string values = getDropdownValues(jobId, Convert.ToInt32(last));
                    dropdownValues.Text = " " + values.Trim();
                }
                else
                {
                    textBoxType.Checked = true;
                    dropdownType.Checked = false;
                    dropdownValues.Visible = false;
                    valuesLabel.Visible = false;
                    regexTextBox.Visible = true;
                    msgTextBox.Visible = true;
                    regex.Visible = true;
                    alert.Visible = true;

                    if (isLabel1Set == true)  // If textbox type
                    {
                       int success = showControlInfo(jobId, Convert.ToInt32(last));
                       if (success == -1)
                       {
                            string msg = "Error 74a: Issue occured while retrieving saved index data. Contact system admin.";
                            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                       }
                    }
                    else
                    {
                        string placeholder = string.Empty;
                        if (last == "1") placeholder = " Required";
                        else placeholder = " Optional";
                        labelTextBox.Attributes["placeholder"] = placeholder;
                        regexTextBox.Attributes["placeholder"] = " Optional. Remove delimiters //";
                        msgTextBox.Attributes["placeholder"] = " Message of what a valid entry should be.";
                    }
                }
                labelControlsTable.Visible = true;
                setupTitle.Visible = true;
                labelTextBox.Focus();
            }
            catch(Exception ex)
            {
                string msg = "Error 74b: Issue occured while retrieveing saved index data. Contact sytem admin.";
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + ex.Message + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            } 
        }


        // 'TYPE' RADIO GROUP CHECKED.
        protected void radioBtnChanged_Click(object sender, EventArgs e)
        {
            if (dropdownType.Checked)
            {
                dropdownValues.Visible = true;
                valuesLabel.Visible = true;
                labelTextBox.Focus();
                regexTextBox.Visible = false;
                msgTextBox.Visible = false;
                regex.Visible = false;
                alert.Visible = false;
            }
            else
            {
                dropdownValues.Visible = false;
                valuesLabel.Visible = false;
                labelTextBox.Focus();
                regexTextBox.Visible = true;
                msgTextBox.Visible = true;
                regex.Visible = true;
                alert.Visible = true;
            }
        }

        // 'SET' CLICKED: SET INDEX FORM CONTROLS. FUNCTION
        protected void setRules_Click(object sender, EventArgs e)
        {
            string selectedJob = "";
            try
            {
                int jobID = 0;
                // Make sure a job is selected & LABEL1 is filled.
                if (this.selectJob.SelectedValue == "Select")
                {
                    string msg = "Please select a specific job to configure!";
                    ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                    jobAbb.Text = string.Empty;
                    jobAbb.Focus();
                    return;
                }
                else if (this.label1.Text == string.Empty)
                {
                    string msg = "LABEL1 is required!";
                    ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                    if (labelControlsTable.Visible)
                    {
                        labelTextBox.Text = string.Empty;
                        labelTextBox.Focus();
                    }
                    return;
                }

                // Check whether any label was set.
                bool noLabelSet = true;
                for (int i=1; i<=5; i++)
                {
                    if (ViewState["labelValuesedit" + i] != null)
                    {
                        noLabelSet = false;
                    }
                }
                if (noLabelSet)
                {
                    string msg = "At least one label needs to be set!";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    return;
                }
                selectedJob = selectJob.SelectedValue;
                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand()) 
                    {
                        // First, get job ID of selected job
                        cmd.CommandText = "SELECT ID FROM JOB WHERE ABBREVIATION = @jobAbb";
                        cmd.Parameters.AddWithValue("@jobAbb", selectJob.SelectedValue);
                        con.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null) jobID = (int)result;
                        else
                        {
                            string msg = "Job selected could not be found.";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            selectJob.SelectedValue = "Select";
                            return;
                        }

                        // Then, use that job ID to set job rules into JOB_CONFIG_INDEX
                        cmd.Parameters.Clear();
                        cmd.CommandText =   "INSERT INTO JOB_CONFIG_INDEX" +
                                            "(JOB_ID, LABEL1, REGEX1, ALERT1, LABEL2, REGEX2, ALERT2, LABEL3, REGEX3, ALERT3, LABEL4, REGEX4, ALERT4, LABEL5, REGEX5, ALERT5) " +
                                            "VALUES(@jobID, @label1, @regex1, @alert1, @label2, @regex2, @alert2, @label3, @regex3, @alert3, @label4, @regex4, @alert4, @label5, @regex5, @alert5)";
                        cmd.Parameters.AddWithValue("@jobID", jobID);
                        
                        for (int i=1; i<=5; i++)
                        {
                            var labelValues = new List<string> {string.Empty, string.Empty, string.Empty};
                            if (ViewState["labelValuesedit" + i] != null)
                                labelValues = (List<string>)ViewState["labelValuesedit" + i];
                            if (labelValues[0] == string.Empty)
                            {
                                cmd.Parameters.AddWithValue("@label" + i, DBNull.Value);
                                cmd.Parameters.AddWithValue("@regex" + i, DBNull.Value);
                                cmd.Parameters.AddWithValue("@alert" + i, DBNull.Value);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@label" + i, labelValues[0]);
                                if (labelValues[1] == string.Empty)
                                {
                                    cmd.Parameters.AddWithValue("@regex" + i, DBNull.Value);
                                    cmd.Parameters.AddWithValue("@alert" + i, DBNull.Value);
                                }
                                else
                                {
                                    cmd.Parameters.AddWithValue("@regex" + i, labelValues[1]);
                                    cmd.Parameters.AddWithValue("@alert" + i, labelValues[2]);
                                }
                            }
                        }

                        if (cmd.ExecuteNonQuery() == 1)
                        {
                            string msg = selectJob.SelectedValue + " Job config successfully set.";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            setDropdownColor();
                            clearRules();
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                clearRules();
                string msg;
                if (ex.Message.Contains("Violation of PRIMARY KEY"))
                {
                    msg = "\"" + selectedJob + "\"" + " job has already been configured.";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                }  
                else
                {
                    msg = "Error 75: Issue occured while attempting to configure selected job";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                }
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);

            }
        }



        // 'UNSET' CLICKED: UNSET INPUT-CONTROLS RULES. FUNCTION.
        protected void unsetRules_Click(object sender, EventArgs e)
        {
           
        }


        // PREVENT LINE BREAKS IN GRIDVIEW
        protected void rowDataBound(object sender, GridViewRowEventArgs e)
        {
            try
            {
                // GIVE CUSTOM COLUMN NAMES
                if (e.Row.RowType == DataControlRowType.Header)
                {
                    e.Row.Cells[2].Text = "JOB";
                    if (e.Row.Cells.Count == 4)
                        e.Row.Cells[3].Text = "IDX COUNT";
                    //e.Row.Cells[3].Text = "INDEX";
                    string colBorder = "border-left:1px solid #737373; border-right:1px solid #737373; white-space: nowrap;";
                    for (int i = 0; i < e.Row.Cells.Count; i++)
                        e.Row.Cells[i].Attributes.Add("style", colBorder);
                }
                // Set column borders & Prevent line breaks
                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    string colBorder = "border-width:1px 1px 1px 1px; border-style:solid; border-color:#cccccc; white-space: nowrap;";
                    for (int i = 0; i < e.Row.Cells.Count; i++)
                        e.Row.Cells[i].Attributes.Add("style", colBorder);
                }

                if (e.Row.RowType == DataControlRowType.Pager)
                {
                    string colBorder = "border-left:1px solid #646464; border-right:1px solid #646464; white-space: nowrap;";
                    for (int i = 0; i < e.Row.Cells.Count; i++)
                        e.Row.Cells[i].Attributes.Add("style", colBorder);
                }
            }
            catch (Exception ex)
            {
                string msg = "Error 47: Issue occured while attempting to prevent line breaks within gridview. Contact system admin.";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
        }


        // COLLAPSE ALL SECTIONS. 
        protected void collapseAll_Click(object sender, EventArgs e)
        {   
            try
            {
                if (downArrow.Visible == true)
                {
                    downArrow.Visible = false;
                    upArrow.Visible = true;
                    jobSection.Visible = true;
                    newUserSection.Visible = true;
                    getUnassignedJobs(null);
                    getOperators();
                    assignPanel.Visible = true;
                    jobIndexEditingPanel.Visible = true;
                    getActiveJobs();
                    jobSectionDefault();
                    line.Visible = true;
                    labelsTable.Visible = false;
                    labelControlsTable.Visible = false;
                    setupTitle.Visible = false;
                    //labelDropdown_Click(new object(), new EventArgs());
                }
                else
                {
                    downArrow.Visible = true;
                    upArrow.Visible = false;
                    jobSection.Visible = false;
                    newUserSection.Visible = false;
                    assignPanel.Visible = false;
                    jobIndexEditingPanel.Visible = false;
                    line.Visible = false;
                }
            }
            catch (Exception ex)
            {
                string msg = "Error 79: Issue occured while attempting to hile or collapse all sections. Contacy system admin.";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
        }



        // GET ALL ACTIVE JOBS. HELPER FUNCTION
        private void getUnassignedJobs(Object sender)
        {
            try
            {
                string assigneeName = string.Empty;
                int opID = 0;
                SqlCommand cmd = null;
                Button button = (Button)sender;
                string buttonId = string.Empty;
                if (button != null) buttonId = button.ID;

                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (cmd = con.CreateCommand())
                    {
                        // If 'INACCESSIBLE' clicked
                        if (button != null && (buttonId == "inaccessibleBtn" || buttonId == "jobAccessBtn"))
                        {
                            // Make sure Operator is entered
                            if (assignee.SelectedValue != "Select") assigneeName = assignee.SelectedValue;
                            else
                            {
                                string msg = "Operator field required.";
                                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                assignee.Focus();
                                return;
                            }

                            // Then check if specified operator exists. If so, get ID.
                            cmd.CommandText = "SELECT ID FROM OPERATOR WHERE NAME = @assignedTo";
                            cmd.Parameters.AddWithValue("@assignedTo", assigneeName);
                            con.Open();
                            object result = cmd.ExecuteScalar();
                            if (result != null) opID = (int)result;
                            else
                            {
                                string msg = "Operator entered could not be found.";
                                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                return;
                            }

                            // Then, set cmd to retrieve operator's inaccessible jobs
                            cmd.Parameters.Clear();
                            cmd.CommandText = "SELECT ABBREVIATION " +
                                              "FROM JOB " +
                                              "WHERE ACTIVE=1 AND ID NOT IN (SELECT JOB_ID FROM OPERATOR_ACCESS WHERE OPERATOR_ID=@opId) " +
                                              "ORDER BY ABBREVIATION ASC";
                            cmd.Parameters.AddWithValue("@opId", opID);
                        }
                        else
                        {
                            // If 'INACCESSIBLE' not clicked, set cmd to retrieve all Active jobs.
                            cmd.Parameters.Clear();
                            cmd.CommandText =  "SELECT ABBREVIATION, COUNT(INDEX_DATA.ID) " +
                                               "FROM JOB " +
                                               "LEFT JOIN INDEX_DATA ON JOB.ID=INDEX_DATA.JOB_ID " +
                                               "WHERE ACTIVE=1 " +
                                               "GROUP BY ABBREVIATION " +
                                               "ORDER BY ABBREVIATION ASC";
                                                // "LEFT JOIN OPERATOR_ACCESS ON JOB.ID = OPERATOR_ACCESS.JOB_ID " +   //In case we want jobs inaccessible by everyone
                                                // "WHERE JOB.ACTIVE = 1 AND OPERATOR_ACCESS.JOB_ID IS NULL", con);"
                        }

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataSet ds = new DataSet();
                            da.Fill(ds);
                            if (ds.Tables.Count > 0)
                            {
                                jobAccessGridView.DataSource = ds.Tables[0];
                                jobAccessGridView.DataBind();
                                jobAccessGridView.Visible = true;
                            }

                            // Handling of whether any JOB was returned from DB
                            if (jobAccessGridView.Rows.Count == 0)
                            {
                                jobAccessGridView.Visible = false;
                                jobAccessBtn.Visible = false;
                                deleteAssignedBtn.Visible = false;
                                if (buttonId == "inaccessibleBtn" || buttonId == "jobAccessBtn") jobsLabel.Text = "Operator's Inaccessible Jobs Not Found";
                                else jobsLabel.Text = "No Active Jobs Found";
                                jobsLabel.Visible = true;
                            }
                            else
                            {
                                if (buttonId == "inaccessibleBtn" || buttonId == "jobAccessBtn") jobsLabel.Text = "Inaccessible Jobs";
                                else jobsLabel.Text = "Active Jobs";
                                jobsLabel.Visible = true;
                                jobAccessBtn.Visible = true;
                                deleteAssignedBtn.Visible = false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "Error 81: Issue occured while attempting to retrieve jobs. Contact system admin.";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
        }



        // Get dropdown list job items
        private void getDropdownJobItems()
        {
            try 
            {
                selectJobList.Items.Clear();
                selectJobList.Items.Add("Select");

                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "SELECT ABBREVIATION, ACTIVE FROM JOB ORDER BY ABBREVIATION ASC";
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    string jobAbb = (string)reader.GetValue(0);
                                    bool active = (bool)reader.GetValue(1);
                                    selectJobList.Items.Add(jobAbb);
                                    if (active)
                                    {
                                        // Red config job items from 'JOB' section
                                        foreach (ListItem item in selectJobList.Items)
                                        {
                                            if (item.Value == jobAbb)
                                            {
                                                item.Attributes.Add("style", "color:Red;");
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                string msg = "No Jobs could be found.";
                                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                return;
                            }
                        }
                    }
                }
            }
            catch(Exception ex) 
            {
                string msg = "Error 84: Issue occured while attempting to retrieve jobs. Contact system admin. " ;
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
        }



        // Get dropdown list ACTIVE job items
        private void getActiveJobs()
        {
            try 
            {
                selectJob.Items.Clear();
                selectJob.Items.Add("Select");
                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {   
                        // Get all active jobs
                        cmd.CommandText = "SELECT ABBREVIATION FROM JOB WHERE ACTIVE=1 ORDER BY ABBREVIATION ASC";
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    string jobAbb = (string)reader.GetValue(0);
                                    selectJob.Items.Add(jobAbb);
                                }
                            }
                            else
                            {
                                string msg = "No Active jobs could be found";
                                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                return;
                            }
                        }
                    }
                }
                setDropdownColor();
            }
            catch(Exception ex) 
            {
                string msg = "Error 86: Issue occured while attempting to retrieve active jobs. Contact system admin. ";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
        }



        // ASSIGN JOB TO OPERATOR. HELPER FUNCTION
        private bool AssignJob(string assignee, string abbr)
        {
            try
            {
                int opID = 0, jobID = 0;
                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        // First check if specified operator exists. If so, get operator ID.
                        cmd.CommandText = "SELECT ID FROM OPERATOR WHERE NAME = @assignedTo";
                        cmd.Parameters.AddWithValue("@assignedTo", assignee);
                        con.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null) opID = (int)result;
                        else
                        {
                            string msg = "Specified operator could not be found!";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            return false;
                        }

                        // Then, get ID of job to be assigned.
                        cmd.Parameters.Clear();
                        cmd.CommandText = "SELECT ID FROM JOB WHERE ABBREVIATION = @abbr";
                        cmd.Parameters.AddWithValue("@abbr", abbr);
                        try {
                            object result2 = cmd.ExecuteScalar();
                            if (result != null) jobID = (int)result2;
                        }
                        catch (SqlException ex) {
                            string msg = "Error 88: Issue occured while attempting to retrieve job ID. Contact system admin. ";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            // Log the exception and notify system operators
                            ExceptionUtility.LogException(ex);
                            ExceptionUtility.NotifySystemOps(ex);
                        }

                        // Now, Save operator ID & new job ID in OPERATOR_ACCESS
                        if (opID > 0 && jobID > 0)
                        {
                            cmd.Parameters.Clear();
                            cmd.CommandText = "INSERT INTO OPERATOR_ACCESS (OPERATOR_ID, JOB_ID) VALUES(@opId, @jobID)";
                            cmd.Parameters.AddWithValue("@opID", opID);
                            cmd.Parameters.AddWithValue("@jobID", jobID);
                            if (cmd.ExecuteNonQuery() == 1) return true;
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);

                // Skip jobs that have already been made accessible to specified operator.
                if (ex.Message.Contains("Violation of UNIQUE KEY") || ex.Message.Contains("Violation of PRIMARY KEY"))
                {
                    return false;
                }
                else
                {
                    string msg = "Error 88: Issue occured while attempting to assign " + abbr + " to operator. Contact system admin. ";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                }
                return false;
            }
        }



        // SET COLOR FOR DROPDOWN CONFIGURED JOB ITEMS. HELPER FUNCTION
        private void setDropdownColor()
        {   
            try
            {
                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText =   "SELECT ABBREVIATION " +
                                            "FROM JOB " +
                                            "INNER JOIN JOB_CONFIG_INDEX ON JOB.ID = JOB_CONFIG_INDEX.JOB_ID " +
                                            "WHERE JOB.ACTIVE = 1";
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    // Red config job items from 'JOB INDEX CONFIG' section
                                    foreach (ListItem item in selectJob.Items)
                                    {
                                        if (item.Value == (string)reader.GetValue(0))
                                        {
                                            item.Attributes.Add("style", "color:Red;");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "Error 90: Issue occured while attempting to color appropriate jobs. Contact system admin. ";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
        }




        // SET COLOR FOR DROPDOWN ACTIVE JOB ITEMS. HELPER FUNCTION
        private void jobSectionDropdownColor()
        {
            try
            {
                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "SELECT ABBREVIATION " +
                                          "FROM JOB " +
                                          "WHERE JOB.ACTIVE = 1";
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    // Red active job items from 'JOB' section
                                    foreach (ListItem item in selectJobList.Items)
                                    {
                                        if (item.Value == (string)reader.GetValue(0))
                                        {
                                            item.Attributes.Add("style", "color:Red;");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "Error 90a: Issue occured while attempting to color appropriate jobs. Contact system admin. ";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
        }



        // HANDLE NEXT PAGE CLICK. FUNCTION
        protected void pageChange_Click(object sender, GridViewPageEventArgs e)
        {   
            try
            {
                jobAccessGridView.PageIndex = e.NewPageIndex;
                object b = this.Master.FindControl("MainContent").FindControl("inaccessibleBtn") as Button;

                if (jobsLabel.Text == "Accessible Jobs")
                    getAssignedJobs();
                else if (jobsLabel.Text == "Inaccessible Jobs")
                    getUnassignedJobs(b);
                else
                    getUnassignedJobs(null);
            }
            catch (Exception ex)
            {
                string msg = "Error 91: Issue occured while attempting to operator's ID. Contact system admin. " ;
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
        }



        // CLEAR RULES. HELPER FUNCTION
        private void clearRules()
        {
            selectJob.SelectedValue = "Select";
            label1.Text = string.Empty;
            label1.Attributes["placeholder"] = " Required only for Set";
            label1.Focus();
            label2.Text = string.Empty;
            label2.Attributes["placeholder"] = " Optional";
            label3.Text = string.Empty;
            label3.Attributes["placeholder"] = " Optional";
            label4.Text = string.Empty;
            label4.Attributes["placeholder"] = " Optional";
            label5.Text = string.Empty;
            label5.Attributes["placeholder"] = " Optional";
            for (int i = 1; i <= 5; i++) ViewState["labelValuesedit" + i] = null;
        }


        // JOB SECTION DEFAULTS. HELPER FUNCTION
        private void jobSectionDefault()
        {
            selectAction.SelectedValue = "create";
            selectJobList.Visible = false;
            deleteJobBtn.Visible = false;
            editJobBtn.Visible = false;

            jobAbb.Visible = true;
            createJobBtn.Visible = true;
            jobNameLabel.Visible = true;
            jobName.Visible = true;
            jobName.Attributes["placeholder"] = " Required";
            jobActiveLabel.Visible = true;
            jobActiveBtn.Visible = true;
            jobActiveBtn.SelectedValue = "1";
            jobAssignedToLabel.Visible = true;
            jobAssignedTo.Visible = true;
            getOperators();
        }

        // CHECK CONTROL TYPE
        private Tuple<bool,bool> isDropdownType(int order, int jobId)
        {
            using (SqlConnection con = Helper.ConnectionObj)
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "SELECT LABEL" + order + ", TABLEID" + order + " FROM JOB_CONFIG_INDEX WHERE JOB_ID=@jobID";
                    cmd.Parameters.AddWithValue("@jobID", jobId);
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows && reader.Read())
                        {
                            // If Control type = dropdown, hide textbox & associates
                            if (reader.GetValue(0) != DBNull.Value)
                            {
                                if (reader.GetValue(1) != DBNull.Value)
                                    return Tuple.Create(true,true);
                                return Tuple.Create(true, false);
                            }
                            return Tuple.Create(true, false);
                        }
                        return Tuple.Create(false,false);
                    }
                }
            }
        }


        // GET DROPDOWN VALUES
        private string getDropdownValues(int jobId, int order)
        {
            using (SqlConnection con = Helper.ConnectionObj)
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    string values = string.Empty;
                    cmd.CommandText = "SELECT VALUE FROM INDEX_TABLE_FIELD WHERE ID=@id";
                    cmd.Parameters.AddWithValue("@id", jobId + order.ToString());
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                values += reader.GetValue(0).ToString() + ", ";
                            }
                            values = values.Substring(0, values.Length - 2);
                            return values;
                        }
                        return string.Empty;
                    }
                }
            }
        }

        // SAVE DROPDOWN VALUES
        private int saveDropdownValues (int jobId, string value, int controlBeingSet, int dropdownOrder)
        {   
            try
            {
                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        // Save new table id
                        cmd.CommandText = "INSERT INTO INDEX_TABLE_FIELD(ID, VALUE, ORD) VALUES(@id, @value, @ord)";
                        cmd.Parameters.AddWithValue("@id", jobId + controlBeingSet.ToString());
                        cmd.Parameters.AddWithValue("@value", value);
                        cmd.Parameters.AddWithValue("@ord", dropdownOrder);
                        con.Open();
                        if (cmd.ExecuteNonQuery() == 1)
                        {
                            cmd.Parameters.Clear();
                            return 1;
                        }
                        return 0;
                    }
                }
            }
            catch(Exception ex)
            {
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
                return -1; // Error
            }
        }

        // DELETE DROPDOWN VALUES
        private int deleteDropdownValues(int jobId, int order)
        {
            try
            {
                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        // Clear table id if exists
                        con.Open();
                        cmd.CommandText = "DELETE FROM INDEX_TABLE_FIELD WHERE ID=@id";
                        cmd.Parameters.AddWithValue("@id", jobId + order.ToString());
                        return cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
                return -1; // Error
            }
        }


        // SAVE JOB CONFIG
        private int saveJobConfig(int jobId, int order, string controlType, string sqlType)
        {
            try
            {
                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        // Now set job config index
                        con.Open();

                        // If 1st control aka Job not yet configured
                        if (sqlType == "insert") 
                        {
                            cmd.CommandText =   "INSERT INTO JOB_CONFIG_INDEX" +
                                                "(JOB_ID, LABEL1, REGEX1, ALERT1, TABLEID1, LABEL2, REGEX2, ALERT2, TABLEID2, LABEL3, REGEX3, ALERT3, TABLEID3, " +
                                                "LABEL4, REGEX4, ALERT4, TABLEID4, LABEL5, REGEX5, ALERT5, TABLEID5) " +
                                                "VALUES(@jobID, @label1, @regex1, @alert1, @tableid1, @label2, @regex2, @alert2, @tableid2, " +
                                                "@label3, @regex3, @alert3, @tableid3, @label4, @regex4, @alert4, @tableid4, @label5, @regex5, @alert5, @tableid5)";
                            if (controlType == "dropdown")
                            {
                                for (int i = 1; i <= 5; i++)
                                {
                                    if (i == 1)
                                    {
                                        cmd.Parameters.AddWithValue("@label" + i, labelTextBox.Text.Trim());
                                        if (dropdownValues.Text == string.Empty)
                                            cmd.Parameters.AddWithValue("@tableid" + i, DBNull.Value);
                                        else cmd.Parameters.AddWithValue("@tableid" + i, jobId + order.ToString());
                                    }
                                    else
                                    {
                                        cmd.Parameters.AddWithValue("@label" + i, DBNull.Value);
                                        cmd.Parameters.AddWithValue("@tableid" + i, DBNull.Value);
                                    }
                                    cmd.Parameters.AddWithValue("@regex" + i, DBNull.Value);
                                    cmd.Parameters.AddWithValue("@alert" + i, DBNull.Value);
                                }
                                cmd.Parameters.AddWithValue("@jobID", jobId);
                            }
                            else // If control = textbox
                            {
                                for (int i = 1; i <= 5; i++)
                                {
                                    if (i == 1)
                                    {
                                        cmd.Parameters.AddWithValue("@label" + i, labelTextBox.Text.Trim());
                                        if (regexTextBox.Text != string.Empty)
                                            cmd.Parameters.AddWithValue("@regex" + i, regexTextBox.Text.Trim());
                                        else cmd.Parameters.AddWithValue("@regex" + i, DBNull.Value);
                                        if (msgTextBox.Text != string.Empty)
                                            cmd.Parameters.AddWithValue("@alert" + i, msgTextBox.Text.Trim());
                                        else cmd.Parameters.AddWithValue("@alert" + i, DBNull.Value);
                                    }
                                    else
                                    {
                                        cmd.Parameters.AddWithValue("@label" + i, DBNull.Value);
                                        cmd.Parameters.AddWithValue("@regex" + i, DBNull.Value);
                                        cmd.Parameters.AddWithValue("@alert" + i, DBNull.Value);
                                    }
                                    cmd.Parameters.AddWithValue("@tableid" + i, DBNull.Value);
                                }
                                cmd.Parameters.AddWithValue("@jobID", jobId);
                            }
                            return cmd.ExecuteNonQuery();
                        }
                        else // if job already configured
                        {
                            cmd.CommandText =   "UPDATE JOB_CONFIG_INDEX " +
                                                "SET LABEL" + order + "=@label, REGEX" + order + "=@regex, ALERT" + order + "=@alert, TABLEID" + order + "=@tableid " +
                                                "WHERE JOB_ID=@jobID";

                            // If control type is dropdown
                            if (controlType == "dropdown")
                            {
                                if (labelTextBox.Text == string.Empty)
                                {
                                    cmd.Parameters.AddWithValue("@label", DBNull.Value);
                                    cmd.Parameters.AddWithValue("@tableid", DBNull.Value);
                                }
                                else
                                {
                                    cmd.Parameters.AddWithValue("@label", labelTextBox.Text.Trim());
                                    if (dropdownValues.Text == string.Empty)
                                        cmd.Parameters.AddWithValue("@tableid", DBNull.Value);
                                    else cmd.Parameters.AddWithValue("@tableid", jobId + order.ToString());
                                } 
                                cmd.Parameters.AddWithValue("@regex", DBNull.Value);
                                cmd.Parameters.AddWithValue("@alert", DBNull.Value);
                                cmd.Parameters.AddWithValue("@jobID", jobId);
                            }
                            else    // if control type is textbox
                            {
                                if (labelTextBox.Text == string.Empty)
                                {
                                    cmd.Parameters.AddWithValue("@label", DBNull.Value);
                                    cmd.Parameters.AddWithValue("@regex", DBNull.Value);
                                    cmd.Parameters.AddWithValue("@alert", DBNull.Value);
                                }
                                else
                                {
                                    cmd.Parameters.AddWithValue("@label", labelTextBox.Text.Trim());
                                    if (regexTextBox.Text != string.Empty)
                                        cmd.Parameters.AddWithValue("@regex", regexTextBox.Text.Trim());
                                    else cmd.Parameters.AddWithValue("@regex", DBNull.Value);
                                    if (msgTextBox.Text != string.Empty)
                                        cmd.Parameters.AddWithValue("@alert", msgTextBox.Text.Trim());
                                    else cmd.Parameters.AddWithValue("@alert", DBNull.Value);
                                }
                                cmd.Parameters.AddWithValue("@tableid", DBNull.Value);
                                cmd.Parameters.AddWithValue("@jobID", jobId);
                            }
                            return cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
                return -1; // Error
            }
        }

        // HIDE LABEL INPUT FORM
        private void hideLabelControlsTable()
        {
            setDropdownColor();
            labelControlsTable.Visible = false;
            setupTitle.Visible = false;
            for (int i = 1; i <= 5; i++)
            {
                LinkButton btn = this.Master.FindControl("MainContent").FindControl("edit" + i) as LinkButton;
                TextBox t = this.Master.FindControl("MainContent").FindControl("label" + i) as TextBox;
                if (btn.Visible == false)
                {
                    btn.Visible = true;
                    if (labelTextBox.Text != string.Empty)
                        t.Text = " " + labelTextBox.Text.Trim();
                    else if (labelTextBox.Text == string.Empty && i != 1)
                        t.Text = " Optional";
                }
            }
        }

        // HANDLE RESULT
        private void handleResult(int result, string e)
        {
            if (result != 1)
            {
                string msg = "Error 73" + e + ". Could not save job config. Contact system admin.";
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                selectJob.SelectedValue = "Select";
                return;
            }
            else
            {
                hideLabelControlsTable();
                return;
            }
        }


        // RETRIEVE TEXTBOX CONTROL TYPE INFOS
        private int showControlInfo (int jobId, int controlBeingSet)
        {
            try
            {
                // Retrieve DB saved values
                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText =   "SELECT LABEL" + controlBeingSet + ", REGEX" + controlBeingSet + ", ALERT" + controlBeingSet + " " +
                                            "FROM JOB_CONFIG_INDEX " +
                                            "WHERE JOB_ID=@jobID";
                        cmd.Parameters.AddWithValue("jobID", jobId);
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                if (reader.Read() && reader.GetValue(0) != null)
                                {
                                    if (reader.GetValue(0).ToString() != DBNull.Value.ToString())
                                    {
                                        labelTextBox.Text = " " + (string)reader.GetValue(0);
                                        if (reader.GetValue(1).ToString() != string.Empty)
                                            regexTextBox.Text = " " + reader.GetValue(1).ToString();
                                        else
                                            regexTextBox.Attributes["placeholder"] = " Optional. Remove delimiters //";
                                        if (reader.GetValue(2).ToString() != string.Empty)
                                            msgTextBox.Text = " " + reader.GetValue(2).ToString();
                                        else
                                            msgTextBox.Attributes["placeholder"] = " Message of what a valid entry should be.";
                                    }
                                }
                            }
                        }
                    }
                }
                return 1;
            }
            catch(Exception ex)
            {
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
                return -1;
            }
        }


        // PRINT VARIOUS MSG ON SCREEN INSTEAD OF A POPUP
        private void onScreenMsg(string msg, string color, string from)
        {
            var screenMsg = new TableCell();
            var screenMsgRow = new TableRow();
            screenMsg.Text = msg;
            screenMsg.Attributes["style"] = "color:" + color;
            screenMsgRow.Cells.Add(screenMsg);
            if (from == "configSection") configMsg.Rows.Add(screenMsgRow);
            //else manualEntryMsg.Rows.Add(screenMsgRow);
        }

    }
}