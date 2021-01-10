using Microsoft.Azure.Cosmos.Table;
using System;

namespace MealMatch.Lib.Storage
{
    public class Voter : TableEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
