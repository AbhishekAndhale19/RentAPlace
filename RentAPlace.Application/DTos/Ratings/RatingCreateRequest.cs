namespace RentAPlace.Application.DTOs.Ratings
{
    public class RatingCreateRequest
    {
        public int Stars { get; set; } // 1–5
        public string Comment { get; set; } = string.Empty;
    }
}