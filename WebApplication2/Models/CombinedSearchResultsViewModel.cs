using DoAnCoSoAPI.Entities;

namespace WebApplication2.Models
{
    public class CombinedSearchResultsViewModel
    {
        public List<User> Users { get; set; }
        public List<User_Post> Posts { get; set; }
    }
}
