using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SmartHome.Connector.Settings;
using SmartHome.Data.DTO;
using SmatHome.Connector;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace SmartHome.Connector.Services
{
    public class RabbitMQListener
    {
        private readonly string _exchangeName;
        private readonly string _queueName;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMQListener(ApiSettings api, RabbitMQSettings rabbitMQ)
        {
            _exchangeName = rabbitMQ.ExchangeName;
            _queueName = rabbitMQ.QueueName;

            var factory = new ConnectionFactory() { HostName = rabbitMQ.Host, UserName = rabbitMQ.Username, Password = rabbitMQ.Password };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: _exchangeName, type: "topic", durable: true);
            _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueBind(queue: _queueName, exchange: _exchangeName, routingKey: _queueName);
        }

        public void StartListening(Action<PointDto?> processMessage)
        {
            var consumer = new EventingBasicConsumer(_channel);
            try
            {
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    try
                    {
                        var point = JsonConvert.DeserializeObject<PointDto>(message);
                        processMessage(point);
                    }
                    catch (JsonReaderException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    catch (SignalRException e)
                    {
                        throw e;
                    }
                };

                _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
            }
            catch (SignalRException e)
            {
                throw e;
            }



        }

        public void StopListening()
        {
            _channel.Close();
            _connection.Close();
        }
    }
}
