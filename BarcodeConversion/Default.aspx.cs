using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Globalization;
using BarcodeConversion.App_Code;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic.FileIO;
using System.IO;
using System.Linq;
using System.Data;

namespace BarcodeConversion
{

    public partial class _Default : Page
    {
        public void Page_Init(object o, EventArgs e)
        {
            Page.MaintainScrollPositionOnPostBack = true;
        }

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
                ViewState["manualEntries"] = null;
                ViewState["fileContent"] = null;
                generateIndexSection.Visible = false;
                indexCreationSection.Visible = false;
                uploadSuccess.Text = "";
                viewContentBtn.Text = "View";
                viewContentBtn.Visible = false;
                saveIndexesBtn.Visible = false;
                printIndexesBtn.Visible = false;
                GridView1.Visible = false;

                for (int i = 1; i <= 5; i++)
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
                            cmd.CommandText = "SELECT JOB_ID, LABEL1, REGEX1, ALERT1, LABEL2, REGEX2, ALERT2, LABEL3," + 
                                                      "REGEX3, ALERT3,LABEL4, REGEX4, ALERT4, LABEL5, REGEX5, ALERT5 " + 
                                              "FROM JOB_CONFIG_INDEX " + 
                                              "WHERE JOB_ID = @jobID";
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
                                            if (reader.GetValue(i) != DBNull.Value) // If label i is set
                                            {
                                                var tuple = Tuple.Create("", "", "");
                                                string label = (string)reader.GetValue(i);
                                                string regex = string.Empty;
                                                string alert = string.Empty;
                                                if (reader.GetValue(i + 1) != DBNull.Value) // if regex i is set
                                                {
                                                    regex = (string)reader.GetValue(i + 1);
                                                    alert = (string)reader.GetValue(i + 2);
                                                }
                                                tuple = Tuple.Create(label, regex, alert);

                                                regexList.Add(tuple);
                                                string text = label;
                                                text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLower());
                                                Label l = this.Master.FindControl("MainContent").FindControl("LABEL" + j) as Label;
                                                l.Text = text + " :";
                                                l.Visible = true;
                                                TextBox t = this.Master.FindControl("MainContent").FindControl("label" + j + "Box") as TextBox;
                                                t.Visible = true;
                                                
                                                if (i == 1)
                                                {
                                                    t.Attributes["placeholder"] = " Required";
                                                    t.Focus();
                                                }
                                                else
                                                {
                                                    if (regex == string.Empty) t.Attributes["placeholder"] = " Optional";
                                                    else t.Attributes["placeholder"] = " Required";
                                                }
                                                j += 1;
                                            }
                                            i += 3;
                                        }
                                        Session["regexList"] = regexList; // Contains (label,regex,alert) for each label set at Index Config Section
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



        // 'UPLOAD' CLICKED: UPLOAD CSV INDEX DATA FILE.
        protected void upload_Click(object sender, EventArgs e)
        {
            if ((File1.PostedFile != null) && (File1.PostedFile.ContentLength > 0))
            {
                // Clear previous upload displays
                uploadSuccess.Text = "";
                viewContentBtn.Text = "View";
                viewContentBtn.Visible = false;
                saveIndexesBtn.Visible = false;
                printIndexesBtn.Visible = false;
                GridView1.Visible = false;

                // Check file extension first
                String extension = Path.GetExtension(File1.PostedFile.FileName);
                if (extension.ToLower() != ".csv")
                {
                    var error = new TableCell();
                    var errorRow = new TableRow();
                    error.Text = "Only csv files are allowed!";
                    error.Attributes["style"] = "color:red;";
                    errorRow.Cells.Add(error);
                    fileEntryMsg.Rows.Add(errorRow);
                    return;
                }

                // Upload file
                string fileName = Path.GetFileName(File1.PostedFile.FileName);
                string SaveLocation = Server.MapPath("App_Data") + "\\" + fileName;
                try
                {
                    // Save file
                    if (File.Exists(SaveLocation)) File.Delete(SaveLocation);
                    File1.PostedFile.SaveAs(SaveLocation);
                    bool isFileValid = true;
                    var fileContent = new List<List<string>>();

                    // Process file for error
                    using (TextFieldParser parser = new TextFieldParser(SaveLocation))
                    {
                        parser.TextFieldType = FieldType.Delimited;
                        parser.SetDelimiters(",");
                        while (!parser.EndOfData)
                        {
                            //Process row
                            var lineNumber = parser.LineNumber;
                            string[] fields = parser.ReadFields();
                            List<string> line = new List<string>(fields);
                            fileContent.Add(line);
                            var regexList = (List<Tuple<string, string, string>>)Session["regexList"];

                            // If more row items than required
                            if (line.Count > regexList.Count)
                            {
                                var errorMsg = new TableCell();
                                var errorMsgRow = new TableRow();
                                errorMsg.Text = "This job requires that every row in your csv file contains no more than " + regexList.Count + " items.";
                                errorMsg.Attributes["style"] = "color:red;";
                                errorMsgRow.Cells.Add(errorMsg);
                                fileEntryMsg.Rows.Add(errorMsgRow);

                                var error = new TableCell();
                                var errorRow = new TableRow();
                                error.Text = "For instance: row number  " + lineNumber + " has " + line.Count + " items instead of " + regexList.Count+".";
                                error.Attributes["style"] = "color:red;";
                                errorRow.Cells.Add(error);
                                fileEntryMsg.Rows.Add(errorRow);
                                isFileValid = false;

                                // Clear list of file contents if errors found
                                fileContent.Clear();
                                return;
                            }

                            //Process field
                            for (int i = 1; i <= regexList.Count; i++)
                            {
                                if (regexList[i - 1].Item2 != string.Empty) // If regex exists for this label
                                {
                                    string label = regexList[i - 1].Item1;
                                    string pattern = @regexList[i - 1].Item2;
                                    Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
                                    
                                    if (!r.IsMatch(fields[i - 1]))
                                    {
                                        string msg = label + ": " + regexList[i - 1].Item3;
                                        var error = new TableCell();
                                        error.Text = "Cell value \""+ (fields[i - 1]) + "\" for index data attribute \"" + label+ "\" at location (row, col) = (" + lineNumber + ", " + i + ") is not valid:   " + regexList[i - 1].Item3;
                                        error.Attributes["style"] = "color:red;";
                                        var errorRow = new TableRow();
                                        errorRow.Cells.Add(error);
                                        fileEntryMsg.Rows.Add(errorRow);
                                        isFileValid = false;
                                    }
                                }
                                else
                                {   
                                    if (i == 1 && fields[0] == string.Empty)
                                    {
                                        var error = new TableCell();
                                        error.Text = "First item of row " + lineNumber + " in csv file can not be blank.";
                                        error.Attributes["style"] = "color:red;";
                                        var errorRow = new TableRow();
                                        errorRow.Cells.Add(error);
                                        fileEntryMsg.Rows.Add(errorRow);
                                        isFileValid = false;
                                    }
                                }
                            }
                        }
                    }

                    // Process file for Barcode indexing if no error found
                    if (isFileValid)
                    {
                        string shortFileName;
                        if (fileName.Length > 15)
                            shortFileName = fileName.Substring(0, 9) + " ... " + fileName.Substring((fileName.Length - 6), 6);
                        else shortFileName = fileName;
                        uploadSuccess.Text = "\"" + shortFileName + "\"" + " file uploaded successfully!";
                        uploadHidden.Text = fileName;
                        uploadSuccess.Attributes["style"] = "color:green;";
                        uploadedFileMenu.Visible = true;
                        uploadSuccess.Visible = true;
                        viewContentBtn.Visible = true;
                        saveIndexesBtn.Visible = true;
                        printIndexesBtn.Visible = true;
                    }
                    else
                    {
                        // Delete file if errors found
                        File.Delete(SaveLocation);
                        fileContent.Clear();
                    }
                }
                catch (Exception ex)
                {
                    uploadSuccess.Text = "Error 4a:  Couldn't upload file. Make sure it's a csv file. " + ex.Message;
                    uploadSuccess.Attributes["style"] = "color:red;";
                    uploadSuccess.Visible = true;
                    return;
                }
            }
            else
            {
                uploadSuccess.Text = "Please select a csv file to upload.";
                uploadSuccess.Visible = true;
            }
        }



        // FILE 'SAVE INDEXES' CLICKED: SAVE UPLOADED FILE CONTENTS
        protected void saveIndexes_Click(object sender, EventArgs e)
        {
            try
            {
                ViewState["manualEntries"] = null; // Clearing any cached manual entries

                // Get file name & Set save location
                var fileName = uploadHidden.Text;
                if (fileName == string.Empty)
                {
                    uploadSuccess.Text = "Error 4b:  Couldn't retrieve file name. Contact system admin. ";
                    uploadSuccess.Attributes["style"] = "color:red;";
                    uploadSuccess.Visible = true;
                    return;
                }
                string SaveLocation = Server.MapPath("App_Data") + "\\" + fileName;
                var fileContent = new List<List<string>>();
                int countSavedRecords = 0;
                int countFileRecords = File.ReadLines(SaveLocation).Count();

                // Process file to generate & save indexes
                using (TextFieldParser parser = new TextFieldParser(SaveLocation))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");
                    while (!parser.EndOfData)
                    {
                        // Get row entries
                        var lineNumber = parser.LineNumber;
                        string[] fields = parser.ReadFields();
                        List<string> lineEntries = new List<string>(fields);
                        if (lineEntries.Count < 5)
                        {
                            int diff = 5 - lineEntries.Count;
                            for (int i = 1; i <= diff; i++) lineEntries.Add(string.Empty);
                        }
                        // Get barcode
                        string barcodeIndex = generateBarcode();
                        if (barcodeIndex.Contains("Error"))
                        {
                            uploadSuccess.Text = "Error 4cc:  Error occurred while saving. Only "+countSavedRecords+" records saved! Contact system admin.";
                            uploadSuccess.Attributes["style"] = "color:red;";
                            uploadSuccess.Visible = true;
                            return;
                        }
                        // Save barcode and entries
                        string result = saveIndexAndEntries(barcodeIndex, lineEntries);
                        if (result.Contains("Error"))
                        {
                            uploadSuccess.Text = "Error 4cc:  Error occurred while saving. Only " + countSavedRecords + " records saved! Contact system admin.";
                            uploadSuccess.Attributes["style"] = "color:red;";
                            uploadSuccess.Visible = true;
                            return;
                        }
                        // Stick barcode to the end of entries
                        lineEntries.Add(barcodeIndex);
                        fileContent.Add(lineEntries);
                        countSavedRecords++;
                    }
                }
                // Make sure all file records have been saved
                if (countFileRecords == countSavedRecords)
                {
                    uploadSuccess.Visible = false;
                    var error = new TableCell();
                    var errorRow = new TableRow();
                    error.Text = countSavedRecords + " index strings successfully saved...";
                    error.Attributes["style"] = "color:green;";
                    errorRow.Cells.Add(error);
                    fileEntryMsg.Rows.Add(errorRow);
                    
                    viewContentBtn.Visible = false;
                    saveIndexesBtn.Visible = false;
                    printIndexesBtn.Visible = false;
                    GridView1.Visible = false;

                    ViewState["fileContent"] = fileContent;
                }
            }
            catch(Exception ex)
            {
                uploadSuccess.Text = "Error 4c:  Couldn't process file. Contact system admin." + ex.Message;
                uploadSuccess.Attributes["style"] = "color:red;";
                uploadSuccess.Visible = true;
                return;
            }
        }



        // 'PRINT INDEXES' CLICKED: SAVE & PRINT UPLOADED FILE CONTENTS
        protected void printIndexes_Click(object sender, EventArgs e)
        {
            try
            {
                // Set stage
                Button b = (Button)sender;

                // First, save index(es)
                var fileContent = new List<List<string>>();
                var manualEntries = new List<string>();
                if (b.ID == "printIndexesBtn")
                {
                    saveIndexes_Click(new object(), new EventArgs());
                    fileContent = (List<List<string>>)ViewState["fileContent"];
                    if (fileContent == null) return;
                }
                else if (b.ID == "saveAndPrint")
                {
                    saveIndex_Click(new object(), new EventArgs());
                    manualEntries = (List<string>)ViewState["manualEntries"];
                    if (manualEntries == null) return;
                }

                // Clear page
                formPanel.Visible = false;

                // Start writing index sheet pages
                Response.Write("<div id = 'pageToPrint' style='margin-top:-50px;'>");

                // Write index sheet pages
                if (b.ID == "printIndexesBtn")
                {
                    foreach (List<string> entries in fileContent)
                    {
                        string result = writeIndexPage(entries);
                        if (result.Contains("Error"))
                        {
                            string msg = result;
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            return;
                        }
                    }
                }
                else if (b.ID == "saveAndPrint")
                {
                    string result = writeIndexPage(manualEntries);
                    if (result.Contains("Error"))
                    {
                        string msg = result;
                        ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                        return;
                    }
                }
               
                // Close div tag
                Response.Write("</div>");

                // Finally, Print Index sheet.
                ClientScript.RegisterStartupScript(this.GetType(), "PrintOperation", "printing();", true);
            }
            catch (Exception ex)
            {
                uploadSuccess.Text = "Error 4cc:  Couldn't process file. Contact system admin." + ex.Message;
                uploadSuccess.Attributes["style"] = "color:red;";
                uploadSuccess.Visible = true;
                return;
            }
        }


        // 'VIEW CONTENT' CLICKED: VIEW CVS INDEX DATA FILE.
        protected void viewContent_Click(object sender, EventArgs e)
        {
            if (viewContentBtn.Text == "Hide")
            {
                GridView1.Visible = false;
                viewContentBtn.Text = "View";
            }
            else
            {
                GridView1.Visible = true;
                viewContentBtn.Text = "Hide";
            }

            // Get file path
            var fileName = uploadHidden.Text;
            if (fileName == string.Empty)
            {
                uploadSuccess.Text = "Error 4b:  Couldn't retrieve file name. Contact system admin. ";
                uploadSuccess.Attributes["style"] = "color:red;";
                uploadSuccess.Visible = true;
                return;
            }
            string SaveLocation = Server.MapPath("App_Data") + "\\" + fileName;

            // Get row count
            int colCount = 0;
            var regexList = (List<Tuple<string, string, string>>)Session["regexList"];
            if (regexList != null) colCount = regexList.Count;

            //Create a DataTable.
            DataTable dt = new DataTable();
            for (int i=0; i<colCount; i++)
            {
                dt.Columns.Add(regexList[i].Item1, typeof(string));
            }

            //Read the contents of CSV file.
            string csvData = File.ReadAllText(SaveLocation);

            //Execute a loop over the rows.
            foreach (string row in csvData.Split('\n'))
            {
                if (!string.IsNullOrEmpty(row))
                {
                    dt.Rows.Add();
                    int i = 0;

                    //Execute a loop over the columns.
                    foreach (string cell in row.Split(','))
                    {
                        dt.Rows[dt.Rows.Count - 1][i] = cell;
                        i++;
                    }
                }
            }

            //Bind the DataTable.
            GridView1.DataSource = dt;
            GridView1.DataBind();
        }


        // PREVENT LINE BREAKS
        protected void rowDataBound(object sender, GridViewRowEventArgs e)
        {
            try
            {
                // Set column borders & Prevent line breaks
                if (e.Row.RowType == DataControlRowType.Header)
                {
                    string colBorder = "border: 1px solid #717171; white-space: nowrap; font-family: Arial";
                    for (int i = 0; i < e.Row.Cells.Count; i++)
                        e.Row.Cells[i].Attributes.Add("style", colBorder);
                }

                // Set column borders & Prevent line breaks
                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    string colBorder = "border: 1px solid #cccccc; white-space: nowrap;";
                    for (int i = 0; i < e.Row.Cells.Count; i++)
                        e.Row.Cells[i].Attributes.Add("style", colBorder);
                }
            }
            catch (Exception ex)
            {
                string msg = "Issue occured while attempting to prevent line breaks in table. Contact system admin." + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "34");
            }
        }
        // GENERATE INDEX. HELPER
        private string generateBarcode()
        {
            try
            {
                if (!Page.IsValid) return "Error: page invalid";

                // Making the Index string
                string year = DateTime.Now.ToString("yy");
                JulianCalendar jc = new JulianCalendar();
                string julianDay = jc.GetDayOfYear(DateTime.Now).ToString();
                string time = DateTime.Now.ToString("HHmmssfff");
                generateIndexSection.Visible = true;
                return selectJob.SelectedValue.ToUpper() + year + julianDay + time;

                // Convert index to barcode
                // imgBarcode.ImageUrl = string.Format("ShowCode39Barcode.ashx?code={0}&ShowText={1}&Thickness={2}",indexString,showTextValue, 1);
            }
            catch (Exception ex)
            {
                string msg = "Error 4b: Issue occured while attempting to generate Index. Contact system admin. ";
                return msg + ex.Message;
            }
        }



        // MANUAL 'SAVE INDEX' CLICKED: SAVING INDEX INTO DB.
        protected void saveIndex_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            ViewState["fileContent"] = null; // Clearing any cached file entries

            // Get operator's entries & Check regex rules
            var regexList = (List<Tuple<string, string, string>>)Session["regexList"];
            var entries = new List<string>();
            for (int i = 1; i <= 5; i++)
            {
                TextBox c = this.Master.FindControl("MainContent").FindControl("label" + i + "Box") as TextBox;
                if (c.Visible == true)
                {   
                    // Make sure 1st field not blank
                    if (i == 1 && c.Text == string.Empty)
                    {
                        string label = regexList[0].Item1;
                        string msg = label + " field is required!";
                        ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                        c.Focus();
                        return;
                    }

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
                    entries.Add(c.Text);
                }
                else
                    entries.Add(string.Empty);
            }

            // Get barcode
            string barcodeIndex = generateBarcode();
            if (barcodeIndex.Contains("Error"))
            {
                string msg = barcodeIndex;
                ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                return;
            }

            // Save barcode and entries
            string result = saveIndexAndEntries(barcodeIndex, entries);
            if (result.Contains("Error"))
            {
                uploadSuccess.Text = "Error 4cc:  Error occurred while saving. Contact system admin.";
                uploadSuccess.Attributes["style"] = "color:red;";
                uploadSuccess.Visible = true;
                return;
            }
            // Stick barcode to the end of entries
            entries.Add(barcodeIndex);
            ViewState["manualEntries"] = entries;
        }



        // SAVE INDEX & ENTRIES. HELPER
        private string saveIndexAndEntries(string index, List<string>entries)
        {
            try
            {
                // First, get current user id via name.
                string user = Environment.UserName;
                int opID = Helper.getUserId(user);
                if (opID == 0)
                {
                    string msg = "Error 05: Couldn't identify active user. Contact system admin.";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    return msg;
                }

                // Then, get selected job id
                int jobID = getJobId(this.selectJob.SelectedValue);
                if (jobID == 0)
                {
                    string msg = "Error 06: Couldn't identify the selected job. Contact system admin.";
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                    selectJob.SelectedValue = "Select";
                    return msg;
                }

                // Saving
                string barcodeIndex = index;
                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "INSERT INTO INDEX_DATA (JOB_ID, BARCODE, VALUE1, VALUE2, " +
                                            "VALUE3, VALUE4, VALUE5, OPERATOR_ID, CREATION_TIME, PRINTED) VALUES(@jobId, @barcodeIndex," +
                                            " @val1, @val2, @val3, @val4, @val5, @opId, @time, @printed)";
                        cmd.Parameters.AddWithValue("@jobID", jobID);
                        cmd.Parameters.AddWithValue("@barcodeIndex", barcodeIndex);
                        for (int i = 1; i <= 5; i++)
                        {   
                            if (entries[i - 1] != string.Empty) cmd.Parameters.AddWithValue("@val" + i, entries[i - 1]);
                            else cmd.Parameters.AddWithValue("@val" + i, DBNull.Value);
                        }
                        cmd.Parameters.AddWithValue("@opId", opID);
                        cmd.Parameters.AddWithValue("@time", DateTime.Now);
                        cmd.Parameters.AddWithValue("@printed", 0);

                        con.Open();
                        if (cmd.ExecuteNonQuery() == 1)
                        {
                            //Button c = this.Master.FindControl("MainContent").FindControl("label1Box") as Button;
                            Control c = Helper.GetPostBackControl(this.Page);
                            if (c != null && c.ID == "saveIndex")
                            {
                                var error = new TableCell();
                                var errorRow = new TableRow();
                                error.Text = "Index string successfully saved...";
                                error.Attributes["style"] = "color:green;";
                                errorRow.Cells.Add(error);
                                manualEntryMsg.Rows.Add(errorRow);
                                clearFields();
                            }
                            return "pass";
                        }
                        else
                        {
                            string msg = "Error-07: Issue occured while attempting to save. Contact system admin.";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            return msg;
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
                    return msg;
                }
                else
                {
                    string msg = "Error 09: Issue occured while attempting to save index. Contact system admin." + Environment.NewLine + ex.Message;
                    ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('"+ msg + "');", true);
                    return msg;
                }
            }
        }
        

        // WRITE INDEX SHEET PAGE CONTENT. HELPER
        private string writeIndexPage(List<string> indexRecord)
        {
            try
            {
                //Get index string
                string indexString = indexRecord.Last();
                Image imgBarcode = new Image();
                imgBarcode.ImageUrl = string.Format("ShowCode39BarCode.ashx?code={0}&ShowText=1&Height=50", indexString.PadLeft(8, '0'));

                // Write Index sheet page content
                Response.Write(
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
                var regexList = (List<Tuple<string, string, string>>)Session["regexList"];

                for (int i = 0; i < regexList.Count; i++)
                {
                    string label = regexList[i].Item1;
                    Response.Write(
                        "<tr>" +
                            "<td style='font-size:25px; font-weight:500;'>" + label + "</td>" +
                            "<td style='font-size:25px; font-weight:500; padding-left:15px;'>" + indexRecord[i].ToUpper() + "</td>" +
                        "</tr>"
                    );
                }
                Response.Write(
                            "<tr>" +
                                "<td style='font-size:25px; font-weight:500;'>Date Created: </td>" +
                                "<td style='font-size:25px; font-weight:500; padding-left:15px;'>" + DateTime.Now + "</td>" +
                            "</tr>" +
                        "</table >"
                );
                return "pass";
            }
            catch(Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }


        // SET INDEX AS PRINTED IN DB. 
        protected string setIndexAsPrinted(string index)
        {
            try
            {
                if (!Page.IsValid) return "Error: Page invalid";
                var counter = 0;
                string indexString = index;
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
                        }
                        else
                        {
                            string msg = "Error 11: Index saved, but issue occured while attempting to set it to PRINTED. Contact system admin.";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            return msg;
                        }

                        // Confirmation msg & back to unprinted indexes gridview
                        if (counter == 1)
                        {
                            ClientScript.RegisterStartupScript(this.GetType(), "fadeoutOperation", "FadeOut2();", true);
                            return "pass";
                        }
                        else
                        {
                            string msg = "Error 12: Index saved, but issue occured while attempting to set it to PRINTED. Contact system admin.";
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            return msg;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "Error: Index saved, but issue occured while attempting to set it to PRINTED. Contact system admin." + Environment.NewLine + ex.Message;
                System.Windows.Forms.MessageBox.Show(msg, "Error 13");
                return msg;
            }
        }



        // SET INDEX AS PRINTED AFTER INDEX SHEET PRINTOUT 
        protected void setAsPrinted_Click(object sender, EventArgs e)
        {
            try
            {
                formPanel.Visible = true;

                // For manual entries
                if (ViewState["manualEntries"] != null)
                {
                    var manualEntries = (List<string>)ViewState["manualEntries"];
                    string result = setIndexAsPrinted(manualEntries.Last());
                    if (result.Contains("Error"))
                    {
                        string msg = result;
                        ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                        return;
                    }
                    var error = new TableCell();
                    var errorRow = new TableRow();
                    error.Text = "Index string successfully saved and set as PRINTED...";
                    error.Attributes["style"] = "color:green;";
                    errorRow.Cells.Add(error);
                    manualEntryMsg.Rows.Add(errorRow);
                    clearFields();
                }

                // For uploaded file
                else if (ViewState["fileContent"] != null)
                {
                    var fileContent = (List<List<string>>)ViewState["fileContent"];
                    int countPass = 0;
                    foreach (List<string> entries in fileContent)
                    {
                        string result = setIndexAsPrinted(entries.Last());
                        if (result.Contains("Error"))
                        {
                            string msg = result;
                            ClientScript.RegisterStartupScript(this.GetType(), "myalert", "alert('" + msg + "');", true);
                            return;
                        }
                        else if (result == "pass") countPass++;
                    }
                    var error = new TableCell();
                    var errorRow = new TableRow();
                    error.Text = countPass + " index strings successfully saved and set as PRINTED...";
                    error.Attributes["style"] = "color:green;";
                    errorRow.Cells.Add(error);
                    fileEntryMsg.Rows.Add(errorRow);
                }
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
            ViewState["manualEntries"] = null;
            ViewState["fileContent"] = null;
            TextBox c = this.Master.FindControl("MainContent").FindControl("label1Box") as TextBox;
            if (c.Visible) c.Focus();
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
                int opID = Helper.getUserId(user);
                if (opID == 0)
                {
                    noJobsFound.Visible = true;
                    return;
                }

                // Then, get all jobS accessible to current operator from OPERATOR_ACCESS.
                using (SqlConnection con = Helper.ConnectionObj)
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "SELECT ABBREVIATION " +
                                          "FROM JOB " +
                                          "JOIN OPERATOR_ACCESS ON JOB.ID=OPERATOR_ACCESS.JOB_ID " +
                                          "WHERE ACTIVE=1 AND OPERATOR_ID=@userId " +
                                          "ORDER BY ABBREVIATION ASC";
                        cmd.Parameters.AddWithValue("@userId", opID);
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
                                // If no accessible jobs found, display message.
                                noJobsFound.Visible = true;
                                return;
                            }
                        }
                    }
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
    }
}

