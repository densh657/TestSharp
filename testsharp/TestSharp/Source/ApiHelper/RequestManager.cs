using System.IO;
using System.Net;
using AventStack.ExtentReports;
using AventStack.ExtentReports.MarkupUtils;
using Newtonsoft.Json.Linq;
using TestSharp.Source.Reporter;
using TestSharp.Source.Reports;

namespace TestSharp.Source.ApiHelpers
{
    public static class RequestManager
    {
        public static HttpStatusCode StatusCode;
        
        public static HttpStatusCode ReturnStatusCode(HttpWebRequest httpWebRequest)
        {
            HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();
            StatusCode = response.StatusCode;

            return StatusCode;
        }
        
        public static JObject Post(string endpoint, JObject request, bool attachReport = true)
        {
            if (attachReport)
            {
                ExtentManager.LogStep($"Sending POST Request to the Endpoint: <br> <a href=\"{endpoint}\">{endpoint}</a>");
                ExtentManager.LogStep(MarkupHelper.CreateCodeBlock(request.ToString(), CodeLanguage.Json));
            }

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(endpoint);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.UseDefaultCredentials = true;
            httpWebRequest.PreAuthenticate = true;
            httpWebRequest.UserAgent = "Auto Test";
            httpWebRequest.Credentials = CredentialCache.DefaultCredentials;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                JObject json = request;
                streamWriter.Write(json);
            }

            try
            {
                HttpWebResponse httpResponse = (HttpWebResponse) httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    string result = streamReader.ReadToEnd();
                    if (attachReport)
                    {
                        ExtentManager.LogStep($"Got the response with status code: {ReturnStatusCode(httpWebRequest)}");
                        ExtentManager.LogStep(MarkupHelper.CreateCodeBlock(result, CodeLanguage.Json));
                    }
                    return JObject.Parse(result);
                }
            }
            catch (WebException ex)
            {
                using (var stream = ex.Response.GetResponseStream()) 
                using (var streamReader = new StreamReader(stream))
                {
                    ExtentManager.LogStep($"Got bad response: ", Status.Fail);
                    ExtentManager.LogStep(MarkupHelper.CreateCodeBlock(streamReader.ReadToEnd(), CodeLanguage.Json), Status.Fail);
                    ExtentManager.LogStep($"{ReturnStatusCode(httpWebRequest)}");
                }

                return null;
            }
        }

        public static JObject Get(string endpoint, string request)
        {
            var httpWebRequest = HttpWebRequest.Create(endpoint);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";
 
            try
            {
                var response = (HttpWebResponse)httpWebRequest.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
 
                return JObject.Parse(responseString);
            }
            catch (WebException)
            {
                return null;
            }
        }

    }
}