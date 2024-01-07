using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class Agent : MonoBehaviour
{

    public PrometeoCarController carController;

    public Sensor sensorFront;
    public Sensor sensorBack;
    public Sensor sensorLeftmid;
    public Sensor sensorRightmid;
    public Sensor sensorLeftfront;
    public Sensor sensorRightfront;
    public Sensor sensorLeftback;
    public Sensor sensorRightback;

    public int[] currentGenome = new int[180];
    public float[] sensorValues = new float[8];
    public float[] engineWeights = new float[9];
    public float[] steeringWeights = new float[9];

    public Transform wheelFrontLeft;
    public Transform wheelFrontRight;
    public Transform wheelBackLeft;
    public Transform wheelBackRight;

    private Transform wheelFrontLeftTarget;
    private Transform wheelFrontRightTarget;
    private Transform wheelBackLeftTarget;
    private Transform wheelBackRightTarget;

    public float FITNESSVALUE;
    public enum MuscleSignal
    {
        MinusOne = -1,
        Zero = 0,
        PlusOne = 1
    }

    // Example muscles that will receive signals from the brain
    public MuscleSignal engineMuscleSignal;
    public MuscleSignal steeringWheelMuscleSignal;

    /// <summary>
    /// The function "ReceiveBrainSignal" is used to receive brain signals.
    /// </summary>
    private void ReceiveBrainSignal()
    {

        sensorValues[0] = sensorFront.GetDistance();
        sensorValues[1] = sensorBack.GetDistance();
        sensorValues[2] = sensorLeftmid.GetDistance();
        sensorValues[3] = sensorRightmid.GetDistance();
        sensorValues[4] = sensorLeftfront.GetDistance();
        sensorValues[5] = sensorRightfront.GetDistance();
        sensorValues[6] = sensorLeftback.GetDistance();
        sensorValues[7] = sensorRightback.GetDistance();

        // Process received signals
        BrainFunction(sensorValues);
    }
    /// <summary>
    /// The function "ProcessMuscleSignals" is used to process muscle signals.
    /// </summary>
    private void ProcessMuscleSignals()
    {
        // Engine muscle action based on brain signal
        switch (engineMuscleSignal)
        {
            case MuscleSignal.MinusOne:
                carController.GoReverse();
                break;
            case MuscleSignal.Zero:
                carController.ThrottleOff();
                break;
            case MuscleSignal.PlusOne:
                carController.GoForward();
                break;
            default:
                break;
        }

        // Steering wheel muscle action based on brain signal
        switch (steeringWheelMuscleSignal)
        {
            case MuscleSignal.MinusOne:
                carController.TurnLeft();
                break;
            case MuscleSignal.Zero:
                carController.ResetSteeringAngle();
                break;
            case MuscleSignal.PlusOne:
                carController.TurnRight();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// This function takes in an array of float values representing sensor data and performs some brain
    /// function.
    /// </summary>
    /// <param name="sensors">An array of floating-point numbers representing the sensor
    /// readings.</param>
    void BrainFunction(float[] sensors)
    {
        //Output ProcessMuscleSignals() with different values for engineMuscleSignal and steeringWheelMuscleSignal
        engineMuscleSignal = sign(sigmoidFunction(linearFunction(engineWeights, sensors)));
        steeringWheelMuscleSignal = sign(sigmoidFunction(linearFunction(steeringWeights, sensors)));

        ProcessMuscleSignals();
    }

    /// <summary>
    /// The linearFunction calculates the output value based on the coefficients and sensor values
    /// provided.
    /// </summary>
    /// <param name="coeff">An array of coefficients for a linear function. The coefficients represent
    /// the slope and y-intercept of the linear function in the form y = mx + b, where m is the slope
    /// and b is the y-intercept.</param>
    /// <param name="sensors">An array of float values representing the sensor readings.</param>
    float linearFunction(float[] coeff, float[] sensors)
    {
        float result = 0;
        for (int i = 0; i < sensors.Length; i++)
        {
            result += coeff[i] * sensors[i];
        }
        result += coeff[8];
        return result;
    }

    /// <summary>
    /// The sigmoidFunction is a mathematical function that maps any real number to a value between 0
    /// and 1.
    /// </summary>
    /// <param name="x">The input value for which the sigmoid function will be calculated.</param>
    float sigmoidFunction(float x)
    {
        return 1 / (1 + Mathf.Exp(-x));
    }
    
    /// <summary>
    /// The function "sign" takes a float parameter "x" and returns the muscle signal.
    /// </summary>
    /// <param name="x">A float value representing the input signal for the muscle.</param>
    MuscleSignal sign(float x)
    {
        float margin = 0f;

        if (x < (0.5 - margin))
        {
            return MuscleSignal.MinusOne;
        }
        else if (x > (0.5 + margin))
        {
            return MuscleSignal.PlusOne;
        }
        else
        {
            return MuscleSignal.Zero;
        }
    }

    /// <summary>
    /// The function converts an array of integers representing a genome into an array of floating-point
    /// numbers.
    /// </summary>
    /// <param name="genome">An array of integers representing a genome.</param>
    float[] convertGenomeToFloatingPoint(int[] genome)
    {
        float[] floatingPointGenome = new float[genome.Length / 10];

        //Each float value is represented by signed 10 total bit, 1 sign bit, 4 exponent bits, 5 mantissa/fraction bits
        for (int i = 0; i < genome.Length; i += 10)
        {
            double sign = Math.Pow(-1, genome[i]);

            //Calculate exponent
            double exponent = 0;
            double bias = Math.Pow(2, 3) - 1; //2^(k-1) - 1 where k is the number of exponent bits (4 in this case)
            for (int j = 1; j < 5; j++)
            {
                exponent += genome[i + j] * Math.Pow(2, (4 - j));
            }
            double biasedExponent = exponent - bias;


            //Calculate mantissa
            double mantissa = 0;
            for (int j = 5; j < 10; j++)
            {
                mantissa += genome[i + j] * Math.Pow(2, (4 - j));
            }

            //Calculate floating point value
            floatingPointGenome[i / 10] = (float)(sign * (1 + mantissa) * Math.Pow(2, biasedExponent));
        }
        return floatingPointGenome;
    }


    /// <summary>
    /// The lossFunction() calculates and returns a floating-point value representing the loss.
    /// </summary>
    float lossFunction()
    {
        if (wheelFrontLeftTarget == null || wheelFrontRightTarget == null || wheelBackLeftTarget == null || wheelBackRightTarget == null)
        {
            return 100;
        }

        //Calculate distance between the four wheels and the four targets
        float distanceFrontLeft = Vector2.Distance(wheelFrontLeft.position, wheelFrontLeftTarget.position);
        float distanceFrontRight = Vector2.Distance(wheelFrontRight.position, wheelFrontRightTarget.position);
        float distanceBackLeft = Vector2.Distance(wheelBackLeft.position, wheelBackLeftTarget.position);
        float distanceBackRight = Vector2.Distance(wheelBackRight.position, wheelBackRightTarget.position);

        return ((distanceFrontLeft + distanceFrontRight + distanceBackLeft + distanceBackRight) / 4) - 0.38f; //0.38f is threshold because Unity Distance function calculate wheel model which is not zeroed on ground level

    }

    /// <summary>
    /// The fitnessFunction() function calculates the fitness value for a given individual in a genetic
    /// algorithm.
    /// </summary>
    public float fitnessFunction()
    {
        FITNESSVALUE = 1 / (1 + lossFunction());
        return FITNESSVALUE;
    }

    /// <summary>
    /// The RandomInitialize function is used to initialize a random value.
    /// </summary>
    public void RandomInitialize()
    {
        int[] RandomGenome = new int[180];
        for (int i = 0; i < RandomGenome.Length; i++)
        {
            RandomGenome[i] = UnityEngine.Random.Range(0, 2);
        }

        currentGenome = RandomGenome;

        //Convert genome to floating point
        float[] floatingPointGenome = convertGenomeToFloatingPoint(RandomGenome);

        //The first 9 floatingPoint is engine weights
        for (int j = 0; j < 9; j++)
        {
            engineWeights[j] = floatingPointGenome[j];
        }

        // //The next 9 floatingPoint is steering weights
        for (int j = 9; j < 18; j++)
        {
            steeringWeights[j - 9] = floatingPointGenome[j];
        }
    }

    /// <summary>
    /// The function sets the genome of an object using an array of integers.
    /// </summary>
    /// <param name="Genome">The `Genome` parameter is an array of integers.</param>
    public void setGenome(int[] Genome)
    {
        currentGenome = Genome;

        //Convert genome to floating point
        float[] floatingPointGenome = convertGenomeToFloatingPoint(Genome);

        //The first 9 floatingPoint is engine weights
        for (int j = 0; j < 9; j++)
        {
            engineWeights[j] = floatingPointGenome[j];
        }

        // //The next 9 floatingPoint is steering weights
        for (int j = 9; j < 18; j++)
        {
            steeringWeights[j - 9] = floatingPointGenome[j];
        }
    }

    /// <summary>
    /// The function Mutate takes a probability value as input and performs a mutation operation.
    /// </summary>
    /// <param name="probability">The probability parameter is a float value that represents the
    /// likelihood of a mutation occurring.</param>
    public void Mutate(float probability)
    {
        for (int i = 0; i < currentGenome.Length; i++)
        {
            if (UnityEngine.Random.Range(0f, 1f) < probability)
            {
                currentGenome[i] = UnityEngine.Random.Range(0, 2);
            }
        }

        //Convert genome to floating point
        float[] floatingPointGenome = convertGenomeToFloatingPoint(currentGenome);

        //The first 9 floatingPoint is engine weights
        for (int j = 0; j < 9; j++)
        {
            engineWeights[j] = floatingPointGenome[j];
        }

        // //The next 9 floatingPoint is steering weights
        for (int j = 9; j < 18; j++)
        {
            steeringWeights[j - 9] = floatingPointGenome[j];
        }
    }

    /// <summary>
    /// The Start function is a special function in Unity that is called once when the script is enabled
    /// or when the game starts.
    /// </summary>
    void Start()
    {

        //Assign private Transform wheelFrontLeftTarget, wheelFrontRightTarget, wheelBackLeftTarget, wheelBackRightTarget; to the wheels
        wheelFrontLeftTarget = GameObject.Find("wheelFrontLeftTarget").transform;
        wheelFrontRightTarget = GameObject.Find("wheelFrontRightTarget").transform;
        wheelBackLeftTarget = GameObject.Find("wheelBackLeftTarget").transform;
        wheelBackRightTarget = GameObject.Find("wheelBackRightTarget").transform;
    }

    private float timer = 0.0f;
    private float waitTime = 0.1f; // Set this to 0.1 for 100ms

    /// <summary>
    /// The Update function is a built-in function in Unity that is called every frame to update the
    /// game state.
    /// </summary>
    void Update()
    {
        timer += Time.deltaTime;

        if (timer > waitTime)
        {
            ReceiveBrainSignal();
            timer = 0.0f;
        }
    }
}
