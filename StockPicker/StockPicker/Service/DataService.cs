using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net; 
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using StockPicker.Model;
using System.IO;
using System.Xml.Serialization;
using StockPicker.Utils;

namespace StockPicker.Service
{
    class DataService
    {
        private const string ALL_STOCK_TODAY_DATA_URL = @"http://quote.tool.hexun.com/hqzx/quote.aspx?type=2&market=0&sortType=3&updown=up&page=1&count=3000";
        private const string STOCK_HISTORY_URL_PATTERN = @"http://table.finance.yahoo.com/table.csv?s=";

        public Dictionary<string, StockData> stockDatas = new Dictionary<string, StockData>();

        public DataService()
        {
            downloadAllStockForToday();

            int success = 0, failed = 0;
            int total = stockDatas.Count;
            int processed = 0;

            HashSet<string> codes = new HashSet<string>(stockDatas.Keys);
            foreach (string code in codes)
            {
                string name = stockDatas[code].name;
                Console.WriteLine(++processed + "/" + total + " Processing " + code + " " + name);
                if (!loadStockHistory(code))
                {
                    if (downloadStockHistory(code))
                    {
                        success++;
                    }
                    else
                    {
                        failed++;
                    }
                }
                else
                {
                    Console.WriteLine("Loaded.");
                }
            }
            Console.WriteLine("Total: " + total);
            Console.WriteLine("Success: " + success);
            Console.WriteLine("Failed: " + failed);

        }

        public DataService(string code)
        {
            downloadAllStockForToday();
            if (!loadStockHistory(code))
            {
                downloadStockHistory(code);
            }
        }

        public void downloadAllStockForToday()
        {
            string response = null;
            WebClient webClient = new WebClient();
            webClient.Encoding = System.Text.Encoding.Default;
            try
            {
                response = webClient.DownloadString(ALL_STOCK_TODAY_DATA_URL);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

            Regex reg = new Regex(@"\['(.*?)','(.*?)',(.*?),.*?,.*?,(.*?),(.*?),(.*?),(.*?),.*?,.*?,.*?,.*?\]");
            MatchCollection matchCollection = reg.Matches(response);

            DateTime today = getLastWorkDay();
            foreach (Match match in matchCollection)
            {
                string code = match.Groups[1].Value;
                string name = match.Groups[2].Value;
                double close = double.Parse(match.Groups[3].Value);
                double open = double.Parse(match.Groups[4].Value);
                double high = double.Parse(match.Groups[5].Value);
                double low = double.Parse(match.Groups[6].Value);
                long volume = (long)double.Parse(match.Groups[7].Value);

                StockDailyData stockDailyData = new StockDailyData(today, open, close, high, low, volume);
                if (stockDatas.ContainsKey(code))
                {
                    stockDatas[code].dailyData[0] = stockDailyData;
                }
                else
                {
                    StockData stockData = new StockData(code, name);
                    stockData.addDailyData(stockDailyData);
                    stockDatas.Add(code, stockData);
                }
            }

            //Console.WriteLine(today.Date + " Total Stock Number : " + matchCollection.Count);

        }

        private bool downloadStockHistory(string code)
        {
            StockData stockData = stockDatas[code];

            WebClient webClient = new WebClient();
            string response = null;
            string uri = STOCK_HISTORY_URL_PATTERN + stockData.getFullCode();

            Console.WriteLine("Trying to download history for " + code + stockData.name);
            Console.WriteLine("Url : " + uri);
            try
            {
                response = webClient.DownloadString(uri);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Download Failed.");
                return false;
            }

            Regex reg = new Regex(@"([0-9]{4})-([0-9]{2})-([0-9]{2}),(.*?),(.*?),(.*?),(.*?),(.*?),([0-9]+\.[0-9]+)");
            MatchCollection matchCollection = reg.Matches(response);

            foreach (Match match in matchCollection)
            {
                int year = int.Parse(match.Groups[1].Value);
                int month = int.Parse(match.Groups[2].Value);
                int day = int.Parse(match.Groups[3].Value);
                DateTime date = new DateTime(year, month, day);
                if (date.Equals(stockData.dailyData[0].date))
                {
                    continue;
                }

                long volume = (long)(double.Parse(match.Groups[8].Value)/100);
                if (volume == 0) continue;

                double close = double.Parse(match.Groups[7].Value);
                double adj = double.Parse(match.Groups[9].Value);
                double rate = adj / close;
                double open = double.Parse(match.Groups[4].Value) * rate;
                double high = double.Parse(match.Groups[5].Value) * rate;
                double low = double.Parse(match.Groups[6].Value) * rate;

                StockDailyData stockDailyData = new StockDailyData(date, open, adj, high, low, volume);
                stockData.addDailyData(stockDailyData);
                //Console.WriteLine(code + stockData.name + " " + date + " |open " + open + " |close " + adj + " |high " + high + " |low " + low + " |vol " + volume);
            }

            Console.WriteLine("Download success.");
            saveToFile(code);

            return true;
        }

        private bool loadStockHistory(string code)
        {
            string filePath = CommonUtils.generateStockFilePath(code);
            if (!File.Exists(filePath))
            {
                return false;
            }

            Stream fStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            XmlSerializer xmlFormat = new XmlSerializer(typeof(StockData), new Type[] { typeof(StockData), typeof(StockDailyData) });
            StockData stockData = null;
            try
            {
                stockData = (StockData)xmlFormat.Deserialize(fStream);
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                fStream.Close();
            }

            if (stockData.dailyData[0].date.Equals(getLastWorkDay()))
            {
                stockData.dailyData[0] = stockDatas[code].dailyData[0];
                stockDatas[code] = stockData;

                saveToFile(code);
                return true;
            }

            else if (stockData.dailyData[0].date.Equals(getLast2WorkDay()))
            {
                foreach (StockDailyData stockDailyData in stockData.dailyData)
                {
                    stockDatas[code].addDailyData(stockDailyData);
                }

                saveToFile(code);
                return true;
            }

            else
            {
                return false;
            }
        }

        private DateTime getLastWorkDay()
        {
            DateTime today = DateTime.Today;
            if (today.DayOfWeek == DayOfWeek.Saturday)
            {
                today = today.AddDays(-1);
            }
            else if (today.DayOfWeek == DayOfWeek.Sunday)
            {
                today = today.AddDays(-2);
            }
            return today;
        }

        private DateTime getLast2WorkDay()
        {
            DateTime yesterday = DateTime.Today.AddDays(-1);
            if (yesterday.DayOfWeek == DayOfWeek.Saturday)
            {
                yesterday = yesterday.AddDays(-1);
            }
            else if (yesterday.DayOfWeek == DayOfWeek.Sunday)
            {
                yesterday = yesterday.AddDays(-2);
            }
            return yesterday;
        }

        private void saveToFile(string code)
        {
            string filePath = CommonUtils.generateStockFilePath(code);
            Stream fStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite);
            XmlSerializer xmlFormat = new XmlSerializer(typeof(StockData), new Type[] { typeof(StockData), typeof(StockDailyData) });
            xmlFormat.Serialize(fStream, stockDatas[code]);
            fStream.Close();
        }
    }
}
