namespace MealMatch.Models
{
    public class YelpRequest
    {
        public string Location { get; set; }
        public string Search { get; set; }
        public int Page { get; set; } = 0;
    }
}
