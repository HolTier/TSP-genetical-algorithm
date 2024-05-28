using System;
using System.Collections.Concurrent;
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
        private List<List<string>> population;
        public bool canEvolve = true;
        private ConcurrentDictionary<List<string>, double> fitnessCache;
        private object populationLock = new object();

        public TSPGenetic(List<City> cities, City cityA)
        {
            AddNewCity += AddCity;
            Shuffle += (sender, e) =>
            {
                canEvolve = false;
                cities = e.Cities;
                cityA = e.CityA;
                citiesWithStart = new List<City> { cityA };
                citiesWithStart.AddRange(cities);
                fitnessCache = new ConcurrentDictionary<List<string>, double>(new ListComparer<string>());
                canEvolve = true;
                
            };

            this.cities = cities;
            this.cityA = cityA;
            citiesWithStart = new List<City> { cityA };
            citiesWithStart.AddRange(cities);
            fitnessCache = new ConcurrentDictionary<List<string>, double>(new ListComparer<string>());
        }

        private void AddCity(object sender, AddNewCityEventArgs e)
        {
            lock (populationLock)
            {
                canEvolve = false;
                cities.Add(e.City);
                citiesWithStart.Add(e.City);

                if (population != null)
                {
                    foreach (List<string> genome in population)
                    {
                        genome.Add(e.City.Gen);
                    }
                }

                canEvolve = true;
            }
        }

        public async void GeneticAlgorithm(int populationSize, int generations)
        {
            lock (populationLock)
            {
                population = GeneratePopulation(populationSize, cities, cityA);
            }

            for (int i = 0; i < generations; i++)
            {
                if (canEvolve)
                {
                    // Calculate fitness values
                    var fitnessValues = population.AsParallel().Select(genome => fitnessFunction(genome)).ToList();

                    // Sort population by fitness
                    var sortedPopulation = population
                        .Select((genome, index) => new { Genome = genome, Fitness = fitnessValues[index] })
                        .OrderBy(x => x.Fitness)
                        .ToList();

                    // Select the best genomes
                    var bestGenomes = sortedPopulation.Take(populationSize / 2).Select(x => x.Genome).ToList();
                    var newPopulation = new List<List<string>>(bestGenomes);

                    // Crossover
                    for (int j = 0; j < bestGenomes.Count - 1; j += 2)
                    {
                        newPopulation.Add(OrderCrossover(bestGenomes[j], bestGenomes[j + 1]));
                        newPopulation.Add(OrderCrossover(bestGenomes[j + 1], bestGenomes[j]));
                    }

                    // Mutate
                    for (int j = (int)(newPopulation.Count / 2); j < newPopulation.Count; j++)
                    {
                        newPopulation[j] = Mutate(newPopulation[j]);
                    }

                    // Update population
                    lock (populationLock)
                    {
                        population = newPopulation;
                    }

                    // Get the best genome
                    var bestGenome = sortedPopulation.First().Genome;
                    var bestFitness = sortedPopulation.First().Fitness;

                    // Notify the UI
                    GenerationCompleted?.Invoke(this, new GenerationCompletedEventArgs
                    {
                        BestGenome = bestGenome,
                        Cost = bestFitness
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

        private List<string> Mutate(List<string> genome)
        {
            var newGenome = new List<string>(genome.ToList());
            var random = new Random();

            // Swap two random genes
            if (random.Next(0, 100) < 10)
            {
                var twoRandomGenes = Enumerable.Range(1, genome.Count - 1).OrderBy(x => Guid.NewGuid()).Take(2).ToList();
                var temp = newGenome[twoRandomGenes[0]];
                newGenome[twoRandomGenes[0]] = newGenome[twoRandomGenes[1]];
                newGenome[twoRandomGenes[1]] = temp;
            }

            return newGenome;
        }

        private List<string> OrderCrossover(List<string> parent1, List<string> parent2)
        {
            // Order crossover
            // Copy the parents
            var random = new Random();
            var parent1Copy = new List<string>(parent1.Skip(1));
            var parent2Copy = new List<string>(parent2.Skip(1));

            // Select two random points
            int length = parent1Copy.Count;
            int point1 = random.Next(1, length);
            int point2 = random.Next(1, length);

            if (point1 > point2)
            {
                (point1, point2) = (point2, point1);
            }

            // Create the child genome
            var childGenome = new string[length];
            for (int i = point1; i <= point2; i++)
            {
                childGenome[i] = parent1Copy[i];
            }

            // Copy the rest of the genes from parent2
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

            // Add the first gene from parent1
            var finalGenome = new List<string> { parent1[0] };
            finalGenome.AddRange(childGenome);

            return finalGenome;
        }

        private List<List<string>> GeneratePopulation(int populationSize, List<City> cities, City cityA)
        {
            var population = new List<List<string>>();
            for (int i = 0; i < populationSize; i++)
            {
                population.Add(GenerateGenome(cities, cityA));
            }
            return population;
        }

        private List<string> GenerateGenome(List<City> cities, City cityA)
        {
            var random = new Random();
            var shuffledCities = cities.OrderBy(x => random.Next()).ToList();

            var genomeCode = new List<string> { cityA.Gen };
            genomeCode.AddRange(shuffledCities.Select(city => city.Gen));

            return genomeCode;
        }

        private double fitnessFunction(List<string> genome)
        {
            // Check if the fitness value is already calculated
            if (fitnessCache.TryGetValue(genome, out var cachedValue))
            {
                return cachedValue;
            }

            // Calculate the total distance
            double totalDistance = 0;
            for (int i = 0; i < genome.Count - 1; i++)
            {
                // Find the cities in the list
                var city1 = citiesWithStart.Find(x => x.Gen == genome[i]);
                var city2 = citiesWithStart.Find(x => x.Gen == genome[i + 1]);
                totalDistance += city1.DistanceTo(city2);
            }

            // Add the distance from the last city to the first city
            var lastCity = citiesWithStart.Find(x => x.Gen == genome[genome.Count - 1]);
            var firstCity = citiesWithStart.Find(x => x.Gen == genome[0]);
            totalDistance += lastCity.DistanceTo(firstCity);

            // Cache the fitness value
            fitnessCache[genome] = totalDistance;
            return totalDistance;
        }

        private class ListComparer<T> : IEqualityComparer<List<T>>
        {
            // Compare two lists
            public bool Equals(List<T> x, List<T> y)
            {
                if (x == null || y == null || x.Count != y.Count)
                {
                    return false;
                }

                for (int i = 0; i < x.Count; i++)
                {
                    if (!x[i].Equals(y[i]))
                    {
                        return false;
                    }
                }
                return true;
            }

            // Get the hash code of the list
            public int GetHashCode(List<T> obj)
            {
                if (obj == null) return 0;

                int hash = 17;
                foreach (var element in obj)
                {
                    hash = hash * 23 + (element == null ? 0 : element.GetHashCode());
                }
                return hash;
            }
        }
    }
}
