using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace DoAnCoSoAPI.Entities
{
    public class Class
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public Class(string id, string name)
        {
            Id = id;
            Name = name;
        }
        public static List<Class> AllClasses { get; } = new List<Class>()
        {
        new Class("1", "Lớp 10"),
        new Class("2", "Lớp 11"),
        new Class("3", "Lớp 12")
        };
    }
}

