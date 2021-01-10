using Microsoft.Azure.Cosmos.Table;
using System;

namespace MealMatch.Lib.Storage
{
    public class SessionVote : TableEntity
    {
        public Guid UserId { get; set; }
        public int ItemId { get; set; }
        public Guid SessionId { get; set; }
        public bool Value { get; set; }
    }
}
