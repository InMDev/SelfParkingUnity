# Self Parallel-Parking in Unity
Using Genetic Evolutionary Algorithm for self parallel parking in Unity from scratch

# The idea
1. Create movement to the car: engine & steering wheel
2. Give sensor that calculate distance near the car
3. Give Brain to the car that takes in the sensor values and outputs the engine & steering wheel strength
4. Run the Genetic Algorithm (GA) for training

# Genetic Algorithm (GA)
1. Initialization: Create diverse sets of genes representing 9 initial weights for engine and steering (using a 10-bit structure: 1 sign bit, 4 exponent bits, and 5 mantissa bits).
2. Fitness Evaluation: Define the fitness function based on how accurately the car approaches the target parking spot. This function measures the proximity to the desired location.
3. Selection: Assess the fitness of each individual car and select potential parents for mating based on their performance.
4. Crossover (Mating): Combine genomes of selected parent cars in a 50/50 manner, allowing for the exchange of genetic material. This aims to produce improved offspring by inheriting the best features from their parents.
5. Mutation: Introduce random mutations during mating, randomly flipping bits in the child genomes. This introduces variability, potentially leading to novel car behaviors. However, excessive mutations might negatively impact the healthy genomes.

Iteration: Repeat steps 2 to 5 until a stopping condition is met, such as reaching a certain number of generations or achieving the desired proximity to the parking spot (e.g., within 1 meter). If the condition is met, stop; otherwise, continue iterating to refine the car's self-parking abilities.

