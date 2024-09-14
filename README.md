# TSP Genetic Algorithm

This project is a C# implementation of a Genetic Algorithm (GA) to solve the Traveling Salesman Problem (TSP), with a WinForms-based graphical interface for visualization. The algorithm evolves a population of potential routes to find an optimized solution for the TSP using evolutionary techniques such as selection, crossover, and mutation.

## Table of Contents
- [Features](#features)
- [Installation](#installation)
- [Usage](#usage)
- [Algorithm Overview](#algorithm-overview)
- [Configuration](#configuration)
- [License](#license)

## Features
- Solves the Traveling Salesman Problem using a Genetic Algorithm in C#.
- Visual representation of the TSP solution using Windows Forms.
- Configurable genetic algorithm parameters: population size, mutation rate, generations, etc.
- Real-time visualization of the algorithm's progress.

## Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/HolTier/TSP-genetical-algorithm.git

2. Open the solution file `TSP_GeneticAlgorithm.sln` in Visual Studio (or your preferred C# IDE).

3. Build the solution to restore NuGet packages and compile the project.

4. Run the application from Visual Studio by pressing `F5` or selecting **Debug** > **Start Debugging**.

## Usage

The application provides a graphical user interface (GUI) where you can:

- Set parameters such as population size, mutation rate, number of generations, etc.
- Visualize the progress of the genetic algorithm as it attempts to find the shortest route.
- Watch the final solution and the shortest path found after the algorithm completes its execution.

## Algorithm Overview

### Genetic Algorithm Steps:

1. **Initialization**: Generate a random population of potential routes.
2. **Selection**: Choose the best-performing routes based on their total travel distance.
3. **Crossover**: Combine parts of two parent routes to create offspring.
4. **Mutation**: Apply random changes to offspring routes to introduce genetic diversity.
5. **Evaluation**: Calculate the fitness of each route by determining the total travel distance.
6. **Termination**: Continue evolving the population until the number of generations is reached or an optimal route is found.

### TSP Representation:

- The cities in the TSP are represented as points on a 2D plane.
- The goal is to find the shortest route that visits each city exactly once and returns to the starting city.

## Configuration

You can adjust the following genetic algorithm parameters in the Code:

- **Population Size**: The number of routes in each generation.
- **Mutation Rate**: The probability of mutations occurring in offspring.
- **Generations**: The total number of generations for which the algorithm will run.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

