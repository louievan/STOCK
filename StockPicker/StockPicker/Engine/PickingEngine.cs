using StockPicker.Model;
using StockPicker.Service;
using StockPicker.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPicker.Engine
{
    class PickingEngine
    {
        RatingService ratingService;
        DataService dataService;

        private const string HEADER = "股票代码,股票名称,最深回撤,起涨日期,形态得分,成交量得分,均线得分,总得分";
        private const string TEMPLATE = "{0},{1},{2},{3},{4},{5},{6}";

        public PickingEngine(RatingService ratingService, DataService dataService)
        {
            this.ratingService = ratingService;
            this.dataService = dataService;
        }

        public void PickStock()
        {
            Dictionary<string, RatingResult> candidates = new Dictionary<string, RatingResult>();

            foreach (string code in dataService.stockDatas.Keys)
            {
                StockData stockData = dataService.stockDatas[code];

                RatingResult score = ratingService.rate(stockData);
                if (score.getTotalScore() > 0)
                {
                    candidates.Add(code, score);
                }
            }

            string filePath = CommonUtils.generateReportFilePath();
            Stream fStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite);
            StreamWriter sWriter = new StreamWriter(fStream, Encoding.Default);
            sWriter.WriteLine(HEADER);

            int totalCandidates = candidates.Count;
            for (int i = 1; i <= totalCandidates; i++)
            {
                double max = 0;
                string maxCode = null;
                foreach (string code in candidates.Keys)
                {
                    if (candidates[code].getTotalScore() > max)
                    {
                        max = candidates[code].getTotalScore();
                        maxCode = code;
                    }
                }
                string line = string.Format(TEMPLATE, 
                    maxCode,
                    dataService.stockDatas[maxCode].name,
                    string.Format("{0:N2}%", candidates[maxCode].dropRate * 100),
                    candidates[maxCode].startDate,
                    string.Format("{0:N2}", candidates[maxCode].figureBounceScore),
                    string.Format("{0:N2}", candidates[maxCode].volumeScore),
                    string.Format("{0:N2}", candidates[maxCode].maScore),
                    string.Format("{0:N2}", candidates[maxCode].getTotalScore())
                );

                Console.WriteLine(i + "." + line);
                sWriter.WriteLine(line);

                candidates.Remove(maxCode);
            }

            sWriter.Close();
            fStream.Close(); 

            CommonUtils.sendMail();
        }

    }
}
