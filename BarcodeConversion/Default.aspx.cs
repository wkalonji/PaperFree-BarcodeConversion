using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Globalization;
using BarcodeConversion.App_Code;
using System.Text.RegularExpressions;

namespace BarcodeConversion
{

    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                label1Box.Focus();

                // 'JOB ABBREVIATION' DROPDOWN FILL: POPULATE DROPDOWN
                selectJob_Click(new object(), new EventArgs());
            }
            setDropdownColor();
        }


        // 'JOB ABBREVIATION' DROPDOWN ITEM SELECTION: SET & DISPLAY CONTROLS OF SELECTED JOB. 
        protected void onJobSelect(object sender, EventArgs e)
        {
            try
            {
                // Set stage
                indexSavedMsg.Visible = false;
                generateIndexSection.Visible = false;
                indexCreationSection.Visible = false;
                for (int i=1; i<=5; i++)
                {
                    Label l = this.Master.FindControl("MainContent").FindControl("LABEL" + i) as Label;
                    l.Visible = false;
                    TextBox t = this.Master.FindControl("MainContent").FindControl("label" + i + "Box") as TextBox;
                    t.Visible = false;
                    t.Text = string.Empty;
                }

                // Make sure a job is selected
                if (this.selectJob.SelectedValue != "Select")
                {
                    // First, get selected job ID.
                    int jobID = getJobId(this.selectJob.SelectedValue);

                    if (jobID == 0)
                    {
                        string msg = "Selected job not found. Contact system admin.";
                        System.Windows.Forms.MessageBox.Show(msg, "Error 02");
                        selectJob.SelectedValue = "Select";
                        return;
                    }

                    // Then, check whether that job is configured, if so, display controls.
                    using (SqlConnection con = Helper.ConnectionObj)
                    {
                        using (SqlCommand cmd = con.CreateCommand())
                        {
                            cmd.CommandText = "SELECT JOB_ID, LABEL1, REGEX1, ALERT1, LABEL2, REGEX2, ALERT2, LABEL3, REGEX3, ALERT3," +
                                              "LABEL4, REGEX4, ALERT4, LABEL5, REGEX5, ALERT5 FROM JOB_CONFIG_INDEX WHERE JOB_ID = @jobID";
                            cmd.Parameters.AddWithValue("@jobID", jobID);
                            con.Open();
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    indexCreationSection.Visible = true;
                                    int i = 1;
                                    int j = 1;
                                    var regexList = new List<Tuple<string, string, string>> { };
                                    // Set & display controls
                                    while (reader.Read())
                                    {
                                        while (i <= 13)
                                        {
                                            if (reader.GetValue(i) != DBNull.Value)
                                            {
                                                var tuple = Tuple.Create("","","");
                                                if (reader.GetValue(i + 1) != DBNull.Value)
                                                    tuple = Tuple.Create((string)reader.GetValue(i), (string)reader.GetValue(i + 1), (string)reader.GetValue(i + 2));
                                                else
                                                    tuple = Tuple.Create((string)reader.GetValue(i), "", "");
                                                regexList.Add(tuple);
                                                string text = (string)reader.GetValue(i);
                                                text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLower());
                                                Label l = this.Master.FindControl("MainContent").FindControl("LABEL" + j) as Label;
                                                l.Text = text + " :";
                                                l.Visible = true;
                                                TextBox t = this.Master.FindControl("MainContent").FindControl("label" + j + "Box") as TextBox;
                                                t.Visible = true;
                                                if (i == 1) t.Focus();
                                                j += 1;
                                            }
                                            i += 3;
                                        }
                                        ViewState["regexList"] = regexList;
                                    }
                                    generateIndexSection.Visible = true;
                                }
                                else
                                {
                                    string msg = "The \"" + selectJob.SelectedValue + "\" job that you selected has not yet been configured by your system admin." 
                                                + " Only jobs in red can be processed.";
                                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                    selectJob.SelectedValue = "Select";
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "Issue occured while attempting to retrieve selected job's form controls. Contact system admin." + Environment.NewLine + ex.Message;
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('Error 04: " + msg + "');", true);
            }
        }      


        // GENERATE INDEX AND BARCODE FROM FORM DATA.
        private void generateBarcode()
        {
            try
            {
                if (!Page.IsValid) return;

                // First, get list of all entries.
                indexSetPrintedMsg.Visible = false;
                List<EntryContent> allEntriesList = new List<EntryContent>();
                allEntriesList = getEntries();
                ViewState["allEntriesList"] = allEntriesList;

                // If Index form not filled
                if (allEntriesList.Count == 0)
                {
                    string msg = "All fields are required!";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    return;
                }
                else
                {   
                    // Check Regex rules


                    // Making the Index string
                    string year = DateTime.Now.ToString("yy");
                    JulianCalendar jc = new JulianCalendar();
                    string julianDay = jc.GetDayOfYear(DateTime.Now).ToString();
                    string time = DateTime.Now.ToString("HHmmssfff");
                    
                    ViewState["allEntriesConcat"] = selectJob.SelectedValue.ToUpper() + year + julianDay + time;
                }
                string indexString = (string)ViewState["allEntriesConcat"];
                indexSavedMsg.Visible = false;
                generateIndexSection.Visible = true;

                // Convert index to barcode
                // imgBarcode.ImageUrl = string.Format("ShowCode39Barcode.ashx?code={0}&ShowText={1}&Thickness={2}",indexString,showTextValue, 1);
            }
            catch (Exception ex)
            {
                string msg = "Issue occured while attempting to generate Index. Contact system admin." + Environment.NewLine + ex.Message;
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('Error 05a: " + msg + "');", true);
            }
        }



        // 'SAVE INDEX' CLICKED: SAVING INDEX INTO DB.
        protected void saveIndex_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                generateBarcode();
                // First, get current user id via name.
                string user = Environment.UserName;
                int opID = Helper.getUserId(user);
                if (opID == 0)
                {
                    string msg = "Error 05: Couldn't identify active user. Contact system admin.";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    return;
                }

                // Then, get selected job id
                int jobID = getJobId(this.selectJob.SelectedValue);
                if (jobID == 0)
                {
                    string msg = "Error 06: Couldn't identify the selected job. Contact system admin.";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    selectJob.SelectedValue = "Select";
                    return;
                }

                // Saving
                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "INSERT INTO INDEX_DATA (JOB_ID, BARCODE, VALUE1, VALUE2, " +
                                            "VALUE3, VALUE4, VALUE5, OPERATOR_ID, CREATION_TIME, PRINTED) VALUES(@jobId, @barcodeIndex," +
                                            " @val1, @val2, @val3, @val4, @val5, @opId, @time, @printed)";
                        cmd.Parameters.AddWithValue("@jobID", jobID);
                        cmd.Parameters.AddWithValue("@barcodeIndex", ViewState["allEntriesConcat"]);
                        var regexList = (List<Tuple<string, string, string>>)ViewState["regexList"];
                        for (int i=1; i<=5; i++)
                        {   
                            TextBox c = this.Master.FindControl("MainContent").FindControl("label" + i + "Box") as TextBox;
                            if (c.Visible == true)
                            {
                                // Check regex rules
                                if (regexList[i - 1].Item2 != string.Empty)
                                {
                                    string label = regexList[i - 1].Item1;
                                    string pattern = @regexList[i - 1].Item2;
                                    Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
                                    if (!r.IsMatch(c.Text))
                                    {
                                        string msg = label + ": " + regexList[i - 1].Item3;
                                        ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                        c.Focus();
                                        return;
                                    }
                                }
                                cmd.Parameters.AddWithValue("@val" + i, c.Text);
                            }
                            else { cmd.Parameters.AddWithValue("@val" + i, DBNull.Value); }
                        }
                        cmd.Parameters.AddWithValue("@opId", opID);
                        cmd.Parameters.AddWithValue("@time", DateTime.Now);
                        cmd.Parameters.AddWithValue("@printed", 0);
                        con.Open();
                        if (cmd.ExecuteNonQuery() == 1)
                        {
                            indexSavedMsg.Visible = true;
                            ClientScript.RegisterStartupScript(this.GetType(), "fadeoutOperation", "FadeOut();", true);
                            clearFields();
                        }
                        else
                        {
                            string msg = "Error-07: Issue occured while attempting to save. Contact system admin.";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Violation of UNIQUE KEY"))
                {
                    string msg = "Error 08: The Index you are trying to save already exists! Click 'Generate Index' button to generate a new index.";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                }
                else
                {
                    string msg = "Issue occured while attempting to save index. Contact system admin." + Environment.NewLine + ex.Message;
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('Error 09: " + msg + "');", true);
                }
            }
        }


        // 'SAVE & PRINT' CLICKED: SAVE THEN PRINT INDEX. 
        protected void saveAndPrint_Click(object sender, EventArgs e)
        {   
            try
            {
                // First, save index
                saveIndex_Click(new object(), new EventArgs());

                // Clear page
                formPanel.Visible = false;
                indexSavedMsg.Visible = false;
                Image imgBarcode = new Image();

                // Write Index sheet page content
                string indexString = (string)ViewState["allEntriesConcat"];
                imgBarcode.ImageUrl = string.Format("ShowCode39BarCode.ashx?code={0}&ShowText=1&Height=50", indexString.PadLeft(8, '0'));

                Response.Write(
                    "<div id = 'pageToPrint' style='margin-top:-50px;'>" +
                        "<div>" +
                            "<div style='font-size:25px; font-weight:500;'>" +
                                "<img src='" + imgBarcode.ImageUrl + "' height='160px' width='500px' style='margin-top:0px; '> " +
                            "</div>" +
                            "<div style='font-size:25px; font-weight:500; text-align:right;' >" +
                                "<img src='" + imgBarcode.ImageUrl + "' height='160px' width='500px' style='margin-top:250px; margin-right:-180px;' class='rotate'> " +
                            "</div>" +
                        "</div>" +

                        "<table style='margin-top:250px; margin-bottom:580px; margin-left:40px;'>" +
                            "<tr>" +
                                "<td style='font-size:25px; font-weight:500;'> Index String: </td>" +
                                "<td style='font-size:25px; font-weight:500; padding-left:15px;'>" + indexString.ToUpper() + "</td>" +
                            "</tr>"
                 );
                List<EntryContent> allEntriesList = new List<EntryContent>();
                allEntriesList = (List<EntryContent>)ViewState["allEntriesList"];

                foreach (var entry in allEntriesList)
                {
                    Response.Write(
                        "<tr>" +
                            "<td style='font-size:25px; font-weight:500;'>" + entry.labelText + "</td>" +
                            "<td style='font-size:25px; font-weight:500; padding-left:15px;'>" + entry.text.ToUpper() + "</td>" +
                        "</tr>"
                    );
                }
                Response.Write(
                            "<tr>" +
                                "<td style='font-size:25px; font-weight:500;'>Date Created: </td>" +
                                "<td style='font-size:25px; font-weight:500; padding-left:15px;'>" + DateTime.Now + "</td>" +
                            "</tr>" +
                        "</table >" +
                    "</div>"
                );

                // Finally, Print Index sheet.
                ClientScript.RegisterStartupScript(this.GetType(), "PrintOperation", "printing();", true);
            }
            catch (Exception ex)
            {
                string msg = "Issue occured while attempting to setup the printing job. Contact system admin." + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "Error 10");
            }     
        }



        // SET INDEX AS PRINTED IN DB. 
        protected void setIndexAsPrinted()
        {
            try
            {
                if (!Page.IsValid) return;
                var counter = 0;
                string indexString = (string)ViewState["allEntriesConcat"];

                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "UPDATE INDEX_DATA SET PRINTED = @printed WHERE BARCODE = @barcodeIndex";
                        cmd.Parameters.AddWithValue("@printed", 1);
                        cmd.Parameters.AddWithValue("@barcodeIndex", indexString);
                        con.Open();
                        if (cmd.ExecuteNonQuery() == 1)
                        {
                            counter++;
                            indexSetPrintedMsg.Visible = true;
                            indexSavedMsg.Visible = false;
                        }
                        else
                        {
                            string msg = "Error 11: Index saved, but issue occured while attempting to set it to PRINTED. Contact system admin.";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                        }

                        // Confirmation msg & back to unprinted indexes gridview
                        if (counter == 1)
                        {
                            ClientScript.RegisterStartupScript(this.GetType(), "fadeoutOperation", "FadeOut2();", true);
                        }
                        else
                        {
                            string msg = "Error 12: Index saved, but issue occured while attempting to set it to PRINTED. Contact system admin.";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "Index saved, but issue occured while attempting to set it to PRINTED. Contact system admin." + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "Error 13");
            }
        }



        // SET INDEX AS PRINTED AFTER INDEX SHEET PRINTOUT 
        protected void setAsPrinted_Click(object sender, EventArgs e)
        {
            try
            {
                formPanel.Visible = true;
                setIndexAsPrinted();
            }
            catch (Exception ex)
            {
                string msg = "Issue occured while attempting to set job as PRINTED. Contact system admin." + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "Error 14");
            }
        }



        // BACK TO BLANK FORM
        protected void backToForm_Click(object sender, EventArgs e)
        {
            formPanel.Visible = true;
        }



        // (HIDDEN) 'GENERATE JOBS' CLICKED: GET YOUR ACCESSIBLE JOBS.
        protected void selectJob_Click(Object sender, EventArgs e)
        {
            try
            {
                // First, get current user id via name.
                string user = Environment.UserName;
                List<int> jobIdList = new List<int>();
                noJobsFound.Visible = false;
                int jobID = 0;
                int opID = Helper.getUserId(user);
                if (opID == 0)
                {
                    noJobsFound.Visible = true;
                    return;
                }

                // Then, get all job IDs accessible to current operator from OPERATOR_ACCESS.
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
                                    jobID = (int)reader.GetValue(0);
                                    jobIdList.Add(jobID);
                                }
                            }
                            else
                            {   
                                // If no accessible jobs found, display message.
                                noJobsFound.Visible = true;
                                return;
                            }
                        }
                    }
                }

                // Now, for each job ID, get corresponding job abbreviation.
                if (jobIdList.Count > 0)
                {
                    foreach (var id in jobIdList)
                    {
                        using (SqlConnection con = Helper.ConnectionObj)
                        {
                            using (SqlCommand cmd = con.CreateCommand())
                            {
                                cmd.CommandText = "SELECT ABBREVIATION FROM JOB WHERE ID = @job";
                                cmd.Parameters.AddWithValue("@job", id);
                                con.Open();
                                var result = cmd.ExecuteScalar();
                                if (result != null)
                                {
                                    // Fill dropdown list
                                    string jobAbb = result.ToString();
                                    selectJob.Items.Add(jobAbb);
                                }
                                else
                                {
                                    string msg = "Error 15: Issue occured while attempting to retrieve jobs accessible to you. Contact system admin.";
                                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                                    return;
                                }
                            }
                        }
                    }
                }
                else
                {
                    string msg = "Error 16: Issue occured while attempting to retrieve jobs accessible to you. Contact system admin.";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    return;
                }
            }
            catch (Exception ex)
            {
                string msg = "Issue occured while attempting to retrieve jobs accessible to you. Contact system admin." + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "Error 17");
            }
        }



        // SET COLOR FOR DROPDOWN CONFIGURED JOB ITEMS.
        private void setDropdownColor()
        {
            try
            {
                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "SELECT ABBREVIATION " +
                                          "FROM JOB " +
                                          "INNER JOIN JOB_CONFIG_INDEX ON JOB.ID = JOB_CONFIG_INDEX.JOB_ID";
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
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
                string msg = "Issue occured while attempting to color configured jobs in dropdown. Contact system admin." + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "Error 18");
            }
        }


        // GET JOB ID VIA SELECTED JOB ABBREV.
        private int getJobId(string jobAbb)
        {
            try 
            {
                int jobID = 0;
                using(SqlConnection con = Helper.ConnectionObj) 
                {
                    using (SqlCommand cmd = con.CreateCommand()) 
                    {
                        cmd.CommandText = "SELECT ID FROM JOB WHERE ABBREVIATION = @abb";
                        cmd.Parameters.AddWithValue("@abb", jobAbb);
                        con.Open();
                        var result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            jobID = (int) result; 
                            return jobID;
                        }
                        else return jobID;
                    }
                }  
            } 
            catch(Exception ex) 
            {
                string msg = "Issue occured while attempting to identify the selected Job. Contact system admin." + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "Error 01");
                return 0;
            }
        }



        // CLEAR TEXT FIELDS.
        private void clearFields()
        {
            try
            {
                List<TextBox> textBoxList = new List<TextBox>();
                textBoxList.Add(label1Box);
                textBoxList.Add(label2Box);
                textBoxList.Add(label3Box);
                textBoxList.Add(label4Box);
                textBoxList.Add(label5Box);

                foreach (var textBox in textBoxList)
                {
                    if (textBox.Visible == true) textBox.Text = string.Empty;
                }
                foreach (var textBox in textBoxList)
                {
                    if (textBox.Visible == true)
                    {
                        textBox.Focus();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "Issue occured while attempting to clear text fields. Contact system admin." + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "Error 19");
            }
        }



        // ALL ENTRIES.
        private List<EntryContent> getEntries()
        {
            try
            {
                List<EntryControl> controlList = new List<EntryControl>();
                List<EntryContent> contentList = new List<EntryContent>();
                EntryControl entry1 = new EntryControl(LABEL1, label1Box);
                controlList.Add(entry1);
                EntryControl entry2 = new EntryControl(LABEL2, label2Box);
                controlList.Add(entry2);
                EntryControl entry3 = new EntryControl(LABEL3, label3Box);
                controlList.Add(entry3);
                EntryControl entry4 = new EntryControl(LABEL4, label4Box);
                controlList.Add(entry4);
                EntryControl entry5 = new EntryControl(LABEL5, label5Box);
                controlList.Add(entry5);

                foreach (var control in controlList)
                {
                    if (control.textBox.Visible == true && control.textBox.Text == string.Empty)
                    {
                        // Return empty list
                        control.textBox.Focus();
                        contentList = new List<EntryContent>();
                        return contentList;
                    }
                    else if (control.textBox.Visible == true && control.textBox.Text != string.Empty)
                    {
                        EntryContent sampleEntry = new EntryContent(control.label.Text, control.textBox.Text);
                        contentList.Add(sampleEntry);
                    }
                }
                return contentList;
            }
            catch (Exception ex)
            {
                string msg = "Issue occured while retrieving entered entries. Contact system admin." +
                                    System.Environment.NewLine + ex.Message;
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('Error 03: " + msg + "');", true);
                return new List<EntryContent>();
            }
        }    
    }
}

