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
        public IActionResult Index()
        {
            //create API call to retrieve data from jService
            //
            // variables
            //      clientJService : The reference to the HTTPClient
            //      jServiceURL : The url of the API
            //      jServiceResponse : The data that is returned by the call to the API
            //      jsonInstance : This will be a specific instance of the data returned from the API.
            //      tempCategory : A temporary intance of the Category class so we can instantiate the category method of the JServiceData class
            //      jsonArray : An array where each element contains all the data returned pertaining to a single question
            //      categoryArray : used to split jsonArray into 2 pieces.  Element 0 has data which will populate the methods in the
            //                      JServiceData class and Element 1 has the data which will populate the methods in the Category class
            //      OneCategory : Is used to hold the deserialized object returned by JsonConvert
            //
            var clientJService = new HttpClient();
            var jServiceURL = "";
            var jServiceResponse = "";

            //
            // we need to pick 6 catgories to play the game.  Generate 6 random number between 1 and 10000.  Within a for loop, Use this
            // number as the offset in the API call to retrieve a category.  Then save the category data to the database table Category.
            // Once they are saved  we can call the API to retrieve 5 questions for each of the selected categories.
            //


            //
            //delete the current contents of the category table in the database
            //
            repo.InsertDeleteCategory("D",0,"");
            //
            // delete the current contents of the question table in the database
            //
            repo.InsertDeleteQuestion("D", null, 0);
            //
            // Now, we will do API calls with random numbers to retrieve 6 random categories.  Each category record will contain 
            // the category ID and the title of the category as well as at least 5 category questions.  Through testing, I found that not all
            // the records that come back have valid fields in an all the elements.  For example, for category 2846, word processing, 3 of the
            // returned questions dont have the question field populated and they have null for the value field.  Because of this, I had to modify
            // my code to do the API call, parse the question data, and populate the database.  Then, I execute a read of the database selecting all
            // the question records for the category ID currently being worked on.  I interigate the records.  If the record is deemed to be valid, I 
            // bump a counter.  If, after looking at all the records for that category the counter = 5, I have 5 valid questions for that category.  If
            // not, I have to delete the questions and category from the database.  Once I have validated 6 sets of questions, I can exit from the loop
            // and present the data to the user.
            //
            var categoryRecordsAdded = 0;
            var exitCategoryLoop = false;

            do
            {
                //
                // Generate a random number with the current date millisecond as the seed value.  Then use that number as the category
                // to get.  
                //
                DateTime localDate = DateTime.Now;
                Random rnd = new Random(localDate.Millisecond);
                var categoryToGet = rnd.Next(1,15000);
                jServiceURL = "https://jservice.io/api/category?id=" + categoryToGet;
                jServiceResponse = clientJService.GetStringAsync(jServiceURL).Result;
                //
                // The returned data has category information up front and at least 5 questions at the end.  Break up the response based
                // on [{.
                //
                var getCategoryArray = jServiceResponse.Split("[{");
                //
                // As a result of the split, the data in getCategoryArray[0] does not end in a valid format for JsonConvert so we need to append
                // a bogus value and an end bracket to the string so we can deserialize the string
                //
                getCategoryArray[0] = getCategoryArray[0] + "10}";
                //
                // Deserialize the string into the categoryToAdd class
                //
                CategoryTitle categoryToAdd = JsonConvert.DeserializeObject<CategoryTitle>(getCategoryArray[0]);
                //
                // We have isolated the fields needed to insert them into the category table. Call the InsertDeleteCategory method with instructions
                // to insert the category ID and the category Title into the database
                //
                repo.InsertDeleteCategory("I", categoryToAdd.id, categoryToAdd.title);
                //
                // Now parse the back half of the returned data which contains the individual questions and answers information
                //
                var questionArray = getCategoryArray[1].Split("},{");
                var parse = new ParseQuestionJSON();
                parse.ParseQuestion(repo, questionArray);

                //
                // the category records and the 5 corresponding question recods have been written to the database.  Verify the data.
                //

                //
                // Retrieve all the question records from the database
                //
                var questionsForCategory = repo.GetGameQuestions(categoryToAdd.id);
                //
                // for each record retrieved, check to verify that the question is of least a length of 3.  If it is, assume it is a valid record and
                // bump a counter.  
                //
                 
                var countValidRecords = 0;
                foreach (var question in questionsForCategory)
                {
                    if (question.question.Length > 3)
                    {
                        countValidRecords++;
                    }
                }
                //
                // If we do not have 5 valid question records, delete the category record and question records from the database
                //
                if (countValidRecords != 5)
                {
                    repo.InsertDeleteCategory("D", categoryToAdd.id, "");
                    repo.InsertDeleteQuestion("D", null, categoryToAdd.id);
                } else
                {
                    //
                    // If we have 5 valid question records, bump the counter of valid categories.  Once we have 6 valid records, exit the loop. Otherwise
                    // do another API call and process the returned record.
                    //
                    categoryRecordsAdded++;
                    if (categoryRecordsAdded==6)
                    {
                        exitCategoryLoop = true;
                    }
                }

            } while (exitCategoryLoop == false);

            //
            // All data needed for the game has been written to the database. Call GetGameQuestions
            //

            dynamic mymodel = new ExpandoObject();
            mymodel.category = repo.GetGameCategory();
            mymodel.question = repo.GetGameQuestions(0);
            return View(mymodel);
        }
    }
}
