using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockPicker.Model;
using System.IO;
using StockPicker.Utils;

namespace StockPicker.Engine
{
    class RecommendingEngine
    {
        private PickingService pickingEngine { get; set; }
        private List<Recommendation> existingRecommendations;

        private const string HEADER = "股票代码,股票名称,最深回撤,起涨日期,形态得分,成交量得分,均线得分,总得分";
        private const string TEMPLATE = "{0},{1},{2},{3},{4},{5},{6},{7}";

        public RecommendingEngine(PickingService pickingEngine)
        {
            this.pickingEngine = pickingEngine;
        }

        public void recommend(int number)
        {
            List<Recommendation> recommendations = pickingEngine.PickStock(number);

            string filePath = CommonUtils.generateReportFilePath();
            Stream fStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite);
            StreamWriter sWriter = new StreamWriter(fStream, Encoding.Default);
            sWriter.WriteLine(HEADER);

            foreach (Recommendation r in recommendations)
            {
                string line = string.Format(TEMPLATE,
                    r.code,
                    r.name,
                    string.Format("{0:N2}%", r.result.dropRate * 100),
                    r.result.startDate,
                    string.Format("{0:N2}", r.result.figureBounceScore),
                    string.Format("{0:N2}", r.result.volumeScore),
                    string.Format("{0:N2}", r.result.maScore),
                    string.Format("{0:N2}", r.result.getTotalScore())
                );
                Console.WriteLine(line);
                sWriter.WriteLine(line);
            }


            sWriter.Close();
            fStream.Close();
        }

        public void monitor(int threshold)
        {
            existingRecommendations = pickingEngine.PickStock(threshold);
            while (true)
            {
                List<Recommendation> recommendations = pickingEngine.PickStock(threshold);
                foreach (Recommendation r in recommendations)
                {
                    if (!existingRecommendations.Contains(r))
                    {
                        Console.WriteLine(r.code);
                        existingRecommendations.Add(r);
                    }
                }
            }
        }

    }
}
