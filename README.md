# Google Hash Code 2021 qualification round

Google Hash Code 2021 qualification round: Traffic Signalling. 

## Algorithm description
1. Load data.
2. Remove unused streets.
3. Build basic green light cycle for each intersection - 1 second duration for each incoming street.
4. Run a simulation - if there's a blocked car in the junction - try to swap the current green light with green light for the street of the blocked car.
5. Run a simulation - count the number of car waiting in each junction per street. For the intersections with the top 2% of blocked cars - increase the cycle duration of the most blocked street by 1.
6. While there was a recent improvement, go to 5.
7. For all the cars that didn't complete the drive on time - scan the streets - if only cars that didn't finish passing in that street - remove the street from the intersection green lights.
8. Scan all the cars that didn't finish and try to put them back. Restore the green lights from (7) when trying. Select the car closest to finish (by time).

Some improvement is possible by small tweaks to the parameters of step (5). For simplicity & a single solution for all inputs there's only one variant in this source.

## Score

| Input | Score | Max Known Score | Upper Bound |
| --- | --- | --- | --- |
| A – An example | 2,002 | 2,002 | 2,002 |
| B – By the ocean | 4,568,819 | 4,570,431 | 4,576,202 |
| C – Checkmate | 1,306,213 | 1,315,077 | 1,328,389 |
| D – Daily commute | 2,483,375 | 2,565,531 | 3,986,591 |
| E – Etoile | 731,726 | 768,012 | 921,203 |
| F – Forever jammed | 1,460,486 |  1,472,822 | 1,765,068 |
| Total | 10,552,621 | 10,693,875 | N/A |

Notes:
* Score - The score of this code, with the current commited parameters.
* Max Known Score - The highest score by input published by anyone. Highest score on the extended round was 10,693,875 (team UVIGO Bruteforcers).
* Upper Bound - Hypothetical upper bound for the score. Calculated by: Sum for all cars[Duration - Car path length + Car bonus]. 
