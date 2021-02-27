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

* A – An example: 2,002
* B – By the ocean: 4,568,819
* C – Checkmate: 1,306,205
* D – Daily commute: 2,483,375
* E – Etoile: 731,002
* F – Forever jammed: 1,422,042

Score: 10,513,445
