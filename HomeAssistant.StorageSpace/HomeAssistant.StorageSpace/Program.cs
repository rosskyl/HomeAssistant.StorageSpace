using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using Microsoft.Extensions.Configuration.FileExtensions;
using Microsoft.Extensions.Configuration.Json;
using uPLibrary.Networking.M2Mqtt;
using System.Text;
using System.Threading.Tasks;

namespace HomeAssistant.StorageSpace
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var config = BuildConfig();
            var mqttConfig = new MqttConfig();

            config.Bind(mqttConfig);

            MqttClient client = new MqttClient(mqttConfig.Host);

            var clientId = Guid.NewGuid().ToString();
            client.Connect(clientId);

            var drives = DriveInfo.GetDrives();
            foreach (var drive in drives)
            {
                var pct = (double)drive.AvailableFreeSpace / (double)drive.TotalSize * 100;
                var message = string.Format("{0:0.0}", pct);
                var topic = mqttConfig.Topic + "/" + drive.Name.Replace(":\\", "");
                var r = client.Publish(topic, Encoding.UTF8.GetBytes(message));
                await Task.Delay(1000);
            }

            client.Disconnect();
        }

        private static IConfiguration BuildConfig()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            return builder.Build();
        }
    }
}
