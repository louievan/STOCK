using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockPicker.Model;
using StockPicker.Service;
using StockPicker.Engine;
using StockPicker.Utils;

namespace StockPicker
{
    class Program
    {
        static void Main(string[] args)
        {
            DataService dataService = new DataService();
            RatingService ratingService = new RatingService();

            //ratingService.rate(dataService.stockDatas["601222"]);

            PickingService pickingService = new PickingService(ratingService, dataService);
            RecommendingEngine recommendingEngine = new RecommendingEngine(pickingService);

            //recommendingEngine.monitor(10);
            recommendingEngine.recommend(50);

            CommonUtils.sendMail();
        }
    }
}
