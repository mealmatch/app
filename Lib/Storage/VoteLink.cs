using Microsoft.Azure.Cosmos.Table;
using System;

namespace MealMatch.Lib.Storage
{
    public class VoteLink : TableEntity
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
    }
}
