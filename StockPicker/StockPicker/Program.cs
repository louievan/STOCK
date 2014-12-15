using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockPicker.Model;
using StockPicker.Service;
using StockPicker.Engine;

namespace StockPicker
{
    class Program
    {
        static void Main(string[] args)
        {
            DataService dataService = new DataService();
            RatingService ratingService = new RatingService();

            //ratingService.rate(dataService.stockDatas["601222"]);

            PickingEngine pickingEngine = new PickingEngine(ratingService, dataService);
            pickingEngine.PickStock();
        }
    }
}
