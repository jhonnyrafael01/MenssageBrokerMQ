using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FB.Platform.Entity
{
    public class Cliente
    {
        //[BsonId]
        //[BsonRepresentation(BsonType.ObjectId)]
        //public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        //[BsonIgnore]
        //public string IdString
        //{
        //    get => Id.ToString();
        //    set => Id = ObjectId.Parse(value);
        //}
        public int Id { get; set; }
        public string Nome { get; set; }
        public int Idade { get; set; }
        public string CPF { get; set; }
        public int Status { get; set; }
        //public DateTime CreateAt => Id.CreationTime;
    }
}
