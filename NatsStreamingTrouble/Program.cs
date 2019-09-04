using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using NATS.Client;
using STAN.Client;

namespace NatsStreamingTrouble
{
    public class Program
    {
        static void Main(string[] args)
        {
            //Choose one method

            DeadlockInStanOptions();

            //DeadlockWhenDisconnectedAllNodes();

            //WorkingNatsOptions();

            Console.ReadLine();

        }

        //Deadlock when i use simple stan configuration for cluster
        static async void DeadlockInStanOptions()
        {
            var cf = new StanConnectionFactory();

            var opts = StanOptions.GetDefaultOptions();
            opts.NatsURL = "nats://127.0.0.1:4223,nats://127.0.0.1:4224";
            opts.PingInterval = 500;

            var opts2 = StanOptions.GetDefaultOptions();
            opts2.NatsURL = "nats://127.0.0.1:4222";


            using (var producer = cf.CreateConnection("test-cluster", "appname", opts))
            using (var consumer = cf.CreateConnection("test-cluster", "appname2", opts2))
            {
                using (consumer.Subscribe("foo", (sender, handlerArgs) => Console.WriteLine(
                    System.Text.Encoding.UTF8.GetString(handlerArgs.Message.Data))))
                {
                    Console.WriteLine("Connected. ConnServ {0}.", producer?.NATSConnection?.ConnectedUrl);

                    //first success send
                    producer?.Publish("foo", System.Text.Encoding.UTF8.GetBytes("first success hello"));

                    //stop connected stan container, simulation disconnect 
                    var firstUrl = producer?.NATSConnection?.ConnectedUrl;
                    var clusterContainerName = firstUrl.Contains("4224") ? "nats-2" : "nats-1";
                    DockerController.SendStopContainer(clusterContainerName);

                    //simulation of sending 10 messages to diconnected node
                    for (var i = 0; i < 10; i++)
                    {
                        var success = false;
                        while (!success)
                        {
                            Console.WriteLine($"Publishing.  ConnState {producer?.NATSConnection?.State}. Connected to {producer?.NATSConnection?.ConnectedUrl}");
                            try
                            {
                                producer?.Publish("foo", System.Text.Encoding.UTF8.GetBytes("hello"));
                                success = true;
                            }
                            catch (Exception e)
                            {
                                System.Console.WriteLine("exception:  {0}", e.Message);
                            }

                            await Task.Delay(1000);
                        }
                    }

                    DockerController.SendStartContainer(clusterContainerName);
                }
            }
        }

        //After second error Connection close i get deadlock
        static async void DeadlockWhenDisconnectedAllNodes()
        {
            var cf = new StanConnectionFactory();

            Options nopts = ConnectionFactory.GetDefaultOptions();

            nopts.MaxReconnect = Options.ReconnectForever;
            nopts.MaxPingsOut = 2;
            nopts.PingInterval = 500;
            nopts.Servers = new[] { "nats://127.0.0.1:4223"};
            nopts.ReconnectedEventHandler = (o, a) =>
            {
                Debug.WriteLine("Reconnected.");
            };

            var opts = StanOptions.GetDefaultOptions();
            opts.NatsConn = new ConnectionFactory().CreateConnection(nopts);

            var opts2 = StanOptions.GetDefaultOptions();
            opts2.NatsURL = "nats://127.0.0.1:4222";


            using (var producer = cf.CreateConnection("test-cluster", "appname", opts))
            using (var consumer = cf.CreateConnection("test-cluster", "appname2", opts2))
            {
                using (consumer.Subscribe("foo", (sender, handlerArgs) => Console.WriteLine(
                    System.Text.Encoding.UTF8.GetString(handlerArgs.Message.Data))))
                {
                    Console.WriteLine("Connected. ConnServ {0}.", producer?.NATSConnection?.ConnectedUrl);

                    //first success send
                    producer?.Publish("foo", System.Text.Encoding.UTF8.GetBytes("first success hello"));

                    //stop connected stan container, simulation disconnect 
                    var firstUrl = producer?.NATSConnection?.ConnectedUrl;
                    var clusterContainerName = firstUrl.Contains("4224") ? "nats-2" : "nats-1";
                    DockerController.SendStopContainer(clusterContainerName);

                    //simulation of sending 10 messages to diconnected node
                    for (var i = 0; i < 10; i++)
                    {
                        var success = false;
                        while (!success)
                        {
                            Console.WriteLine($"Publishing.  ConnState {producer?.NATSConnection?.State}. Connected to {producer?.NATSConnection?.ConnectedUrl}");
                            try
                            {
                                producer?.Publish("foo", System.Text.Encoding.UTF8.GetBytes("hello"));
                                success = true;
                            }
                            catch (Exception e)
                            {
                                System.Console.WriteLine("exception:  {0}", e.Message);
                            }

                            await Task.Delay(1000);
                        }
                    }

                    DockerController.SendStartContainer(clusterContainerName);
                }
            }
        }

        //it works, but sometimes i get deadlock too
        static async void WorkingNatsOptions()
        {
            var cf = new StanConnectionFactory();

            Options nopts = ConnectionFactory.GetDefaultOptions();

            nopts.MaxReconnect = Options.ReconnectForever;
            nopts.MaxPingsOut = 2;
            nopts.PingInterval = 500;
            nopts.Servers = new[] { "nats://127.0.0.1:4223", "nats://127.0.0.1:4224" };
            nopts.ReconnectedEventHandler = (o, a) =>
            {
                Debug.WriteLine("Reconnected.");
            };

            var opts = StanOptions.GetDefaultOptions();
            opts.NatsConn = new ConnectionFactory().CreateConnection(nopts);

            var opts2 = StanOptions.GetDefaultOptions();
            opts2.NatsURL = "nats://127.0.0.1:4222";


            using (var producer = cf.CreateConnection("test-cluster", "appname", opts))
            using (var consumer = cf.CreateConnection("test-cluster", "appname2", opts2))
            {
                using (consumer.Subscribe("foo", (sender, handlerArgs) => Console.WriteLine(
                    System.Text.Encoding.UTF8.GetString(handlerArgs.Message.Data))))
                {
                    Console.WriteLine("Connected. ConnServ {0}.", producer?.NATSConnection?.ConnectedUrl);

                    //first success send
                    producer?.Publish("foo", System.Text.Encoding.UTF8.GetBytes("first success hello"));

                    //stop connected stan container, simulation disconnect 
                    var firstUrl = producer?.NATSConnection?.ConnectedUrl;
                    var clusterContainerName = firstUrl.Contains("4224") ? "nats-2" : "nats-1";
                    DockerController.SendStopContainer(clusterContainerName);

                    //simulation of sending 10 messages to diconnected node
                    for (var i = 0; i < 10; i++)
                    {
                        var success = false;
                        while (!success)
                        {
                            Console.WriteLine($"Publishing.  ConnState {producer?.NATSConnection?.State}. Connected to {producer?.NATSConnection?.ConnectedUrl}");
                            try
                            {
                                producer?.Publish("foo", System.Text.Encoding.UTF8.GetBytes("hello"));
                                success = true;
                            }
                            catch (Exception e)
                            {
                                System.Console.WriteLine("exception:  {0}", e.Message);
                            }

                            await Task.Delay(1000);
                        }
                    }

                    DockerController.SendStartContainer(clusterContainerName);
                }
            }
        }
    }
}
