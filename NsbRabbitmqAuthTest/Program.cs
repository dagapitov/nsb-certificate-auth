using NServiceBus;
using System;
using System.Threading.Tasks;

namespace NsbRabbitmqAuthTest
{
    class Program
    {
        private const string QueueName = "Sample.RabbitMQ.Commands";

        static void Main()
        {
            AsyncMain().GetAwaiter().GetResult();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        static async Task AsyncMain()
        {
            var endpointConfiguration = new EndpointConfiguration(
                endpointName: QueueName);

            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.EnableInstallers();

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            var connectionString =
                //@"Host=mac-rabbitmq;UseTls=true;Username=ImageGenClient;Password=MySecretPassword";
                "Host=mac-rabbitmq;UseTls=true;CertPath=C:\\gh\\ssl.pfx;CertPassphrase=MySecretPassword";

            transport.ConnectionString(connectionString);

            endpointConfiguration.UsePersistence<InMemoryPersistence>();

            try
            {
                // Initialize the endpoint with the finished configuration
                var endpointInstance = await Endpoint.Start(endpointConfiguration)
                    .ConfigureAwait(false);
                try
                {
                    await SendOrder(endpointInstance)
                        .ConfigureAwait(false);
                }
                finally
                {
                    await endpointInstance.Stop()
                        .ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }
        }

        static async Task SendOrder(IEndpointInstance endpointInstance)
        {
            Console.WriteLine("Press any key to exit");

            var id = Guid.NewGuid();

            var placeOrder = new PlaceOrder
            {
                Id = id
            };
            await endpointInstance.Send(QueueName, placeOrder)
                .ConfigureAwait(false);
            Console.WriteLine($"Sent a PlaceOrder message with id: {id:N}");
        }
    }
    public class PlaceOrder : ICommand
    {
        public Guid Id { get; set; }
    }

    public class PlaceOrderHandler :
    IHandleMessages<PlaceOrder>
    {
        public Task Handle(PlaceOrder message, IMessageHandlerContext context)
        {
            Console.WriteLine($"PlaceOrder {message.Id}");
            return Task.FromResult(true);
        }
    }
}
