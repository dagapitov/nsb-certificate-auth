using System;
using System.Net.Security;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Util;

namespace RabbitMQTest
{
    public class Program
    {
        public static void Main(string[] args)
        {

            try
            {

                var hostName = "mac-rabbitmq";
                var cf = new ConnectionFactory
                {
                    HostName = hostName,
                    VirtualHost = "/",
                    // Client certificate authentication doesn't work without next line (can't find in NServiceBus.RabbitMQ)
                    AuthMechanisms = new AuthMechanismFactory[] { new ExternalMechanismFactory() },
                    Ssl = new SslOption
                    {
                        Enabled = true,
                        ServerName = hostName,
                        //Might need next lines uncommented for self-signed certificates
                        AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateNameMismatch |
                                                SslPolicyErrors.RemoteCertificateChainErrors,
                        CertPath = @"C:\gh\ssl.pfx",
                        CertPassphrase = "MySecretPassword"
                    }
                };

                using (IConnection conn = cf.CreateConnection("ConsoleApp"))
                {
                    using (IModel ch = conn.CreateModel())
                    {
                        ch.QueueDeclare("rabbitmq-dotnet-test", false, false, false, null);
                        ch.BasicPublish("", "rabbitmq-dotnet-test", null,
                            Encoding.UTF8.GetBytes("Hello, World"));
                        BasicGetResult result = ch.BasicGet("rabbitmq-dotnet-test", true);
                        if (result == null)
                        {
                            Console.WriteLine("No message received.");
                        }
                        else
                        {
                            Console.WriteLine("Received:");
                            DebugUtil.DumpProperties(result, Console.Out, 0);
                        }
                        ch.QueueDelete("rabbitmq-dotnet-test");
                    }
                }
            }
            catch (BrokerUnreachableException bex)
            {
                Exception ex = bex;
                while (ex != null)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("inner:");
                    ex = ex.InnerException;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Console.ReadKey();
        }
    }
}