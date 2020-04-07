using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using Microsoft.Extensions.Configuration.FileExtensions;
using Microsoft.Extensions.Configuration.Json;
using uPLibrary.Networking.M2Mqtt;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using uPLibrary.Networking.M2Mqtt.Messages;

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
                try
                {
                    if (drive.DriveType != DriveType.CDRom)
                    {
                        var pct = (float)(1000 * (drive.TotalSize - drive.AvailableFreeSpace) / drive.TotalSize)/10;
                        var topic = mqttConfig.Topic + "/" + drive.Name.Replace(":\\", "");
                        var r = client.Publish(topic, Encoding.UTF8.GetBytes(pct.ToString()), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
                        await Task.Delay(1000);
                    }
                }
                catch (Exception) { }
            }

            client.Disconnect();
        }

        private static IConfiguration BuildConfig()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json");

            return builder.Build();
        }
    }
}
