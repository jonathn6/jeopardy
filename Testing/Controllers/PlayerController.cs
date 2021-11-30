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
    public class PlayerController : Controller
    {
        //
        // The PlayerController controlls the getting the player infomation from the database and determining whether the payer is a new player or not
        //
        //      ActionResult GotPlayer
        //          Takes in the data entered on the form by the user and checks to see if the user is in the database.  If the user is not in the database,
        //          write a record to the database. 
        //
        // 
        private readonly IPlayerRepository repo;
        public PlayerController(IPlayerRepository repo)
        {
            this.repo = repo;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GotPlayer (Player person)
        {
            //
            // This section of code retrieves the player information entered by the user on the initial sign in screen.
            //
            //
            // variables
            //      lastName : This will be the last name the user entered on the form
            //      firstName : This will be the first name the user entered on the form.
            //      callPlayer : used to hold the data that comes back from a read of the player database table
            //   
            var lastName = person.LastName;
            var firstName = person.FirstName;
            //
            // read contents of the player table in the database tring to retrieve the lastname and firstname entered on the form
            // parameters passed:
            //      parm(1) - lastName - The last name of the player to SELECT from the database
            //      parm(2) - firstName = The first name of the player to SELELCT from the database 
            // 
            var callPlayer = repo.GotPlayer(lastName, firstName);
            //
            // if the call returns a null value in the LastName field, the database did not return any player information.  In this case, write a record to the database
            // with the last name and first name that the user entered.
            //
            if (callPlayer.LastName == null)
            {
                repo.WritePlayer(lastName, firstName);
                callPlayer.LastName = lastName;
                callPlayer.FirstName = firstName;
            }

            callPlayer.QuestionsRight = 0;
            callPlayer.QuestionsWrong = 0;
            callPlayer.TotalQuestions = 0;
            callPlayer.TotalWinnings = 0;

            return View(callPlayer);
        }
    }
}
