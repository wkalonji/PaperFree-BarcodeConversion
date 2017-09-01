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
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    Session["jobsList"] = populateJobsList();
                    getIndexes("All Jobs", "meOnly", "allTime", "allSheets");
                    indexeStatusGridView.Visible = true;
                }

                // Make date fields entries persist
                from.Attributes.Add("readonly", "readonly");
                to.Attributes.Add("readonly", "readonly");

                // Reset gridview page
                Control c = Helper.GetPostBackControl(this.Page);
                if (c != null && (c.ID == "resetBtn" || c.ID == "whoFilter" || c.ID == "whenFilter" ||
                    c.ID == "whatFilter" || c.ID == "recordsPerPage")) indexeStatusGridView.PageIndex = 0;
            }
            catch (Exception ex)
            {
                string msg = "Issue occured while loading this page. Contact your system admin." + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "37");
            }
        }



        // 'RESET' CLICKED: RESET FILTER TO DEFAULT VALUES. FUNCTION
        protected void reset_Click(object sender, EventArgs e)
        {
            try
            {
                jobsFilter.SelectedValue = "All Jobs";
                whoFilter.SelectedValue = "meOnly";
                whenFilter.SelectedValue = "allTime";
                whatFilter.SelectedValue = "allSheets";
                getIndexes("All Jobs", "meOnly", "allTime", "allSheets");
                indexeStatusGridView.Visible = true;
                sortOrder.Text = "Sorted By : CREATION_TIME ASC";
            }
            catch (Exception ex)
            {
                string msg  = "Issue occured while attempting reset. Contact your system admin." + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "38");    
            }
        }



        // 'ALL JOBS' DROPDOWN: POPULATE JOBSFILTER. 
        private Dictionary<int, string> populateJobsList()
        {
            try
            {
                jobsFilter.Items.Clear();
                jobsFilter.Items.Add("All Jobs");

                // First, get current user id via name.
                string user = Environment.UserName;
                List<int> jobIdList = new List<int>();
                Dictionary<int, string> jobsDict = new Dictionary<int, string>();
                int opID = Helper.getUserId(user);
                if (opID == 0) return new Dictionary<int, string>();

                // Then, get all job IDs accessible to current user from OPERATOR_ACCESS.
                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "SELECT JOB_ID FROM OPERATOR_ACCESS WHERE OPERATOR_ACCESS.OPERATOR_ID = @userId";
                        cmd.Parameters.AddWithValue("@userId", opID);
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    int jobID = (int)reader.GetValue(0);
                                    jobIdList.Add(jobID);
                                }
                            }
                            else return new Dictionary<int, string>();
                        }
                    }
                }

                // Now, for each job ID, get corresponding job abbreviation.
                if (jobIdList.Count > 0)
                {
                    using (SqlConnection con = Helper.ConnectionObj)
                    {
                        con.Open();
                        foreach (var jobId in jobIdList)
                        {
                            using (SqlCommand cmd = con.CreateCommand())
                            {
                                cmd.CommandText = "SELECT ABBREVIATION FROM JOB WHERE ID = @job";
                                cmd.Parameters.AddWithValue("@job", jobId);
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    if (reader.HasRows)
                                    {
                                        while (reader.Read())
                                        {
                                            string jobAbb = (string)reader.GetValue(0);
                                            jobsFilter.Items.Add(jobAbb);
                                            jobsDict.Add(jobId, jobAbb);
                                        }
                                        jobsFilter.AutoPostBack = true;
                                    }
                                    else return new Dictionary<int, string>();
                                }
                            }
                        }
                    }
                }
                else
                {
                    return new Dictionary<int, string>();
                }
                return jobsDict;
            }
            catch (Exception ex)
            {
                string msg  = "Issue occured while attempting to populate jobs dropdown. Contact system admin." + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "39");
                return new Dictionary<int, string>();
            }
        }



        // 'JOBSFILTER', 'WHO' & 'WHAT' FILTER CHANGED.
        protected void onSelectedChange(object sender, EventArgs e)
        {
            try
            {
                getIndexes(jobsFilter.SelectedValue, whoFilter.SelectedValue, whenFilter.SelectedValue, whatFilter.SelectedValue);
            }
            catch (Exception ex)
            {
                string msg = "Issue occured while attempting to process filter entries. Contact system admin." + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "40");
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
                }
                else
                {
                    timePanel.Visible = true;
                    gridHeader.Visible = false;
                    indexeStatusGridView.Visible = false;
                }
            }
            catch (Exception ex)
            {   
                string msg  = "Issue occured while attempting to process filter entries. Contact system admin." + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "41");
            }
        }



        // 'SUBMIT' CLICKED: DATE FIELDS ENTERED.
        protected void submit_Click(object sender, EventArgs e)
        {
            try
            {
                getIndexes(jobsFilter.SelectedValue, whoFilter.SelectedValue, whenFilter.SelectedValue, whatFilter.SelectedValue);
            }
            catch (Exception ex)
            {   
                string msg  = "Issue occured while attempting to process filter entries. Contact system admin." + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "42");
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
                string msg  = "Issue occured while attempting to change page & process filter entries. Contact system admin." + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "43");
            }
        }



        // GET FILTERED INDEXES. 
        protected void getIndexes(string jobAbb, string who, string when, string what)
        {
            try
            {
                Page.Validate();
                if (!Page.IsValid) return;

                // First, populate 'All Jobs' filter
                Dictionary<int, string> jobsDict = (Dictionary<int, string>)Session["jobsList"];

                string user = Environment.UserName;
                int opID = Helper.getUserId(user);
                if (opID == 0 || jobsDict.Count == 0)
                {
                    description.Text = "No indexes found with the specified filter entries.";
                    recordsPerPageLabel.Visible = false;
                    recordsPerPage.Visible = false;
                    sortOrder.Visible = false;
                    return;
                }

                // Get job ID of selected Job
                int jobID = 0;
                foreach (var jobTuple in jobsDict)
                {
                    if (jobTuple.Value == jobAbb) jobID = jobTuple.Key;
                }

                SqlCommand cmd = new SqlCommand();
                string cmdString = "SELECT NAME, BARCODE, VALUE1, VALUE2, VALUE3, VALUE4, VALUE5, CREATION_TIME, PRINTED " +
                                        "FROM INDEX_DATA " +
                                        "INNER JOIN OPERATOR ON INDEX_DATA.OPERATOR_ID=OPERATOR.ID WHERE ";
                using (SqlConnection con = Helper.ConnectionObj)
                {
                    if (who == "meOnly")
                    {
                        if (when == "allTime")
                        {
                            timePanel.Visible = false;
                            if (what == "allSheets")
                            {
                                if (jobAbb == "All Jobs") cmd = new SqlCommand(cmdString + "OPERATOR_ID=@opId", con);
                                else
                                {
                                    cmd = new SqlCommand(cmdString + "OPERATOR_ID=@opId AND JOB_ID=@jobID", con);
                                    cmd.Parameters.AddWithValue("@jobID", jobID);
                                }
                                description.Text = "Your Indexes for all Time.";
                            }
                            else if (what == "printed")
                            {
                                if (jobAbb == "All Jobs") cmd = new SqlCommand(cmdString + "OPERATOR_ID=@opId AND PRINTED=1", con);
                                else
                                {
                                    cmd = new SqlCommand(cmdString + "OPERATOR_ID=@opId AND JOB_ID=@jobID AND PRINTED=1", con);
                                    cmd.Parameters.AddWithValue("@jobID", jobID);
                                }
                                description.Text = "Your Printed Indexes for all Time.";
                            }
                            else if (what == "notPrinted")
                            {
                                if (jobAbb == "All Jobs") cmd = new SqlCommand(cmdString + "OPERATOR_ID=@opId AND PRINTED=0", con);
                                else
                                {
                                    cmd = new SqlCommand(cmdString + "OPERATOR_ID=@opId AND JOB_ID=@jobID AND PRINTED=0", con);
                                    cmd.Parameters.AddWithValue("@jobID", jobID);
                                }
                                description.Text = "Your Unprinted Indexes for all Time.";
                            }
                        }
                        else if (when == "pickRange")
                        {
                            DateTime start = DateTime.Parse(Request.Form[from.UniqueID]);
                            DateTime end = DateTime.Parse(Request.Form[to.UniqueID]);
                            if (start == default(DateTime))
                            {
                                string msg = "Please pick a start date.";
                                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                return;
                            }
                            if (end == default(DateTime))
                            {
                                string msg = "Please pick an end date.";
                                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                return;
                            }

                            if (what == "allSheets")
                            {
                                if (jobAbb == "All Jobs") cmd = new SqlCommand(cmdString + "OPERATOR_ID=@opId AND (CREATION_TIME BETWEEN @start AND @end)", con);
                                else
                                {
                                    cmd = new SqlCommand(cmdString + "OPERATOR_ID=@opId AND JOB_ID=@jobID AND (CREATION_TIME BETWEEN @start AND @end)", con);
                                    cmd.Parameters.AddWithValue("@jobID", jobID);
                                }
                                description.Text = "Your Indexes from " + start + " to " + end + ".";
                            }
                            else if (what == "printed")
                            {
                                if (jobAbb == "All Jobs") cmd = new SqlCommand(cmdString + "OPERATOR_ID=@opId AND PRINTED=1 AND (CREATION_TIME BETWEEN @start AND @end)", con);
                                else
                                {
                                    cmd = new SqlCommand(cmdString + "OPERATOR_ID=@opId AND JOB_ID=@jobID AND PRINTED=1 AND (CREATION_TIME BETWEEN @start AND @end)", con);
                                    cmd.Parameters.AddWithValue("@jobID", jobID);
                                }
                                description.Text = "Your Printed Indexes from " + start + " to " + end + ".";
                            }
                            else if (what == "notPrinted")
                            {
                                if (jobAbb == "All Jobs") cmd = new SqlCommand(cmdString + "OPERATOR_ID=@opId AND PRINTED=0 AND (CREATION_TIME BETWEEN @start AND @end)", con);
                                else
                                {
                                    cmd = new SqlCommand(cmdString + "OPERATOR_ID=@opId AND JOB_ID=@jobID AND PRINTED=0 AND (CREATION_TIME BETWEEN @start AND @end)", con);
                                    cmd.Parameters.AddWithValue("@jobID", jobID);
                                }
                                description.Text = "Your Unprinted Indexes from " + start + " to " + end + ".";
                            }
                            cmd.Parameters.AddWithValue("@start", start);
                            cmd.Parameters.AddWithValue("@end", end);
                        }
                        cmd.Parameters.AddWithValue("@opId", opID);
                    }
                    else if (who == "everyone")
                    {
                        if (when == "allTime")
                        {
                            timePanel.Visible = false;
                            if (what == "allSheets")
                            {
                                string cmdStringShort = cmdString;
                                if (jobAbb == "All Jobs") cmd = new SqlCommand(cmdStringShort.Substring(0, cmdString.Length - 7), con);
                                else
                                {
                                    cmd = new SqlCommand(cmdStringShort.Substring(0, cmdString.Length - 7) + " WHERE JOB_ID=@jobID", con);
                                    cmd.Parameters.AddWithValue("@jobID", jobID);
                                }
                                description.Text = "Operators' Indexes for all Time.";
                            }
                            else if (what == "printed")
                            {
                                if (jobAbb == "All Jobs") cmd = new SqlCommand(cmdString + "PRINTED=1", con);
                                else
                                {
                                    cmd = new SqlCommand(cmdString + "JOB_ID=@jobID AND PRINTED=1", con);
                                    cmd.Parameters.AddWithValue("@jobID", jobID);
                                }
                                description.Text = "Operators' Printed Indexes for all Time.";
                            }
                            else if (what == "notPrinted")
                            {
                                if (jobAbb == "All Jobs") cmd = new SqlCommand(cmdString + "PRINTED=0", con);
                                else
                                {
                                    cmd = new SqlCommand(cmdString + "JOB_ID=@jobID AND PRINTED=0", con);
                                    cmd.Parameters.AddWithValue("@jobID", jobID);
                                }
                                description.Text = "Operators' Unprinted Indexes for all Time.";
                            }
                        }
                        else if (when == "pickRange")
                        {
                            DateTime start = DateTime.Parse(Request.Form[from.UniqueID]);
                            DateTime end = DateTime.Parse(Request.Form[to.UniqueID]);
                            if (start == default(DateTime))
                            {
                                string msg = "Please pick a start date.";
                                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                return;
                            }
                            if (end == default(DateTime))
                            {
                                string msg = "Please pick an end date.";
                                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                return;
                            }

                            if (what == "allSheets")
                            {
                                if (jobAbb == "All Jobs") cmd = new SqlCommand(cmdString + "CREATION_TIME BETWEEN @start AND @end", con);
                                else
                                {
                                    cmd = new SqlCommand(cmdString + "JOB_ID=@jobID AND CREATION_TIME BETWEEN @start AND @end", con);
                                    cmd.Parameters.AddWithValue("@jobID", jobID);
                                }
                                description.Text = "Operators' Indexes from " + start + " to " + end + ".";
                            }
                            else if (what == "printed")
                            {
                                if (jobAbb == "All Jobs") cmd = new SqlCommand(cmdString + "PRINTED=1 AND (CREATION_TIME BETWEEN @start AND @end)", con);
                                else
                                {
                                    cmd = new SqlCommand(cmdString + "JOB_ID=@jobID AND PRINTED=1 AND (CREATION_TIME BETWEEN @start AND @end)", con);
                                    cmd.Parameters.AddWithValue("@jobID", jobID);
                                }
                                description.Text = "Operators' Printed Indexes From " + start + " to " + end + ".";
                            }
                            else if (what == "notPrinted")
                            {
                                if (jobAbb == "All Jobs") cmd = new SqlCommand(cmdString + "PRINTED=0 AND (CREATION_TIME BETWEEN @start AND @end)", con);
                                else
                                {
                                    cmd = new SqlCommand(cmdString + "JOB_ID=@jobID AND PRINTED=0 AND (CREATION_TIME BETWEEN @start AND @end)", con);
                                    cmd.Parameters.AddWithValue("@jobID", jobID);
                                }
                                description.Text = "Operators' Unprinted Indexes from " + start + " to " + end + ".";
                            }
                            cmd.Parameters.AddWithValue("@start", start);
                            cmd.Parameters.AddWithValue("@end", end);
                        }
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
                                gridHeader.Visible = true;
                                recordsPerPageLabel.Visible = false;
                                recordsPerPage.Visible = false;
                                sortOrder.Visible = false;
                            }
                            else
                            {
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
                    msg = "Date fields required!";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                }
                else
                {
                    msg = "Issue occured while attempting to process filter entries." + Environment.NewLine + ex.Message;
                    System.Windows.Forms.MessageBox.Show(msg, "Error 45");
                }
            }
        }



        // RECORDS PER PAGE
        protected void onSelectedRecordsPerPage(object sender, EventArgs e)
        {
            try
            {
                indexeStatusGridView.PageSize = Int32.Parse(recordsPerPage.SelectedValue);
                getIndexes(jobsFilter.SelectedValue, whoFilter.SelectedValue, whenFilter.SelectedValue, whatFilter.SelectedValue);
                sortOrder.Text = "Sorted By : CREATION_TIME ASC";
            }
            catch (Exception ex)
            {
                string msg = "Issue occured while attempting to display requested number of records. Contact your system admin." + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "46");
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
                    //e.Row.Cells[1].Text = "OPERATOR";
                    //e.Row.Cells[2].Text = "JOB_ID";
                    //e.Row.Cells[3].Text = "INDEX";
                }
                // Set column borders & Prevent line breaks
                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    string colBorder = "border-left:1px solid #cccccc; border-right:1px solid #cccccc; white-space: nowrap;";
                    for (int i=0; i<e.Row.Cells.Count; i++)
                        e.Row.Cells[i].Attributes.Add("style", colBorder);
                }
            }
            catch (Exception ex)
            {   
                string msg  = "Issue occured while attempting to prevent line breaks within gridview. Contact system admin.." + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "47");
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
                    //Sort the data.
                    dt.DefaultView.Sort = e.SortExpression + " " + GetSortDirection(e.SortExpression);
                    sortOrder.Text = "Sorted By : " + dt.DefaultView.Sort;
                    indexeStatusGridView.DataSource = Session["TaskTable"];
                    indexeStatusGridView.DataBind();
                }
            }
            catch (Exception ex)
            {
                string msg  = "Issue occured while attempting to sort table. Contact system admin." + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "48");    
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
                string msg  = "Issue occured while attempting to sort table. Contact system admin." + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "49");
                return "ASC";
            }
        }
    }
}