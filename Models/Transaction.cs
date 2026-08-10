namespace WebApplication1.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        public decimal amount { get; set; }

        public DateTime date { get; set; }

        public string? description { get; set; }

        public TransactionType type { get; set; }

        public int categoryId { get; set; }

        public Category? category { get; set; }

        public int UserId { get; set; }

        public User? User { get; set; }
    }
}
