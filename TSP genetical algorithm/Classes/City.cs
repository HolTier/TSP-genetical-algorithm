using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP_genetical_algorithm.Classes
{
    public class City
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Name { get; set; }

        // Gen is used to identify the city in the population array, stored as binary
        public string Gen { get; set; }
        public int GenDecimal { get; set; }

        public City(int x, int y, string name, int genDecimal)
        {
            X = x;
            Y = y;
            Name = name;
            GenDecimal = genDecimal;

            Gen = Convert.ToString(genDecimal, 2).PadLeft(4, '0');
        }

        public double DistanceTo(City city)
        {
            int xDistance = Math.Abs(X - city.X);
            int yDistance = Math.Abs(Y - city.Y);
            double distance = Math.Sqrt((xDistance * xDistance) + (yDistance * yDistance));

            return distance;
        }

        public override string ToString()
        {
            return $"{Name} ({X}, {Y}) cityGen: {Gen}";
        }
    }
}
