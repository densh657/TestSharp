using TestSharp.Source.DataHelpers;

namespace TestSharp.TestCases
{
    public class DataSetup
    {
        public string jsonPath;

        public DataSetup(string jsonPath)
        {
            this.jsonPath = jsonPath;
        }

        public string streetAddress => (string)JsonManager.ReturnJsonObject(jsonPath)["propertyAddress"]?["streetAddress"];
        public string addressLine2 => (string)JsonManager.ReturnJsonObject(jsonPath)["propertyAddress"]?["addressLine2"];
        public string city => (string)JsonManager.ReturnJsonObject(jsonPath)["propertyAddress"]?["city"]; 
        public string zipCode => (string)JsonManager.ReturnJsonObject(jsonPath)["propertyAddress"]?["zipCode"];

        public string firstName => (string)JsonManager.ReturnJsonObject(jsonPath)["propertyAddress"]?["firstName"];
        public string lastName => (string)JsonManager.ReturnJsonObject(jsonPath)["propertyAddress"]?["lastName"];
        public string fullName => (string)JsonManager.ReturnJsonObject((jsonPath))["propertyAddress"]?["fullName"];
        public string company => (string)JsonManager.ReturnJsonObject((jsonPath))["propertyAddress"]?["company"];
        public string phoneNumber => (string)JsonManager.ReturnJsonObject(jsonPath)["propertyAddress"]?["phoneNumber"];
        public string emailAddress => (string)JsonManager.ReturnJsonObject(jsonPath)["propertyAddress"]?["emailAddress"];
        //
        // public string firstName => (string)JsonManager.ReturnJsonObject(jsonPath)["contactUs"]?["firstName"];
        // public string lastName => (string)JsonManager.ReturnJsonObject(jsonPath)["contactUs"]?["lastName"];
        // public string email => (string)JsonManager.ReturnJsonObject(jsonPath)["contactUs"]?["email"];
        // public string phone => (string)JsonManager.ReturnJsonObject(jsonPath)["contactUs"]?["phone"];
        // public string jobTitle => (string)JsonManager.ReturnJsonObject(jsonPath)["contactUs"]?["jobTitle"];
        // public string company => (string)JsonManager.ReturnJsonObject(jsonPath)["contactUs"]?["company"];
        // public string country => (string)JsonManager.ReturnJsonObject(jsonPath)["contactUs"]?["country"];
        // public string comments => (string)JsonManager.ReturnJsonObject(jsonPath)["contactUs"]?["comments"];
    }
}