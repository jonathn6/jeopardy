using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Testing
{
    public class CategorySetup
    {
        private readonly IGameRepository repo;
        public Game SetUpCategory(IGameRepository repo, Player person, Category passedCategory)
        {    
            //
            // create an objet which will be passed to the view.  The object will contain data for a single category, a single player, and a set of questions and answers
            //
            var newGame = new Game();
            newGame.CategoryData = new Category();
            newGame.PlayerData = new Player();

            //
            // store the category informatin in the object
            //
            newGame.CategoryData.Title = passedCategory.Title;
            newGame.CategoryData.ID = repo.GetGameCategoryID(passedCategory.Title);
            //
            // delete the category records from the database so the same category is not selected again
            //
            repo.InsertDeleteCategory("D", newGame.CategoryData.ID, "");
            //
            // store the player infrormation in the object
            //
            newGame.PlayerData = person;
            //
            // call the GetGameQuestions function to retrieve all the questions and answers for the selected category and store that inforamtion in the object
            //
            newGame.QuestionData = repo.GetGameQuestions(newGame.CategoryData.ID);
            return newGame;
        }
    }
}
