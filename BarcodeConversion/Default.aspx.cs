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
using System.Web;

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
                Page.Form.Attributes.Add("enctype", "multipart/form-data");
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
                uploadedFileMenu.Visible = false;

                for (int i = 1; i <= 10; i++)
                {
                    Label l = this.Master.FindControl("MainContent").FindControl("LABEL" + i) as Label;
                    l.Visible = false;
                    TextBox t = this.Master.FindControl("MainContent").FindControl("label" + i + "Box") as TextBox;
                    t.Visible = false;
                    t.Text = string.Empty;
                    DropDownList d = this.Master.FindControl("MainContent").FindControl("label" + i + "Dropdown") as DropDownList;
                    d.Visible = false;
                }

                // Make sure a job is selected
                if (this.selectJob.SelectedValue != "Select")
                {
                    // First, get selected job ID.
                    int jobID = getJobId(this.selectJob.SelectedValue);

                    if (jobID <= 0)
                    {
                        string msg = "Error 02:     Selected job not found. Contact system admin.";
                        ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                        selectJob.SelectedValue = "Select";
                        return;
                    }

                    // Then, check whether that job is configured, if so, display controls.
                    using (SqlConnection con = Helper.ConnectionObj)
                    {
                        using (SqlCommand cmd = con.CreateCommand())
                        {
                            cmd.CommandText = "SELECT JOB_ID, LABEL1, LABEL2, LABEL3, LABEL4, LABEL5, LABEL6, LABEL7, LABEL8, LABEL9, LABEL10, " +
                                                             "REGEX1, REGEX2, REGEX3, REGEX4, REGEX5, REGEX6, REGEX7, REGEX8, REGEX9, REGEX10, " +
                                                             "ALERT1, ALERT2, ALERT3, ALERT4, ALERT5, ALERT6, ALERT7, ALERT8, ALERT9, ALERT10, " +
                                                             "TABLEID1, TABLEID2, TABLEID3, TABLEID4, TABLEID5, TABLEID6, TABLEID7, TABLEID8, TABLEID9, TABLEID10 " +
                                              "FROM JOB_CONFIG_INDEX " + 
                                              "WHERE JOB_ID=@jobID";
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
                                    if (reader.Read())
                                    {
                                        while (i <= 10)
                                        {
                                            if (reader.GetValue(i) != DBNull.Value) // If label i is set
                                            {
                                                if (reader.GetValue(i + 30) == DBNull.Value) // If control is a Textbox
                                                {
                                                    var tuple = Tuple.Create("", "", "");
                                                    string label = (string)reader.GetValue(i);
                                                    string regex = string.Empty;
                                                    string alert = string.Empty;
                                                    if (reader.GetValue(i + 10) != DBNull.Value) // if regex i is set
                                                    {
                                                        regex = (string)reader.GetValue(i + 10);
                                                        alert = (string)reader.GetValue(i + 20);
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
                                                        t.Attributes["placeholder"] = "Required";
                                                        t.Focus();
                                                    }
                                                    else
                                                    {
                                                        if (regex == string.Empty) t.Attributes["placeholder"] = "Optional";
                                                        else t.Attributes["placeholder"] = "Required";
                                                    }
                                                    //j += 1;
                                                }
                                                else    // If control is a dropdown
                                                {       
                                                    string label = (string)reader.GetValue(i);
                                                    var tuple = Tuple.Create(label, "", "");
                                                    regexList.Add(tuple);
                                                    label = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(label.ToLower());
                                                    Label l = this.Master.FindControl("MainContent").FindControl("LABEL" + j) as Label;
                                                    l.Text = label + " :";
                                                    l.Visible = true;
                                                    DropDownList d = this.Master.FindControl("MainContent").FindControl("label" + j + "Dropdown") as DropDownList;
                                                    d.Visible = true;
                                                    d.Items.Clear(); // Clear current dropdown items
                                                    d.Items.Add("");

                                                    // Fill dropdown
                                                    using (SqlCommand cmd2 = con.CreateCommand())
                                                    {
                                                        cmd2.CommandText = "SELECT VALUE FROM INDEX_TABLE_FIELD WHERE ID=@id ORDER BY ORD";
                                                        cmd2.Parameters.AddWithValue("@id", reader.GetValue(i + 30).ToString());
                                                        using (SqlDataReader reader2 = cmd2.ExecuteReader())
                                                        {
                                                            while (reader2.Read())
                                                            {
                                                                d.Items.Add(reader2.GetValue(0).ToString());
                                                            }
                                                        }
                                                    }
                                                }

                                            }
                                            else
                                            {
                                                var tuple = Tuple.Create("", "", "");
                                                regexList.Add(tuple);
                                            } 
                                            i += 1; j += 1; 
                                        }
                                        Session["regexList"] = regexList; // Contains (label,regex,alert) for each label set at Index Config Section
                                    }
                                    generateIndexSection.Visible = true;
                                    lastValuesEntered.Checked = false;
                                }
                                else
                                {
                                    string msg = "The \"" + selectJob.SelectedValue + "\" job that you selected has not yet been configured."
                                                + " Only jobs listed in green can be processed.";
                                    var screenMsg = new TableCell();
                                    var screenMsgRow = new TableRow();
                                    screenMsg.Text = msg;
                                    screenMsg.Attributes["style"] = "color:#ff3333;";
                                    screenMsgRow.Cells.Add(screenMsg);
                                    notConfigScreenMsg.Rows.Add(screenMsgRow);
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
                string msg = "Issue occured while attempting to retrieve selected job index data controls. Contact system admin.";
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('Error 03: " + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
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
                    string msg = "Only csv files are allowed!";
                    string color = "#ff3333;";
                    onScreenMsg(msg, color, "file");
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
                    bool isFileBlank = true;
                    var fileContent = new List<List<string>>();

                    // Process file for error
                    using (TextFieldParser parser = new TextFieldParser(SaveLocation))
                    {
                        parser.TextFieldType = FieldType.Delimited;
                        parser.SetDelimiters(",");
                        while (!parser.EndOfData)
                        {
                            isFileBlank = false;

                            // Process row
                            var lineNumber = parser.LineNumber;
                            string[] fields = parser.ReadFields();
                            List<string> line = new List<string>(fields);

                            // Check if row is made of blank entries
                            bool noEntry = true;
                            foreach (var item in line)
                                if (item != string.Empty) noEntry = false;
                            if (noEntry == true)
                            {
                                string msg = "Row " +lineNumber+ ": All row entries can not be blank. At least one index data must be present.";
                                string color = "#ff3333;";
                                onScreenMsg(msg, color, "file");
                                return;
                            }
                            
                            fileContent.Add(line);

                            var regexList = (List<Tuple<string, string, string>>)Session["regexList"];
                            int labelsCount = 0;
                            int regexCount = 0;
                            string required = string.Empty;
                            foreach (Tuple<string,string,string>controlSettings in regexList)
                            {
                                if (controlSettings.Item1 != string.Empty)  // Check how many controls were set
                                    labelsCount++;
                                if (controlSettings.Item2 != string.Empty)  // Check how many regex were set
                                    regexCount++;
                            }

                            // If less row items than the min required
                            if (line.Count < regexCount)
                            {
                                string msg = "This job requires that every row in your csv file contains no less than " + regexCount + " item(s). That is not the case for row " + lineNumber + ".";
                                string color = "#ff3333;";
                                onScreenMsg(msg, color, "file");

                                // Clear list of file contents if errors found
                                fileContent.Clear();
                                return;
                            }

                            // If more row items than the max required
                            if (line.Count > labelsCount)
                            {
                                string msg = "This job requires that every row in your csv file contains no more than " + labelsCount + " item(s).";
                                string color = "#ff3333;";
                                onScreenMsg(msg, color, "file");
                                
                                msg = "For instance: row number  " + lineNumber + " has " + line.Count + " item(s) instead of " + labelsCount + ".";
                                color = "#ff3333;";
                                onScreenMsg(msg, color, "file");
                                isFileValid = false;

                                // Clear list of file contents if errors found
                                fileContent.Clear();
                                return;
                            }

                            // Then, check whether any line entry is a dropdown value.
                            int jobID = getJobId(this.selectJob.SelectedValue);
                            using (SqlConnection con = Helper.ConnectionObj)
                            {
                                using (SqlCommand cmd = con.CreateCommand()) // Retrieve all the tableids
                                {
                                    cmd.CommandText = "SELECT TABLEID1, TABLEID2, TABLEID3, TABLEID4, TABLEID5, TABLEID6, TABLEID7, TABLEID8, TABLEID9, TABLEID10 " +
                                                      "FROM JOB_CONFIG_INDEX " +
                                                      "WHERE JOB_ID=@jobID";
                                    cmd.Parameters.AddWithValue("@jobID", jobID);
                                    con.Open();
                                    using (SqlDataReader reader = cmd.ExecuteReader())
                                    {
                                        if (reader.HasRows && reader.Read())
                                        {
                                            for (int i = 0; i <= 9; i++) // For each tableid
                                            {
                                                bool found = false;
                                                string validEntries = string.Empty;
                                                if (reader.GetValue(i).ToString() != DBNull.Value.ToString()) // If tableid[i] exists
                                                {
                                                    using (SqlCommand cmd2 = con.CreateCommand()) // Retrieve all values related to the tableid
                                                    {
                                                        cmd2.CommandText = "SELECT VALUE FROM INDEX_TABLE_FIELD WHERE ID=@id ORDER BY ORD";
                                                        cmd2.Parameters.AddWithValue("@id", reader.GetValue(i).ToString());
                                                        using (SqlDataReader reader2 = cmd2.ExecuteReader()) // Get tableid values
                                                        {
                                                            while (reader2.Read())  // For each tableid value, check whether line entry is among those values
                                                            {
                                                                string dropdownValue = reader2.GetValue(0).ToString();
                                                                if ((String.Compare(line[i],dropdownValue,true) == 0) || line[i] == string.Empty)
                                                                    found = true;
                                                                validEntries += dropdownValue + ", ";
                                                            }
                                                        }
                                                    }

                                                    if (found == false) // if line entry not among tableid values
                                                    {
                                                        uploadSuccess.Visible = false;
                                                        string msg;
                                                        if (line[i] != string.Empty)
                                                            msg = "Value at (Row,Col):(" + lineNumber + ", " + i + ") =  '" + line[i] + 
                                                            "'  is invalid. Valid entry must be one of following: " + validEntries.Remove(validEntries.Length - 2, 2) + ", or blank."; 
                                                        else
                                                            msg = "Value at (Row,Col):(" + lineNumber + ", " + i + ") does not exist. Valid entry must be one of following: " 
                                                            + validEntries.Remove(validEntries.Length - 2, 2) + ", or blank.";
                                                        string color = "#ff3333;";
                                                        onScreenMsg(msg, color, "file");
                                                        return;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            //Process field for regex
                            for (int i = 1; i <= regexList.Count; i++)
                            {
                                if (regexList[i - 1].Item2 != string.Empty) // If regex exists for this label
                                {
                                    string label = regexList[i - 1].Item1;
                                    string pattern = @regexList[i - 1].Item2;
                                    Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
                                    
                                    if ((i - 1) > fields.Length)
                                    {
                                        string msg1 = label + ": " + regexList[i - 1].Item3;
                                        string msg = "Line " + lineNumber + "is missing required item " + i;
                                        string color = "#ff3333;";
                                        onScreenMsg(msg, color, "file");
                                        isFileValid = false;

                                        return;
                                    }

                                    if (!r.IsMatch(fields[i - 1]))
                                    {
                                        string msg1 = label + ": " + regexList[i - 1].Item3;
                                        string msg = "Value '"+ (fields[i - 1]) + "' for index data '" + label+ "' at location (row, col) = (" + lineNumber + ", " + i + ") is not valid:    " + regexList[i - 1].Item3;
                                        string color = "#ff3333;";
                                        onScreenMsg(msg, color, "file");
                                        isFileValid = false;
                                    }
                                }
                                else
                                {
                                    TextBox c = this.Master.FindControl("MainContent").FindControl("label" + i + "Box") as TextBox;
                                    DropDownList d = this.Master.FindControl("MainContent").FindControl("label" + i + "Dropdown") as DropDownList;
                                    if (c.Visible == true)
                                    {
                                        if (i == 1 && fields[0] == string.Empty)
                                        {
                                            string msg = "1st item of row " + lineNumber + " can not be blank.";
                                            string color = "#ff3333;";
                                            onScreenMsg(msg, color, "file");
                                            isFileValid = false;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Check if File is blank
                    if (isFileBlank == true)
                    {
                        string msg = "File seems to be blank.";
                        string color = "#ff3333;";
                        onScreenMsg(msg, color, "file");
                        return;
                    }

                    // Process file for Barcode indexing if no error found
                    if (isFileValid)
                    {
                        string shortFileName;
                        if (fileName.Length > 17)
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
                    string msg = "Error 4: Could not upload file. Check that it is a csv file and that its structure & contents belong to the currently selected Job.";
                    ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);

                    // Log the exception and notify system operators
                    ExceptionUtility.LogException(ex);
                    ExceptionUtility.NotifySystemOps(ex);
                    return;
                }
            }
            else
            {
                string msg = "Please select a csv file to upload.";
                string color = "#ff3333;display:block;";
                onScreenMsg(msg, color, "file");
            }
        }



        // FILE 'SAVE' CLICKED: SAVE UPLOADED FILE CONTENTS
        protected void saveIndexes_Click(object sender, EventArgs e)
        {
            try
            {
                ViewState["manualEntries"] = null; // Clearing any cached manual entries
                ViewState["manualPrintCancelled"] = null; 

                // Get file name & Set save location
                var fileName = uploadHidden.Text;
                if (fileName == string.Empty)
                {
                    string msg = "Error 4a:  Could not retrieve file name. Contact system admin. ";
                    ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
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

                        // Get number of controls set for this job
                        var regexList = (List<Tuple<string, string, string>>)Session["regexList"];
                        int labelsCount = 0;
                        foreach (Tuple<string, string, string> controlSettings in regexList)
                        {
                            if (controlSettings.Item1 != string.Empty)
                                labelsCount++;
                        }
                        
                        var newLineEntries = new List<string>();
                        if (lineEntries.Count <= labelsCount)
                        {
                            foreach (string field in lineEntries)
                            {
                                newLineEntries.Add(field);
                            }
                            int diff = labelsCount - lineEntries.Count;
                            for (int i = 1; i <= diff; i++) newLineEntries.Add(string.Empty);
                            lineEntries.Clear();
                            lineEntries = newLineEntries;
                        }

                        // add blanks to make 10 items per row
                        if (lineEntries.Count < 10)
                        {
                            int diff = 10 - lineEntries.Count;
                            for (int i = 1; i <= diff; i++) lineEntries.Add(string.Empty);
                        }
                        // Check to make sure that file's line entries are within dropdown range if it applies
                        // First, get selected job ID.
                        int jobID = getJobId(this.selectJob.SelectedValue);

                        if (jobID <= 0)
                        {
                            string msg = "Error 4b: Selected job not found. Contact system admin.";
                            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                            selectJob.SelectedValue = "Select";
                            return;
                        }

                        // Get barcode
                        string barcodeIndex = generateBarcode();
                        if (barcodeIndex.Contains("Error"))
                        {
                            string msg = "Error 4c:  Error occurred while generating barcode! Contact system admin.";
                            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                            return;
                        }
                        // Save barcode and entries
                        string result = saveIndexAndEntries(barcodeIndex, lineEntries);
                        if (result.Contains("Error"))
                        {
                            string msg = "Only " + countSavedRecords + " records were saved! Contact system admin.";
                            string color = "#ff3333;";
                            onScreenMsg(msg, color, "file");
                            return;
                        }
                        // Stick barcode to the end of entries
                        lineEntries.Add(barcodeIndex);
                        fileContent.Add(lineEntries);
                        countSavedRecords++;
                    }
                }

                // Make sure all file records have been saved
                if (countFileRecords >= countSavedRecords)
                {
                    uploadSuccess.Visible = false;
                    string msg = countSavedRecords + " index string(s) saved.";
                    string color = "green;";
                    onScreenMsg(msg, color, "file");

                    uploadedFileMenu.Visible = false;
                    viewContentBtn.Visible = false;
                    saveIndexesBtn.Visible = false;
                    printIndexesBtn.Visible = false;
                    GridView1.Visible = false;

                    ViewState["fileContent"] = fileContent;
                    ScriptManager.RegisterStartupScript(indexCreationUpdatePanel, indexCreationUpdatePanel.GetType(), "fadeoutOp", "FadeOut2();", true);
                }
                ViewState["filePrintCancelled"] = countSavedRecords;
            }
            catch(Exception ex)
            {
                string msg = "Error 4d:  Couldn't process file. Contact system admin.";
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);

                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
                return;
            }
        }



        // 'SAVE & PRINT' CLICKED: SAVE & PRINT UPLOADED FILE CONTENTS or Manual entries
        protected void printIndexes_Click(object sender, EventArgs e)
        {
            try
            {
                // Set stage
                Button b = (Button)sender;

                // First, save index(es) if File Entries
                var fileContent = new List<List<string>>();
                var manualEntries = new List<string>();
                if (b.ID == "printIndexesBtn")
                {
                    saveIndexes_Click(new object(), new EventArgs());
                    fileContent = (List<List<string>>)ViewState["fileContent"];
                    if (fileContent == null) return;
                }
                // Or save index(es) if Manual Entries
                else if (b.ID == "saveAndPrint")
                {
                    // Check if anything was entered at all. If nothing entered, break out.
                    bool nothing = isNothingEntered();
                    if (nothing) return;

                    // Then save
                    saveIndex_Click(new object(), new EventArgs());
                    manualEntries = (List<string>)ViewState["manualEntries"];
                    if (manualEntries == null) return;
                }

                // Clear page
                formPanel.Visible = false;

                // Start writing index sheet pages
                indexPage.Visible = true;
                string indexPageString = "";
                indexPageString += "<div id = 'pageToPrint' style='margin-top:-50px;'>";

                // Write index sheet pages if File Entries
                if (b.ID == "printIndexesBtn")
                {
                    int currentCount = 0;
                    foreach (List<string> entries in fileContent)
                    {
                        currentCount++;
                        var result = writeIndexPage(entries, currentCount, fileContent.Count);
                        if (result.Item1.Contains("Error"))
                        {
                            string msg = result.Item1;
                            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                            return;
                        }
                        else indexPageString += result.Item2;
                    }
                }
                // Or Write index sheet pages if Manual Entries
                else if (b.ID == "saveAndPrint")
                {
                    var result = writeIndexPage(manualEntries, 1, 1);
                    if (result.Item1.Contains("Error"))
                    {
                        string msg = result.Item1;
                        ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                        return;
                    }
                    else indexPageString += result.Item2;
                }

                // Close pageToPrint & wrapper div tags
                indexPageString += "</div>";

                indexPage.InnerHtml = indexPageString;

                // Finally, Print Index sheet.
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "PrintOperation", "printing();", true);
            }
            catch (Exception ex)
            {
                Button b = (Button)sender;
                string msg = "Error 4f:  Couldn't process file. Contact system admin.";
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);

                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
                return;
            }
        }


        // 'VIEW CONTENT' CLICKED: VIEW CVS INDEX DATA FILE.
        protected void viewContent_Click(object sender, EventArgs e)
        {   
            try 
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
                    string msg = "Error 4g:  Could not retrieve file name. Contact system admin. ";
                    ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                    return;
                }
                string SaveLocation = Server.MapPath("App_Data") + "\\" + fileName;

                // Get column count
                int colCount = 0;
                var regexList = (List<Tuple<string, string, string>>)Session["regexList"];
                foreach (Tuple<string, string, string> controlSettings in regexList)
                {
                    if (controlSettings.Item1 != string.Empty)
                        colCount++;
                }

                //Create a DataTable.
                DataTable dt = new DataTable();
                for (int i = 0; i < colCount; i++)
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
            catch(Exception ex)
            {
                string msg = "Error 33a: Issue occured while attempting to display contents. Contact system admin.";
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
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
                string msg = "Error 34: Issue occured while attempting to prevent line breaks in table. Contact system admin.";
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
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
                string julianDay = DateTime.Today.DayOfYear.ToString();
                string ThreeDigitsJulian = julianDay.ToString().PadLeft(3, '0');
                string time = DateTime.Now.ToString("HHmmssfff");
                generateIndexSection.Visible = true;
                return selectJob.SelectedValue.ToUpper() + year + ThreeDigitsJulian + time;

                // Convert index to barcode
                // imgBarcode.ImageUrl = string.Format("ShowCode39Barcode.ashx?code={0}&ShowText={1}&Thickness={2}",indexString,showTextValue, 1);
            }
            catch (Exception ex)
            {
                string msg = "Error 4h: Issue occured while attempting to generate Index. Contact system admin. ";
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
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

            for (int i = 1; i <= 10; i++)
            {
                TextBox c = this.Master.FindControl("MainContent").FindControl("label" + i + "Box") as TextBox;
                DropDownList d = this.Master.FindControl("MainContent").FindControl("label" + i + "Dropdown") as DropDownList;
                
                if (c != null && c.Visible == true)     // If control is a textbox
                {
                    // Make sure 1st field not blank
                    if (i == 1 && c.Text == string.Empty)
                    {
                        string label = regexList[0].Item1;
                        string msg = label + " field is required.";
                        string color = "#ff3333;";
                        onScreenMsg(msg, color, "manual");
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
                            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                            c.Focus();
                            return;
                        }
                    }
                    entries.Add(c.Text);
                }
                else if (d != null && d.Visible == true)    // If control is a dropdown
                    entries.Add(d.SelectedValue);
                else
                    entries.Add(string.Empty);
            }

            // Check if anything was entered at all. If nothing entered, break out.
            bool nothing = isNothingEntered();
            if (nothing) return;

            // Get barcode
            string barcodeIndex = generateBarcode();
            if (barcodeIndex.Contains("Error"))
            {
                string msg = barcodeIndex;
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                return;
            }

            // Temporarely disable Save Index button to prevent quick multiple saves.
            Control manualSave = Helper.GetPostBackControl(this.Page);
            if (manualSave != null && manualSave.ID == "saveIndex" && lastValuesEntered.Checked)
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "pauseSave", "saveBtnPause();", true);

            // Save barcode and entries
            string result = saveIndexAndEntries(barcodeIndex, entries);
            if (result.Contains("Error")) return;

            // Stick barcode to the end of entries
            entries.Add(barcodeIndex);
            ViewState["manualEntries"] = entries;

            if(result.Contains("pass"))
            {
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "fadeoutOp", "FadeOut();", true);
                ViewState["manualPrintCancelled"] = 1;
            }
        }



        // SAVE INDEX & ENTRIES. HELPER
        private string saveIndexAndEntries(string index, List<string>entries)
        {
            try
            {
                // First, get current user id via name.
                string user = HttpContext.Current.User.Identity.Name.Split('\\').Last();
                int opID = Helper.getUserId(user);
                if (opID == 0)
                {
                    string msg = "Error 05: Could not identify active user. Contact system admin.";
                    ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                    return msg;
                }

                // Then, get selected job id
                int jobID = getJobId(this.selectJob.SelectedValue);
                if (jobID == 0)
                {
                    string msg = "Error 06: Could not identify the selected job. Contact system admin.";
                    ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
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
                                          "VALUE3, VALUE4, VALUE5, VALUE6, VALUE7, VALUE8, VALUE9, VALUE10, " + 
                                          "OPERATOR_ID, CREATION_TIME, PRINTED) VALUES(@jobId, @barcodeIndex, " +
                                          "@val1, @val2, @val3, @val4, @val5, @val6, @val7, @val8, @val9, @val10, @opId, @time, @printed)";
                        cmd.Parameters.AddWithValue("@jobID", jobID);
                        cmd.Parameters.AddWithValue("@barcodeIndex", barcodeIndex);
                        for (int i = 1; i <= 10; i++)
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
                            Control c = Helper.GetPostBackControl(this.Page);
                            if (c != null && c.ID == "saveIndex" && lastValuesEntered.Checked)
                            {
                                string msg = "Index string saved.";
                                string color = "green;";
                                onScreenMsg(msg, color, "manual");
                                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "fadeoutOper", "FadeOut3();", true);
                            }
                            else if (c != null && c.ID == "saveIndex")
                            {
                                string msg = "Index string saved.";
                                string color = "green;";
                                onScreenMsg(msg, color, "manual");
                                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "fadeoutOp", "FadeOut();", true);
                            }

                            // Remember last values entered
                            if (!lastValuesEntered.Checked) 
                                clearFields();

                            return "pass";
                        }
                        else
                        {
                            string msg = "Error 07: Issue occured while attempting to save. Contact system admin.";
                            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                            return msg;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
                if (ex.Message.Contains("Violation of UNIQUE KEY"))
                {
                    string msg = "Error 08: The Index you are trying to save already exists!";
                    ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                    return msg;
                }
                else if (ex.Message.Contains("String or binary data would be truncated"))
                {
                    string msg = "Error 8a: Entry too long! 30 characters maximum, including blank spaces.";
                    ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                    return msg;
                }
                else
                {
                    string msg = "Error 09: Issue occured while attempting to save index. Contact system admin.";
                    ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('"+ msg + "');", true);
                    return msg;
                }
            }
        }
        

        // WRITE INDEX SHEET PAGE CONTENT. HELPER
        private Tuple<string,string> writeIndexPage(List<string> indexRecord, int currentCount, int totalCount)
        {
            try
            {
                string indexPageString = "";

                //Get index string
                string indexString = indexRecord.Last();
                string jobName = indexString.Substring(0, indexString.Length - 14);
                Image imgBarcode = new Image();
                imgBarcode.ImageUrl = string.Format("ShowCode39BarCode.ashx?code={0}&ShowText=0&Height=65", indexString);

                // Write Index sheet page content if IE browser
                System.Web.HttpBrowserCapabilities browser = Request.Browser;
                if (browser.Browser == "InternetExplorer" || browser.Browser == "IE")
                {
                    // Write to index page
                    indexPageString +=
                        "<div style='font-size:40px; font-family:Arial; font-weight:bold; text-align:center;padding-top:50px;'>" + jobName + " - Index Header" + "</div>" +
                        "<div>" +
                            "<div style='margin-top:70px;text-align:center;'>" +
                                "<img src='" + imgBarcode.ImageUrl + "' height='45px' width='480px' style='border:none;outline:none;'> " +
                            "</div>" +
                            "<div style='font-size:19px;padding-top:1px;font-family:arial;text-align:center;'>" + indexString + "</div>" +
                            "<div style='width:480px; margin-top:210px;float:right;margin-right:-170px;' class='rotate'>" +
                                "<img src='" + imgBarcode.ImageUrl + "' height='45px' width='100%' style='border:none;outline:none;' > " +
                                "<div style='font-size:19px;font-family:arial;text-align:center;width:100%;' >" + indexString + "</div>" +
                            "</div>" +
                        "</div>";

                    // Remove extra space if it's the last page to print
                    if (totalCount == currentCount)
                    {
                        indexPageString +=
                            "<table class='dataSection' style='margin-top:500px; margin-left:130px;padding-top:10px;display:block;'>" +
                                "<tr>" +
                                    "<td style='font-size:20px;'> INDEX STRING: </td>" +
                                    "<td style='font-size:20px; padding-left:15px;'>" + indexString.ToUpper() + "</td>" +
                                "</tr>";
                    }
                    else
                    {
                        indexPageString +=
                           "<table class='dataSection' style='margin-top:500px;margin-bottom:570px; margin-left:130px;padding-top:10px;display:block;'>" +
                                "<tr>" +
                                    "<td style='font-size:20px;'> INDEX STRING: </td>" +
                                    "<td style='font-size:20px; padding-left:15px;'>" + indexString.ToUpper() + "</td>" +
                                "</tr>";
                    }
                    var regexList = (List<Tuple<string, string, string>>)Session["regexList"];

                    for (int i = 0; i < regexList.Count; i++)
                    {
                        if (indexRecord[i] != string.Empty)
                        {
                            string label = regexList[i].Item1;
                            indexPageString += 
                                "<tr>" +
                                    "<td style='font-size:20px;'>" + label.ToUpper() + ":" + "</td>" +
                                    "<td style='font-size:20px; padding-left:15px;;'>" + indexRecord[i].ToUpper() + "</td>" +
                                "</tr>";
                        }
                    }
                    indexPageString += 
                                "<tr>" +
                                    "<td style='font-size:20px;'>DATE PRINTED: </td>" +
                                    "<td style='font-size:20px; padding-left:15px;'>" + DateTime.Now + "</td>" +
                                "</tr>" +
                            "</table>";
                    return Tuple.Create("pass",indexPageString);
                }
                else // Chrome
                {
                    // Write Index sheet page content
                    indexPageString +=
                        "<div style='font-size:50px; font-family:Arial; font-weight:bold; text-align:center;padding-top:50px;'>" + jobName + " - Index Header" + "</div>" +
                        "<div>" +
                            "<div style='margin-top:85px;text-align:center;'>" +
                                "<img src='" + imgBarcode.ImageUrl + "' height='50px' width='600px'> " +
                            "</div>" +
                            "<div style='font-size:24px;padding-top:1px;font-family:arial;text-align:center;'>" + indexString + "</div>" +
                            "<div style='width:600px; margin-top:250px;float:right;margin-right:-130px;' class='rotate'>" +
                                "<img src='" + imgBarcode.ImageUrl + "' height='50px' width='100%' > " +
                                "<div style='font-size:25px;font-family:arial;text-align:center;width:100%;' >" + indexString + "</div>" +
                            "</div>" +
                        "</div>";

                    // Remove extra space if it's the last page to print
                    if (currentCount == totalCount)
                    {
                        indexPageString +=
                            "<table class='dataSection' style='margin-top:630px; margin-left:250px;padding-top:10px;display:block;'>" +
                                "<tr style='padding:0px 0px 0px 0px;'>" +
                                    "<td style='font-size:25px;'> INDEX STRING: </td>" +
                                    "<td style='font-size:25px; padding-left:15px;'>" + indexString.ToUpper() + "</td>" +
                                "</tr>";
                    }
                    else
                    {
                        indexPageString +=
                           "<table class='dataSection' style='margin-top:630px;margin-bottom:570px; margin-left:250px;padding-top:10px;display:block;'>" +
                                "<tr>" +
                                    "<td style='font-size:25px;'> INDEX STRING: </td>" +
                                    "<td style='font-size:25px; padding-left:15px;'>" + indexString.ToUpper() + "</td>" +
                                "</tr>";
                    }

                    var regexList = (List<Tuple<string, string, string>>)Session["regexList"];

                    for (int i = 0; i < regexList.Count; i++)
                    {
                        if (indexRecord[i] != string.Empty)
                        {
                            string label = regexList[i].Item1;
                            indexPageString += 
                                "<tr>" +
                                    "<td style='font-size:25px;'>" + label.ToUpper() + ":" + "</td>" +
                                    "<td style='font-size:25px; padding-left:15px;'>" + indexRecord[i].ToUpper() + "</td>" +
                                "</tr>";
                        }
                    }
                    indexPageString += 
                                "<tr>" +
                                    "<td style='font-size:25px;'>DATE PRINTED: </td>" +
                                    "<td style='font-size:25px; padding-left:15px;'>" + DateTime.Now + "</td>" +
                                "</tr>" +
                            "</table>";
                    return Tuple.Create("pass", indexPageString);
                }
            }
            catch(Exception ex)
            {
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
                string msg = "Error: " + ex.Message;
                return Tuple.Create(msg, "");
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
                        cmd.CommandText = "UPDATE INDEX_DATA SET PRINTED=@printed WHERE BARCODE=@barcodeIndex";
                        cmd.Parameters.AddWithValue("@printed", 1);
                        cmd.Parameters.AddWithValue("@barcodeIndex", indexString);
                        con.Open();
                        int result = cmd.ExecuteNonQuery();
                        if (result == 1)
                        {
                            counter++;
                        }
                        else
                        {
                            string msg = "Error 11: Index saved, but issue occured while attempting to set it to PRINTED. Contact system admin.";
                            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                            clearFields();
                            return msg;
                        }

                        // Confirmation msg & back to unprinted indexes gridview
                        if (counter == 1)
                        {
                            return "pass";
                        }
                        else
                        {
                            string msg = "Error 12: Index saved, but issue occured while attempting to set it to PRINTED. Contact system admin.";
                            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                            clearFields();
                            return msg;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "Error 13: Index saved, but issue occured while attempting to set it to PRINTED. Contact system admin.";
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
                return msg;
            }
        }



        // SET INDEX AS PRINTED AFTER INDEX SHEET PRINTOUT 
        protected void setAsPrinted()
        {
            try
            {
                formPanel.Visible = true;
                satisfied.Visible = false;
                linkToUnprinted.Visible = true;
                formPanelJobSelection.Visible = true;
                indexCreationSection.Visible = true;

                // For manual entries
                if (ViewState["manualEntries"] != null)
                {
                    var manualEntries = (List<string>)ViewState["manualEntries"];
                    string result = setIndexAsPrinted(manualEntries.Last());
                    if (result.Contains("Error"))
                    {
                        string msg1 = result;
                        ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg1 + "');", true);
                        return;
                    }
                    string msg = "Index string saved and set as PRINTED.";
                    string color = "green;";
                    onScreenMsg(msg, color, "manual");
                    clearFields();
                    ScriptManager.RegisterStartupScript(Page, Page.GetType(), "fadeoutOp", "FadeOut();", true);
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
                            string msg1 = result;
                            ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg1 + "');", true);
                            return;
                        }
                        else if (result == "pass") countPass++;
                    }
                    string msg = countPass + " index string(s) saved and set as PRINTED.";
                    string color = "green;";
                    onScreenMsg(msg, color, "file");
                    ScriptManager.RegisterStartupScript(Page, Page.GetType(), "fadeoutOp", "FadeOut2();", true);
                }

                Panel p = Master.FindControl("footerSection") as Panel;
                p.Visible = true;
            }
            catch (Exception ex)
            {
                string msg = "Error 14: Issue occured while attempting to set job as PRINTED. Contact system admin." ;
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
        }


        // GO TO QUESTION
        protected void goToQuestion_Click(object sender, EventArgs e)
        {
            indexPage.Visible = false;
            formPanel.Visible = true;
            satisfied.Visible = true;
            linkToUnprinted.Visible = false;
            formPanelJobSelection.Visible = false;
            indexCreationSection.Visible = false;
            Panel p = Master.FindControl("footerSection") as Panel;
            p.Visible = true;
        }



        // BACK TO BLANK FORM
        protected void backToForm()
        {
            formPanel.Visible = true;
            satisfied.Visible = false;
            linkToUnprinted.Visible = true;
            formPanelJobSelection.Visible = true;
            indexCreationSection.Visible = true;
            
            ViewState["manualEntries"] = null;
            ViewState["fileContent"] = null;
            TextBox t = this.Master.FindControl("MainContent").FindControl("label1Box") as TextBox;
            if (t.Visible) t.Focus();

            if(ViewState["filePrintCancelled"] != null)
            {
                int countPass = (int)ViewState["filePrintCancelled"];

                string msg = countPass + " index string(s) saved but not set as PRINTED";
                string color = "green;";
                onScreenMsg(msg, color, "file");
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "fadeoutOp", "FadeOut2();", true);
            }

            if (ViewState["manualPrintCancelled"] != null)
            {
                string msg = "Index string saved but not set as PRINTED";
                string color = "green;";
                onScreenMsg(msg, color, "manual");
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "fadeoutOp", "FadeOut();", true);
            }

            Panel p = Master.FindControl("footerSection") as Panel;
            p.Visible = true;
        }



        // (HIDDEN) 'GENERATE JOBS' CLICKED: GET YOUR ACCESSIBLE JOBS.
        protected void selectJob_Click(Object sender, EventArgs e)
        {
            try
            {   
                // First, get current user id via name.
                string user = HttpContext.Current.User.Identity.Name.Split('\\').Last();
                List<int> jobIdList = new List<int>();
                noJobsFound.Visible = false;
                int opID = Helper.getUserId(user);
                if (opID == 0)
                {
                    noJobsFound.Visible = true;
                    return;
                }

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
                        bool isAdmin =false;
                        if (result != null)
                            isAdmin = (bool)cmd.ExecuteScalar();
                        cmd.Parameters.Clear(); 

                        // If Admin, get all jobs    
                        if (isAdmin ==  true)
                        {
                            cmd.CommandText =   "SELECT ABBREVIATION " +
                                                "FROM JOB " +
                                                "WHERE ACTIVE=1 " +
                                                "ORDER BY ABBREVIATION ASC";
                        }
                        else // Else get assigned jobs only.
                        {
                            cmd.CommandText =   "SELECT ABBREVIATION " +
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
                string msg = "Error 17: Issue occured while attempting to retrieve jobs accessible to you. Contact system admin.";
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
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
                                            item.Attributes.Add("style", "color:#009900;font-weight:bold;");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "Error 18: Issue occured while attempting to color configured jobs in dropdown. Contact system admin.";
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
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
                string msg = "Error 01: Issue occured while attempting to identify the selected Job. Contact system admin.";
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
                return 0;
            }
        }



        // CLEAR TEXT FIELDS.
        private void clearFields()
        {
            try
            {
                List<TextBox> textBoxList = new List<TextBox>();
                textBoxList.AddRange(new TextBox[] { label1Box, label2Box, label3Box, label4Box, label5Box, label6Box, label7Box, label8Box, label9Box, label10Box, });
                List<DropDownList> dropdownsList = new List<DropDownList>();
                dropdownsList.AddRange(new DropDownList[] { label1Dropdown, label2Dropdown, label3Dropdown, label4Dropdown, label5Dropdown, 
                                                            label6Dropdown, label7Dropdown, label8Dropdown, label9Dropdown, label10Dropdown, });

                foreach (var textBox in textBoxList)
                {
                    if (textBox.Visible == true) textBox.Text = string.Empty;
                }
                foreach (var dropdown in dropdownsList)
                {
                    if (dropdown.Visible == true) dropdown.SelectedValue = "";
                }
            }
            catch (Exception ex)
            {
                string msg = "Error 19: Issue occured while attempting to clear text fields. Contact system admin.";
                ScriptManager.RegisterStartupScript(Page, Page.GetType(), "myalert", "alert('" + msg + "');", true);
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
        }


        // PRINT VARIOUS MSG ON SCREEN INSTEAD OF A POPUP
        private void onScreenMsg(string msg, string color, string from)
        {
            var screenMsg = new TableCell();
            var screenMsgRow = new TableRow();
            screenMsg.Text = msg;
            screenMsg.Attributes["style"] = "font-size:14px; color:" + color;
            screenMsgRow.Cells.Add(screenMsg);
            if (from == "file")fileEntryMsg.Rows.Add(screenMsgRow);
            else if (from == "manual") manualEntryMsg.Rows.Add(screenMsgRow);
        }

        // 'YES' OR 'NO' CLICKED.
        protected void satisfied_Click(object sender, EventArgs e)
        {
            try
            {
                uploadedFileMenu.Visible = false;
                Control c = sender as LinkButton;
                if (c != null)
                {
                    if (c.ID == "yesBtn")
                        setAsPrinted();
                    else 
                        backToForm();
                    satisfied.Visible = false;
                }
            }
            catch (Exception ex)
            {
                Response.Redirect("~/ErrorPage.aspx");

                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
            }
        }


        // CHECK IF NOTHING WAS ENTERED IN MANUAL ENTRIES.
        private bool isNothingEntered()
        {
            try
            {
                bool noEntry = true;
                for (int i = 1; i <= 10; i++)
                {
                    TextBox c = this.Master.FindControl("MainContent").FindControl("label" + i + "Box") as TextBox;
                    DropDownList d = this.Master.FindControl("MainContent").FindControl("label" + i + "Dropdown") as DropDownList;

                    if ((c != null && c.Visible == true && c.Text != string.Empty) || (d != null && d.Visible == true && d.SelectedValue != string.Empty))
                        noEntry = false;
                }
                if (noEntry == true)
                {
                    string msg = "All entries can not be blank. At least one index data must be entered.";
                    string color = "#ff3333;";
                    onScreenMsg(msg, color, "manual");
                    return true;
                }
                else return false;
            }
            catch(Exception ex)
            {
                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
                return true;
            }
        }
    }
}

