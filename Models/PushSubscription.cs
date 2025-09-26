namespace Hogwarts.Models
{
    public class PushSubscription
    {
        public int Id { get; set; }
        public string Endpoint { get; set; } = string.Empty; // URL endpoint-a
        public string Keys { get; set; } = string.Empty;     // Ključevi za decrypt
    }
}
