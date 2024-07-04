using FB.Platforme.Business.Interface;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace FB.Platforme.Business
{
    public class RabbitMqBUS
    {
        public const string MainQueue = "main_queue";
        public const string CreditoQueue = "creadito_queue";
        public const string DeadLetterQueue = "dlq_queue";
        public const string Exchange = "main_exchange";
        public const string RoutingKey = "main_key";

        public const string ErrorExchange = "error_exchange";
        public const string ErrorQueue = "error_queue";

        private readonly IBaseEventHandler _eventHandler;
        private IConnection _connection;
        private IModel _channel;
        private bool _disposed = false;

        public RabbitMqBUS(IBaseEventHandler eventHandler)
        {
            _eventHandler = eventHandler;
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void PublicarFila(string message, string queueName)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            try
            {
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    // Declara a Dead Letter Queue (DLQ)
                    channel.QueueDeclare(queue: DeadLetterQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);

                    //Configura argumentos para a fila principal, apontando para a DLQ em caso de falha
                    var arguments = new Dictionary<string, object>
                    {
                        { "x-dead-letter-exchange", "" },
                        { "x-dead-letter-routing-key", DeadLetterQueue }
                    };
                    // Declara a fila principal
                    channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: arguments);

                    // Declara o exchange
                    channel.ExchangeDeclare(exchange: Exchange, type: "direct");

                    // Liga a fila principal ao exchange
                    if (RoutingKey is not null)
                        channel.QueueBind(queue: queueName, exchange: Exchange, routingKey: queueName);
                    else
                        channel.QueueBind(queue: queueName, exchange: Exchange, routingKey: "");

                    channel.ConfirmSelect();
                    // Publica uma mensagem específica na fila principal
                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(exchange: Exchange, routingKey: queueName, basicProperties: null, body: body);
                    channel.WaitForConfirmsOrDie();
                    Console.WriteLine($" [x] Enviado '{message}'");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" [x] Erro ao conectar ao RabbitMQ: {ex.Message}");
                // Publica um evento de erro
                PublishErrorEvent($"Erro ao publicar mensagem: {message}");
            }
        }

        public void ConsumirFila(string queueName)
        {
            bool connected = false;
            int retryCount = 0;
            int maxRetries = 3;
            int retryDelay = 5000;
            var factory = new ConnectionFactory() { HostName = "localhost" };

            while (!connected && retryCount < maxRetries)
            {
                try
                {
                    if (_disposed) throw new ObjectDisposedException(nameof(RabbitMqBUS));

                    connected = true;

                    // Consome a mensagem da fila principal
                    var consumer = new EventingBasicConsumer(_channel);
                    consumer.Received += async (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var receivedMessage = Encoding.UTF8.GetString(body);
                        Console.WriteLine($" [x] Recebido '{receivedMessage}'");

                        try
                        {
                            await _eventHandler.Handle(receivedMessage);

                            _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        }
                        catch (Exception ex)
                        {
                            // Se ocorrer uma exceção durante o processamento, envie a mensagem para a DLQ
                            Console.WriteLine($" [x] Erro no processamento: {ex.Message}");
                            _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                            Console.WriteLine($" [x] Mensagem enviada para a DLQ: '{receivedMessage}'");
                            // Publica um evento de erro
                            PublishErrorEvent($"Erro no processamento da mensagem: {ex.Message}");
                        }
                    };

                    _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);


                }
                catch (Exception ex)
                {
                    retryCount++;

                    Console.WriteLine($" [x] Erro ao conectar ao RabbitMQ: {ex.Message}");

                    // Publica um evento de erro
                    PublishErrorEvent($"Erro ao conectar ao RabbitMQ: {ex.Message}");

                    if (retryCount < maxRetries)
                    {
                        Console.WriteLine($" [x] Tentando novamente em {retryDelay / 1000} segundos... ({retryCount}/{maxRetries})");
                        System.Threading.Thread.Sleep(retryDelay);
                    }
                    else
                        Console.WriteLine(" [x] Número máximo de tentativas atingido. Encerrando aplicação.");
                }
            }
        }
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }

        private void PublishErrorEvent(string errorMessage)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            try
            {
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    // Declara o exchange de erro
                    channel.ExchangeDeclare(exchange: ErrorExchange, type: "fanout");

                    // Declara a fila de erro
                    channel.QueueDeclare(queue: ErrorQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);

                    // Liga a fila de erro ao exchange de erro
                    channel.QueueBind(queue: ErrorQueue, exchange: ErrorExchange, routingKey: "");

                    // Publica a mensagem de erro no exchange de erro
                    var body = Encoding.UTF8.GetBytes(errorMessage);
                    channel.BasicPublish(exchange: ErrorExchange, routingKey: "", basicProperties: null, body: body);
                    Console.WriteLine($" [x] Evento de erro publicado: '{errorMessage}'");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" [x] Falha ao publicar evento de erro: {ex.Message}");
            }
        }

        public void ConsumirFilaFinal(string queueName, Func<string, Task> processMessage)
        {
            bool connected = false;
            int retryCount = 0;
            int maxRetries = 3;
            int retryDelay = 5000;
            var factory = new ConnectionFactory() { HostName = "localhost" };

            while (!connected && retryCount < maxRetries)
            {
                try
                {
                    if (_disposed) throw new ObjectDisposedException(nameof(RabbitMqBUS));

                    connected = true;

                    // Consome a mensagem da fila principal
                    var consumer = new EventingBasicConsumer(_channel);
                    consumer.Received += async (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var receivedMessage = Encoding.UTF8.GetString(body);
                        Console.WriteLine($" [x] Recebido '{receivedMessage}'");

                        try
                        {
                            await processMessage(receivedMessage);

                            _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        }
                        catch (Exception ex)
                        {
                            // Se ocorrer uma exceção durante o processamento, envie a mensagem para a DLQ
                            Console.WriteLine($" [x] Erro no processamento: {ex.Message}");
                            _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                            Console.WriteLine($" [x] Mensagem enviada para a DLQ: '{receivedMessage}'");
                            // Publica um evento de erro
                            PublishErrorEvent($"Erro no processamento da mensagem: {ex.Message}");
                        }
                    };

                    _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);


                }
                catch (Exception ex)
                {
                    retryCount++;

                    Console.WriteLine($" [x] Erro ao conectar ao RabbitMQ: {ex.Message}");

                    // Publica um evento de erro
                    PublishErrorEvent($"Erro ao conectar ao RabbitMQ: {ex.Message}");

                    if (retryCount < maxRetries)
                    {
                        Console.WriteLine($" [x] Tentando novamente em {retryDelay / 1000} segundos... ({retryCount}/{maxRetries})");
                        System.Threading.Thread.Sleep(retryDelay);
                    }
                    else
                        Console.WriteLine(" [x] Número máximo de tentativas atingido. Encerrando aplicação.");
                }
            }
        }
    }
}
