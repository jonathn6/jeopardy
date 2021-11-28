using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Testing
{
    public interface IPlayerRepository
    {
        public Player GotPlayer(string lastName, string firstName);
        public void WritePlayer(string lastName, string firstName);

    }
}
