using DoAnCoSoAPI.Entities;

namespace WebApplication2.Models
{
    public class IndexViewData
    {
        public List<User> Users { get; set; }
        public bool IsAdmin { get; set; }
    }
}
