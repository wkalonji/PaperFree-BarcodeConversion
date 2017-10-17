using System;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using BarcodeConversion.App_Code;

namespace BarcodeConversion
{
    public class Global : HttpApplication
    {

        // Code that runs on application startup
        void Application_Start(object sender, EventArgs e)
        {
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        // Code that runs when an unhandled error occurs
        void Application_Error(object sender, EventArgs e)
        {
            // Get the exception object.
            Exception exc = Server.GetLastError();
            Server.Transfer("~/ErrorPage.aspx");

            // Log the exception and notify system operators
            ExceptionUtility.LogException(exc);
            ExceptionUtility.NotifySystemOps(exc);

            // Clear the error from the server
            Server.ClearError();
        }
    }
}