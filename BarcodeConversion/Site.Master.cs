using System;
using System.Web.UI;
using System.Data.SqlClient;
using BarcodeConversion.App_Code;
using System.Web.UI.WebControls;

namespace BarcodeConversion
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {   
            // SHOW 'SETTINGS' BUTTON IF ADMIN. IF NEW, SAVE USER.
            bool isAdmin = false;
            try
            {
                string user = Environment.UserName;
                if (user != null) 
                {
                    using (SqlConnection con = Helper.ConnectionObj)
                    {
                        using (SqlCommand cmd = con.CreateCommand())
                        {
                            // If user exists, get Admin status
                            cmd.CommandText = "SELECT ADMIN FROM OPERATOR WHERE NAME=@user";
                            cmd.Parameters.AddWithValue("@user", user);
                            con.Open();
                            object result = cmd.ExecuteScalar();
                            if (result != null)
                                isAdmin = (bool)cmd.ExecuteScalar();
                            else 
                            {
                                // If user doesn't exist, register user and set Admin status to Operator.
                                using (SqlCommand cmd2 = con.CreateCommand()) 
                                {
                                    cmd2.CommandText = "INSERT INTO OPERATOR (NAME, ADMIN) VALUES(@user,@admin)";
                                    cmd2.Parameters.AddWithValue("@user", user);
                                    cmd2.Parameters.AddWithValue("@admin", 0);
                                    try
                                    {
                                        cmd2.ExecuteNonQuery();
                                    }
                                    catch (SqlException ex)
                                    {
                                        string msg = "Issue occured trying to save operator. Please contact system admin. " + Environment.NewLine + ex.Message;
                                        Response.Redirect("~/ErrorPage.aspx");

                                        // Log the exception and notify system operators
                                        ExceptionUtility.LogException(ex);
                                        ExceptionUtility.NotifySystemOps(ex);
                                        
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Redirect("~/ErrorPage.aspx");

                // Log the exception and notify system operators
                ExceptionUtility.LogException(ex);
                ExceptionUtility.NotifySystemOps(ex);
                
            }
            if (isAdmin) settings.Visible = true;


            Control c = Helper.GetPostBackControl(this.Page);
            // Show/Hile footer before printing
            if (c != null)
            {
                if (c.ID == "printIndexesBtn" || c.ID == "saveAndPrint" || c.ID == "printBarcodeBtn" || c.ID == "reprintBtn")
                {
                    footerSection.Visible = false;
                }
            }
            else
                footerSection.Visible = true;
        }
        
    }
}