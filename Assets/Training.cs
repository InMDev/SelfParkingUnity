using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Text;

public class Training : MonoBehaviour
{
    public int GENERATION_SIZE;
    public float LONG_LIVING_CHAMPIONS_PERCENTAGE;
    public float MUTATION_PROBABILITY;
    public int MAX_GENERATIONS_NUM;
    private int GENOME_LENGTH = 180; // Assuming genome length is 180
    public float LIFE_TIME;

    public GameObject carPrefab;

    private List<GameObject> cars = new List<GameObject>();

    private List<Agent> generation = new List<Agent>();

    private List<float> fitnessResults = new List<float>();

    void Start()
    {
        // Assuming Generation is a list of Genome
        CreateGeneration(GENERATION_SIZE);

        StartCoroutine(RunGenerations());
    }

    /// <summary>
    /// The function "RunGenerations" is a private IEnumerator that runs a series of generations.
    /// </summary>
    private IEnumerator RunGenerations()
    {
        for (int generationIndex = 0; generationIndex < MAX_GENERATIONS_NUM; generationIndex++)
        {
            if (generationIndex > 0)
            {
                // Reset all cars from the previous generation
                ResetAllCars();
                yield return new WaitForSeconds(1f);
            }

            GenerateCars();
            yield return new WaitForSeconds(LIFE_TIME);

            StartCoroutine(NaturalSelection());
            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(1f);
        // Save the best Genome by Debug log and concatenate the generation[generation.Count - 1].currentGenome as a string
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < generation[generation.Count - 1].currentGenome.Length; i++)
        {
            sb.Append(generation[generation.Count - 1].currentGenome[i]);
        }
        Debug.Log("Best:" + sb.ToString());
    }

    void Update()
    {
        //If press space bar, print the best fitness result and the genetic
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Generation: " + generation.Count);
            Debug.Log("Fitness: " + generation[generation.Count - 1].fitnessFunction());
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < generation[generation.Count - 1].currentGenome.Length; i++)
            {
                sb.Append(generation[generation.Count - 1].currentGenome[i]);
            }
            Debug.Log("Current" + sb.ToString());
        }
    }

    /// <summary>
    /// The function "ResetAllCars" is a private method that resets all cars.
    /// </summary>
    private void ResetAllCars()
    {
        Debug.Log("reset Car");
        for (int i = 0; i < cars.Count; i++)
        {
            cars[i].SetActive(false);
            //Reset car position to 0 0 0 and rotation to 0 0 0
            cars[i].transform.position = new Vector3(0, 0, 0);
            cars[i].transform.rotation = Quaternion.identity;
            //Reset car velocity to 0
            cars[i].GetComponent<Rigidbody>().velocity = Vector3.zero;
            //Reset car angular velocity to 0
            cars[i].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }
    }

    /// <summary>
    /// The function "GenerateCars" is used to generate cars.
    /// </summary>
    private void GenerateCars()
    {
        Debug.Log("Generate Car");
        Debug.Log("Generation Count: " + generation.Count);
        //Check if generation > number of cars then remove generation to match number of cars
        if (generation.Count > cars.Count)
        {
            generation.RemoveRange(cars.Count, generation.Count);
        }

        for (int i = 0; i < generation.Count; i++)
        {
            //Set the genome of the car to the genome of the agent
            cars[i].GetComponent<Agent>().setGenome(generation[i].currentGenome);
            //Set the car active
            cars[i].SetActive(true);
        }
    }

    /// <summary>
    /// The function "CreateGeneration" creates a new generation of a specified size.
    /// </summary>
    /// <param name="generationSize">The generationSize parameter represents the number of individuals
    /// in a generation.</param>
    private void CreateGeneration(int generationSize)
    {
        //For each generationSize instantiate a car and add it to the generation list
        for (int i = 0; i < generationSize; i++)
        {
            GameObject car = Instantiate(carPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            Agent agent = car.GetComponent<Agent>();
            agent.RandomInitialize();
            generation.Add(agent);
            cars.Add(car);
        }
    }


    /// <summary>
    /// The function "Mate" takes in two Agent objects, representing the genomes of the father and
    /// mother, and performs some operation.
    /// </summary>
    /// <param name="Agent">The "Agent" is a class or data structure that represents an individual in a
    /// genetic algorithm. It typically contains the genetic information or genome of the individual, as
    /// well as any other relevant attributes or methods. In this case, the "fatherGenome" and
    /// "motherGenome" parameters are instances</param>
    /// <param name="Agent">The "Agent" is a class or data structure that represents an individual in a
    /// genetic algorithm. It typically contains the genetic information or genome of the individual, as
    /// well as any other relevant attributes or methods. In this case, the "fatherGenome" and
    /// "motherGenome" parameters are instances</param>
    private void Mate(Agent fatherGenome, Agent motherGenome)
    {
        //Set childagent1 and childagent2 to the same genome as the father and mother
        Agent child1Agent = fatherGenome;
        Agent child2Agent = motherGenome;

        //Create new genome for child1 and child2
        int[] genomeChild1 = new int[GENOME_LENGTH];
        int[] genomeChild2 = new int[GENOME_LENGTH];

        for (int i = 0; i < GENOME_LENGTH; i++)
        {
            //Random 50% chance to take the gene from the father or the mother
            /* The code is randomly selecting genes from either the father's or the mother's genome to
            create the genomes for the two child agents. */
            genomeChild1[i] = Random.Range(0, 2) == 0 ? fatherGenome.currentGenome[i] : motherGenome.currentGenome[i];
            genomeChild2[i] = Random.Range(0, 2) == 0 ? fatherGenome.currentGenome[i] : motherGenome.currentGenome[i];
        }

        //Set the genome of the childagent1 and childagent2 to the new genome
        child1Agent.setGenome(genomeChild1);
        child2Agent.setGenome(genomeChild2);

        //Mutate
        child1Agent.Mutate(MUTATION_PROBABILITY);
        child2Agent.Mutate(MUTATION_PROBABILITY);

        generation.Add(child1Agent);
        generation.Add(child2Agent);
    }

    /// <summary>
    /// The function "NaturalSelection" is an IEnumerator that represents a process of natural
    /// selection.
    /// </summary>
    private IEnumerator NaturalSelection()
    {
        Debug.Log("Selection Car");
        //For each agent in the generation list, calculate the fitness
        for (int i = 0; i < generation.Count; i++)
        {
            fitnessResults.Add(generation[i].fitnessFunction());
        }

        //Sort the generation list by FITNESSVALUE which is a public float in the Agent class
        generation.Sort((x, y) => y.FITNESSVALUE.CompareTo(x.FITNESSVALUE));

        //Remove the worst performing agents from the generation list
        int numToKeep = (int)(GENERATION_SIZE * LONG_LIVING_CHAMPIONS_PERCENTAGE);
        generation.RemoveRange(numToKeep, generation.Count - numToKeep);

        int currGenerationSize = generation.Count;

        ////Mate the best performing agents
        for (int i = 0; i < currGenerationSize - 1; i++)
        {
            Mate(generation[i], generation[i + 1]);
            yield return null; // Wait for the next frame
        }
    }
}