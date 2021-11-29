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
            var lastName = person.LastName;
            var firstName = person.FirstName;
            var callPlayer = repo.GotPlayer(lastName, firstName);
            if (callPlayer.LastName == null)
            {
                repo.WritePlayer(lastName, firstName);
                callPlayer.LastName = lastName;
                callPlayer.FirstName = firstName;
                callPlayer.GamesLost = 0;
                callPlayer.GamesWon = 0;
                callPlayer.GamesStarted = 0;
                callPlayer.TotalWinnings = 0;
            }

            return View(callPlayer);
        }
    }
}
