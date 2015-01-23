using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockPicker.Model
{
    public class RatingResult
    {
        public double figureBounceScore;
        public double maScore;
        public double volumeScore;

        public string startDate;
        public double winningPoint;
        public double losingPoint;
        public double dropRate;

        public double startPoint;
        public double highPoint;
        public double retrievePoint;
        public double currentPoint;

        public RatingResult()
        {
            figureBounceScore = 0;
            maScore = 0;
            volumeScore = 0;
        }

        public double getTotalScore()
        {
            return figureBounceScore * maScore * volumeScore * dropRate;
        }
    }
}
