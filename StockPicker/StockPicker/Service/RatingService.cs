using StockPicker.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPicker.Service
{
    class RatingService
    {
        static int MAX_SEARCH_DEPTH = 59;
        static int MIN_SEARCH_DEPTH = 7;
        static int MIN_MAX_DEPTH = 2;


        public RatingResult rate(StockData stockData)
        {
            RatingResult result = new RatingResult();
            if (stockData.dailyData.Count < 120)
            {
                return result;
            }

            double total = 0;
            double ma5 = 0, ma10 = 0, ma20 = 0, ma30 = 0, ma60 = 0, ma120 = 0;

            for (int i = 0; i < 120; i++)
            {
                total += stockData.dailyData[i].close;
                if (i == 4) 
                    ma5 = total / 5;
                if (i == 9) 
                    ma10 = total / 10;
                if (i == 19) 
                    ma20 = total / 20;
                if (i == 29) 
                    ma30 = total / 30;
                if (i == 59) 
                    ma60 = total / 60;
                if (i == 119) 
                    ma120 = total / 120;
            }

            result.maScore =  (ma10 / ma5) * (ma20 / ma10) * (ma30 / ma60) * (ma60 / ma120);

            double highScore = 0;
            int highDepth = 0;
            double highDropRate = 0;

            for (int searchDepth = MIN_SEARCH_DEPTH; searchDepth <= MAX_SEARCH_DEPTH; searchDepth++)
            {
                if(stockData.dailyData[searchDepth].low > stockData.dailyData[searchDepth-1].low
                    || stockData.dailyData[searchDepth].low > stockData.dailyData[searchDepth+1].low)
                {
                    continue;
                }

                double max = 0;
                int maxDate = 0;

                bool fail = false;
                for (int i = 0; i < searchDepth; i++)
                {
                    if (stockData.dailyData[i].low < stockData.dailyData[searchDepth].low)
                    {
                        fail = true;
                        break;
                    }

                    if (stockData.dailyData[i].high > max)
                    {
                        max = stockData.dailyData[i].high;
                        maxDate = i;
                    }
                }
                if (fail)
                {
                    continue;
                }
                if (maxDate <= MIN_MAX_DEPTH)
                {
                    continue;
                }

                fail = false;
                for (int i = 1; i <= maxDate; i++)
                {
                    if (stockData.dailyData[i].low < stockData.dailyData[0].low)
                    {
                        fail = true;
                        break;
                    }
                }
                if (fail)
                {
                    continue;
                }

                double raise = max - stockData.dailyData[searchDepth].low;
                double drop = max - stockData.dailyData[0].low;
                double dropRate = drop / raise;
                if (dropRate > 0.45 && dropRate < 0.55)
                {
                    double score = raise / stockData.dailyData[searchDepth].low / searchDepth * 1000;
                    if (score > highScore)
                    {
                        highScore = score;
                        highDepth = searchDepth;
                        highDropRate = dropRate;
                    }
                }
            }

            if (highScore > 0)
            {
                result.figureBounceScore = highScore;
                result.dropRate = highDropRate;
                result.startDate = stockData.dailyData[highDepth].date.ToShortDateString();
            }

            long volumeBefore = 0;
            long volumeAfter = 0;

            for (int i = 0; i <= highDepth; i++)
            {
                volumeAfter += stockData.dailyData[i].volume;
            }

            for (int i = highDepth + 1; i <= highDepth * 2 + 1; i++)
            {
                volumeBefore += stockData.dailyData[i].volume;
            }

            double volumeScore = (double)volumeAfter / (double)volumeBefore;
            result.volumeScore = volumeScore;

            if (stockData.dailyData[0].isRaise() && stockData.dailyData[0].low < stockData.dailyData[1].low && stockData.dailyData[0].close > stockData.dailyData[1].close)
            {
                result.figureRevertScore = ((stockData.dailyData[1].low / stockData.dailyData[0].low) * (stockData.dailyData[0].close / stockData.dailyData[1].close) - 1) * 100;
            }
            else
            {
                result.figureRevertScore = 0;
            }

            return result;
        }
    }
}
