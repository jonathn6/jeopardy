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
            var quote = "\"";

            do
            {
                loopCounter++;
                var insideQuestion = passedArray[loopCounter].Split("game_id");
                var tempString1 = insideQuestion[0].Substring(0, insideQuestion[0].Length - 2);
                var tempString2 = "{" + tempString1 + "}";
                var tempString3 = tempString2.Replace("null", "2500");
                var tempString4 = tempString3.Replace("\u003Ci\u003E", "");
                var tempString5 = tempString4.Replace("\u003C/i\u003E", "");
                Question oneQuestion = JsonConvert.DeserializeObject<Question>(tempString5);
                if (oneQuestion.answer != "null")
                {
                    oneQuestion.question = oneQuestion.question.Replace(quote,"").Trim();
                    oneQuestion.answer = oneQuestion.answer.Replace(quote,"").Trim();
                    oneQuestion.answer = oneQuestion.answer.Replace("<i>","").Trim();
                    oneQuestion.answer = oneQuestion.answer.Replace("</i>", "").Trim();
                    oneQuestion.answer = oneQuestion.answer.Replace("&", "and").Trim();
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
