namespace DoAnCoSoAPI.Entities
{
    public class Subject
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public Subject(string id, string name)
        {
            Id = id;
            Name = name;
        }
        public static List<Subject> AllSubjects { get; } = new List<Subject>()
        {
        new Subject("1", "Toán"),
        new Subject("2", "Ngữ văn"),
        new Subject("3", "Lịch sử"),
        new Subject("4", "Địa lý"),
        new Subject("5", "Vật lí"),
        new Subject("6", "Hóa học"),
        new Subject("7", "Sinh học"),
        new Subject("8", "Tiếng Anh"),
        new Subject("9", "GDQP-AN")
        };

    }

}
