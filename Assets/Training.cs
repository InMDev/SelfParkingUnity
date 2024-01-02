using UnityEngine;
using System.Collections.Generic;

public class Training : MonoBehaviour
{
    public const int GENERATION_SIZE = 10;
    public const float LONG_LIVING_CHAMPIONS_PERCENTAGE = 0.06f;
    public const float MUTATION_PROBABILITY = 0.04f;
    public const int MAX_GENERATIONS_NUM = 50;
    public const int GENOME_LENGTH = 180; // Assuming genome length is 180
    public GameObject carPrefab;

    private List<Agent> generation = new List<Agent>();

    void Start()
    {
        // Assuming Generation is a list of Genome
        CreateGeneration(GENERATION_SIZE);

        for (int generationIndex = 0; generationIndex < MAX_GENERATIONS_NUM; generationIndex++)
        {

        }

    }

    private void CreateGeneration(int generationSize)
    {
        //For each generationSize instantiate a car and add it to the generation list
        for (int i = 0; i < generationSize; i++)
        {
            GameObject car = Instantiate(carPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            Agent agent = car.GetComponent<Agent>();
            agent.RandomInitialize();
            generation.Add(agent);
        }
    }


    private void Mate(int[] fatherGenome, int[] motherGenome)
    {
        GameObject child1 = Instantiate(carPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        GameObject child2 = Instantiate(carPrefab, new Vector3(0, 0, 0), Quaternion.identity);

        int[] genomeChild1 = new int[GENOME_LENGTH];
        int[] genomeChild2 = new int[GENOME_LENGTH];

        for (int i = 0; i < fatherGenome.Length; i++)
        {
            //Random 50% chance to take the gene from the father or the mother
            genomeChild1[i] = Random.Range(0, 2) == 0 ? fatherGenome[i] : motherGenome[i];
            genomeChild2[i] = Random.Range(0, 2) == 0 ? fatherGenome[i] : motherGenome[i];
        }
        
        Agent child1Agent = child1.GetComponent<Agent>();
        Agent child2Agent = child2.GetComponent<Agent>();

        child1Agent.setGenome(genomeChild1);
        child2Agent.setGenome(genomeChild2);

        //Mutate
        child1Agent.Mutate(MUTATION_PROBABILITY);
        child2Agent.Mutate(MUTATION_PROBABILITY);

        generation.Add(child1Agent);
        generation.Add(child2Agent);
    }

}