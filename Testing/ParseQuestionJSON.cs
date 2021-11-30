using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Testing
{
    public class ParseQuestionJSON
    {
        public void ParseQuestion(ICategoryRepository passedRepo, string[] passedArray)
        {
            var exitloop = false;
            var loopCounter = -1;
            var recordsWritten = 0;

            do
            {
                loopCounter++;
                var insideQuestion = passedArray[loopCounter].Split("game_id");
                var tempString1 = insideQuestion[0].Substring(0, insideQuestion[0].Length - 2);
                var tempString2 = "{" + tempString1 + "}";
                var tempString3 = tempString2.Replace("null", "2500");
                var tempString4 = tempString3.Replace("\u003Ci\u003E", "");
                var tempString5 = tempString4.Replace("\u003C/i\u003E", "");
                var tempString6 = tempString5.Replace('"', ' ').Trim();
                Question oneQuestion = JsonConvert.DeserializeObject<Question>(tempString6);
                if (oneQuestion.answer != "null")
                {
                    passedRepo.InsertDeleteQuestion("I", oneQuestion, 0);
                    recordsWritten++;
                }
                if (recordsWritten == 5)
                {
                    exitloop = true;
                }

            } while (exitloop == false);
            return;
        }

    }
}
