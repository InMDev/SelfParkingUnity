using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
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

    //public TextMeshProUGUI sensorFrontText;
    //public TextMeshProUGUI sensorBackText;
    //public TextMeshProUGUI sensorLeftmidText;
    //public TextMeshProUGUI sensorRightmidText;
    //public TextMeshProUGUI sensorLeftfrontText;
    //public TextMeshProUGUI sensorRightfrontText;
    //public TextMeshProUGUI sensorLeftbackText;
    //public TextMeshProUGUI sensorRightbackText;

    public TextMeshProUGUI lossUIText;

    public float[] sensorValues = new float[8];
    public float[] engineWeights = new float[9];
    public float[] steeringWeights = new float[9];

    public Transform wheelFrontLeft;
    public Transform wheelFrontRight;
    public Transform wheelBackLeft;
    public Transform wheelBackRight;

    public Transform wheelFrontLeftTarget;
    public Transform wheelFrontRightTarget;
    public Transform wheelBackLeftTarget;
    public Transform wheelBackRightTarget;

    public bool isRandomGenome = false;
    public float loss;

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
        //replace text with sensor values & round to 2 decimal places
        //sensorfronttext.text = system.math.round(sensorfront.getdistance(), 2).tostring();
        //sensorbacktext.text = system.math.round(sensorback.getdistance(), 2).tostring();
        //sensorleftmidtext.text = system.math.round(sensorleftmid.getdistance(), 2).tostring();
        //sensorrightmidtext.text = system.math.round(sensorrightmid.getdistance(), 2).tostring();
        //sensorleftfronttext.text = system.math.round(sensorleftfront.getdistance(), 2).tostring();
        //sensorrightfronttext.text = system.math.round(sensorrightfront.getdistance(), 2).tostring();
        //sensorleftbacktext.text = system.math.round(sensorleftback.getdistance(), 2).tostring();
        //sensorrightbacktext.text = system.math.round(sensorrightback.getdistance(), 2).tostring();

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

        loss = lossFunction();
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
        float margin = 0.4f;

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
        //Calculate distance between the four wheels and the four targets
        float distanceFrontLeft = Vector2.Distance(wheelFrontLeft.position, wheelFrontLeftTarget.position);
        float distanceFrontRight = Vector2.Distance(wheelFrontRight.position, wheelFrontRightTarget.position);
        float distanceBackLeft = Vector2.Distance(wheelBackLeft.position, wheelBackLeftTarget.position);
        float distanceBackRight = Vector2.Distance(wheelBackRight.position, wheelBackRightTarget.position);

        return ((distanceFrontLeft + distanceFrontRight + distanceBackLeft + distanceBackRight) / 4) - 0.38f; //0.38f is threshold because Unity Distance function calculate wheel model which is not zeroed on ground level

    }

    float fitnessFunction()
    {
        return 1 / (1 + lossFunction());
    }

    void Start()
    {
        if (isRandomGenome)
        {
            int[] RandomGenome = new int[180];
            for (int i = 0; i < RandomGenome.Length; i++)
            {
                RandomGenome[i] = UnityEngine.Random.Range(0, 2);
            }

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

        // Start receiving brain signals every 100ms (using InvokeRepeating)
        InvokeRepeating(nameof(ReceiveBrainSignal), 0f, 0.1f);
    }
}
