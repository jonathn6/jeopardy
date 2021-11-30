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
        //
        // The GameController controlls the flow of the game once the categories and questions have been saved to the database.
        // The user will select an initial category from the Category view.  Once a category is selected, control is passed to the
        // GameController.  GameController will perform the following functions:
        //
        //      ActionResult Index
        //          Take the passed cateogy ID, retrieve the category Title and the associated questions (and answer data), as well as all the
        //          player infomation and pass it along to the view.
        //
        //      ActionResult VerifyAnswers
        //          Take in the form data and verify whether the user entered the correct answer or not. Return a GameSummary class to the view
        //
        //      ActionResult AnotherRound
        //          Determine if there are more categories for the user to answer.  If there are, pass them to the view.  If there are not, pass
        //          an exit game class to the view.
        //
        // 
        private readonly IGameRepository repo;

        public GameController(IGameRepository repo)
        {
            this.repo = repo;
        }
        [HttpPost]
        public ActionResult AnotherRound(Player person)
        {
            //
            // This section of code sets up to retrieve the remaining categories from the database and displays them to the user for selection.
            // If there are no more categories left in the database, the view will display a game summary.
            //
            //
            // variables
            //      exitGame : An instance of Finished.  Used to tell the view if the game is over or not.
            //      mymodel : A dynamic object that will contain a category object, a player object, and a Finished object. All this data is passed to the view.
            //      tempCategory : used to retrieve all the remaining category data from the database
            //      
            //
            // create an instance of the Finished class.  We use this to inform the view whether to display categories or a game summary
            //
            var exitGame = new Finished();
            //
            // create a dynamis object which will be used to hold all objects which are sent to the view
            //
            dynamic mymodel = new ExpandoObject();
            //
            // call the GetGameCategory method.  This will retrieve all the category data from the database
            //
            var tempCategory = repo.GetGameCategory();

            mymodel.category = tempCategory;
            //
            // if tempCategory.Any() is true, then the call to the database returned data and we need to tell the view to display the categories that remain.
            // if tempCategory.Any() is false, then we have finished the game and we need to tell the view to display a game summary
            //
            if (tempCategory.Any())
            {
                exitGame.ExitGame = false;
            }
            else
            {
                exitGame.ExitGame = true;
            }
            mymodel.player = person;
            mymodel.exit = exitGame;
            return View(mymodel);
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
            // variables
            //      form : of type IFormCollection, this will contain the passed form data
            //      p : of type FormData this will contain data from the form that MVC was able to parse (all the player information)
            //      numberOfElements : The total number of form data elements returned 
            //      formElementNames : Since the form data elements are generated within a foreach loop, they can not be named manually.  So, I generated the name of the
            //              input box to be _questionID.  
            //      dbAnswer : An array of type Answer. Will contain answer information pulled from the database
            //      userAnswer : An array used to hold the answers to the questions that the user typed in
            //      person : of type Player.  Will hold all player information
            //      gameSummary : of type GameSummary, this will hold player information as well as the queestion, the answer from the database, the answer the user entered,
            //              the value of the question.
            //
            //
            // pick up the number of data elements returned on the form (I dont think I use this anymore)
            //
            var numberOfElements = form.Count;
            //
            // pick up the form element names from the form.key.  Since the answer <input> were generated within a foreach loop, I named the elements _questionID.  So,
            // I need to pick up the element name, remove the _, and retrieve that question (and answer information) from the database to check whether the user
            // entered the correct answer or not.
            //
            string[] formElementNames = form.Keys.ToArray();
            //
            // create an object to hold the answer data that comes back from the database
            //
            var dbAnswer = new Answer[5];
            //
            // create an array to hold the answers entered by the user
            //
            var userAnswer = new string[5];

            //
            // loop 5 times
            //
            for (var r=0;r<=4;r++)
            {
                //
                // elementName will be the last 4 characters of the form element name. This will be the question ID that gets sent to GetSingleAnswer.
                //
                var elementName = Int32.Parse(formElementNames[r].Substring(1, 4));
                //
                // retrieve a single question (and answer data) from the database and store it in dbAnswer
                //
                dbAnswer[r] = repo.GetSingleAnswer(elementName);
                //
                // store the answer that the user entered in the form in userAnswer
                //
                userAnswer[r] = form[formElementNames[r]].ToString().Trim();
            }

            //
            // Now, we should have the 5 database answers and their point values and the answers that the user enterd.  Check to see if the user knew the answer.
            // If the user got the answer correct, add the points into the total points and increment the questionsright and totalquestions fields.  If the answer was
            // incorrect, increment the questionswrong and totalquestions fields.
            //
            //
            // Check the user supplied answer to the database answer.  Accumulate the number of questions answered right, wrong, and total points 
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
            //
            // create a new instance of Player.  This will hold all the updated player statistics so we can update the database and send the updated data to the view
            //
            var person = new Player();
            person.LastName = p.LastName;
            person.FirstName = p.FirstName;
            person.QuestionsRight = p.QuestionsRight;
            person.QuestionsWrong = p.QuestionsWrong;
            person.TotalQuestions = p.TotalQuestions;
            person.TotalWinnings = p.TotalWinnings;
            //
            // update the database with the updated person
            //
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

            gameSummary.DBQuestion1 = dbAnswer[0].question;
            gameSummary.DBQuestion2 = dbAnswer[1].question;
            gameSummary.DBQuestion3 = dbAnswer[2].question;
            gameSummary.DBQuestion4 = dbAnswer[3].question;
            gameSummary.DBQuestion5 = dbAnswer[4].question;

            gameSummary.DBAnswer1 = dbAnswer[0].answer;
            gameSummary.DBAnswer2 = dbAnswer[1].answer;
            gameSummary.DBAnswer3 = dbAnswer[2].answer;
            gameSummary.DBAnswer4 = dbAnswer[3].answer;
            gameSummary.DBAnswer5 = dbAnswer[4].answer;

            gameSummary.DBValue1 = dbAnswer[0].Value;
            gameSummary.DBValue2 = dbAnswer[1].Value;
            gameSummary.DBValue3 = dbAnswer[2].Value;
            gameSummary.DBValue4 = dbAnswer[3].Value;
            gameSummary.DBValue5 = dbAnswer[4].Value;
            var rsult = "";

            if (userAnswer[0].ToLower() == dbAnswer[0].answer.ToLower())
            {
                gameSummary.UserAnswer1 = "Your answer of " + dbAnswer[0].answer + " is correct!";
            } else
            {
                rsult = userAnswer[0].Length > 0 ? gameSummary.UserAnswer1 = "Sorry, but your answer of " + userAnswer[0] + " is incorrect." : gameSummary.UserAnswer1 = "You did not provide an answer so obviously, your answer is incorrect";
            }
            //
            //
            //
            if (userAnswer[1].ToLower() == dbAnswer[1].answer.ToLower())
            {
                gameSummary.UserAnswer2 = "Your answer of " + dbAnswer[1].answer + " is correct!";
            }
            else
            {
                rsult = userAnswer[1].Length > 0 ? gameSummary.UserAnswer2 = "Sorry, but your answer of " + userAnswer[1] + " is incorrect." : gameSummary.UserAnswer2 = "A blank answer is no answer so, your answer is incorrect";
            }
            //
            //
            //
            if (userAnswer[2].ToLower() == dbAnswer[2].answer.ToLower())
            {
                gameSummary.UserAnswer3 = "Your answer of " + dbAnswer[2].answer + " is correct!";
            }
            else
            {
                rsult = userAnswer[2].Length > 0 ? gameSummary.UserAnswer3 = "Sorry, but your answer of " + userAnswer[2] + " is incorrect." : gameSummary.UserAnswer3 = "Your answer was empty so your answer is incorrect";
            }//
             //
             //
            if (userAnswer[3].ToLower() == dbAnswer[3].answer.ToLower())
            {
                gameSummary.UserAnswer4 = "Your answer of " + dbAnswer[3].answer + " is correct!";
            }
            else
            {
                rsult = userAnswer[3].Length > 0 ? gameSummary.UserAnswer4 = "Sorry, but your answer of " + userAnswer[3] + " is incorrect." : gameSummary.UserAnswer4 = "I was not expecting that!";
            }
            //
            //
            //
            if (userAnswer[4].ToLower() == dbAnswer[4].answer.ToLower())
            {
                gameSummary.UserAnswer5 = "Your answer of " + dbAnswer[4].answer + " is correct!";
            }
            else
            {
                rsult = userAnswer[4].Length > 0 ? gameSummary.UserAnswer5 = "Sorry, but your answer of " + userAnswer[4] + " is incorrect." : gameSummary.UserAnswer5 = "You didnt really expect to get credit for an empty answer, did you?";
            }
            //
            // delete all the questions in the database with the current Category ID
            // parameters passed:
            //      parm(1) - D - instruction to delete
            //      parm(2) - null - question to insert if the instruction is to insert otherwise pass null
            //      parm(3) - Category ID - The questions of this category ID that should be deleted or inserted depending on the instruction
            //
            repo.InsertDeleteQuestion("D", null, p.CategoryID);
            //
            // return the gameSummary object to the view
            //
            return View(gameSummary);
        }
        [HttpPost]
        public IActionResult Index(Player person, Category passedCategory)
        {
            // Set up the object that is passed to the initial view.
            //
            // variables
            //      newSetUp : create an instance of CategorySetup so we can establis the game objects
            //
            var newSetUp = new CategorySetup();
            var newGame = newSetUp.SetUpCategory(repo, person, passedCategory);
            //
            // send the object to the view
            //
            return View(newGame);
        }
    }
}
