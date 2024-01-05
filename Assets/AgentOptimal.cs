using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class AgentOptimal : MonoBehaviour
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

    public TextMeshProUGUI lossUIText;

    public string Genome;
    private int[] currentGenome = new int[180];
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

    // Define enum for MuscleSignal
    public enum MuscleSignal
    {
        MinusOne = -1,
        Zero = 0,
        PlusOne = 1
    }

    // Example muscles that will receive signals from the brain
    public MuscleSignal engineMuscleSignal;
    public MuscleSignal steeringWheelMuscleSignal;

    // Define the function to receive signals from the brain
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

    // Define the function to process received signals and perform actions accordingly
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

    void BrainFunction(float[] sensors)
    {
        //Output ProcessMuscleSignals() with different values for engineMuscleSignal and steeringWheelMuscleSignal
        engineMuscleSignal = sign(sigmoidFunction(linearFunction(engineWeights, sensors)));
        steeringWheelMuscleSignal = sign(sigmoidFunction(linearFunction(steeringWeights, sensors)));

        float loss = lossFunction();
        lossUIText.text = loss.ToString();
        ProcessMuscleSignals();
    }

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

    float sigmoidFunction(float x)
    {
        return 1 / (1 + Mathf.Exp(-x));
    }
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

    public void fitnessFunction()
    {
        FITNESSVALUE = 1 / (1 + lossFunction());
    }


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

    

    void Start()
    {
        //Convert string genome to int array split by each character and convert to int
        int[] genome = new int[Genome.Length];
        for (int i = 0; i < Genome.Length; i++)
        {
            genome[i] = int.Parse(Genome[i].ToString());
        }

        setGenome(genome);

        //Assign private Transform wheelFrontLeftTarget, wheelFrontRightTarget, wheelBackLeftTarget, wheelBackRightTarget; to the wheels
        wheelFrontLeftTarget = GameObject.Find("wheelFrontLeftTarget").transform;
        wheelFrontRightTarget = GameObject.Find("wheelFrontRightTarget").transform;
        wheelBackLeftTarget = GameObject.Find("wheelBackLeftTarget").transform;
        wheelBackRightTarget = GameObject.Find("wheelBackRightTarget").transform;

        // Start receiving brain signals every 100ms (using InvokeRepeating)
        InvokeRepeating(nameof(ReceiveBrainSignal), 0f, 0.1f);
    }
}
