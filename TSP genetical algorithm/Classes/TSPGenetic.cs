﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP_genetical_algorithm.Classes
{
    public class GenerationCompletedEventArgs : EventArgs
    {
        public ulong BestGenome { get; set; }
        public double Cost { get; set; }
    }

    public class TSPGenetic
    {
        public EventHandler<GenerationCompletedEventArgs> GenerationCompleted;

        public TSPGenetic()
        {
        }

        // Evolutionary main loop
        public async void GeneticAlgorithm(List<City> cities, City cityA ,int populationSize, int generations)
        {
            List<ulong> population = GeneratePopulation(populationSize, cities, cityA);
            List<City> citiesWithStart = [cityA, .. cities];

            for (int i = 0; i < generations; i++)
            {
                // Calculate the fitness of each genome
                List<double> fitnessValues = new List<double>();

                foreach (ulong genome in population)
                {
                    fitnessValues.Add(fitnessFunction(genome, citiesWithStart));
                }

                // Sort the population by fitness 
                List<ulong> sortedPopulation = population.OrderBy(x => fitnessFunction(x, citiesWithStart)).ToList();

                // Select the best genomes
                List<ulong> bestGenomes = sortedPopulation.GetRange(0, populationSize / 2);

                // Crossover
                List<ulong> newPopulation = bestGenomes;

                //for (int j = 0; j < bestGenomes.Count; j += 2)
                //{
                //    uint parent1 = bestGenomes[j];
                //    uint parent2 = bestGenomes[j + 1];

                //    uint child1 = Crossover(parent1, parent2);
                //    uint child2 = Crossover(parent2, parent1);

                //    newPopulation.Add(child1);
                //    newPopulation.Add(child2);
                //}

                // Mutate
                for (int j = 0; j < newPopulation.Count / 2; j++)
                {
                    newPopulation[j] = Mutate(newPopulation[j], citiesWithStart);
                }

                // Replace the old population with the new population
                population = [..bestGenomes, ..newPopulation];

                // Sort the population by fitness
                population = population.OrderBy(x => fitnessFunction(x, citiesWithStart)).ToList();

                // Raise the event
                GenerationCompleted?.Invoke(this, 
                    new GenerationCompletedEventArgs() 
                    { 
                        // Return the genom with the lowest cost
                        BestGenome = population.Find(x => fitnessFunction(x, citiesWithStart) == population.Min(y => fitnessFunction(y, citiesWithStart))),
                        Cost = population.Min(x => fitnessFunction(x, citiesWithStart))
                    });

                //await Task.Delay(100);
            }
        }

        private static ulong Mutate(ulong v, List<City> cities)
        {
            List<string> cityGenes = ConvertUintToListOfStrings(v);

            // Switch genes place (1% chance), except for the first gene
            Random random = new Random();
            if (random.Next(0, 100) < 50)
            {
                // Get two random genes, without repeat
                List<int> twoRandomGenes = new List<int>();

                twoRandomGenes.AddRange(Enumerable.Range(1, cityGenes.Count - 1).OrderBy(x => Guid.NewGuid()).Take(2));

                string temp = cityGenes[twoRandomGenes[0]];
                cityGenes[twoRandomGenes[0]] = cityGenes[twoRandomGenes[1]];
                cityGenes[twoRandomGenes[1]] = temp;
            }

            // return UINT
            return Convert.ToUInt64(string.Join("", cityGenes), 2);
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

        private static ulong Crossover(ulong parent1, ulong parent2)
        {
            // Convert parent1 to binary string
            List<string> cityGenesParent1 = ConvertUintToListOfStrings(parent1);

            // Convert parent2 to binary string
            List<string> cityGenesParent2 = ConvertUintToListOfStrings(parent2);

            // Crossover
            string firstGen = cityGenesParent1[0];

            // Remove the first gene from the list on both parents
            cityGenesParent1.RemoveAt(0);
            cityGenesParent2.RemoveAt(0);

            // Split the list in half
            int half = cityGenesParent1.Count / 2 + 1;

            // Create the child genome
            string childGenome = firstGen;

            for (int i = 0; i < half; i++)
            {
                childGenome += cityGenesParent1[i];
            }

            for (int i = half; i < cityGenesParent2.Count; i++)
            {
                childGenome += cityGenesParent2[i];
            }

            // Convert binary string to uint
            ulong child = Convert.ToUInt64(childGenome, 2);

            return child;

        }

        private static ulong GenerateGenome(List<City> cities, City cityA)
        {
            // shuffle the list
            Random random = new Random();
            List<City> shuffledCities = cities.OrderBy(x => random.Next()).ToList();

            // generate the genome
            string genomeCode = cityA.Gen;

            foreach (City city in shuffledCities)
            {
                genomeCode += city.Gen;
            }

            // Convert binary string to uint
            ulong genomeUint = Convert.ToUInt64(genomeCode, 2);

            return genomeUint;
        }

        private static List<ulong> GeneratePopulation(int populationSize, List<City> cities, City cityA)
        {
            List<ulong> population = new List<ulong>();

            for (int i = 0; i < populationSize; i++)
            {
                population.Add(GenerateGenome(cities, cityA));
            }

            return population;
        }

        private static double fitnessFunction(ulong genome, List<City> cities)
        {
            // Convert uint to binary string
            List<string> cityGenes = ConvertUintToListOfStrings(genome);

            // Calculate the total distance
            double totalDistance = 0;

            for (int i = 0; i < cityGenes.Count - 1; i++)
            {
                City city1 = cities.Find(x => x.Gen == cityGenes[i]);
                City city2 = cities.Find(x => x.Gen == cityGenes[i + 1]);

                totalDistance += city1.DistanceTo(city2);
            }

            // Add the distance from the last city to the first city
            City lastCity = cities.Find(x => x.Gen == cityGenes[cityGenes.Count - 1]);
            City firstCity = cities.Find(x => x.Gen == cityGenes[0]);

            totalDistance += lastCity.DistanceTo(firstCity);

            return totalDistance;
        }
    }
}