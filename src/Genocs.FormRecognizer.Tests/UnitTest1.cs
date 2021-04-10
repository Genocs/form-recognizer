using System.Collections.Generic;
using Xunit;

namespace Genocs.FormRecognizer.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            // my pretend dataset
            List<string> fields = new List<string>();
            // my 'columns'
            fields.Add("this_thing");
            fields.Add("that_thing");
            fields.Add("the_other");

            dynamic exo = new System.Dynamic.ExpandoObject();

            foreach (string field in fields)
            {
                ((IDictionary<string, object>)exo).Add(field, field + "_data");
            }

            // output - from Json.Net NuGet package
            var res = Newtonsoft.Json.JsonConvert.SerializeObject(exo);

        }
    }
}
