using DoAnCoSoAPI.Entities;

namespace WebApplication2.Models
{
    public class FriendRequestViewModel
    {
        public string Id { get; set; }
        public string SenderId { get; set; }
        public User Sender { get; set; }
    }
}
