using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SmartHome.Connector.Settings;
using SmartHome.Data.DTO;
using System.Text;

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

        public void StartListening(Action<PointDto> processMessage)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var point = JsonConvert.DeserializeObject<PointDto>(message);

                if (point == null) return;

                processMessage(point);
            };

            _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
        }

        public void StopListening()
        {
            _channel.Close();
            _connection.Close();
        }
    }
}
