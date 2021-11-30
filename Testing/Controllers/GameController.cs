using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Testing.Controllers
{
    public class GameController : Controller

    {
        private readonly IGameRepository repo;

        public GameController(IGameRepository repo)
        {
            this.repo = repo;
        }
        [HttpPost]
        public ActionResult VerifyAnswers(IFormCollection form, [FromForm] FormData p)
        {
            //
            // we know that there are 14 form elements being returned, 5 answers, 6 fields from the player class, the category ID, the category title, and 1 field that we are not concerned with 
            // the first 5 elements are the 5 answers.  The element names were dynamically generated based on the ID of the question. So we can pick up the element name
            // and do a read from the database to get the correct answer to each of the questions.  In addition, since we can get data back for this question, we can also 
            // retrive the point value of the question.
            //
            var numberOfElements = form.Count;
            string[] formElementNames = form.Keys.ToArray();
            var dbAnswer = new Answer[5];
            var userAnswer = new string[5];

            for (var r=0;r<=4;r++)
            {
                var elementName = Int32.Parse(formElementNames[r].Substring(1, 4));
                dbAnswer[r] = repo.GetSingleAnswer(elementName);
                userAnswer[r] = form[formElementNames[r]].ToString();
            }


            //
            // Now, we should have the 5 database answers and their point values and the answers that the user enterd.  Check to see if the user knew the answer.
            // If the user got the answer correct, add the points into the total points and increment the questionsright and totalquestions fields.  If the answer was
            // incorrect, increment the questionswrong and totalquestions fields.
            //

            //
            // Check the user supplied answer to the database answer
            //
            for (var r=0;r<5;r++)
            {
                p.TotalQuestions++;
                if (userAnswer[r].ToLower() == dbAnswer[r].answer.ToLower())
                {
                    p.QuestionsRight++;
                    p.TotalWinnings = p.TotalWinnings + dbAnswer[r].Value;
                } else
                {
                    p.QuestionsWrong++;
                }
            }
            
            var person = new Player();
            person.LastName = p.LastName;
            person.FirstName = p.FirstName;
            person.QuestionsRight = p.QuestionsRight;
            person.QuestionsWrong = p.QuestionsWrong;
            person.TotalQuestions = p.TotalQuestions;
            person.TotalWinnings = p.TotalWinnings;

            repo.UpdatePlayer(person);
            //
            // create the object that will be displayed in the summary view
            //
            var gameSummary = new GameSummary();
            gameSummary.CategoryTitle = p.CategoryTitle;
            gameSummary.LastName = p.LastName;
            gameSummary.FirstName = p.FirstName;
            gameSummary.QuestionsRight = p.QuestionsRight;
            gameSummary.QuestionsWrong = p.QuestionsWrong;
            gameSummary.TotalQuestions = p.TotalQuestions;
            gameSummary.TotalWinnings = p.TotalWinnings;

            var result = userAnswer[0].ToLower() == dbAnswer[0].answer.ToLower() ? gameSummary.gotQuestion1Right = true : gameSummary.gotQuestion1Right = false;
            result = userAnswer[1].ToLower() == dbAnswer[1].answer.ToLower() ? gameSummary.gotQuestion2Right = true : gameSummary.gotQuestion2Right = false;
            result = userAnswer[2].ToLower() == dbAnswer[2].answer.ToLower() ? gameSummary.gotQuestion3Right = true : gameSummary.gotQuestion3Right = false;
            result = userAnswer[3].ToLower() == dbAnswer[3].answer.ToLower() ? gameSummary.gotQuestion4Right = true : gameSummary.gotQuestion4Right = false;
            result = userAnswer[4].ToLower() == dbAnswer[4].answer.ToLower() ? gameSummary.gotQuestion5Right = true : gameSummary.gotQuestion5Right = false;


            repo.InsertDeleteQuestion("D", null, p.CategoryID);
            return View(gameSummary);
        }
        [HttpPost]
        public IActionResult Index(Player person, Category passedCategory)
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
            //
            //
            //
            //dynamic mymodel = new ExpandoObject();
            //mymodel.Game = newGame;
            //
            // send the object to the view
            //
            return View(newGame);
        }
    }
}
