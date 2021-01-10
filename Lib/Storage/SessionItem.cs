using Microsoft.Azure.Cosmos.Table;
using System;

namespace MealMatch.Lib.Storage
{
    public class SessionItem : TableEntity
    {
        public int Id { get; set; }
        public Guid SessionId { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string YelpId { get; set; }
        public string ImageUrl { get; set; }
        public string Url { get; set; }
        public int YayVotes { get; set; } = 0;
        public int NayVotes { get; set; } = 0;
    }
}
