using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Testing
{
    public interface IGameRepository
    {
        public IEnumerable<Category> GetGameCategory();
        public int GetGameCategoryID(string categoryTitle);
        public string GetFirstCategoryID();
        public void InsertDeleteCategory(string WhichFunction, int Category, string CategoryTitle);
        public IEnumerable<Question> GetGameQuestions(int CategoryID);
        public void InsertDeleteQuestion(string WhichFunction, Question QuestionToInsert, int CategoryID);
        public Answer GetSingleAnswer(int ID);
        public void UpdatePlayer(Player person);
    }
}
