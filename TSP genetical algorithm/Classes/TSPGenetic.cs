using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP_genetical_algorithm.Classes
{
    public class GenerationCompletedEventArgs : EventArgs
    {
        public List<string> BestGenome { get; set; }
        public double Cost { get; set; }
    }

    public class AddNewCityEventArgs : EventArgs
    {
        public City City { get; set; }
    }

    public class ShuffleEventArgs : EventArgs
    {
        public City CityA { get; set; }
        public List<City> Cities { get; set; }
    }

    public class TSPGenetic
    {
        public EventHandler<GenerationCompletedEventArgs> GenerationCompleted;
        public EventHandler<AddNewCityEventArgs> AddNewCity;
        public EventHandler<ShuffleEventArgs> Shuffle;

        private List<City> cities;
        private City cityA;
        private List<City> citiesWithStart;
        List<List<string>> population;

        private bool canEvolve = true;

        public TSPGenetic(List<City> cities, City cityA)
        {
            AddNewCity += AddCity;
            Shuffle += (sender, e) =>
            {
                canEvolve = false;
                cities = e.Cities;
                cityA = e.CityA;
                citiesWithStart = [cityA, ..cities];
                canEvolve = true;
            };

            this.cities = cities;
            this.cityA = cityA;

            citiesWithStart = [cityA, ..cities ];

        }

        private void AddCity(object? sender, AddNewCityEventArgs e)
        {
            canEvolve = false;
            cities.Add(e.City);
            citiesWithStart.Add(e.City);

            // Add the new city to the population (on end of the genome)
            if(population != null)
            {
                foreach (List<string> genome in population)
                {
                    genome.Add(e.City.Gen);
                }
            }

            canEvolve = true;
        }

        // Evolutionary main loop
        public async void GeneticAlgorithm(int populationSize, int generations)
        {
            population = GeneratePopulation(populationSize, cities, cityA);


            for (int i = 0; i < generations; i++)
            {
                if (canEvolve)
                {
                    // Calculate the fitness of each genome
                    List<double> fitnessValues = new List<double>();

                    foreach (List<string> genome in population)
                    {
                        fitnessValues.Add(fitnessFunction(genome, citiesWithStart));
                    }

                    // Sort the population by fitness 
                    List<List<string>> sortedPopulation = population.OrderBy(x => fitnessFunction(x, citiesWithStart.ToList())).ToList();

                    // Select the best genomes
                    List<List<string>> bestGenomes = sortedPopulation.GetRange(0, populationSize / 2);

                    // Order Crossover
                    List<List<string>> newPopulation = [..bestGenomes];

                    for (int j = 0; j < bestGenomes.Count - 1; j += 2)
                    {
                        List<string> child1Genome = OrderCrossover(bestGenomes[j], bestGenomes[j + 1]);
                        newPopulation.Add(child1Genome);

                        List<string> child2Genome = OrderCrossover(bestGenomes[j + 1], bestGenomes[j]);
                        newPopulation.Add(child2Genome);
                    }

                    // Mutate
                    for (int j = 0; j < newPopulation.Count / 2; j++)
                    {
                        newPopulation[j] = Mutate(newPopulation[j], citiesWithStart.ToList());
                    }

                    // Replace the old population with the new population
                    //population = [.. bestGenomes, .. newPopulation];
                    population = newPopulation;

                    // Sort the population by fitness
                    //population = population.OrderBy(x => fitnessFunction(x, citiesWithStart.ToList())).ToList();

                    // Raise the event
                    GenerationCompleted?.Invoke(this,
                        new GenerationCompletedEventArgs()
                        {
                            // Return the genom with the lowest cost
                            BestGenome = population.Find(x => fitnessFunction(x, citiesWithStart.ToList())
                                == population.Min(y => fitnessFunction(y, citiesWithStart.ToList()))),
                            Cost = population.Min(x => fitnessFunction(x, citiesWithStart.ToList()))
                        });

                    await Task.Delay(100);
                }
                else
                {
                    i--;
                    break;
                }
            }
            
        }

        private static List<string> Mutate(List<string> genome, List<City> cities)
        {
            // Copy the genome
            List<string> newGenome = new List<string>(genome);

            // Switch genes place (1% chance), except for the first gene
            Random random = new Random();
            if (random.Next(0, 100) < 1)
            {
                // Get two random genes, without repeat
                List<int> twoRandomGenes = new List<int>();

                twoRandomGenes.AddRange(Enumerable.Range(1, genome.Count - 1).OrderBy(x => Guid.NewGuid()).Take(2));

                string temp = newGenome[twoRandomGenes[0]];
                newGenome[twoRandomGenes[0]] = newGenome[twoRandomGenes[1]];
                newGenome[twoRandomGenes[1]] = temp;
            }

            // return UINT
            return newGenome;
        }

        public static List<string> ConvertUintToListOfStrings(ulong v)
        {
            // Convert uint to binary string
            string genomeCode = Convert.ToString((long)v, 2);

            // Ensure the length of genomeCode is a multiple of 4 by adding leading zeros if necessary
            int remainder = genomeCode.Length % 4;
            if (remainder != 0)
            {
                genomeCode = genomeCode.PadLeft(genomeCode.Length + (4 - remainder), '0');
            }

            // Split the binary string into 4-bit strings
            List<string> cityGenes = new List<string>();
            for (int i = 0; i < genomeCode.Length - 1; i += 4)
            {
                cityGenes.Add(genomeCode.Substring(i, 4));
            }

            return cityGenes;
        }

        private List<string> OrderCrossover(List<string> parent1, List<string> parent2)
        {
            // Initialize random number generator
            Random random = new Random();

            // Create the copy of the parents and remove the first gene
            List<string> parent1Copy = new List<string>(parent1);
            List<string> parent2Copy = new List<string>(parent2);

            parent1Copy.RemoveAt(0);
            parent2Copy.RemoveAt(0);

            // Determine crossover points
            int length = parent1Copy.Count;
            int point1 = random.Next(1, length);  // Ensure not to pick the first gene
            int point2 = random.Next(1, length);

            // Ensure point1 is less than point2
            if (point1 > point2)
            {
                int temp = point1;
                point1 = point2;
                point2 = temp;
            }

            // Create the child genome with placeholders
            List<string> childGenome = new List<string>(new string[length]);

            // Copy the substring from parent1 to child
            for (int i = point1; i <= point2; i++)
            {
                childGenome[i] = parent1Copy[i];
            }

            // Fill the remaining positions with genes from parent2
            int currentIndex = (point2 + 1) % length;
            foreach (var gene in parent2Copy)
            {
                if (!childGenome.Contains(gene))
                {
                    while (childGenome[currentIndex] != null)
                    {
                        currentIndex = (currentIndex + 1) % length;
                    }
                    childGenome[currentIndex] = gene;
                    currentIndex = (currentIndex + 1) % length;
                }
            }

            // Add the first gene
            childGenome.Insert(0, parent1[0]);

            return childGenome;
        }

        private static List<string> GenerateGenome(List<City> cities, City cityA)
        {
            // shuffle the list
            Random random = new Random();
            List<City> shuffledCities = cities.OrderBy(x => random.Next()).ToList();

            // generate the genome
            List<string> genomeCode = new List<string>() { cityA.Gen };

            foreach (City city in shuffledCities)
            {
                genomeCode.Add(city.Gen);
            }


            return genomeCode;
        }

        private static List<List<string>> GeneratePopulation(int populationSize, List<City> cities, City cityA)
        {
            List<List<string>> population = new List<List<string>>();

            for (int i = 0; i < populationSize; i++)
            {
                population.Add(GenerateGenome(cities, cityA));
            }

            return population;
        }

        private static double fitnessFunction(List<string> genome, List<City> cities)
        {
            // Calculate the total distance
            double totalDistance = 0;

            for (int i = 0; i < genome.Count - 1; i++)
            {
                City city1 = cities.Find(x => x.Gen == genome[i]);
                City city2 = cities.Find(x => x.Gen == genome[i + 1]);

                totalDistance += city1.DistanceTo(city2);
            }

            // Add the distance from the last city to the first city
            City lastCity = cities.Find(x => x.Gen == genome[genome.Count - 1]);
            City firstCity = cities.Find(x => x.Gen == genome[0]);

            totalDistance += lastCity.DistanceTo(firstCity);

            return totalDistance;
        }
    }
}
