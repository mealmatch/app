using System;
using Microsoft.Azure.Cosmos.Table;

namespace MealMatch.Lib.Storage
{
    public class Session : TableEntity
    {
        public Guid Id { get; set; }
        public Guid VoteLinkId { get; set; }
        public string Name { get; set; }
        public int NumVotesToWin { get; set; } = 0;
        public int NumWinsToEnd { get; set; } = 0;
        public int NumWins { get; set; } = 0;
        public int NumVoters { get; set; } = 0;
        public int NumItems { get; set; }

        public int CurrentSelected { get; set; } = 0;
    }
}
