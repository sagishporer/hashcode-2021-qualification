# Google Hash Code 2021 qualification round

Google Hash Code 2021 qualification round: Traffic Signalling. 

## Algorithm description
1. Load data.
2. Remove unused streets.
3. Build basic green light cycle for each intersection - 1 second duration for each incoming street.
4. Run a simulation - if there's a blocked car in the junction - try to swap the current green light with green light for the street of the blocked car.
5. Run a simulation - count the number of car waiting in each junction per street. For the intersections with the top 2% of blocked cars - increase the cycle duration of the most blocked street by 1.
6. While there was a recent improvement, go to 5.

Some improvement is possible by small tweaks to the parameters of step (5). For simplicity & a single solution for all inputs there's only one variant in this source.

## Score

| Input | Score | Max Known Score | Upper Bound |
| --- | --- | --- | --- |
| A – An example | 2,002 | 2,002 | 2,002 |
| B – By the ocean | 4,568,819 | 4,569,036 | 4,576,202 |
| C – Checkmate | 1,306,205 | 1,310,645 | 1,328,389 |
| D – Daily commute | 2,487,443 | 2,498,918 | 3,986,591 |
| E – Etoile | 731,002 | 748,779 | 921,203 |
| F – Forever jammed | 1,422,042 |  1,471,554 | 1,765,068 |
| Total | 10,513,445 | 10,600,934 | N/A |

Notes:
* Score - The score of this code, with the current commited parameters.
* Max Known Score - The highest score by input published by anyone. Highest score on the extended round was 10,621,062.
* Upper Bound - Hypothetical upper bound for the score. Calculated by: Sum for all cars[Duration - Car path length + Car bonus]. 
