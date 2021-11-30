using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Testing
{
    public class GameRepository : IGameRepository
    {
        private readonly IDbConnection _conn;
        public GameRepository(IDbConnection conn)
        {
            _conn = conn;
        }
        public IEnumerable<Category> GetGameCategory()
        {
            return _conn.Query<Category>("SELECT * FROM jeopardy.CATEGORY");
        }
        public int GetGameCategoryID(string passedTitle)
        {
            var SQL = "SELECT ID FROM jeopardy.CATEGORY WHERE Title Like '%" + passedTitle + "%'";
            return _conn.QuerySingle<int>(SQL);
        }
        public void InsertDeleteCategory(string passedWhich, int passedID, string passedTitle)
        {
            switch (passedWhich) 
            {

                case "I":
                    _conn.Execute("INSERT INTO jeopardy.category (ID, Title) VALUES (@id, @title);",
                       new
                       {
                           ID = passedID,
                           title = passedTitle
                       });
                    break;
                case "D":
                    if (passedID == 0)
                    {
                        _conn.Execute("DELETE FROM jeopardy.category");
                    } else
                    {
                        var SQL = "DELETE FROM jeopardy.category WHERE ID = " + passedID;
                        _conn.Execute(SQL);
                    }
                    break;

            }
        }

        public IEnumerable<Question> GetGameQuestions(int passedCategory)
        {
            var SQL = "";
            switch (passedCategory)
            {
                case 0:
                    SQL = "SELECT * FROM jeopardy.QUESTIONS";
                    break;
                default:
                    SQL = "SELECT * FROM jeopardy.QUESTIONS WHERE Category_ID = " + passedCategory;
                    break;
            }
            return _conn.Query<Question>(SQL);
        }

        public void InsertDeleteQuestion(string passedWhich, Question QuestionToInsert, int CategoryID)
        {
            switch (passedWhich)
            {
                case "I":
                    _conn.Execute("INSERT INTO jeopardy.questions (Question, Answer, Category_ID, Value) VALUES (@question, @answer, @category, @dvalue);",
                    new { question = QuestionToInsert.question, 
                            answer = QuestionToInsert.answer,
                            category = QuestionToInsert.category_id,
                            dvalue = QuestionToInsert.Value
                    });
                    break;
                case "D":
                    if (CategoryID == 0)
                    {
                        _conn.Execute("DELETE FROM jeopardy.questions");
                    } else
                    {
                        _conn.Execute("DELETE FROM jeopardy.questions WHERE Category_ID = @catID;",
                            new { catID = CategoryID});
                    }
                    break;
            }
        }

        public Answer GetSingleAnswer(int ID)
        {
            var SQL = "SELECT Question, Answer, Value FROM jeopardy.QUESTIONS WHERE ID = " + ID;
            return _conn.QuerySingle<Answer>(SQL);
        }

        public void UpdatePlayer(Player person)
        {
            _conn.Execute("UPDATE jeopardy.player SET QuestionsRight = @right, QuestionsWrong = @wrong, TotalQuestions = @totalq, TotalWinnings = @totalw;",
                            new { right = person.QuestionsRight,
                                    wrong = person.QuestionsWrong,
                                    totalq = person.TotalQuestions,
                                    totalw = person.TotalWinnings});
        }
    }
}
