using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Testing
{
    public class FormData
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public int QuestionsRight { get; set; }
        public int QuestionsWrong { get; set; }
        public int TotalQuestions { get; set; }
        public int TotalWinnings { get; set; }
        public int CategoryID { get; set; }
        public string CategoryTitle { get; set; }
    }
}
