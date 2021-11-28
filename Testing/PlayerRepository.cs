using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Testing
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly IDbConnection _conn;
        public PlayerRepository(IDbConnection conn)
        {
            _conn = conn;
        }
        public Player GotPlayer(string passedLastName, string passedFirstName)
        {
            var nullPlayer = new Player();
            nullPlayer.LastName = null;
            try
            {
                var SQL = "SELECT * FROM jeopardy.player WHERE LastName LIKE '" + passedLastName + "' AND FirstName LIKE '" + passedFirstName + "'";
                nullPlayer = _conn.QuerySingle<Player>(SQL);

            } catch(Exception e) {
                nullPlayer.FirstName = e.Message;
                nullPlayer.LastName = null;
            }
            
            return nullPlayer;
        }
        public void WritePlayer(string passedLastName, string passedFirstName)
        {
            var SQL = "INSERT INTO jeopardy.player (LastName, FirstName, GamesWon, GamesLost, GamesStarted, TotalWinnings) VALUES ('" + passedLastName + "','" + passedFirstName + "', 0, 0, 0, 0)";
            _conn.Execute(SQL);
            return;
        }
    }
}
