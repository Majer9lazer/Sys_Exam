using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Consumer.RabbitMqPublishMessage
{
    public class RabbitMqMiddlewareBusService
    {
        private readonly IConnectionFactory _connectionFactory;
        public void PublishMessage<T>(T message, string queueName) where T : class
        {
            using (var connection = _connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish(exchange: "",
                    routingKey: queueName,
                    basicProperties: properties,
                    body: body);
            }
        }
        public RabbitMqMiddlewareBusService()
        {
            _connectionFactory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                VirtualHost = "/"
            };
        }
    }
}
