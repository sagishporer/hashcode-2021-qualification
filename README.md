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
9. Hill Climb - repeat until no improvement
   * For each intersection, for each pair of green lights - try swapping - if it improves the score - save.
   * For each intersection, for each green light - try moving to any other place in the green lights order - if it improves the score - save.
   * For each intersection, for green light - try increase/reduce duration by 1-10 - if it improves the score - save.


Some improvement is possible by small tweaks to the parameters of step (5). For simplicity & a single solution for all inputs there's only one variant in this source.

## Score

| Input | Score - Main Branch | + Optimize Params | + Hill Climb | Max Known Score | Upper Bound |
| --- | --- | --- | --- | --- | --- |
| A – An example | 2,002 | 2,002 | 2,002 | <=== | 2,002 |
| B – By the ocean | 4,568,819 | 4,568,869 | 4,570,281 | 4,570,431 | 4,576,202 |
| C – Checkmate | 1,306,213 | 1,306,574 | 1,314,913 | 1,315,372 | 1,328,389 |
| D – Daily commute | 2,483,375 | 2,493,412 | 2,610,027 | <=== | 3,986,591 |
| E – Etoile | 731,726 | 733,936 | 762,253 | 779,279 | 921,203 |
| F – Forever jammed | 1,460,486 | 1,471,203 | 1,480,489 | <=== | 1,765,068 |
| Total | 10,552,621 | 10,575,996 | 10,739,965 | N/A | N/A |

Notes:
* Score - The score of the "main" branch code, with the current commited parameters.
* Optimize Params - The score in the input specific branches with the parameters found to works best for the specific input.
* Hill Climb - The score after optimized parameters and hill climb. The hill climb did not finish on D. Ended at the end of the extended round.
* Max Known Score - The highest score by input published by anyone ([Source - codeforces.com](https://codeforces.com/blog/entry/88188?#comment-768121)).
* Upper Bound - Hypothetical upper bound for the score. Calculated by: Sum for all cars[Duration - Car path length + Car bonus]. 
