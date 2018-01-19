using System;
using System.IO;
using System.Net.Mail;
using System.Web;
using System.Linq;

namespace BarcodeConversion.App_Code
{
    public sealed class ExceptionUtility
    {
         // All methods are static, so this can be private 
        private ExceptionUtility() { }

        // Log an Exception 
        public static void LogException(Exception exc)
        {
            try
            {
                string user = HttpContext.Current.User.Identity.Name.Split('\\').Last();
                var location = exc.StackTrace.Split('\n').Last().Split('\\').Last();
                // Include enterprise logic for logging exceptions 
                // Get the absolute path to the log file 
                string logFile = "~/App_Data/ErrorLog.txt";
                logFile = HttpContext.Current.Server.MapPath(logFile);

                // Open the log file for append and write the log
                StreamWriter sw = new StreamWriter(logFile, true);
                sw.WriteLine("\n*************** {0} ***************", DateTime.Now);
                sw.WriteLine("\nOperator: " + user);

                if (exc.InnerException != null)
                {
                    var innerLocation = exc.InnerException.StackTrace.Split('\n').Last().Split('\\').Last();
                    sw.Write("Inner Exception Message: ");
                    sw.WriteLine(exc.InnerException.Message);
                    sw.WriteLine("Location: " + innerLocation);
                    sw.Write("Inner Source: ");
                    sw.WriteLine(exc.InnerException.Source);
                    sw.WriteLine("Function: " + exc.InnerException.TargetSite);
                    sw.Write("Inner Exception Type: ");
                    sw.WriteLine(exc.InnerException.GetType().ToString());
                    if (exc.InnerException.StackTrace != null)
                    {
                        sw.WriteLine("Inner Stack Trace: ");
                        sw.WriteLine(exc.InnerException.StackTrace);
                    }
                }
                sw.WriteLine("\n"); 
                sw.WriteLine("Exception Message: " + exc.Message);
                sw.WriteLine("Location: " + location);
                sw.WriteLine("Source: " + exc.Source);
                sw.WriteLine("Function: " + exc.TargetSite);
                sw.Write("Exception Type: ");
                sw.WriteLine(exc.GetType().ToString());
                sw.WriteLine("Stack Trace: ");
                if (exc.StackTrace != null)
                {
                    sw.WriteLine(exc.StackTrace);
                    sw.WriteLine();
                }
                sw.Close();
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        // Notify System Operators about an exception 
        public static void NotifySystemOps(Exception exc)
        {
            try
            {
                string innerSummary = string.Empty;
                string user = HttpContext.Current.User.Identity.Name.Split('\\').Last();
                var location = exc.StackTrace.Split('\n').Last().Split('\\').Last();
                if (exc.InnerException != null)
                {
                    var innerLocation = exc.InnerException.StackTrace.Split('\n').Last().Split('\\').Last();
                    innerSummary = "Inner Summary: From " + user + " at " + innerLocation + ": " + exc.InnerException.Message;
                }
                string summary = "\nException Summary:\n\nFrom: " + user + "\nLocation: " + location + "\nType: "+ exc.GetType() + "\nMessage: " + exc.Message;
                //emailException(innerSummary + "\n" + summary, exc);
            }
            catch(Exception ex)
            {
                LogException(ex);
            }
         }

        // Email exception
        public static void emailException(string msg, Exception exc)
        {
            string to = "c-wkalonji@pa.gov";
            string from = "c-wkalonji@pa.gov";
            MailMessage message = new MailMessage(from, to);
            message.Subject = "Exception: " + exc.GetType() + " At: " + exc.StackTrace.Split('\n').Last().Split('\\').Last();
            message.Body = msg;

            SmtpClient client = new SmtpClient();
            client.Host = "smtp.googlemail.com";
            client.Port = 587;
            client.UseDefaultCredentials = true;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
            //client.Credentials = new NetworkCredential("glory.ebtutor@gmail.com", "tutor2016");

            try
            {
                client.Send(message);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }
    }
}