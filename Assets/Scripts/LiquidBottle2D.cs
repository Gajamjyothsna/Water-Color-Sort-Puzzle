using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;

public class LiquidBottle2D : MonoBehaviour
{
    public SpriteRenderer bottleSR;

    [Space(5)]

    [Header("Material Properties")]
    public List<Color> liquidColors;
    public AnimationCurve fillAmountCurve;
    public AnimationCurve rotationAmountCurve;
    public AnimationCurve rotationSpeedCurve;
    public float RotationTime;

    //Private Variables
    private Material bottleMaterial;
    private int numberOfColorsToTransfer = 0;

    [Header("Referenced Second Bottle")]
    public LiquidBottle2D _SecondBottleRef;
    public bool justThisBottle = false; //Transfer from this bottle indication variable


    [Header("Shader Properties")]
    private  const string firstColorPropertyId = "_FirstColor";
    private  const string secondColorPropertyId = "_SecondColor";
    private  const string thirdColorPropertyId = "_ThirdColor";
    private  const string fourthColorPropertyId = "_FourthColor";
    private  const string fillAmountId = "_FillAmount";
    private  const string rotationAmountId = "_RotationAmount";

    [Header("FillAmount Properties")]
    public float[] fillAmounts;
    public float[] rotationValues;

    private int rotationIndex = 0;

    [Range(0,4)]
    public int numberOfColorsInBottle = 4;

    [Header("TopColors")]
    public Color topColor;
    public int numberOfTopColorLayers = 1;

    [Header("Rotation Values")]
    public Transform leftRotationPoint;
    public Transform rightRotationPoint;
    private Transform chosenRotationPoint;
    private float directionMultiplier = 1.0f;

    [Header("Line Renderers")]
    [SerializeField] LineRenderer lineRenderer;

    private Vector3 originalPosition;
    private Vector3 startPosition;
    private Vector3 endPosition;

    [SerializeField] private float FillAmountValue;

    private void Start()
    {
        GetBottleMaterial(); //Getting Bottle Material
        bottleMaterial.SetFloat(fillAmountId, fillAmounts[numberOfColorsInBottle]);
        FillAmountValue = fillAmounts[numberOfColorsInBottle];
        originalPosition = transform.position;
        SetLiquidColors(); //Setting Up Colors
        UpdateTopColorValues();
    }

    private void Update()
    {
        //On Mouse click Starting the Couritine
        if(Input.GetMouseButtonDown(0) && justThisBottle == true)
        {
            UpdateTopColorValues();

            if(_SecondBottleRef.FillBottleColorCheck(topColor))
            {
                ChooseRotationPointAndDirection();

                numberOfColorsToTransfer = Mathf.Min(numberOfTopColorLayers, 4 - _SecondBottleRef.numberOfColorsInBottle);

                for(int i = 0; i < numberOfColorsToTransfer; i++) 
                {
                    _SecondBottleRef.liquidColors[_SecondBottleRef.numberOfColorsInBottle + i] = topColor;
                }

                _SecondBottleRef.SetLiquidColors();
            }

            CalculateRotationIndex(4 - _SecondBottleRef.numberOfColorsInBottle);
            StartCoroutine(MoveBottle());
        }
    }


    private void SetLiquidColors()//Setting the Liquid colors
    {
        bottleMaterial?.SetColor(firstColorPropertyId,liquidColors[3]);
        bottleMaterial?.SetColor(secondColorPropertyId,liquidColors[2]);
        bottleMaterial?.SetColor(thirdColorPropertyId,liquidColors[1]);
        bottleMaterial?.SetColor(fourthColorPropertyId,liquidColors[0]);
    }

    public Material GetBottleMaterial()
    {
        if(bottleSR is not null)
        {
            bottleMaterial =  bottleSR?.material;
            return bottleMaterial;
        }
        else
        {
            Debug.Log($"Couldn't Find SpriteRenderer on the : {this.name},  Pls Add the SpriteRenderer Component");
            return null;
        }
    }


    //Handles the Bottle rotation and transition
    public IEnumerator StartPouring()
    {
         float t = 0;
         float lerpValue;
         float angleValue;

        float lastAngleValue = 0;
        //Runs the while loop till the timer ran out
        while (t < RotationTime)
        {
            lerpValue = t / RotationTime;
            angleValue = Mathf.Lerp(0f, directionMultiplier * rotationValues[rotationIndex], lerpValue);

            //Applying the Lerped Angle value in each frame for smooth linear transistion
            // transform.eulerAngles = new Vector3(0, 0, angleValue);

            transform.RotateAround(chosenRotationPoint.position, Vector3.forward, lastAngleValue - angleValue);

            float rotValue = rotationAmountCurve.Evaluate(angleValue);
            float fillAmountValue = fillAmountCurve.Evaluate(angleValue);

            
            //Applying the Lerped rotation and Fill Amount value in each frame for smooth linear transistion
            bottleMaterial.SetFloat(rotationAmountId, rotValue);

            if (fillAmounts[numberOfColorsInBottle] > fillAmountCurve.Evaluate(angleValue))
            {
                if(lineRenderer.enabled == false)
                {
                    lineRenderer.startColor = topColor;
                    lineRenderer.endColor = topColor;

                    lineRenderer.SetPosition(0, chosenRotationPoint.position);
                    lineRenderer.SetPosition(1, chosenRotationPoint.position - Vector3.up * 10f);
                    lineRenderer.enabled = true;
                }
                bottleMaterial.SetFloat(fillAmountId, fillAmountValue);
                FillAmountValue = fillAmountValue;

                _SecondBottleRef.FillUp(fillAmountCurve.Evaluate(lastAngleValue) - fillAmountCurve.Evaluate(angleValue));
            }
            //Incrmenting timer value and slowing down the roation value for smooth liquid flowing effect
            t += Time.deltaTime * rotationSpeedCurve.Evaluate(angleValue);
            lastAngleValue = angleValue;
            yield return null;
        }

        
        //Applying the Final Rotation and Other material Property values
        angleValue = directionMultiplier * rotationValues[rotationIndex];
      //  transform.eulerAngles = new Vector3(0, 0, angleValue);
        float finalRotValue = rotationAmountCurve.Evaluate(angleValue);
        float finalFillAmountValue = fillAmountCurve.Evaluate(angleValue);

        numberOfColorsInBottle -= numberOfColorsToTransfer;
        _SecondBottleRef.numberOfColorsInBottle += numberOfColorsToTransfer;

        //Applying the material Property values
        bottleMaterial.SetFloat(rotationAmountId, finalRotValue);
        bottleMaterial.SetFloat(fillAmountId, finalFillAmountValue);

        lineRenderer.enabled = false;
        StartCoroutine(RotateBottleBack());
        
    }

    private IEnumerator RotateBottleBack()
    {
        float t = 0;
        float lerpValue;
        float angleValue;

        float lastAngleValue = directionMultiplier * rotationValues[rotationIndex];
        //Runs the while loop till the timer ran out
        while (t < RotationTime)
        {
            lerpValue = t / RotationTime;
            angleValue = Mathf.Lerp(directionMultiplier * rotationValues[rotationIndex], 0f, lerpValue);

            //Applying the Lerped Angle value in each frame for smooth linear transistion
            // transform.eulerAngles = new Vector3(0, 0, angleValue);

            transform.RotateAround(chosenRotationPoint.position, Vector3.forward, lastAngleValue - angleValue);

            float rotValue = rotationAmountCurve.Evaluate(angleValue);

            //Applying the Lerped rotation and Fill Amount value in each frame for smooth linear transistion
            bottleMaterial.SetFloat(rotationAmountId, rotValue);

            lastAngleValue = angleValue;

            //Incrmenting timer value and slowing down the roation value for smooth liquid flowing effect
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        UpdateTopColorValues();
        //Applying the Final Rotation and Other material Property values
        angleValue = 0f;
        transform.eulerAngles = new Vector3(0, 0, angleValue);
        float finalRotValue = rotationAmountCurve.Evaluate(angleValue);
        float finalFillAmountValue = fillAmountCurve.Evaluate(angleValue);

        //Applying the material Property values
        bottleMaterial.SetFloat(rotationAmountId, finalRotValue);

        StartCoroutine(MoveBottleBack());
    }

    //Which color is top of the Bottle

    public void UpdateTopColorValues()
    {
        if(numberOfColorsInBottle!=0)
        {
            numberOfTopColorLayers = 1;
            topColor = liquidColors[numberOfColorsInBottle - 1];

            if(numberOfColorsInBottle == 4)
            {
                if (liquidColors[3].Equals(liquidColors[2]))
                {
                    numberOfTopColorLayers = 2;

                    if (liquidColors[2].Equals(liquidColors[1]))
                    {
                        numberOfTopColorLayers = 3;

                        if (liquidColors[1].Equals(liquidColors[0]))
                        {
                            numberOfTopColorLayers = 4;
                        }
                    }
                }
            }

            else if(numberOfColorsInBottle == 3)
            {
                if (liquidColors[2].Equals(liquidColors[1]))
                {
                    numberOfTopColorLayers = 2;

                    if (liquidColors[1].Equals(liquidColors[0]))
                    {
                        numberOfTopColorLayers = 3;
                    }
                }
            }

            else if(numberOfColorsInBottle == 2)
            {
                if (liquidColors[1].Equals(liquidColors[0]))
                {
                    numberOfTopColorLayers = 2;
                }
            }

            rotationIndex = 3 - (numberOfColorsInBottle - numberOfTopColorLayers);
        }
    }

    public bool FillBottleColorCheck(Color _colorToCheck)
    {
        if (numberOfColorsInBottle == 0) return true;
        else
        {
            if(numberOfColorsInBottle == 4)
            {
                return false;
            }
            else
            {
                if(topColor.Equals(_colorToCheck)) return true;
                else { return false; }
            }
        }
    }

    private void CalculateRotationIndex(int numberOfEmptySpacesInSecondBottle)
    {
        rotationIndex = 3 - (numberOfColorsInBottle - Mathf.Min(numberOfEmptySpacesInSecondBottle, numberOfTopColorLayers));
    }

    private void FillUp(float _fillAmountToAdd)
    {
        Debug.Log("FillUp Amount" + _fillAmountToAdd);

        bottleMaterial.SetFloat(fillAmountId, bottleMaterial.GetFloat(fillAmountId) + _fillAmountToAdd);

        FillAmountValue = _fillAmountToAdd + bottleMaterial.GetFloat(fillAmountId);
    }

    private void ChooseRotationPointAndDirection()
    {
        if(transform.position.x > _SecondBottleRef.transform.position.x)
        {
            chosenRotationPoint = leftRotationPoint;
            directionMultiplier = -1.0f;
        }
        else
        {
            chosenRotationPoint = rightRotationPoint;
            directionMultiplier = 1.0f;
        }
    }

    public void StartColorTransfer()
    {
        ChooseRotationPointAndDirection();

        Debug.Log("Number of Top ColorLayers" + numberOfTopColorLayers);
        Debug.Log("secondBottleRef" + _SecondBottleRef.numberOfColorsInBottle);
        numberOfColorsToTransfer = Mathf.Min(numberOfTopColorLayers, 4 - _SecondBottleRef.numberOfColorsInBottle);
        Debug.Log("Number of Colors to transfer" + numberOfColorsToTransfer);
        for (int i = 0; i < numberOfColorsToTransfer; i++)
        {
            _SecondBottleRef.liquidColors[_SecondBottleRef.numberOfColorsInBottle + i] = topColor;
        }

        _SecondBottleRef.SetLiquidColors();

        CalculateRotationIndex(4 - _SecondBottleRef.numberOfColorsInBottle);

        transform.GetComponent<SpriteRenderer>().sortingOrder += 2;
        bottleSR.sortingOrder += 2;

        StartCoroutine(MoveBottle());
    }

    private IEnumerator MoveBottle()
    {
        Debug.Log("Move Bottle");
        startPosition = transform.position;
        if(chosenRotationPoint == leftRotationPoint)
        {
            endPosition = _SecondBottleRef.rightRotationPoint.position;
        }
        else
        {
            endPosition = _SecondBottleRef.leftRotationPoint.position;
        }

        float t = 0;
        while(t<=1)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            t += Time.deltaTime * 2;

            yield return new WaitForEndOfFrame();
        }

        transform.position = endPosition;

        StartCoroutine(StartPouring());
    }

    private IEnumerator MoveBottleBack()
    {
        startPosition = transform.position;
        endPosition = originalPosition;

        float t = 0;
        while (t <= 1)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            t += Time.deltaTime * 2;

            yield return new WaitForEndOfFrame();
        }

        transform.position = endPosition;

        transform.GetComponent<SpriteRenderer>().sortingOrder -= 2;
        bottleSR.sortingOrder -= 2;

    }
}
