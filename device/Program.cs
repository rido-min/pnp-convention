using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using PnPConvention;
using System;
using System.Text;
using System.Threading.Tasks;

namespace device
{
    class Program
    {
        static string CS = Environment.GetEnvironmentVariable("CS");
        static string mid = "dtmi:com:example:TemperatureController;1";
        static async Task Main(string[] args)
        {
            var dc = DeviceClient.CreateFromConnectionString(CS, TransportType.Mqtt, new ClientOptions { ModelId = mid });
            var client = new PnPClient(dc);
            
            await client.ReportComponentPropertyAsync("deviceInfo", "manuf", "rido");
            var manuf = await client.ReadReportedComponentPropertyAsync<string>("deviceInfo", "manuf");
            
            Console.WriteLine(manuf);

            await client.SendComponentTelemetryValueAsync("themorstat1", JsonConvert.SerializeObject(new { temperature = 12 }));

            await client.SetCommandHandlerAsync("reboot", async (req, ctx) => {
                Console.WriteLine(req.Name);
                Console.WriteLine(req.DataAsJson);
                await Task.Delay(1);
                return new MethodResponse(UTF8Encoding.UTF8.GetBytes("{}"), 200);
            }, null);

            await client.SetComponentCommandHandlerAsync("thermostat1", "getMaxMinReport", async (req, ctx) => {
                Console.WriteLine(req.Name);
                Console.WriteLine(req.DataAsJson);
                await Task.Delay(1);
                return new MethodResponse(UTF8Encoding.UTF8.GetBytes("{}"), 200);
            }, null);

            client.SetDesiredPropertyUpdateCommandHandler("thermostat1", (twin) => {
                Console.WriteLine(twin.ToJson());
                var targetTemp1 = twin.GetPropertyValue<int>("thermostat1", "targetTemperature");
                Console.WriteLine(targetTemp1);
            });

            client.SetDesiredPropertyUpdateCommandHandler("thermostat2", (twin) => {
                Console.WriteLine(twin.ToJson());
                var targetTemp2 = twin.GetPropertyValue<int>("thermostat2", "targetTemperature");
                Console.WriteLine(targetTemp2);
            });

            Console.ReadLine();
        }
    }
}
