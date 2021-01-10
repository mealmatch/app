using System.Collections.Generic;

namespace MealMatch.Models
{
    public class CreateRequest
    {
        public string Name { get; set; }
        public bool Shuffle { get; set; }
        public int NumVotesToWin { get; set; }
        public int NumWinsToEnd { get; set; }
        public List<OptionItem> Options { get; set; }
    }
}
