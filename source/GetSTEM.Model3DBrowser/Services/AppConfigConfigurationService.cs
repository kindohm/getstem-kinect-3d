using System.Configuration;
using System.IO;
using System.Xml.Serialization;
using GetSTEM.Model3DBrowser.Models;

namespace GetSTEM.Model3DBrowser.Services
{
    public class AppConfigConfigurationService : IConfigurationService
    {

        public ModelConfiguration GetModelConfiguration()
        {
            var path = ConfigurationManager.AppSettings["ConfigurationFile"];
            var serializer = new XmlSerializer(typeof(ModelConfiguration));
            using (var stream = new FileStream(path, FileMode.Open))
            {
                var result = (ModelConfiguration)serializer.Deserialize(stream);
                return result;
            }
        }
    }
}
