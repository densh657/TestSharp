using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Extensions.Configuration;

namespace TestSharp.Source.Utilities
{
    public class Common
    {
        
        private static readonly string binDirectory = DirectoryManager.GetBinDirectory() + @"\";

        public static string ConvertImageToBase64(string path)
        {
            using (Image image = Image.FromFile(binDirectory + path))
            {
                using (MemoryStream m = new MemoryStream())
                {
                    image.Save(m, image.RawFormat);
                    byte[] imageBytes = m.ToArray();

                    string base64String = Convert.ToBase64String(imageBytes);
                    return base64String;
                }
            }
        }

        public static string ConvertXmlToBase64(string path)
        {
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.Load(binDirectory + path); //sample xml content
            var xmlDocString = xmlDoc.InnerXml;
            var xmlDocByteArray = Encoding.ASCII.GetBytes(xmlDocString);
            return Convert.ToBase64String(xmlDocByteArray);
        }

        public static void UpdateXmlValue(string xmlPath, string xPath, string value)
        {
            XDocument doc = XDocument.Load(binDirectory + xmlPath);
            doc.XPathSelectElement(xPath).Value = value;
            doc.Save(binDirectory + xmlPath);
        }

        /// <summary>
        /// Retrieves value from appsettings.json
        /// </summary>
        /// <param name="configKey">Key to find value from config file</param>
        /// <returns>string</returns>
        public static string ReadConfig(string configKey)
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(DirectoryManager.GetBinDirectory())
                   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfiguration config = builder.Build();

            return config[configKey];
        }
    }
}