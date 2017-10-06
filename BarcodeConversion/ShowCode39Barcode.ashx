<%@ WebHandler Language="C#" Class="ShowCode39Barcode" %>

using System;
using System.Web;
using BarcodeConversion.App_Code;

public class ShowCode39Barcode : IHttpHandler {

    public void ProcessRequest (HttpContext context) {
        context.Response.ContentType = "image/gif";

        var barcode = new Code39Barcode();

        // Read in the user's inputs from the querystring
        barcode.BarcodeText = context.Request.QueryString["code"].ToUpper();
        barcode.ShowBarcodeText = context.Request.QueryString["ShowText"] != "0";
        if (context.Request.QueryString["thickness"] == "3")
            barcode.BarcodeWeight = BarcodeWeight.Large;
        else if (context.Request.QueryString["thickness"] == "2")
            barcode.BarcodeWeight = BarcodeWeight.Medium;
        else 
            barcode.BarcodeWeight = BarcodeWeight.Small;

        if (!string.IsNullOrEmpty(context.Request.QueryString["Height"]))
            barcode.Height = Convert.ToInt32(context.Request.QueryString["Height"]);

        context.Response.BinaryWrite(barcode.Generate());
    }

    public bool IsReusable {
        get {
            return false;
        }
    }

}