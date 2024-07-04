using FB.Platform.Entity;
using MongoDB.Driver;

namespace FB.Platforme.Business
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            _database = client.GetDatabase("RabbitMQ");
        }

        public IMongoCollection<Cliente> Clientes => _database.GetCollection<Cliente>(nameof(Cliente));
        public IMongoCollection<PropostaCliente> PropostaCliente => _database.GetCollection<PropostaCliente>(nameof(PropostaCliente));
        public IMongoCollection<CartaoCredito> CartaoCredito => _database.GetCollection<CartaoCredito>(nameof(CartaoCredito));
    }
}
