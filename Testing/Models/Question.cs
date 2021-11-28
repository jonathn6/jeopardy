using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Testing
{
    public class Question
    {
        public int id { get; set; }
        public string answer { get; set; }
        public string question { get; set; }
        public int Value { get; set; }
        public DateTime airdate { get; set; }
        public int category_id { get; set; }
        public int game_id { get; set; }
        public int invalid_count { get; set; }
    }
}
