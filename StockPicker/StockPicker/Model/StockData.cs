using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPicker.Model
{
    public class StockData
    {
        public string code;
        public string name;
        public List<StockDailyData> dailyData;

        public StockData()
        {

        }

        public StockData(string code, string name)
        {
            this.code = code;
            this.name = name;
            this.dailyData = new List<StockDailyData>();
        }

        public void addDailyData(StockDailyData dailyData)
        {
            this.dailyData.Add(dailyData);
        }

        public string getFullCode()
        {
            if (code == null || code.Length != 6)
            {
                return null;
            }

            if (code.StartsWith("6"))
            {
                return code + ".ss";
            }
            else
            {
                return code + ".sz";
            }
        }
    }
}
