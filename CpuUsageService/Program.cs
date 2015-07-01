using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Eneter.Messaging.DataProcessing.Serializing;
using Eneter.Messaging.MessagingSystems.MessagingSystemBase;
using Eneter.Messaging.MessagingSystems.WebSocketMessagingSystem;
using Eneter.Messaging.Nodes.Broker;

namespace CpuUsageService
{
    // Message that will be notified.
    public class CpuUpdateMessage
    {
        public float Usage { get; set; }
    }

    class Program
    {


   

        static async Task SendMonitoringMessages(IDuplexBroker broker, CancellationToken cancellationToken)
        {
            var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ISerializer aSerializer = new DataContractJsonStringSerializer();
            while (!cancellationToken.IsCancellationRequested)
            {
                CpuUpdateMessage aMessage = new CpuUpdateMessage();
                aMessage.Usage = cpuCounter.NextValue();
                object aSerializedMessage = aSerializer.Serialize<CpuUpdateMessage>(aMessage);
                broker.SendMessage("MyCpuUpdate", aSerializedMessage);
                try
                {
                    await Task.Delay(500, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                }
            }
        }


        static void Main(string[] args)
        {


            // Create broker.
            IDuplexBrokerFactory aBrokerFactory = new DuplexBrokerFactory();
            IDuplexBroker aBroker = aBrokerFactory.CreateBroker();

            // Communicate using WebSockets.
            IMessagingSystemFactory aMessaging = new WebSocketMessagingSystemFactory();
            IDuplexInputChannel anInputChannel = aMessaging.CreateDuplexInputChannel("ws://127.0.0.1:8843/CpuUsage/");

            anInputChannel.ResponseReceiverConnected += (x, y) =>
            {
                Console.WriteLine("Connected client: " + y.ResponseReceiverId);
            };
            anInputChannel.ResponseReceiverDisconnected += (x, y) =>
            {
                Console.WriteLine("Disconnected client: " + y.ResponseReceiverId);
            };

            // Attach input channel and start listeing.
            aBroker.AttachDuplexInputChannel(anInputChannel);


            var tokenSource = new CancellationTokenSource();

            var sendMonitoringMessages = Task.Run(() => SendMonitoringMessages(aBroker, tokenSource.Token), tokenSource.Token);

            Console.WriteLine("CpuUsageService is running press ENTER to stop.");
            Console.ReadLine();
            tokenSource.Cancel();

            try
            {
                sendMonitoringMessages.Wait();
            }
            catch (AggregateException)
            {
            }
            catch (TaskCanceledException)
            {
            }

            // Detach the input channel and stop listening.
            aBroker.DetachDuplexInputChannel();
        }
    }
}
