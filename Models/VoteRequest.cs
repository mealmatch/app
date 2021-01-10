using System;

namespace MealMatch.Models
{
    public class VoteRequest
    {
        public Guid UserId { get; set; }
        public Guid SessionId { get; set; }
        public int ItemId { get; set; }
        public bool Vote { get; set; }
    }
}
