using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SmartHome.Data;
using SmartHome.Data.DTO;
using SmartHome.Data.Entities;
using SmartHome.Logic;
using System.Text;
using System.Threading.Channels;

//var factory = new ConnectionFactory { HostName = "localhost" };
//using var connection = factory.CreateConnection();
//using var channel = connection.CreateModel();

//channel.QueueDeclare(queue: "hello",
//    durable: false,
//    exclusive: false,
//    autoDelete: false,
//    arguments: null);

//Console.WriteLine(" [*] Waiting for messages.");

//var consumer = new EventingBasicConsumer(channel);
//consumer.Received += (model, ea) =>
//{
//    var body = ea.Body.ToArray();
//    var message = Encoding.UTF8.GetString(body);

//    Console.WriteLine($" [x] Received {message}");
//    // here channel could also be accessed as ((EventingBasicConsumer)sender).Model
//    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
//};
//channel.BasicConsume(queue: "hello",
//                     autoAck: false,
//                     consumer: consumer);

//Console.WriteLine("Press Enter to exit.");
//Console.ReadLine();

namespace SmatHome.Connector
{
    public class RabbitMQListener
    {
        private readonly string _exchangeName;
        private readonly string _queueName;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMQListener(string exchangeName, string queueName)
        {
            _exchangeName = exchangeName;
            _queueName = queueName;

           /// var factory = new ConnectionFactory() { HostName = "localhost", UserName = "rmuser", Password = "rmpassword" };
            var factory = new ConnectionFactory() { HostName = "192.168.3.21", UserName = "rmuser", Password = "rmpassword" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: _exchangeName, type: "topic", durable: true);
            _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueBind(queue: _queueName, exchange: _exchangeName, routingKey: _queueName);
        }

        public void StartListening(HubConnection connection)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.ConsumerCancelled += (model, ea) =>
            {
                Console.WriteLine("Close");
            };
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                // process message and save to database

                var time = ea.BasicProperties.IsTimestampPresent() ? DateTime.FromFileTimeUtc(ea.BasicProperties.Timestamp.UnixTime) : DateTime.Now;
                // todo: parse time
                var point = JsonConvert.DeserializeObject<PointDto>(message);

                if (point != null)
                {
                    var dataId = point.Id;

                    using (var db = new SmartHomeDbContext())
                    {
                        var data = new Point
                        {
                            Value = point.Value,
                            // Name = "C",
                            Name = point.Name,
                            DateTime = time,
                            DataId = dataId
                        };
                        db.Add(data);
                        db.SaveChanges();

                        connection.InvokeAsync("RabbitMQMessage", data); //TODO: check
                    };
                }
            };
            _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
        }
    }
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine(" [*] Start listening...");

            var connection = new HubConnectionBuilder()
    .WithUrl("http://192.168.3.21:5200/hub") // Specify the URL of your SignalR hub
    .Build();

            await connection.StartAsync();
            Console.WriteLine($" [*] Signalr state = {connection.State}");

            var rabbitMqListener = new RabbitMQListener("amq.topic", "sensors_data");
            rabbitMqListener.StartListening(connection);

            while (true)
            {
                await Task.Delay(1000); // Delay for 1 second before looping again
            }
            //Console.ReadKey();
        }
    }
}

