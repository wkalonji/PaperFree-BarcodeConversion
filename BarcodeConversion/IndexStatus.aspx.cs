using BarcodeConversion.App_Code;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace BarcodeConversion
{
    public partial class IndexStatus : System.Web.UI.Page
    {
        public void Page_Init(object o, EventArgs e)
        {
            Page.MaintainScrollPositionOnPostBack = true;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack) // First page load
                {
                    Session["jobsList"] = populateJobsList();
                    getIndexes("Your Jobs", "meOnly", "allTime", "allSheets");
                    indexeStatusGridView.Visible = true;
                }

                // Reset gridview page
                Control c = Helper.GetPostBackControl(this.Page);
                if (c != null && (c.ID == "resetBtn" || c.ID == "whoFilter" || c.ID == "whenFilter" ||
                    c.ID == "whatFilter" || c.ID == "recordsPerPage")) indexeStatusGridView.PageIndex = 0;
                
                // Persits date entries
                if (c != null && (c.ID == "jobsFilter" || c.ID == "whoFilter" ||
                    c.ID == "whatFilter" || c.ID == "dates"))
                {
                    if (timePanel.Visible == true) 
                    {
                        if (ViewState["from"] != null) from.Text = ViewState["from"].ToString();
                        if (ViewState["to"] != null) to.Text = ViewState["to"].ToString();
                    }
                }
                    
            }
            catch (Exception ex)
            {
                string msg = "Error 37: Issue occured while loading this page. Contact your system admin.";
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
        }



        // 'RESET' CLICKED: RESET FILTER TO DEFAULT VALUES. FUNCTION
        protected void reset_Click(object sender, EventArgs e)
        {
            try
            {
                jobsFilter.SelectedValue = "Your Jobs";
                whoFilter.SelectedValue = "meOnly";
                whenFilter.SelectedValue = "allTime";
                whatFilter.SelectedValue = "allSheets";
                getIndexes("Your Jobs", "meOnly", "allTime", "allSheets");
                indexeStatusGridView.Visible = true;
                indexeStatusGridView.PageIndex = 0;
                sortOrder.Text = "Sorted By : CREATION_TIME ASC";
            }
            catch (Exception ex)
            {
                string msg  = "Error 38: Issue occured while attempting reset. Contact your system admin.";
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
        }



        // 'Your Jobs' DROPDOWN: POPULATE JOBSFILTER. 
        private Dictionary<int, string> populateJobsList()
        {
            try
            {
                jobsFilter.Items.Clear();
                jobsFilter.Items.Add("Your Jobs");
                
                // First, get current user id via name.
                string user = Environment.UserName;
                List<int> jobIdList = new List<int>();
                Dictionary<int, string> jobsDict = new Dictionary<int, string>();
                int opID = Helper.getUserId(user);
                if (opID == 0) return new Dictionary<int, string>();

                // Check if current operator is Admin &
                // Then, get all appropriate jobs for current operator from OPERATOR_ACCESS.
                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        // Get operator's Admin status
                        cmd.CommandText = "SELECT ADMIN FROM OPERATOR WHERE ID = @userId";
                        cmd.Parameters.AddWithValue("@userId", opID);
                        con.Open();
                        object result = cmd.ExecuteScalar();
                        bool isAdmin = false;
                        if (result != null)
                            isAdmin = (bool)cmd.ExecuteScalar();
                        ViewState["isAdmin"] = isAdmin;
                        cmd.Parameters.Clear();

                        // If Admin, get all jobs    
                        if (isAdmin == true)
                        {
                            cmd.CommandText =   "SELECT ID, ABBREVIATION " +
                                                "FROM JOB " +
                                                "WHERE ACTIVE=1 " +
                                                "ORDER BY ABBREVIATION ASC";
                        }
                        else // Else get assigned jobs only.
                        {
                            cmd.CommandText =   "SELECT ID, ABBREVIATION " +
                                                "FROM JOB " +
                                                "JOIN OPERATOR_ACCESS ON JOB.ID=OPERATOR_ACCESS.JOB_ID " +
                                                "WHERE ACTIVE=1 AND OPERATOR_ID=@userId " +
                                                "ORDER BY ABBREVIATION ASC";
                            cmd.Parameters.AddWithValue("@userId", opID);
                        }

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    int jobId = (int)reader.GetValue(0);
                                    string jobAbb = (string)reader.GetValue(1);
                                    jobsFilter.Items.Add(jobAbb);
                                    jobsDict.Add(jobId, jobAbb);
                                }
                                jobsFilter.AutoPostBack = true;
                            }
                            else return new Dictionary<int, string>();
                        }
                    }
                }
                return jobsDict;
            }
            catch (Exception ex)
            {
                string msg  = "Error 39: Issue occured while attempting to populate jobs dropdown. Contact system admin.";
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
                return new Dictionary<int, string>();
            }
        }



        // 'JOBSFILTER', 'WHO' & 'WHAT' FILTER CHANGED.
        protected void onSelectedChange(object sender, EventArgs e)
        {
            try
            {   
                sortOrder.Text = "Sorted By : CREATION_TIME ASC";
                getIndexes(jobsFilter.SelectedValue, whoFilter.SelectedValue, whenFilter.SelectedValue, whatFilter.SelectedValue);
            }
            catch (Exception ex)
            {
                string msg = "Error 40: Issue occured while attempting to process filter entries. Contact system admin.";
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
        }



        // 'WHEN' FILTER CHANGED.
        protected void onSelectWhen(object sender, EventArgs e)
        {
            try
            {
                if (whenFilter.SelectedValue == "allTime")
                {
                    timePanel.Visible = false;
                    getIndexes(jobsFilter.SelectedValue, whoFilter.SelectedValue, whenFilter.SelectedValue, whatFilter.SelectedValue);
                    indexeStatusGridView.Visible = true;
                    description.Visible = true;
                }
                else
                {
                    from.Text = string.Empty;
                    to.Text = string.Empty;
                    timePanel.Visible = true;
                    gridContainer.Visible = false;
                    indexeStatusGridView.Visible = false;
                    description.Visible = false;
                }
            }
            catch (Exception ex)
            {   
                string msg  = "Error 41: Issue occured while attempting to process filter entries. Contact system admin.";
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
        }



        // 'SUBMIT' CLICKED: DATE FIELDS ENTERED.
        protected void submit_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime start = DateTime.Parse(Request.Form[from.UniqueID]);
                DateTime end = DateTime.Parse(Request.Form[to.UniqueID]).Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                if (start != null) ViewState["from"] = start.Date.ToString("MM/dd/yyyy");
                if (end != null) ViewState["to"] = end.Date.ToString("MM/dd/yyyy");
                getIndexes(jobsFilter.SelectedValue, whoFilter.SelectedValue, whenFilter.SelectedValue, whatFilter.SelectedValue);
                from.Text = start.Date.ToString("MM/dd/yyyy");
                to.Text = end.Date.ToString("MM/dd/yyyy");
            }
            catch (Exception ex)
            {   
                if (ex.Message.Contains("String was not recognized as a valid DateTime"))
                {
                    ViewState["from"] = null;
                    ViewState["to"] = null;
                    
                    string msg = "All date fields required.";
                    onScreenMsg(msg, "#ff3333;", "dateRange");
                    from.Text = string.Empty;
                    to.Text = string.Empty;
                }
                else
                {
                    string msg = "Error 42: Issue occured while attempting to process filter entries. Contact system admin.";
                    ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                }
                
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
                indexeStatusGridView.PageIndex = e.NewPageIndex;
                getIndexes(jobsFilter.SelectedValue, whoFilter.SelectedValue, whenFilter.SelectedValue, whatFilter.SelectedValue);
            }
            catch (Exception ex)
            {   
                string msg  = "Error 43:   Issue occured while attempting to change page & process filter entries. Contact system admin.";
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
        }



        // GET FILTERED INDEXES. 
        protected void getIndexes(string jobAbb, string who, string when, string what)
        {
            try
            {
                Page.Validate();
                if (!Page.IsValid) return;

                // First, populate 'Your Jobs' filter
                Dictionary<int, string> jobsDict = (Dictionary<int, string>)Session["jobsList"];

                string user = Environment.UserName;
                int opID = Helper.getUserId(user);
                if (opID == 0 || jobsDict.Count == 0)
                {
                    description.Text = "No indexes found with the specified filter entries.";
                    description.Visible = true;
                    recordsPerPageLabel.Visible = false;
                    recordsPerPage.Visible = false;
                    sortOrder.Visible = false;
                    gridContainer.Visible = false;
                    return;
                }

                // Get job ID of selected Job
                int jobID = 0;
                foreach (var jobTuple in jobsDict)
                {
                    if (jobTuple.Value == jobAbb) jobID = jobTuple.Key;
                }

                SqlCommand cmd = new SqlCommand();
                bool isAdmin = (bool)ViewState["isAdmin"];
                string cmdString = string.Empty;
                
                //if (isAdmin == true)
                //{
                //    cmdString = "SELECT NAME, BARCODE, VALUE1, VALUE2, VALUE3, VALUE4, VALUE5, CREATION_TIME, PRINTED " +
                //                "FROM INDEX_DATA " +
                //                "INNER JOIN OPERATOR ON INDEX_DATA.OPERATOR_ID=OPERATOR.ID WHERE ";
                //}
                //else
                //{
                //    cmdString = "SELECT DISTINCT NAME, BARCODE, VALUE1, VALUE2, VALUE3, VALUE4, VALUE5, CREATION_TIME, PRINTED " +
                //                "FROM INDEX_DATA " +
                //                "INNER JOIN OPERATOR ON INDEX_DATA.OPERATOR_ID=OPERATOR.ID " +
                //                "INNER JOIN OPERATOR_ACCESS ON OPERATOR_ACCESS.JOB_ID=INDEX_DATA.JOB_ID " +
                //                "WHERE ";
                //}

                using (SqlConnection con = Helper.ConnectionObj)
                {
                    if (who == "meOnly")
                    {   
                        if (isAdmin)
                            cmdString = "SELECT NAME, BARCODE, VALUE1, VALUE2, VALUE3, VALUE4, VALUE5, CREATION_TIME, PRINTED " +
                                        "FROM INDEX_DATA " +
                                        "INNER JOIN OPERATOR ON INDEX_DATA.OPERATOR_ID=OPERATOR.ID WHERE INDEX_DATA.OPERATOR_ID=@opId ";
                        else
                            cmdString = "SELECT NAME, BARCODE, VALUE1, VALUE2, VALUE3, VALUE4, VALUE5, CREATION_TIME, PRINTED " +
                                        "FROM INDEX_DATA " +
                                        "INNER JOIN OPERATOR ON INDEX_DATA.OPERATOR_ID=OPERATOR.ID " +
                                        "INNER JOIN OPERATOR_ACCESS ON OPERATOR_ACCESS.JOB_ID=INDEX_DATA.JOB_ID " +
                                        "WHERE OPERATOR_ACCESS.OPERATOR_ID=OPERATOR.ID AND INDEX_DATA.OPERATOR_ID=@opId ";
                        if (when == "allTime")
                        {
                            timePanel.Visible = false;
                            if (what == "allSheets")
                            {
                                if (jobAbb == "Your Jobs") 
                                    cmd = new SqlCommand(cmdString, con);
                                else
                                {
                                    cmd = new SqlCommand(cmdString + "AND INDEX_DATA.JOB_ID=@jobID", con);
                                    cmd.Parameters.AddWithValue("@jobID", jobID);
                                }
                                description.Text = "Your Indexes for all Time.";
                            }
                            else if (what == "printed")
                            {
                                if (jobAbb == "Your Jobs") cmd = new SqlCommand(cmdString + "AND PRINTED=1", con);
                                else
                                {
                                    cmd = new SqlCommand(cmdString + " AND INDEX_DATA.JOB_ID=@jobID AND PRINTED=1", con);
                                    cmd.Parameters.AddWithValue("@jobID", jobID);
                                }
                                description.Text = "Your Printed Indexes for all Time.";
                            }
                            else if (what == "notPrinted")
                            {
                                if (jobAbb == "Your Jobs") cmd = new SqlCommand(cmdString + "AND PRINTED=0", con);
                                else
                                {
                                    cmd = new SqlCommand(cmdString + "AND INDEX_DATA.JOB_ID=@jobID AND PRINTED=0", con);
                                    cmd.Parameters.AddWithValue("@jobID", jobID);
                                }
                                description.Text = "Your Unprinted Indexes for all Time.";
                            }
                        }
                        else if (when == "pickRange")
                        {
                            DateTime start = DateTime.Parse(Request.Form[from.UniqueID]);
                            DateTime end = DateTime.Parse(Request.Form[to.UniqueID]).Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                            if (start == default(DateTime))
                            {
                                string msg = "Please pick a start date.";
                                onScreenMsg(msg, "#ff3333;", "dateRange");
                                return;
                            }
                            if (end == default(DateTime))
                            {
                                string msg = "Please pick an end date.";
                                onScreenMsg(msg, "#ff3333;", "dateRange");
                                return;
                            }

                            if (what == "allSheets")
                            {
                                if (jobAbb == "Your Jobs") cmd = new SqlCommand(cmdString + "AND (CREATION_TIME BETWEEN @start AND @end)", con);
                                else
                                {
                                    cmd = new SqlCommand(cmdString + "AND INDEX_DATA.JOB_ID=@jobID AND (CREATION_TIME BETWEEN @start AND @end)", con);
                                    cmd.Parameters.AddWithValue("@jobID", jobID);
                                }
                                description.Text = "Your Indexes from " + start.Date.ToString("MM/dd/yyyy") + " to " + end.Date.ToString("MM/dd/yyyy") + ".";
                            }
                            else if (what == "printed")
                            {
                                if (jobAbb == "Your Jobs") cmd = new SqlCommand(cmdString + "AND PRINTED=1 AND (CREATION_TIME BETWEEN @start AND @end)", con);
                                else
                                {
                                    cmd = new SqlCommand(cmdString + "AND INDEX_DATA.JOB_ID=@jobID AND PRINTED=1 AND (CREATION_TIME BETWEEN @start AND @end)", con);
                                    cmd.Parameters.AddWithValue("@jobID", jobID);
                                }
                                description.Text = "Your Printed Indexes from " + start.Date.ToString("MM/dd/yyyy") + " to " +end.Date.ToString("MM/dd/yyyy") + ".";
                            }
                            else if (what == "notPrinted")
                            {
                                if (jobAbb == "Your Jobs") cmd = new SqlCommand(cmdString + "AND PRINTED=0 AND (CREATION_TIME BETWEEN @start AND @end)", con);
                                else
                                {
                                    cmd = new SqlCommand(cmdString + "AND INDEX_DATA.JOB_ID=@jobID AND PRINTED=0 AND (CREATION_TIME BETWEEN @start AND @end)", con);
                                    cmd.Parameters.AddWithValue("@jobID", jobID);
                                }
                                description.Text = "Your Unprinted Indexes from " +start.Date.ToString("MM/dd/yyyy")+ " to " +end.Date.ToString("MM/dd/yyyy") + ".";
                            }
                            cmd.Parameters.AddWithValue("@start", start);
                            cmd.Parameters.AddWithValue("@end", end);
                        }
                        cmd.Parameters.AddWithValue("@opId", opID);
                    }
                    else if (who == "everyone")
                    {
                        if (isAdmin)
                            cmdString = "SELECT NAME, BARCODE, VALUE1, VALUE2, VALUE3, VALUE4, VALUE5, CREATION_TIME, PRINTED " +
                                        "FROM INDEX_DATA " +
                                        "INNER JOIN OPERATOR ON INDEX_DATA.OPERATOR_ID=OPERATOR.ID ";
                        else
                            cmdString = "SELECT NAME, BARCODE, VALUE1, VALUE2, VALUE3, VALUE4, VALUE5, CREATION_TIME, PRINTED " +
                                        "FROM INDEX_DATA " +
                                        "INNER JOIN OPERATOR ON INDEX_DATA.OPERATOR_ID=OPERATOR.ID " +
                                        "WHERE INDEX_DATA.JOB_ID IN (SELECT JOB_ID FROM OPERATOR_ACCESS WHERE OPERATOR_ACCESS.OPERATOR_ID=@opId) ";
                        if (when == "allTime")
                        {
                            timePanel.Visible = false;
                            if (what == "allSheets")
                            {
                                string cmdStringShort = cmdString.Substring(0,cmdString.Length - 6);
                                if (jobAbb == "Your Jobs") cmd = new SqlCommand(cmdString, con);
                                else
                                {
                                    cmd = new SqlCommand(cmdString + "AND INDEX_DATA.JOB_ID=@jobID", con);
                                    cmd.Parameters.AddWithValue("@jobID", jobID);
                                }
                                description.Text = "Operators' Indexes for all Time.";
                            }
                            else if (what == "printed")
                            {
                                if (jobAbb == "Your Jobs") cmd = new SqlCommand(cmdString + "AND PRINTED=1", con);
                                else
                                {
                                    cmd = new SqlCommand(cmdString + "AND INDEX_DATA.JOB_ID=@jobID AND PRINTED=1", con);
                                    cmd.Parameters.AddWithValue("@jobID", jobID);
                                }
                                description.Text = "Operators' Printed Indexes for all Time.";
                            }
                            else if (what == "notPrinted")
                            {
                                if (jobAbb == "Your Jobs") cmd = new SqlCommand(cmdString + "AND PRINTED=0", con);
                                else
                                {
                                    cmd = new SqlCommand(cmdString + "AND INDEX_DATA.JOB_ID=@jobID AND PRINTED=0", con);
                                    cmd.Parameters.AddWithValue("@jobID", jobID);
                                }
                                description.Text = "Operators' Unprinted Indexes for all Time.";
                            }
                        }
                        else if (when == "pickRange")
                        {
                            DateTime start = DateTime.Parse(Request.Form[from.UniqueID]);
                            DateTime end = DateTime.Parse(Request.Form[to.UniqueID]).Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                            if (start == default(DateTime))
                            {
                                string msg = "Please pick a start date.";
                                onScreenMsg(msg, "#ff3333;", "dateRange");
                                return;
                            }
                            if (end == default(DateTime))
                            {
                                string msg = "Please pick an end date.";
                                onScreenMsg(msg, "#ff3333;", "dateRange");
                                return;
                            }

                            if (what == "allSheets")
                            {
                                if (jobAbb == "Your Jobs") cmd = new SqlCommand(cmdString + "AND CREATION_TIME BETWEEN @start AND @end", con);
                                else
                                {
                                    cmd = new SqlCommand(cmdString + "AND INDEX_DATA.JOB_ID=@jobID AND CREATION_TIME BETWEEN @start AND @end", con);
                                    cmd.Parameters.AddWithValue("@jobID", jobID);
                                }
                                description.Text = "Operators' Indexes from " +start.Date.ToString("MM/dd/yyyy")+ " to " + end.Date.ToString("MM/dd/yyyy") + ".";
                            }
                            else if (what == "printed")
                            {
                                if (jobAbb == "Your Jobs") cmd = new SqlCommand(cmdString + "AND PRINTED=1 AND (CREATION_TIME BETWEEN @start AND @end)", con);
                                else
                                {
                                    cmd = new SqlCommand(cmdString + "AND INDEX_DATA.JOB_ID=@jobID AND PRINTED=1 AND (CREATION_TIME BETWEEN @start AND @end)", con);
                                    cmd.Parameters.AddWithValue("@jobID", jobID);
                                }
                                description.Text = "Operators' Printed Indexes From " +start.Date.ToString("MM/dd/yyyy")+ " to " + end.Date.ToString("MM/dd/yyyy") + ".";
                            }
                            else if (what == "notPrinted")
                            {
                                if (jobAbb == "Your Jobs") cmd = new SqlCommand(cmdString + "AND PRINTED=0 AND (CREATION_TIME BETWEEN @start AND @end)", con);
                                else
                                {
                                    cmd = new SqlCommand(cmdString + "AND INDEX_DATA.JOB_ID=@jobID AND PRINTED=0 AND (CREATION_TIME BETWEEN @start AND @end)", con);
                                    cmd.Parameters.AddWithValue("@jobID", jobID);
                                }
                                description.Text = "Operators' Unprinted Indexes from " +start.Date.ToString("MM/dd/yyyy")+ " to " + end.Date.ToString("MM/dd/yyyy") + ".";
                            }
                            cmd.Parameters.AddWithValue("@start", start);
                            cmd.Parameters.AddWithValue("@end", end);
                        }
                        cmd.Parameters.AddWithValue("@opId", opID);
                    }

                    con.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        using (DataSet ds = new DataSet())
                        {
                            da.Fill(ds);
                            if (ds.Tables.Count > 0)
                            {
                                //Persist the table in the Session object.
                                Session["TaskTable"] = ds.Tables[0];

                                indexeStatusGridView.DataSource = ds.Tables[0];
                                indexeStatusGridView.DataBind();
                            }
                            con.Close();

                            // Handling of whether any index was returned from DB
                            if (indexeStatusGridView.Rows.Count == 0)
                            {
                                description.Text = "No indexes found with the specified filter entries.";
                                description.Visible = true;
                                gridContainer.Visible = false;
                                gridHeader.Visible = false;
                                recordsPerPageLabel.Visible = false;
                                recordsPerPage.Visible = false;
                                sortOrder.Visible = false;
                            }
                            else
                            {
                                description.Visible = true;
                                gridContainer.Visible = true;
                                gridHeader.Visible = true;
                                recordsPerPageLabel.Visible = true;
                                recordsPerPage.Visible = true;
                                sortOrder.Visible = true;
                                indexeStatusGridView.Visible = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg;   
                if (ex.Message.Contains("valid DateTime"))
                {
                    msg = "All date fields required.";
                    onScreenMsg(msg, "#ff3333;", "dateRange");
                }
                else
                {
                    msg = "Error 45: Issue occured while attempting to process filter entries." ;
                    ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                    // Log the exception and notify system operators
                    ExceptionUtility.LogException(ex);
                    ExceptionUtility.NotifySystemOps(ex);
                }
            }
        }



        // RECORDS PER PAGE
        protected void onSelectedRecordsPerPage(object sender, EventArgs e)
        {
            try
            {
                if (recordsPerPage.SelectedValue != "all")
                {
                    indexeStatusGridView.AllowPaging = true;
                    indexeStatusGridView.PageSize = Int32.Parse(recordsPerPage.SelectedValue);
                }
                else indexeStatusGridView.AllowPaging = false;
                getIndexes(jobsFilter.SelectedValue, whoFilter.SelectedValue, whenFilter.SelectedValue, whatFilter.SelectedValue);
                sortOrder.Text = "Sorted By : CREATION_TIME ASC";
            }
            catch (Exception ex)
            {
                string msg = "Error 46: Issue occured while attempting to display requested number of records. Contact your system admin.";
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
        }



        // PREVENT LINE BREAKS IN GRIDVIEW
        protected void rowDataBound(object sender, GridViewRowEventArgs e)
        {
            try
            {
                // GIVE CUSTOM COLUMN NAMES
                if (e.Row.RowType == DataControlRowType.Header)
                {   
                    if (jobsFilter.SelectedValue != "Your Jobs")
                    {
                        // Get job ID of selected Job
                        int jobID = 0;
                        Dictionary<int, string> jobsDict = (Dictionary<int, string>)Session["jobsList"];
                        foreach (var jobTuple in jobsDict)
                        {
                            if (jobTuple.Value == jobsFilter.SelectedValue) jobID = jobTuple.Key;
                        }

                        // Retrieve job labels
                        var jobLabels = new Dictionary<string, string>();
                        using (SqlConnection con = Helper.ConnectionObj)
                        {
                            using (SqlCommand cmd = con.CreateCommand())
                            {
                                cmd.CommandText = "SELECT LABEL1, LABEL2, LABEL3, LABEL4, LABEL5 " +
                                                  "FROM JOB_CONFIG_INDEX " +
                                                  "WHERE JOB_ID = @jobID";
                                cmd.Parameters.AddWithValue("@jobID", jobID);
                                con.Open();
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    if (reader.HasRows)
                                    {
                                        while (reader.Read())
                                        {
                                            for (int i = 1; i <= 5; i++)
                                            {
                                                if (reader.GetValue(i - 1) != DBNull.Value)
                                                    jobLabels["label" + i] = (string)reader.GetValue(i - 1);
                                                else
                                                    jobLabels["label" + i] = string.Empty;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        ViewState["jobLabels"] = jobLabels;

                        // Rename columns headers & remove empty columns
                        for (int i = 3; i <= 7; i++)
                        {
                            if (jobLabels["label" + (i - 2)] == string.Empty)
                                e.Row.Cells[i].Visible = false;
                            else
                                ((LinkButton)e.Row.Cells[i].Controls[0]).Text = jobLabels["label" + (i - 2)].ToUpper();
                        }
                    }
                    ((LinkButton)e.Row.Cells[1].Controls[0]).Text = "OPERATOR";
                    ((LinkButton)e.Row.Cells[2].Controls[0]).Text = "INDEX";
                    ((LinkButton)e.Row.Cells[e.Row.Cells.Count - 2].Controls[0]).Text = "CREATION TIME";
                    // Set column borders & Prevent headers' line breaks
                    string colBorder = "border-left:1px solid #737373; border-right:1px solid #737373; white-space: nowrap;";
                    for (int i = 0; i < e.Row.Cells.Count; i++)
                        e.Row.Cells[i].Attributes.Add("style", colBorder);
                }

                // Set column borders & Prevent rows' line breaks
                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    // Remove empty columns
                    var jobLabels = (Dictionary<string, string>)ViewState["jobLabels"];
                    if (jobsFilter.SelectedValue != "Your Jobs")
                    {
                        for (int i = 3; i <= 7; i++)
                        {
                            if (jobLabels["label" + (i - 2)] == string.Empty)
                                e.Row.Cells[i].Visible = false;
                        }
                    }

                    // Set column borders
                    string colBorder = "border-width:1px 1px 1px 1px; border-style:solid; border-color:#cccccc; white-space: nowrap;"; 
                    for (int i=0; i<e.Row.Cells.Count; i++)
                        e.Row.Cells[i].Attributes.Add("style", colBorder);
                }

                if (e.Row.RowType == DataControlRowType.Pager)
                {
                    string colBorder = "border-left:1px solid #737373; border-right:1px solid #737373; white-space: nowrap;";
                    for (int i = 0; i < e.Row.Cells.Count; i++)
                        e.Row.Cells[i].Attributes.Add("style", colBorder);
                }
            }
            catch (Exception ex)
            {   
                string msg  = "Error 47: Issue occured while attempting to prevent line breaks within gridview. Contact system admin.";
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
        }



        // SORT ANY GRIDVIEW COLUMN. 
        protected void gridView_Sorting(object sender, GridViewSortEventArgs e)
        {
            try
            {
                //Retrieve the table from the session object.
                DataTable dt = Session["TaskTable"] as DataTable;

                if (dt != null)
                {
                    string label;
                    if (e.SortExpression.Contains("VALUE") && jobsFilter.SelectedValue != "Your Jobs")
                    {
                        string last = e.SortExpression.Substring(e.SortExpression.Length - 1, 1);
                        int lastInt = Convert.ToInt32(last);
                        var jobLabels = (Dictionary<string, string>)ViewState["jobLabels"];
                        label = jobLabels["label" + lastInt];
                        //string directSymbol = 
                    }
                    else if (e.SortExpression.Contains("NAME"))
                    {
                        label = "OPERATOR";
                    }
                    else if (e.SortExpression.Contains("BARCODE"))
                    {
                        label = "INDEX";
                    }
                    else
                        label = e.SortExpression;
                    //Sort the data.
                    string sortDirection = GetSortDirection(e.SortExpression);
                    dt.DefaultView.Sort = e.SortExpression + " " + sortDirection;
                    sortOrder.Text = "Sorted By : " + label.ToUpper() + "  " + sortDirection;
                    indexeStatusGridView.DataSource = Session["TaskTable"];
                    indexeStatusGridView.DataBind();
                }
            }
            catch (Exception ex)
            {
                string msg  = "Error 48: Issue occured while attempting to sort table. Contact system admin.";
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
        }


        // GET SORTING ORDER
        private string GetSortDirection(string column)
        {
            try
            {
                // By default, set the sort direction to ascending.
                string sortDirection = "ASC";

                // Retrieve the last column that was sorted.
                string sortExpression = ViewState["SortExpression"] as string;

                if (sortExpression != null)
                {
                    // Check if the same column is being sorted. Otherwise, the default value can be returned.
                    if (sortExpression == column)
                    {
                        string lastDirection = ViewState["SortDirection"] as string;
                        if ((lastDirection != null) && (lastDirection == "ASC"))
                        {
                            sortDirection = "DESC";
                        }
                    }
                }

                // Save new values in ViewState.
                ViewState["SortDirection"] = sortDirection;
                ViewState["SortExpression"] = column;

                return sortDirection;
            }
            catch (Exception ex)
            {
                string msg  = "Error 49: Issue occured while attempting to sort table. Contact system admin.";
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
                return "ASC";
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
            if (from == "dateRange") dateAlertMsg.Rows.Add(screenMsgRow);
        }

    }
}