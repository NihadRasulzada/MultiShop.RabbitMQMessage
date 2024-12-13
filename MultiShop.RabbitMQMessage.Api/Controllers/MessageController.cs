using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MultiShop.RabbitMQMessage.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        [HttpPost]
        public IActionResult CreateMessage()
        {
            ConnectionFactory connectionFactory = new ConnectionFactory()
            {
                HostName = "localhost"
            };

            IConnection connection = connectionFactory.CreateConnection();

            IModel channel = connection.CreateModel();

            channel.QueueDeclare("Queue1", false, false, false, arguments: null);

            string messageContent = "Hello this is RabbitMq queue message";

            byte[] byteMessageContent = System.Text.Encoding.UTF8.GetBytes(messageContent);

            channel.BasicPublish(exchange: "", routingKey: "Queue1", basicProperties: null, body: byteMessageContent);

            return Ok("Message Added");
        }

        private static string _message;

        [HttpGet]
        public IActionResult GetMessage()
        {
            ConnectionFactory connectionFactory = new ConnectionFactory()
            {
                HostName = "localhost"
            };
            IConnection connection = connectionFactory.CreateConnection();
            IModel channel = connection.CreateModel();
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, x) =>
            {
                var body = x.Body.ToArray();
                _message = System.Text.Encoding.UTF8.GetString(body);
            };

            channel.BasicConsume(queue: "Queue1", autoAck: false, consumer: consumer);

            if(string.IsNullOrEmpty(_message))
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            return Ok(_message);
        }
    }
}
