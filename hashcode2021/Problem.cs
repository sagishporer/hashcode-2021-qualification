using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace hashcode2021
{
    class Problem
    {
        public int Duration { get; private set; }
        public int BonusPerCar { get; private set; }
        public List<Intersection> Intersections { get; private set; }
        public Dictionary<string, Street> Streets { get; private set; }
        public List<Car> Cars { get; private set; }

        private Problem(int duration, int numberOfIntersections, int bonusPerCar, Dictionary<string, Street> streets, List<Car> cars)
        {
            this.Duration = duration;
            this.BonusPerCar = bonusPerCar;
            this.Streets = streets;
            this.Cars = cars;

            // Build intersections
            Intersections = new List<Intersection>();
            for (int i = 0; i < numberOfIntersections; i++)
                Intersections.Add(new Intersection(i, new List<Street>(), new List<Street>()));

            foreach (Street street in streets.Values)
            {
                Intersections[street.StartIntersection].OutgoingStreets.Add(street);
                Intersections[street.EndIntersection].IncomingStreets.Add(street);
            }

            // Count number of times a street is used as incoming street
            foreach (Car car in this.Cars)
                for (int i = 0; i < car.Streets.Count - 1; i++)
                    car.Streets[i].IncomingUsageCount++;

            // Update the number of cars on the street at the start
            foreach (Street street in this.Streets.Values)
                street.CarsOnStart = 0;

            foreach (Car car in this.Cars)
                car.Streets[0].CarsOnStart++;

        }

        /// <summary>
        /// Optimize green light by runnig full simulation & try to change the order of the green lights 
        /// to make the cars pass intersections without waiting. 
        /// </summary>
        /// <returns></returns>
        public int OptimizeGreenLightOrder(Solution solution, HashSet<int> dontOptimizeCars)
        {
            int score = 0;
            int currentTime = 0;

            foreach (SolutionIntersection intersetion in solution.Intersections)
                foreach (GreenLightCycle greenLightCycle in intersetion.GreenLigths)
                    greenLightCycle.GreenLightUsed = false;

            // Create list of cars in simulation
            List<CarSimultionPosition> carSimultionPositions = new List<CarSimultionPosition>();
            int simulationCarStart = -(this.Cars.Count + 1);
            foreach (Car car in this.Cars)
            {
                carSimultionPositions.Add(new CarSimultionPosition(car, simulationCarStart));
                simulationCarStart++;
            }

            // Init green lights
            foreach (SolutionIntersection intersection in solution.Intersections)
            {
                intersection.CurrentGreenLigth = 0;
                if (intersection.GreenLigths.Count > 0)
                    intersection.CurrentGreenLightChangeTime = intersection.GreenLigths[0].Duration;
                else
                    intersection.CurrentGreenLightChangeTime = int.MaxValue;
            }

            while (currentTime <= this.Duration)
            {
                // Update traffic lights cycle
                foreach (SolutionIntersection intersection in solution.Intersections)
                {
                    if (intersection.CurrentGreenLightChangeTime <= currentTime)
                    {
                        intersection.CurrentGreenLigth = (intersection.CurrentGreenLigth + 1) % intersection.GreenLigths.Count;
                        intersection.CurrentGreenLightChangeTime = currentTime + intersection.GreenLigths[intersection.CurrentGreenLigth].Duration;
                    }
                }

                // Update cars
                HashSet<int> usedIntersection = new HashSet<int>();
                for (int i = 0; i < carSimultionPositions.Count; i++)
                {
                    CarSimultionPosition carSimultionPosition = carSimultionPositions[i];
                    if (carSimultionPosition.TimeGotHere > currentTime)
                        break;

                    Street street = carSimultionPosition.Car.Streets[carSimultionPosition.StreetNumber];

                    // Check if a car already used this intersection this cycle
                    if (usedIntersection.Contains(street.EndIntersection))
                        continue;

                    SolutionIntersection intersection = solution.Intersections[street.EndIntersection];
                    // Not green light, try swapping to green light
                    if (!street.Name.Equals(intersection.GreenLigths[intersection.CurrentGreenLigth].Street.Name))
                    {
                        // Green light already used - can't do it
                        if (intersection.GreenLigths[intersection.CurrentGreenLigth].GreenLightUsed)
                            continue;

                        // Skip optimizations on specific cars
                        if (dontOptimizeCars.Contains(carSimultionPosition.Car.UniqueID))
                            continue;

                        // Optimization failed to improve
                        //// Car can't finish on time - don't optmize it
                        ////if (carSimultionPosition.TimeLeftOnDrive + currentTime > this.Duration)
                        ////    continue;

                        // Find required green light
                        int requiredGreenLight = -1;
                        for (int g = 0; g < intersection.GreenLigths.Count; g++)
                            if (street.Name.Equals(intersection.GreenLigths[g].Street.Name))
                            {
                                requiredGreenLight = g;
                                break;
                            }

                        // Required green light not found - skip
                        if (requiredGreenLight == -1)
                            continue;

                        // Required green light already used - skip
                        if (intersection.GreenLigths[requiredGreenLight].GreenLightUsed)
                            continue;

                        // Swap possible only if it's the same green light duration
                        if (intersection.GreenLigths[requiredGreenLight].Duration != intersection.GreenLigths[intersection.CurrentGreenLigth].Duration)
                            continue;

                        // Swap green lights - now the car can continue!
                        GreenLightCycle tmp = intersection.GreenLigths[requiredGreenLight];
                        intersection.GreenLigths[requiredGreenLight] = intersection.GreenLigths[intersection.CurrentGreenLigth];
                        intersection.GreenLigths[intersection.CurrentGreenLigth] = tmp;
                    }

                    // Mark intersection as used for this cycle
                    usedIntersection.Add(street.EndIntersection);

                    // Mark green light used - changing no longer possible
                    intersection.GreenLigths[intersection.CurrentGreenLigth].GreenLightUsed = true;

                    // Process car green light
                    carSimultionPosition.StreetNumber++;
                    Street newStreet = carSimultionPosition.GetCurrentStreet();
                    carSimultionPosition.TimeGotHere = currentTime + newStreet.Length;
                    carSimultionPosition.TimeLeftOnDrive -= newStreet.Length;

                    // Check if car finished
                    if (carSimultionPosition.StreetNumber == carSimultionPosition.Car.Streets.Count - 1)
                    {
                        // Check if finished on time - if so give bonus
                        if (carSimultionPosition.TimeGotHere <= this.Duration)
                            score += this.BonusPerCar + (this.Duration - carSimultionPosition.TimeGotHere);

                        carSimultionPositions.RemoveAt(i);
                        i--;
                    }
                }

                // Sort cars by time
                carSimultionPositions.Sort((x, y) => x.TimeGotHere.CompareTo(y.TimeGotHere));

                currentTime++;
            }

            return score;
        }

        public int OptimizeGreenLightOrder2(Solution solution, HashSet<int> dontOptimizeCars)
        {
            CarSimultionPositionByTimeGotHere carSimultionPositionByTimeGotHere = new CarSimultionPositionByTimeGotHere();
            int score = 0;
            int currentTime = 0;

            Dictionary<int, List<CarSimultionPosition>> carQueueByIntersection = new Dictionary<int, List<CarSimultionPosition>>();
            foreach (Intersection intersection in this.Intersections)
                carQueueByIntersection.Add(intersection.ID, new List<CarSimultionPosition>());

            // Create car queue in each intersection
            int simulationCarStart = -(this.Cars.Count + 1);
            foreach (Car car in this.Cars)
            {
                carQueueByIntersection[car.Streets[0].EndIntersection].Add(new CarSimultionPosition(car, simulationCarStart));
                simulationCarStart++;
            }

            // Init green lights
            foreach (SolutionIntersection intersection in solution.Intersections)
            {
                intersection.CurrentGreenLigth = 0;
                if (intersection.GreenLigths.Count > 0)
                    intersection.CurrentGreenLightChangeTime = intersection.GreenLigths[0].Duration;
                else
                    intersection.CurrentGreenLightChangeTime = int.MaxValue;
            }

            while (currentTime <= this.Duration)
            {
                // Update traffic lights cycle
                foreach (SolutionIntersection intersection in solution.Intersections)
                {
                    // Update intersection green light
                    if (intersection.CurrentGreenLightChangeTime <= currentTime)
                    {
                        intersection.CurrentGreenLigth = (intersection.CurrentGreenLigth + 1) % intersection.GreenLigths.Count;
                        intersection.CurrentGreenLightChangeTime = currentTime + intersection.GreenLigths[intersection.CurrentGreenLigth].Duration;
                    }

                    // Update intersection car
                    List<CarSimultionPosition> carQueue = carQueueByIntersection[intersection.ID];
                    bool carPassed = false;
                    for (int i = 0; i < carQueue.Count; i++)
                    {
                        CarSimultionPosition carSimultionPosition = carQueue[i];
                        if (carSimultionPosition.TimeGotHere > currentTime)
                            break;

                        Street street = carSimultionPosition.GetCurrentStreet();
                        if (carPassed)
                            continue;

                        // Not green light - try to swap
                        if (!street.Name.Equals(intersection.GetGreenLightStreet().Name))
                        {                        
                            // Green light already used 
                            if (intersection.GreenLigths[intersection.CurrentGreenLigth].GreenLightUsed)
                                continue;

                            // Skip optimizations on specific cars
                            if (dontOptimizeCars.Contains(carSimultionPosition.Car.UniqueID))
                                continue;

                            // Optimization failed to improve
                            //// Car can't finish on time - don't optmize it
                            ////if (carSimultionPosition.TimeLeftOnDrive + currentTime > this.Duration)
                            ////    continue;

                            // Find required green light
                            int requiredGreenLight = -1;
                            for (int g = 0; g < intersection.GreenLigths.Count; g++)
                                if (street.Name.Equals(intersection.GreenLigths[g].Street.Name))
                                {
                                    requiredGreenLight = g;
                                    break;
                                }

                            // Required green light not found - skip
                            if (requiredGreenLight == -1)
                                continue;

                            // Required green light already used - skip
                            if (intersection.GreenLigths[requiredGreenLight].GreenLightUsed)
                                continue;

                            // Swap possible only if it's the same green light duration
                            if (intersection.GreenLigths[requiredGreenLight].Duration != intersection.GreenLigths[intersection.CurrentGreenLigth].Duration)
                                continue;

                            // Swap green lights - now the car can continue!
                            GreenLightCycle tmp = intersection.GreenLigths[requiredGreenLight];
                            intersection.GreenLigths[requiredGreenLight] = intersection.GreenLigths[intersection.CurrentGreenLigth];
                            intersection.GreenLigths[intersection.CurrentGreenLigth] = tmp;
                        }


                        // Mark intersection as used for this cycle
                        carPassed = true;

                        intersection.GreenLigths[intersection.CurrentGreenLigth].GreenLightUsed = true;

                        // Process car green light
                        carSimultionPosition.StreetNumber++;
                        Street newStreet = carSimultionPosition.GetCurrentStreet();
                        carSimultionPosition.TimeGotHere = currentTime + newStreet.Length;
                        carSimultionPosition.TimeLeftOnDrive -= newStreet.Length;

                        // Check if car finished
                        carQueue.RemoveAt(i);
                        i--;
                        if (carSimultionPosition.StreetNumber == carSimultionPosition.Car.Streets.Count - 1)
                        {
                            // Check if finished on time - if so give bonus
                            if (carSimultionPosition.TimeGotHere <= this.Duration)
                                score += this.BonusPerCar + (this.Duration - carSimultionPosition.TimeGotHere);
                        }
                        else
                        {
                            // Car not finished - add it to the next intersection
                            Utils.AddSorted(
                                carQueueByIntersection[newStreet.EndIntersection],
                                carSimultionPosition,
                                carSimultionPositionByTimeGotHere);
                        }
                    }
                }

                currentTime++;
            }

            return score;
        }

        public int OptimizeGreenLightOrder3(Solution solution, HashSet<int> dontOptimizeCars)
        {
            CarSimultionPositionByTimeGotHere carSimultionPositionByTimeGotHere = new CarSimultionPositionByTimeGotHere();
            CarSimultionPositionByTimeLeft carSimultionPositionByTimeLeft = new CarSimultionPositionByTimeLeft();
            int score = 0;
            int currentTime = 0;

            Dictionary<int, List<CarSimultionPosition>> carQueueByIntersection = new Dictionary<int, List<CarSimultionPosition>>();
            foreach (Intersection intersection in this.Intersections)
                carQueueByIntersection.Add(intersection.ID, new List<CarSimultionPosition>());

            // Create car queue in each intersection
            int simulationCarStart = -(this.Cars.Count + 1);
            foreach (Car car in this.Cars)
            {
                carQueueByIntersection[car.Streets[0].EndIntersection].Add(new CarSimultionPosition(car, simulationCarStart));
                simulationCarStart++;
            }

            // Init green lights
            foreach (SolutionIntersection intersection in solution.Intersections)
            {
                intersection.CurrentGreenLigth = 0;
                if (intersection.GreenLigths.Count > 0)
                    intersection.CurrentGreenLightChangeTime = intersection.GreenLigths[0].Duration;
                else
                    intersection.CurrentGreenLightChangeTime = int.MaxValue;
            }

            while (currentTime <= this.Duration)
            {
                // Update traffic lights cycle
                foreach (SolutionIntersection intersection in solution.Intersections)
                {
                    if (intersection.GreenLigths.Count == 0)
                        continue;

                    // Update intersection green light
                    if (intersection.CurrentGreenLightChangeTime <= currentTime)
                    {
                        intersection.CurrentGreenLigth = (intersection.CurrentGreenLigth + 1) % intersection.GreenLigths.Count;
                        intersection.CurrentGreenLightChangeTime = currentTime + intersection.GreenLigths[intersection.CurrentGreenLigth].Duration;
                    }

                    // Update intersection car
                    List<CarSimultionPosition> carQueue = carQueueByIntersection[intersection.ID];

                    // If it's possible to change the current green light - try selecting the best one
                    if (!intersection.GreenLigths[intersection.CurrentGreenLigth].GreenLightUsed)
                    {
                        Dictionary<int, CarSimultionPosition> firstCarPerStreet = new Dictionary<int, CarSimultionPosition>();
                        for (int i = 0; i < carQueue.Count; i++)
                        {
                            CarSimultionPosition carSimultionPosition = carQueue[i];
                            if (carSimultionPosition.TimeGotHere > currentTime)
                                break;

                            int streetId = carSimultionPosition.GetCurrentStreet().UniqueID;
                            if (!firstCarPerStreet.ContainsKey(streetId))
                                firstCarPerStreet.Add(streetId, carSimultionPosition);
                        }

                        if (firstCarPerStreet.Count == 0)
                            continue;

                        List<CarSimultionPosition> carQueueByTimeLeft = //new List<CarSimultionPosition>(firstCarPerStreet.Values);
                            firstCarPerStreet.Values
                                .OrderBy(o => (o.Car.Streets.Count - o.StreetNumber))
                                .ThenBy(o => -o.TimeLeftOnDrive)
                                .ToList();
                        //carQueueByTimeLeft.Sort(carSimultionPositionByTimeLeft);
                        //carQueueByTimeLeft.Sort(carSimultionPositionByTimeGotHere);

                        foreach (CarSimultionPosition carSimultionPosition in carQueueByTimeLeft)
                        {
                            // Skip optimizations on specific cars
                            if (dontOptimizeCars.Contains(carSimultionPosition.Car.UniqueID))
                                continue;

                            // Find required green light
                            int requiredGreenLight = -1;
                            Street street = carSimultionPosition.GetCurrentStreet();
                            for (int g = 0; g < intersection.GreenLigths.Count; g++)
                                if (street.Name.Equals(intersection.GreenLigths[g].Street.Name))
                                {
                                    requiredGreenLight = g;
                                    break;
                                }

                            // Required green light not found - skip
                            if (requiredGreenLight == -1)
                                continue;

                            // Required green light already used - skip
                            if (intersection.GreenLigths[requiredGreenLight].GreenLightUsed)
                                continue;

                            // Swap possible only if it's the same green light duration
                            if (intersection.GreenLigths[requiredGreenLight].Duration != intersection.GreenLigths[intersection.CurrentGreenLigth].Duration)
                                continue;

                            // Swap green lights - now the car can continue!
                            GreenLightCycle tmp = intersection.GreenLigths[requiredGreenLight];
                            intersection.GreenLigths[requiredGreenLight] = intersection.GreenLigths[intersection.CurrentGreenLigth];
                            intersection.GreenLigths[intersection.CurrentGreenLigth] = tmp;
                            tmp.GreenLightUsed = true;
                            break;
                        }
                    }

                    bool carPassed = false;
                    for (int i = 0; i < carQueue.Count; i++)
                    {
                        CarSimultionPosition carSimultionPosition = carQueue[i];
                        if (carSimultionPosition.TimeGotHere > currentTime)
                            break;

                        Street street = carSimultionPosition.GetCurrentStreet();
                        if (carPassed)
                            continue;

                        // Not green light
                        if (!street.Name.Equals(intersection.GetGreenLightStreet().Name))
                            continue;

                        // Mark intersection as used for this cycle
                        carPassed = true;

                        intersection.GreenLigths[intersection.CurrentGreenLigth].GreenLightUsed = true;

                        // Process car green light
                        carSimultionPosition.StreetNumber++;
                        Street newStreet = carSimultionPosition.GetCurrentStreet();
                        carSimultionPosition.TimeGotHere = currentTime + newStreet.Length;
                        carSimultionPosition.TimeLeftOnDrive -= newStreet.Length;

                        // Check if car finished
                        carQueue.RemoveAt(i);
                        i--;
                        if (carSimultionPosition.StreetNumber == carSimultionPosition.Car.Streets.Count - 1)
                        {
                            // Check if finished on time - if so give bonus
                            if (carSimultionPosition.TimeGotHere <= this.Duration)
                                score += this.BonusPerCar + (this.Duration - carSimultionPosition.TimeGotHere);
                        }
                        else
                        {
                            // Car not finished - add it to the next intersection
                            Utils.AddSorted(
                                carQueueByIntersection[newStreet.EndIntersection],
                                carSimultionPosition,
                                carSimultionPositionByTimeGotHere);
                        }
                    }
                }

                currentTime++;
            }

            return score;
        }

        public int OptimizeGreenLightOrder4(Solution solution, HashSet<int> dontOptimizeCars)
        {
            CarSimultionPositionByTimeGotHere carSimultionPositionByTimeGotHere = new CarSimultionPositionByTimeGotHere();
            CarSimultionPositionByTimeLeft carSimultionPositionByTimeLeft = new CarSimultionPositionByTimeLeft();
            int score = 0;
            int currentTime = 0;

            Dictionary<int, List<CarSimultionPosition>> carQueueByIntersection = new Dictionary<int, List<CarSimultionPosition>>();
            foreach (Intersection intersection in this.Intersections)
                carQueueByIntersection.Add(intersection.ID, new List<CarSimultionPosition>());

            // Create car queue in each intersection
            int simulationCarStart = -(this.Cars.Count + 1);
            foreach (Car car in this.Cars)
            {
                carQueueByIntersection[car.Streets[0].EndIntersection].Add(new CarSimultionPosition(car, simulationCarStart));
                simulationCarStart++;
            }

            // Init green lights
            foreach (SolutionIntersection intersection in solution.Intersections)
            {
                intersection.CurrentGreenLigth = 0;
                if (intersection.GreenLigths.Count > 0)
                    intersection.CurrentGreenLightChangeTime = intersection.GreenLigths[0].Duration;
                else
                    intersection.CurrentGreenLightChangeTime = int.MaxValue;
            }

            while (currentTime <= this.Duration)
            {
                // Update traffic lights cycle
                foreach (SolutionIntersection intersection in solution.Intersections)
                {
                    if (intersection.GreenLigths.Count == 0)
                        continue;

                    // Update intersection green light
                    if (intersection.CurrentGreenLightChangeTime <= currentTime)
                    {
                        intersection.CurrentGreenLigth = (intersection.CurrentGreenLigth + 1) % intersection.GreenLigths.Count;
                        intersection.CurrentGreenLightChangeTime = currentTime + intersection.GreenLigths[intersection.CurrentGreenLigth].Duration;
                    }

                    // Update intersection car
                    List<CarSimultionPosition> carQueue = carQueueByIntersection[intersection.ID];

                    // If it's possible to change the current green light - try selecting the best one
                    if (!intersection.GreenLigths[intersection.CurrentGreenLigth].GreenLightUsed)
                    {
                        Dictionary<int, CarSimultionPosition> firstCarPerStreet = new Dictionary<int, CarSimultionPosition>();
                        for (int i = 0; i < carQueue.Count; i++)
                        {
                            CarSimultionPosition carSimultionPosition = carQueue[i];
                            if (carSimultionPosition.TimeGotHere > currentTime)
                                break;

                            int streetId = carSimultionPosition.GetCurrentStreet().UniqueID;
                            if (!firstCarPerStreet.ContainsKey(streetId))
                                firstCarPerStreet.Add(streetId, carSimultionPosition);
                        }

                        if (firstCarPerStreet.Count == 0)
                            continue;

                        List<CarSimultionPosition> carQueueByTimeLeft = //new List<CarSimultionPosition>(firstCarPerStreet.Values);
                            firstCarPerStreet.Values
                                .OrderBy(o => (o.Car.Streets.Count - o.StreetNumber))
                                .ThenBy(o => o.GetCurrentStreet().IncomingUsageCount)
                                .ToList();
                        //carQueueByTimeLeft.Sort(carSimultionPositionByTimeLeft);
                        //carQueueByTimeLeft.Sort(carSimultionPositionByTimeGotHere);

                        foreach (CarSimultionPosition carSimultionPosition in carQueueByTimeLeft)
                        {
                            // Skip optimizations on specific cars
                            if (dontOptimizeCars.Contains(carSimultionPosition.Car.UniqueID))
                                continue;

                            // Find required green light
                            int requiredGreenLight = -1;
                            Street street = carSimultionPosition.GetCurrentStreet();
                            for (int g = 0; g < intersection.GreenLigths.Count; g++)
                                if (street.Name.Equals(intersection.GreenLigths[g].Street.Name))
                                {
                                    requiredGreenLight = g;
                                    break;
                                }

                            // Required green light not found - skip
                            if (requiredGreenLight == -1)
                                continue;

                            // Required green light already used - skip
                            if (intersection.GreenLigths[requiredGreenLight].GreenLightUsed)
                                continue;

                            // Swap possible only if it's the same green light duration
                            if (intersection.GreenLigths[requiredGreenLight].Duration != intersection.GreenLigths[intersection.CurrentGreenLigth].Duration)
                                continue;

                            // Swap green lights - now the car can continue!
                            GreenLightCycle tmp = intersection.GreenLigths[requiredGreenLight];
                            intersection.GreenLigths[requiredGreenLight] = intersection.GreenLigths[intersection.CurrentGreenLigth];
                            intersection.GreenLigths[intersection.CurrentGreenLigth] = tmp;
                            tmp.GreenLightUsed = true;
                            break;
                        }
                    }

                    bool carPassed = false;
                    for (int i = 0; i < carQueue.Count; i++)
                    {
                        CarSimultionPosition carSimultionPosition = carQueue[i];
                        if (carSimultionPosition.TimeGotHere > currentTime)
                            break;

                        Street street = carSimultionPosition.GetCurrentStreet();
                        if (carPassed)
                            continue;

                        // Not green light
                        if (!street.Name.Equals(intersection.GetGreenLightStreet().Name))
                            continue;

                        // Mark intersection as used for this cycle
                        carPassed = true;

                        intersection.GreenLigths[intersection.CurrentGreenLigth].GreenLightUsed = true;

                        // Process car green light
                        carSimultionPosition.StreetNumber++;
                        Street newStreet = carSimultionPosition.GetCurrentStreet();
                        carSimultionPosition.TimeGotHere = currentTime + newStreet.Length;
                        carSimultionPosition.TimeLeftOnDrive -= newStreet.Length;

                        // Check if car finished
                        carQueue.RemoveAt(i);
                        i--;
                        if (carSimultionPosition.StreetNumber == carSimultionPosition.Car.Streets.Count - 1)
                        {
                            // Check if finished on time - if so give bonus
                            if (carSimultionPosition.TimeGotHere <= this.Duration)
                                score += this.BonusPerCar + (this.Duration - carSimultionPosition.TimeGotHere);
                        }
                        else
                        {
                            // Car not finished - add it to the next intersection
                            Utils.AddSorted(
                                carQueueByIntersection[newStreet.EndIntersection],
                                carSimultionPosition,
                                carSimultionPositionByTimeGotHere);
                        }
                    }
                }

                currentTime++;
            }

            return score;
        }

        /// <summary>
        /// Run simulation with a single car queue
        /// </summary>
        /// <param name="solution"></param>
        /// <returns></returns>
        public SimulationResult RunSimulation(Solution solution)
        {
            CarSimultionPositionByTimeGotHere timeGotHereCompare = new CarSimultionPositionByTimeGotHere();

            SimulationResult simulationResult = new SimulationResult(this.Intersections.Count);
            int currentTime = 0;

            // Create list of cars in simulation
            List<CarSimultionPosition> carSimultionPositions = new List<CarSimultionPosition>();
            int simulationCarStart = -(this.Cars.Count + 1);
            foreach (Car car in this.Cars)
            {
                carSimultionPositions.Add(new CarSimultionPosition(car, simulationCarStart));
                simulationCarStart++;
            }

            // Init green lights
            foreach (SolutionIntersection intersection in solution.Intersections)
                intersection.BuildGreenLightsArray();

            while (currentTime <= this.Duration)
            {
                // Update cars
                HashSet<int> usedIntersection = new HashSet<int>();
                for (int i = 0; i < carSimultionPositions.Count; i++)
                {
                    CarSimultionPosition carSimultionPosition = carSimultionPositions[i];
                    if (carSimultionPosition.TimeGotHere > currentTime)
                        break;

                    Street street = carSimultionPosition.Car.Streets[carSimultionPosition.StreetNumber];

                    // Check if a car already used this intersection this cycle
                    if (usedIntersection.Contains(street.EndIntersection))
                    {
                        simulationResult.IntersectionResults[street.EndIntersection].AddBlockedTraffic(street.Name);
                        continue;
                    }

                    SolutionIntersection intersection = solution.Intersections[street.EndIntersection];
                    if (intersection.GreenLigths.Count == 0)
                        continue;

                    GreenLightCycle currentGreenLight = intersection.GreenLightsArray[currentTime % intersection.GreenLightsArray.Length];

                    // Not green light, skip to next car
                    if (!street.UniqueID.Equals(currentGreenLight.Street.UniqueID))
                    {
                        simulationResult.IntersectionResults[street.EndIntersection].AddBlockedTraffic(street.Name);
                        continue;
                    }

                    // Mark intersection as used for this cycle
                    usedIntersection.Add(street.EndIntersection);

                    // Process car green light
                    carSimultionPosition.StreetNumber++;
                    Street newStreet = carSimultionPosition.GetCurrentStreet();
                    carSimultionPosition.TimeGotHere = currentTime + newStreet.Length;
                    carSimultionPosition.TimeLeftOnDrive -= newStreet.Length;

                    // Check if car finished
                    carSimultionPositions.RemoveAt(i);
                    i--;

                    if (carSimultionPosition.StreetNumber == carSimultionPosition.Car.Streets.Count - 1)
                    {
                        // Check if finished on time - if so give bonus
                        if (carSimultionPosition.TimeGotHere <= this.Duration)
                            simulationResult.Score += this.BonusPerCar + (this.Duration - carSimultionPosition.TimeGotHere);
                    }
                    else
                    {
                        Utils.AddSorted(carSimultionPositions, carSimultionPosition, timeGotHereCompare);
                    }
                }

                // Sort cars by time
                //carSimultionPositions.Sort((x, y) => x.TimeGotHere.CompareTo(y.TimeGotHere));

                currentTime++;
            }

            simulationResult.CarsNotFinished.AddRange(carSimultionPositions);

            return simulationResult;
        }

        /// <summary>
        /// Run simulation where there's a car queue for each intersection
        /// </summary>
        /// <param name="solution"></param>
        /// <returns></returns>
        public SimulationResult RunSimulation2(Solution solution)
        {
            CarSimultionPositionByTimeGotHere carSimultionPositionByTimeGotHere = new CarSimultionPositionByTimeGotHere();
            SimulationResult simulationResult = new SimulationResult(this.Intersections.Count);
            int currentTime = 0;

            Dictionary<int, List<CarSimultionPosition>> carQueueByIntersection = new Dictionary<int, List<CarSimultionPosition>>();
            foreach (Intersection intersection in this.Intersections)
                carQueueByIntersection.Add(intersection.ID, new List<CarSimultionPosition>());

            // Create car queue in each intersection
            int simulationCarStart = -(this.Cars.Count + 1);
            foreach (Car car in this.Cars)
            {
                carQueueByIntersection[car.Streets[0].EndIntersection].Add(new CarSimultionPosition(car, simulationCarStart));
                simulationCarStart++;
            }

            // Init green lights
            foreach (SolutionIntersection intersection in solution.Intersections)
                intersection.BuildGreenLightsArray();

            while (currentTime <= this.Duration)
            {
                // Update traffic lights cycle
                foreach (SolutionIntersection intersection in solution.Intersections)
                {
                    if (intersection.GreenLigths.Count == 0)
                        continue;

                    // Update intersection car
                    List<CarSimultionPosition> carQueue = carQueueByIntersection[intersection.ID];
                    bool carPassed = false;
                    for (int i = 0; i < carQueue.Count; i++)
                    {
                        CarSimultionPosition carSimultionPosition = carQueue[i];
                        if (carSimultionPosition.TimeGotHere > currentTime)
                            break;

                        Street street = carSimultionPosition.GetCurrentStreet();
                        if (carPassed)
                        {
                            simulationResult.IntersectionResults[street.EndIntersection].AddBlockedTraffic(street.Name);
                            continue;
                        }

                        GreenLightCycle currentGreenLight = intersection.GreenLightsArray[currentTime % intersection.GreenLightsArray.Length];

                        // Not green light, skip to next car
                        if (!street.UniqueID.Equals(currentGreenLight.Street.UniqueID))
                        {
                            simulationResult.IntersectionResults[street.EndIntersection].AddBlockedTraffic(street.Name);
                            continue;
                        }

                        // Mark intersection as used for this cycle
                        carPassed = true;

                        // Process car green light
                        carSimultionPosition.StreetNumber++;
                        Street newStreet = carSimultionPosition.GetCurrentStreet();
                        carSimultionPosition.TimeGotHere = currentTime + newStreet.Length;
                        carSimultionPosition.TimeLeftOnDrive -= newStreet.Length;

                        // Check if car finished
                        carQueue.RemoveAt(i);
                        i--;
                        if (carSimultionPosition.StreetNumber == carSimultionPosition.Car.Streets.Count - 1)
                        {
                            // Check if finished on time - if so give bonus
                            if (carSimultionPosition.TimeGotHere <= this.Duration)
                                simulationResult.Score += this.BonusPerCar + (this.Duration - carSimultionPosition.TimeGotHere);
                        } 
                        else
                        {
                            // Car not finished - add it to the next intersection
                            Utils.AddSorted(
                                carQueueByIntersection[newStreet.EndIntersection],
                                carSimultionPosition,
                                carSimultionPositionByTimeGotHere);
                        }
                    }
                }

                currentTime++;
            }

            foreach (List<CarSimultionPosition> cars in carQueueByIntersection.Values)
                simulationResult.CarsNotFinished.AddRange(cars);

            return simulationResult;
        }

        /// <summary>
        /// Run simulation where there's a car queue for each street
        /// </summary>
        /// <param name="solution"></param>
        /// <returns></returns>
        public SimulationResult RunSimulation3(Solution solution)
        {
            CarSimultionPositionByTimeGotHere carSimultionPositionByTimeGotHere = new CarSimultionPositionByTimeGotHere();
            SimulationResult simulationResult = new SimulationResult(this.Intersections.Count);
            int currentTime = 0;

            int maxStreetNumber = 0;
            foreach (Street street in this.Streets.Values)
                maxStreetNumber = Math.Max(maxStreetNumber, street.UniqueID);

            List<CarSimultionPosition>[] carQueueByStreet = new List<CarSimultionPosition>[maxStreetNumber + 1];
            for (int i = 0; i < carQueueByStreet.Length; i++)
                carQueueByStreet[i] = new List<CarSimultionPosition>();

            // Add cars to queues in each intersection and street
            int simulationCarStart = -(this.Cars.Count + 1);
            foreach (Car car in this.Cars)
            {
                Street carStreet = car.Streets[0];
                carQueueByStreet[carStreet.UniqueID].Add(new CarSimultionPosition(car, simulationCarStart));
                simulationCarStart++;
            }

            // Init green lights
            foreach (SolutionIntersection intersection in solution.Intersections)
                intersection.BuildGreenLightsArray();

            while (currentTime <= this.Duration)
            {
                // Update traffic lights cycle
                foreach (SolutionIntersection intersection in solution.Intersections)
                {
                    // It's a 
                    if (intersection.GreenLigths.Count == 0)
                        continue;

                    GreenLightCycle currentGreenLight = intersection.GreenLightsArray[currentTime % intersection.GreenLightsArray.Length];

                    // Update intersection car
                    List<CarSimultionPosition> carQueue = carQueueByStreet[currentGreenLight.Street.UniqueID];
                    bool carPassed = false;
                    int waitOnGreenLight = 0;
                    for (int i = 0; i < carQueue.Count; i++)
                    {
                        CarSimultionPosition carSimultionPosition = carQueue[i];
                        if (carSimultionPosition.TimeGotHere > currentTime)
                            break;

                        Street street = carSimultionPosition.GetCurrentStreet();
                        // Keep waiting, although there's green light on this street
                        if (carPassed)
                        {
                            waitOnGreenLight++;
                            simulationResult.IntersectionResults[street.EndIntersection].AddBlockedTraffic(street.Name);
                            continue;
                        }

                        // Mark intersection as used for this cycle
                        carPassed = true;

                        // Process car green light
                        carSimultionPosition.StreetNumber++;
                        Street newStreet = carSimultionPosition.GetCurrentStreet();
                        carSimultionPosition.TimeGotHere = currentTime + newStreet.Length;
                        carSimultionPosition.TimeLeftOnDrive -= newStreet.Length;

                        // Check if car finished
                        carQueue.RemoveAt(i);
                        i--;
                        if (carSimultionPosition.StreetNumber == carSimultionPosition.Car.Streets.Count - 1)
                        {
                            // Check if finished on time - if so give bonus
                            if (carSimultionPosition.TimeGotHere <= this.Duration)
                                simulationResult.Score += this.BonusPerCar + (this.Duration - carSimultionPosition.TimeGotHere);
                        }
                        else
                        {
                            // Car not finished - add it to the next intersection
                            Utils.AddSorted(
                                carQueueByStreet[newStreet.UniqueID],
                                carSimultionPosition,
                                carSimultionPositionByTimeGotHere);
                        }
                    }
                    simulationResult.IntersectionResults[intersection.GetGreenLightStreet().EndIntersection]
                        .UpdateMaxWaitOnGreenLight(intersection.GetGreenLightStreet().UniqueID, waitOnGreenLight);
                }

                currentTime++;
            }

            foreach (List<CarSimultionPosition> cars in carQueueByStreet)
                simulationResult.CarsNotFinished.AddRange(cars);

            return simulationResult;
        }

        public int CalculateScoreUpperBound()
        {
            int upperBoundScore = 0;
            foreach (Car car in this.Cars)
            {
                int maxBonusTime = this.Duration - car.TimeNeedToDrive();
                // Car can not finish in time
                if (maxBonusTime < 0)
                    continue;

                upperBoundScore += maxBonusTime + this.BonusPerCar;
            }

            return upperBoundScore;
        }

        public int RemoveUnusedStreets()
        {
            int removeStreets = 0;

            foreach (Intersection intersection in Intersections)
            {
                for (int i = intersection.IncomingStreets.Count - 1; i >= 0; i--)
                {
                    Street inStreet = intersection.IncomingStreets[i];
                    if (inStreet.IncomingUsageCount > 0)
                        continue;

                    removeStreets++;
                    intersection.IncomingStreets.RemoveAt(i);
                }
            }

            return removeStreets;
        }

        public static Problem LoadProblem(string fileName)
        {
            int d, s, i, v, f;
            Dictionary<string, Street> streets = new Dictionary<string, Street>();
            List<Car> cars = new List<Car>();

            using (StreamReader sr = new StreamReader(fileName))
            {
                string line = sr.ReadLine();
                string[] parts = line.Split(' ');
                d = int.Parse(parts[0]);
                i = int.Parse(parts[1]);
                s = int.Parse(parts[2]);
                v = int.Parse(parts[3]);
                f = int.Parse(parts[4]);

                for (int street = 0; street < s; street++)
                {
                    line = sr.ReadLine();
                    parts = line.Split(' ');
                    streets.Add(parts[2], new Street(street, int.Parse(parts[0]), int.Parse(parts[1]), parts[2], int.Parse(parts[3])));
                }

                for (int car = 0; car < v; car++)
                {
                    line = sr.ReadLine();
                    parts = line.Split(' ');
                    int p = int.Parse(parts[0]);
                    List<Street> carStreets = new List<Street>();
                    for (int carStreet = 0; carStreet < p; carStreet++)
                        carStreets.Add(streets[parts[1 + carStreet]]);

                    cars.Add(new Car(car, carStreets));
                }

                return new Problem(d, i, f, streets, cars);
            }
        }
    }
}
