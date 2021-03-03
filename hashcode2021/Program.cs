using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace hashcode2021
{
    class Program
    {
        static string[] inputFiles = {
            @"c:\temp\hashcode\a.txt",
            @"c:\temp\hashcode\b.txt",
            @"c:\temp\hashcode\c.txt",
            @"c:\temp\hashcode\d.txt",
            @"c:\temp\hashcode\e.txt",
            @"c:\temp\hashcode\f.txt"
        };

        static void Main(string[] args)
        {
            DateTime startTime = DateTime.Now;

            foreach (string fileName in inputFiles)
            {
                DateTime solveStartTime = DateTime.Now;
                Solve(fileName);
                Console.WriteLine("Solve time: {0}", new TimeSpan(DateTime.Now.Ticks - solveStartTime.Ticks));
            }

            Console.WriteLine("Runtime: {0}", new TimeSpan(DateTime.Now.Ticks - startTime.Ticks));
        }

        static void Solve(string fileName)
        {
            Problem problem = Problem.LoadProblem(fileName);
            Console.WriteLine("*****************");
            Console.WriteLine("{0}, Duration: {1}, Intersections: {2}, Bonus Per Car: {3}, Streets: {4}, Cars: {5}",
                fileName, problem.Duration, problem.Intersections.Count, problem.BonusPerCar, problem.Streets.Count, problem.Cars.Count);
            Console.WriteLine("Score upper bound: {0}", problem.CalculateScoreUpperBound());

            // Remove streets that not car is using for the possible green lights
            int removedStreets = problem.RemoveUnusedStreets();
            Console.WriteLine("Removed streets: {0}", removedStreets);

            Solution solution = new Solution(problem.Intersections.Count);

            // Generate a dummy solution - each incoming street will get a green light for 1 cycle
            InitBasicSolution(problem, solution);

            // Run simulation and try to change the order of green lights to minimize blocking
            problem.OptimizeGreenLightOrder(solution, new HashSet<int>());

            solution = OptimizeCycleDuration(problem, solution);

            // Remove streets where the only car that passes is a car that didn't finish from
            // the green light cycle
            solution = OptimizeCycleClearStreetsCarsDidntFinish(problem, solution);

            // Simulate solution
            SimulationResult simulationResult = problem.RunSimulation(solution);
            Console.WriteLine("Score: {0}", simulationResult.Score);

            // Generate output
            GenerateOutput(solution, fileName);
        }

        private static Solution OptimizeCycleClearStreetsCarsDidntFinish(Problem problem, Solution solution)
        {
            Solution newSolution = (Solution)solution.Clone();

            // Optimization - if a car didn't finish the drive & it's the only car on a street - remove 
            // that street from the green lights & stop the car
            SimulationResult result = problem.RunSimulation(solution);
            RemoveCycleStreetsUsedOnlyByCars(newSolution, result.CarsNotFinished);
            SimulationResult newResult = problem.RunSimulation(newSolution);

            if (newResult.Score > result.Score)
                return newSolution;
            else
                return solution;
        }

        private static void RemoveCycleStreetsUsedOnlyByCars(Solution solution, List<CarSimultionPosition> cars)
        {
            foreach (CarSimultionPosition car in cars)
                for (int s = 0; s < car.Car.Streets.Count - 1; s++)
                {
                    Street street = car.Car.Streets[s];
                    if (street.IncomingUsageCount == 1)
                    {
                        SolutionIntersection intersection = solution.Intersections[street.EndIntersection];
                        for (int g = 0; g < intersection.GreenLigths.Count; g++)
                        {
                            GreenLightCycle greenLightCycle = intersection.GreenLigths[g];
                            if (greenLightCycle.Street.UniqueID == street.UniqueID)
                            {
                                intersection.GreenLigths.RemoveAt(g);
                                g--;
                            }
                        }
                    }
                }
        }

        private static Solution OptimizeCycleDuration(Problem problem, Solution solution)
        {
            Solution bestSolution = null;
            int bestSolutionScore = -1;
            int timesSinceOptimized = 0;
            while (timesSinceOptimized < 10)
            {
                timesSinceOptimized++;

                SimulationResult simulationResult = problem.RunSimulation(solution);

                if (simulationResult.Score > bestSolutionScore)
                {
                    bestSolution = (Solution)solution.Clone();
                    bestSolutionScore = simulationResult.Score;
                    timesSinceOptimized = 0;
                }
                Console.WriteLine("Score: {0}, Max Blocked Traffic: {1}, Cars not finished: {2}, Best score: {3}", simulationResult.Score, simulationResult.GetMaxBlockedTraffic(), simulationResult.CarsNotFinished.Count, bestSolutionScore);

                List<SimulationResult.IntersectionResult> intersectionResults = simulationResult.IntersectionResults.OrderByDescending(o => o.MaxStreetBlockedTraffic).ToList();
                // Remove intersection without blocked cars
                for (int i = 0; i < intersectionResults.Count; i++)
                {
                    if (intersectionResults[i].MaxStreetBlockedTraffic == 0)
                    {
                        intersectionResults.RemoveAt(i);
                        i--;
                    }
                }

                // Nothing to optimize - break
                if (intersectionResults.Count == 0)
                    break;

                // Add cycle time for the top blocked cars.
                // B - 64, C - 105, E - 152, F - 21
                for (int i = 0; i < intersectionResults.Count / 50; i++)
                {
                    SimulationResult.IntersectionResult intersectionResult = intersectionResults[i];
                    SolutionIntersection intersection = solution.Intersections[intersectionResult.ID];
                    foreach (GreenLightCycle greenLightCycle in intersection.GreenLigths)
                        if (greenLightCycle.Street.Name.Equals(intersectionResult.MaxStreetName))
                            greenLightCycle.Duration++;
                }
            }

            return bestSolution;
        }

        private static void InitBasicSolution(Problem problem, Solution solution)
        {
            foreach (Intersection i in problem.Intersections)
            {
                solution.Intersections[i.ID].GreenLigths = new List<GreenLightCycle>();
                foreach (Street street in i.IncomingStreets)
                {
                    GreenLightCycle cycle = new GreenLightCycle(street);
                    cycle.Duration = 1;
                    solution.Intersections[i.ID].GreenLigths.Add(cycle);
                }
            }

        }

        private static void GenerateOutput(Solution solution, string fileName)
        {
            using (StreamWriter sw = new StreamWriter(fileName + ".out"))
            {
                sw.WriteLine(solution.CountIntersectionsWithGreenLights());

                foreach (SolutionIntersection i in solution.Intersections)
                {
                    // Count non-zero duration streets
                    int streetCount = 0;
                    foreach (GreenLightCycle c in i.GreenLigths)
                        if (c.Duration > 0)
                            streetCount++;

                    // Skip intersection - no streets
                    if (streetCount == 0)
                        continue;

                    sw.WriteLine(i.ID);
                    sw.WriteLine(streetCount);
                    foreach (GreenLightCycle c in i.GreenLigths)
                        if (c.Duration > 0)
                        {
                            sw.Write(c.Street.Name);
                            sw.Write(" ");
                            sw.WriteLine(c.Duration);
                        }
                }
            }
        }
    }
}
