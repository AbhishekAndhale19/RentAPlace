namespace RentAPlace.Application.DTOs.Ratings
{
    public class RatingResponse
    {
        public Guid Id { get; set; }
        public int Stars { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}