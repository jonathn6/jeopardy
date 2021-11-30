using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Testing
{
    public class Game 
    {
        public Category CategoryData { get; set; }
        public Player PlayerData { get; set; }
        public IEnumerable<Question> QuestionData { get; set; }
    }
}
