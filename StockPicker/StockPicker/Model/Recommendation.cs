using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPicker.Model
{
    public class Recommendation
    {
        public string code { get; set; }
        public string name { get; set; }
        public RatingResult result { get; set; }

        public Recommendation(string code, string name, RatingResult result)
        {
            this.code = code;
            this.name = name;
            this.result = result;
        }

        public override bool Equals(object obj)
        {
            return this.code.Equals(((Recommendation)obj).code);
        }
    }
}
