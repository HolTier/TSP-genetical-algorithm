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
        private int _generation = 0;
        private City cityA = new City(0, 0, "A", "0001");
        private List<City> cityList = new List<City>()
        {
            new City(32, 41, "B", "0010"),
            new City(50, 50, "C", "0011"),
            new City(10, 0, "D", "0100"),
            new City(0, 10, "E", "0101"),
            new City(50, 0, "F", "0110"),
            new City(33, 50, "G", "0111"),
            new City(12, 10, "H", "1000"),
            new City(35, 33, "I", "1001"),
            new City(100, 10, "J", "1010"),
            new City(91, 50, "K", "1011"),
            new City(105, 30, "L", "1100"),
            new City(111, 12, "M", "1101"),
            new City(60, 60, "N", "1110"),
        };

        private List<City> citiesWithStart;

        private TSPGenetic tspGenetic;

        // FIFO for drawing the lines
        private Queue<ulong> bestGenomes = new Queue<ulong>();

        public TSPForm()
        {
            InitializeComponent();

            citiesWithStart = [cityA, .. cityList];
            tspGenetic = new TSPGenetic();

            tspGenetic.GenerationCompleted += UpdateUI;
            // Draw the cities on the panel
            TSPPanel.Paint += (sender, e) =>
            {
                foreach (City city in citiesWithStart)
                {
                    e.Graphics.FillEllipse(Brushes.Black, city.X*5, city.Y * 5, 5, 5);
                    e.Graphics.DrawString(city.Name, new Font("Arial", 8), Brushes.Black, city.X*5, city.Y*5);
                }
            };

            // Start the task to draw the lines
            Task.Run(DrawLineBetweenCites);

            // start the genetic algorithm as a task
            Task.Run(() => tspGenetic.GeneticAlgorithm(cityList, cityA, 5000, 1000));

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
                    ulong bestGenome = bestGenomes.Dequeue();

                    // Convert the genome to string of cities
                    List<string> citesCode = TSPGenetic.ConvertUintToListOfStrings(bestGenome);

                    // Convert the string of cities to list of cities
                    List<City> citiesFromGenome = new List<City>();
                    foreach (string code in citesCode)
                    {
                        citiesFromGenome.Add(citiesWithStart.Find(c => c.Gen == code));
                    }

                    // Redraw the lines and cities
                    using (Graphics g = TSPPanel.CreateGraphics())
                    {
                        g.Clear(Color.White);
                        foreach (City city in citiesWithStart)
                        {
                            g.FillEllipse(Brushes.Black, city.X * 6, city.Y * 6, 5, 5);
                            g.DrawString(city.Name, new Font("Arial", 8), Brushes.Black, city.X * 5, city.Y * 5);
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
    }
}
