using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Testing
{
    public interface ICategoryRepository
    {
        public IEnumerable<Category> GetAllCategory();
        public void InsertDeleteCategory(string WhichFunction, int Category, string CategoryTitle);
        public IEnumerable<Question> GetGameQuestions(int CategoryID);
        public void InsertDeleteQuestion(string WhichFunction, Question QuestionToInsert, int CategoryID);
    }
}
