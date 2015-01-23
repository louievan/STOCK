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
    class PickingService
    {
        RatingService ratingService;
        DataService dataService;

        private const string HEADER = "股票代码,股票名称,最深回撤,起涨日期,形态得分,成交量得分,均线得分,总得分";
        private const string TEMPLATE = "{0},{1},{2},{3},{4},{5},{6},{7}";

        public PickingService(RatingService ratingService, DataService dataService)
        {
            this.ratingService = ratingService;
            this.dataService = dataService;
        }

        public List<Recommendation> PickStock(int number)
        {
            dataService.downloadAllStockForToday();

            Dictionary<string, RatingResult> candidates = new Dictionary<string, RatingResult>();
            List<Recommendation> recommendations = new List<Recommendation>();

            foreach (string code in dataService.stockDatas.Keys)
            {
                StockData stockData = dataService.stockDatas[code];

                RatingResult score = ratingService.rate(stockData);
                if (score.getTotalScore() > 0)
                {
                    candidates.Add(code, score);
                }
            }

            int totalCandidates = candidates.Count;
            if (number > totalCandidates)
            {
                number = totalCandidates;
            }

            for (int i = 1; i <= number; i++)
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

                Recommendation recommendation = new Recommendation(maxCode, dataService.stockDatas[maxCode].name, candidates[maxCode]);
                recommendations.Add(recommendation);

                candidates.Remove(maxCode);
            }

            return recommendations;
            //CommonUtils.sendMail();
        }

    }
}
