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
            
            await client.ReportComponentPropertyAsync("deviceInfo", "manufacturer", "rido");
            var manufacturer = await client.ReadReportedComponentPropertyAsync<string>("deviceInfo", "manufacturer");

            await client.SendComponentTelemetryValueAsync("themorstat1", JsonConvert.SerializeObject(new { temperature = 11 }));
            await client.SendComponentTelemetryValueAsync("themorstat2", JsonConvert.SerializeObject(new { temperature = 22 }));

            client.SetDesiredPropertyUpdateCommandHandler("thermostat1", async (twin) => {
                Console.WriteLine("T1 " + twin.ToJson());
                var targetTemp1 = twin.GetPropertyValue<int>("thermostat1", "targetTemperature");
                await client.AckDesiredPropertyReadAsync("thermostat1", "targetTemperature", targetTemp1, StatusCodes.Completed, "tt1 received", twin.Version);
                Console.WriteLine(targetTemp1);
            });

            client.SetDesiredPropertyUpdateCommandHandler("thermostat2", async (twin) => {
                Console.WriteLine("T2 " + twin.ToJson());
                var targetTemp2 = twin.GetPropertyValue<int>("thermostat2", "targetTemperature");
                await client.AckDesiredPropertyReadAsync("thermostat2", "targetTemperature", targetTemp2, StatusCodes.Completed, "tt2 received", twin.Version);
                Console.WriteLine(targetTemp2);
            });

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

            await client.SetComponentCommandHandlerAsync("thermostat2", "getMaxMinReport", async (req, ctx) => {
                Console.WriteLine(req.Name);
                Console.WriteLine(req.DataAsJson);
                await Task.Delay(1);
                return new MethodResponse(UTF8Encoding.UTF8.GetBytes("{}"), 200);
            }, null);

            Console.ReadLine();
        }
    }
}
