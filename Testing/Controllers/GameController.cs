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
        public IActionResult Index(Player person, Category passedCategory)
        {
            var newGame = new Game();
            newGame.CategoryData = new Category();
            newGame.PlayerData = new Player();
            newGame.QuestionData = new Question();

            newGame.CategoryData.Title = passedCategory.Title;
            newGame.CategoryData.ID = repo.GetGameCategoryID(passedCategory.Title);
            repo.InsertDeleteCategory("D", newGame.CategoryData.ID, "");
            
            newGame.PlayerData = person;

            dynamic mymodel = new ExpandoObject();
            mymodel.Game = newGame;
            mymodel.Question = repo.GetGameQuestions(newGame.CategoryData.ID);
            return View(mymodel);
        }
    }
}
