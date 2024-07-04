using FB.Platform.Entity;
using MongoDB.Driver;

namespace FB.Platforme.Business
{
    public class ClienteRepository
    {
        private readonly IMongoCollection<Cliente> _clientes;

        public ClienteRepository(MongoDbContext context)
        {
            _clientes = context.Clientes;
        }

        public void Add(Cliente cliente)
        {
            try
            {
                _clientes.InsertOne(cliente);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Update(int id)
        {
            try
            {
                var filter = Builders<Cliente>.Filter.Eq(x => x.Id, id);
                var update = Builders<Cliente>.Update.Set(x => x.Status, 7);

                _clientes.UpdateOne(filter, update);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

}
