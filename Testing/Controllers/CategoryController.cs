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
    public class CategoryController : Controller

    {       
            // The CategoryController is used to call the API to retrieve category/question/answer information and save the data to the database. It then
            // pushes the category ID, category Title, and player information to the View to be displayed.
            //
        private readonly ICategoryRepository repo;
        public CategoryController(ICategoryRepository repo)
        {
            this.repo = repo;
        }
        [HttpPost]
        public IActionResult Index(Player person)
        {
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
            //      categoryRecordsAdded : counter to keep track of how many category records have been added
            //      exitCategoryLoop : used to control when to exit the loop where category records are being processed
            //      quote : used to hold a double quotation mark.  I use this in .REPLACE() functions
            //      categoryToGet : A random number which is passed to the API to retrieve a category with that category ID
            //      getCategoryArray : Used as a storage array to facilitate .SPLIT on the API response 
            //      categoryToAdd : This is an instance of the Category class.  It is used to store the current category ID and the category title.  The data in 
            //                      this instance is provided to the repository function to add a category to the database
            //      questionArray : Used to hold the question data portion of the API response
            //      questionsForCategory : after the API response has been parsed, we read all the questions (and the associated data) from the database to verify
            //                      the validity of the data. 
            //      countValidRecords : used to count the number of question records that are retrieved from the database per category
            //      mymodel : used to pass all necessary class data to the view
            //
            // we need to pick 6 catgories to play the game.  Generate 6 random number between 1 and 10000.  Within a for loop, Use this
            // number as the offset in the API call to retrieve a category.  Then save the category data to the database table Category.
            // Once they are saved  we can call the API to retrieve 5 questions for each of the selected categories.
            //


            //
            // delete the current contents of the category table in the database
            // parameters passed:
            //      D - instructions to delete
            //      0 - Category ID to delete, 0 indicates delete all
            //      null - Category Title to delete, if provided
            //
            repo.InsertDeleteCategory("D", 0, "");
            //
            // delete the current contents of the question table in the database
            // parameters passed:
            //      param(1) - I indicates insert, D indicates delete
            //      param(2) - Question to insert, if the instruction is to insert otherwise null
            //      param(3) - Category ID to insert, if the instruction is to insert otherwise 0
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
            string quote = "\"";

            var clientJService = new HttpClient();
            var jServiceURL = "";
            var jServiceResponse = "";

            do
            {
                //
                // Generate a random number with the current date millisecond as the seed value.  Then use that number as the category
                // to get.  
                //
                DateTime localDate = DateTime.Now;
                Random rnd = new Random(localDate.Millisecond);
                var categoryToGet = rnd.Next(1, 15000);
                //
                // call the API to retrieve a record with 1 category and the corresponding questions data
                //
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
                Category categoryToAdd = JsonConvert.DeserializeObject<Category>(getCategoryArray[0]);
                //
                // We have isolated the fields needed to insert them into the category table. First, do some data cleaning before sending data to the database
                //
                categoryToAdd.Title = categoryToAdd.Title.Replace("'", "");
                categoryToAdd.Title = categoryToAdd.Title.Replace(quote,"");
                //
                // Call the InsertDeleteCategory method with to insert data into the Category table
                // parameters passed:
                //      param(1) - I indicates insert, D indicates delete
                //      param(2) - the category ID to insert
                //      param(3) - the category title to insert
                //
                repo.InsertDeleteCategory("I", categoryToAdd.ID, categoryToAdd.Title);
                //
                // Now parse the back half of the returned data which contains the individual questions and answers information
                //
                var questionArray = getCategoryArray[1].Split("},{");
                //
                // create an instance of the ParseQuestionJSON class and then call the public member passing it the array we received back from the API
                //
                var parse = new ParseQuestionJSON();
                parse.ParseQuestion(repo, questionArray);

                //
                // Retrieve all the question records from the database
                //
                var questionsForCategory = repo.GetGameQuestions(categoryToAdd.ID);
                //
                // for each record retrieved, check to verify that the question is of least a length of 3.  If it is, assume it is a valid record and
                // bump a counter.  If it is not, do not bump the counter.
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
                    repo.InsertDeleteCategory("D", categoryToAdd.ID, "");
                    repo.InsertDeleteQuestion("D", null, categoryToAdd.ID);
                }
                else
                {
                    //
                    // If we have 5 valid question records, bump the counter of valid categories.  Once we have 6 valid records, exit the loop. Otherwise
                    // do another API call and process the returned record.
                    //
                    categoryRecordsAdded++;
                    if (categoryRecordsAdded == 6)
                    {
                        exitCategoryLoop = true;
                    }
                }

            } while (exitCategoryLoop == false);


            //
            // All data needed for the game has been written to the database. Call GetAllCategory() and then display all the available categories to the user
            //
            dynamic mymodel = new ExpandoObject();
            mymodel.category = repo.GetAllCategory();
            mymodel.player = person;
            return View(mymodel);
        }
    }
}
