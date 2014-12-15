using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPicker.Model
{
    public class StockDailyData
    {
        public double open;
        public double close;
        public double high;
        public double low;
        public long volume;
        public DateTime date;

        public StockDailyData()
        {

        }

        public StockDailyData(DateTime date, double open, double close, double high, double low, long volume)
        {
            this.date = date;
            this.open = open;
            this.close = close;
            this.high = high;
            this.low = low;
            this.volume = volume;

        }

        public bool isRaise()
        {
            return close > open ? true : false;
        }
    }
}
