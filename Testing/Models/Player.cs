using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Testing
{
    public class Player
    {
        public int ID { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public int GamesWon { get; set; }
        public int GamesLost { get; set; }
        public int GamesStarted { get; set; }
        public int TotalWinnings { get; set; }
        public int ReturnCode { get; set; }
        public string ReturnString { get; set; }
    }
}
