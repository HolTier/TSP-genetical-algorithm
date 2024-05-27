using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TSP_genetical_algorithm.Classes;

namespace TSP_genetical_algorithm
{
    public partial class TSPForm : Form
    {
        private bool canAddCity = true;
        private int _generation = 0;
        private City cityA = new City(0, 0, "A", 0);
        private List<City> cityList = new List<City>()
        {
            new City(32, 41, "B", 1),
            new City(50, 50, "C", 2),
            new City(10, 0, "D", 3),
            new City(0, 10, "E", 4),
            new City(50, 0, "F", 5),
            new City(33, 50, "G", 6),
            new City(12, 10, "H", 7),
            new City(35, 33, "I", 8),
            new City(100, 10, "J", 9),
            new City(91, 50, "K", 10),
            new City(100, 100, "L", 11),
            new City(0, 60, "M", 12),
            new City(10, 22, "N", 13),
        };

        private List<City> citiesWithStart;

        private TSPGenetic tspGenetic;

        // FIFO for drawing the lines
        private Queue<List<string>> bestGenomes = new Queue<List<string>>();

        public TSPForm()
        {
            InitializeComponent();

            citiesWithStart = [cityA, .. cityList];
            tspGenetic = new TSPGenetic(cityList, cityA);

            tspGenetic.GenerationCompleted += UpdateUI;
            // Draw the cities on the panel
            TSPPanel.Paint += (sender, e) =>
            {
                foreach (City city in citiesWithStart)
                {
                    e.Graphics.FillEllipse(Brushes.Black, city.X * 5, city.Y * 5, 5, 5);
                    e.Graphics.DrawString(city.Name, new Font("Arial", 8), Brushes.Black, city.X * 5, city.Y * 5);
                }
            };

            // Start the task to draw the lines
            Task.Run(DrawLineBetweenCites);

            // start the genetic algorithm as a task
            Task.Run(() => tspGenetic.GeneticAlgorithm(100, 100000));

        }

        private void UpdateUI(object? sender, GenerationCompletedEventArgs e)
        {
            generationLabel.Invoke(new Action(() => generationLabel.Text = $"Generation: {_generation++}"));
            costLabel.Invoke(new Action(() => costLabel.Text = $"Cost: {e.Cost}"));

            bestGenomes.Enqueue(e.BestGenome);

            //return Task.CompletedTask;
        }

        private void DrawLineBetweenCites()
        {
            while (true)
            {
                if (bestGenomes.Count > 0)
                {
                    // Get the best genome from the queue
                    List<string> bestGenome = bestGenomes.Dequeue();

                    // Convert the string of cities to list of cities
                    List<City> citiesFromGenome = new List<City>();
                    foreach (string code in bestGenome)
                    {
                        citiesFromGenome.Add(citiesWithStart.Find(c => c.Gen == code));
                    }

                    // Redraw the lines and cities
                    using (Graphics g = TSPPanel.CreateGraphics())
                    {
                        g.Clear(Color.White);
                        foreach (City city in citiesWithStart.ToList())
                        {
                            g.FillEllipse(Brushes.Black, city.X * 6, city.Y * 6, 5, 5);
                            g.DrawString(city.Name, new Font("Arial", 8), Brushes.Black, city.X * 6, city.Y * 6);
                        }
                        for (int i = 0; i < citiesFromGenome.Count - 1; i++)

                        {
                            g.DrawLine(Pens.Red, citiesFromGenome[i].X * 6, citiesFromGenome[i].Y * 6, citiesFromGenome[i + 1].X * 6, citiesFromGenome[i + 1].Y * 6);
                        }

                        g.DrawLine(Pens.Red, citiesFromGenome[citiesFromGenome.Count - 1].X * 6, citiesFromGenome[citiesFromGenome.Count - 1].Y * 6, cityA.X * 6, cityA.Y * 6);
                    }

                    string cities = string.Join(" -> ", citiesFromGenome.Select(c => c.Name));

                    citiesLabel.Invoke(new Action(() => citiesLabel.Text = cities));
                }
            }
        }

        private void TSPPanel_MouseClick(object sender, MouseEventArgs e)
        {
            // Add new city
            if (e.Button == MouseButtons.Left && canAddCity)
            {
                canAddCity = false;
                // Convert count to ASCII char
                char c = (char)(cityList.Count + 1 + 65);

                City newCity = new City(e.X / 6, e.Y / 6, c.ToString(), citiesWithStart.Max(c => c.GenDecimal) + 1);
                cityList.Add(newCity);
                citiesWithStart.Add(newCity);
                TSPPanel.Invalidate();

                tspGenetic.AddNewCity?.Invoke(this, new AddNewCityEventArgs() { City = newCity });
                canAddCity = true;
            }
        }

        private void shuffleButton_Click(object sender, EventArgs e)
        {
            // Shuffle the cities positions
            Random rnd = new Random();
            foreach (City city in cityList.ToList())
            {
                city.X = rnd.Next(0, 100);
                city.Y = rnd.Next(0, 60);
            }

            // shuffle the city A position
            cityA.X = rnd.Next(0, 100);
            cityA.Y = rnd.Next(0, 60);

            tspGenetic.Shuffle?.Invoke(this, new ShuffleEventArgs() { Cities = cityList, CityA = cityA });

        }
    }
}
