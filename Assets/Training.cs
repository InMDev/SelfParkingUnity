using UnityEngine;
using System.Collections.Generic;

public class Training : MonoBehaviour
{
    public const int GENERATION_SIZE = 10;
    public const float LONG_LIVING_CHAMPIONS_PERCENTAGE = 0.06f;
    public const float MUTATION_PROBABILITY = 0.04f;
    public const int MAX_GENERATIONS_NUM = 2;
    public const int GENOME_LENGTH = 180; // Assuming genome length is 180
    public GameObject carPrefab;

    private List<Agent> generation = new List<Agent>();

    void Start()
    {
        int generationIndex = 0;

        // Assuming Generation is a list of Genome
        CreateGeneration(MAX_GENERATIONS_NUM);
    }

    public void CreateGeneration(int generationSize)
    {
        //For each generationSize instantiate a car and add it to the generation list
        for (int i = 0; i < generationSize; i++)
        {
            GameObject car = Instantiate(carPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            Agent agent = car.GetComponent<Agent>();
            generation.Add(agent);
        }
    }
}