using System;

namespace MealMatch.Lib.Storage
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string m) : base(m) { }
    }
}
