using System;
using System.IO;
using AventStack.ExtentReports;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using TestSharp.Source.Reporter;
using TestSharp.Source.Reports;
using TestSharp.Source.Utilities;

namespace TestSharp.Source.DataHelpers
{
    public static class JsonManager
    {
        private static readonly string _jsonDir = DirectoryManager.GetBinDirectory() + @"\";

        /// <summary>
        /// Retrieves value from jsonFile
        /// </summary>
        /// <param name="jsonFile"></param>
        /// <returns></returns>
        public static JObject ReturnJsonObject(string jsonFile)
        {
            var jsonObj = JObject.Parse(File.ReadAllText(_jsonDir + jsonFile + ".json"));
            return jsonObj;
        }
        
        /// <summary>
        /// Retrieves list from json file
        /// </summary>
        /// <param name="jsonFile"></param>
        /// <returns></returns>
        public static JObject GetListFromJson(string jsonFile)
        {
            var jsonObj = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(_jsonDir + jsonFile + ".json"));
            return jsonObj;
        }

        /// <summary>
        /// Updates value in json file by jToken
        /// </summary>
        /// <param name="jsonFile"></param>
        /// <param name="jsonToken"></param>
        /// <param name="newValue"></param>
        public static void UpdateJsonValue(string jsonFile, string  jsonToken, string newValue)
        {
            try
            {
                string json = File.ReadAllText(_jsonDir + jsonFile + ".json");
                JObject jObject = JsonConvert.DeserializeObject(json) as JObject;

                JToken jToken = jObject.SelectToken(jsonToken);

                jToken.Replace(newValue);

                string updatedJsonString = jObject.ToString();

                File.WriteAllText(_jsonDir + jsonFile + ".json", updatedJsonString);
            }
            catch (NullReferenceException ex)
            {
                ExtentManager.LogStep($"JSON token '{jsonToken}' is missing in the data file '{jsonFile}'. Unable to set value to '{newValue}'.", Status.Fail);
                Assert.Warn($"JSON token '{jsonToken}' is missing in the data file '{jsonFile}'. Unable to set value to '{newValue}'.");
            }
        }
        
        /// <summary>
        /// Updates value in json Object by jToken
        /// </summary>
        /// <param name="jObject"></param>
        /// <param name="jsonToken"></param>
        /// <param name="newValue"></param>
        public static void UpdateJsonValue(JObject jObject, string  jsonToken, string newValue)
        {
            try
            {
                JToken jToken = jObject.SelectToken(jsonToken);
                jToken.Replace(newValue);
            }
            catch (NullReferenceException ex)
            {
                ExtentManager.LogStep($"JSON token '{jsonToken}' is missing in the json object'. Unable to set value to '{newValue}'.");
            }
        }

    }
}