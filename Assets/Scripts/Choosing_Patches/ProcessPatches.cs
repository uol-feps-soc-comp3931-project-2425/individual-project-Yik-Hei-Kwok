using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProcessPatches : MonoBehaviour
{

    [Header("For extracting overlays of each patch")]
    public ComputeShader extractOverlays;
    [Header("For obtaining difference of pixel values between left and right / bottom and top overlays")]
    public ComputeShader differenceInOverlays;

    // compute buffer for passing in array data of patches into shader
    private ComputeBuffer inputPatchData;
    // compute buffer for storing overlay data
    private ComputeBuffer outputOverlayData;

    // compute buffer for storing the difference in values
    private ComputeBuffer differenceOverlapData;
    // compute buffer for storing right and left overlap data
    private ComputeBuffer rightOverlapData;
    private ComputeBuffer leftOverlapData;
    // compute buffer storing values for calculating maximum distance tolerance
    private ComputeBuffer maxDiffOverlapData;

    /*
    Functions Below are for extracting the bottom and right overlay patches of the current selected patch to be placed.
    This information will be proven useful when comparing with the next patches
    */
    public float[] saveBottomOverlay(float[] chosenPatch)
    {
        int patchSize = global.patchData.patchSize;
        int overlapSize = global.patchData.overlapSize;
        int truePatchSize = patchSize - overlapSize;

        int kernalID = extractOverlays.FindKernel("extractBottOverlays");

        // use data in the patch array for the shader
        inputPatchData = new ComputeBuffer(chosenPatch.Length, sizeof(float));
        inputPatchData.SetData(chosenPatch);
        // set the patch data that will be inputted into GPU processing
        extractOverlays.SetBuffer(kernalID, "patchData", inputPatchData);

        // data for extracting overlap regions Top
        extractOverlays.SetInt("overlapSize", overlapSize);
        extractOverlays.SetInt("patchSize", patchSize);
        extractOverlays.SetInt("truePatchSize", truePatchSize);
        extractOverlays.SetInt("imgHeight", global.refImgData.refImgHeight);


        outputOverlayData = new ComputeBuffer(truePatchSize * overlapSize, sizeof(float) * 4);
        extractOverlays.SetBuffer(kernalID, "bottomOverlayOutput", outputOverlayData);

        extractOverlays.Dispatch(kernalID, Mathf.CeilToInt((float)patchSize / 8), Mathf.CeilToInt((float)patchSize / 8), 1);
        float[] bottomOverlayCurrPatch = new float[truePatchSize * overlapSize * 4];
        // store bottom overlap pixel values into the array
        outputOverlayData.GetData(bottomOverlayCurrPatch);

        return bottomOverlayCurrPatch;
    }

    public float[] saveRightOverlay(float[] chosenPatch)
    {
        int patchSize = global.patchData.patchSize;
        int overlapSize = global.patchData.overlapSize;
        int truePatchSize = patchSize - overlapSize;

        int kernalID = extractOverlays.FindKernel("extractRightOverlays");

        // use data in the patch array for the shader
        inputPatchData = new ComputeBuffer(chosenPatch.Length, sizeof(float));
        inputPatchData.SetData(chosenPatch);
        // set the patch data that will be inputted into GPU processing
        extractOverlays.SetBuffer(kernalID, "patchData", inputPatchData);

        // data for extracting overlap regions Top
        extractOverlays.SetInt("overlapSize", overlapSize);
        extractOverlays.SetInt("patchSize", patchSize);
        extractOverlays.SetInt("truePatchSize", truePatchSize);
        extractOverlays.SetInt("imgHeight", global.refImgData.refImgHeight);


        outputOverlayData = new ComputeBuffer(truePatchSize * overlapSize, sizeof(float) * 4);
        extractOverlays.SetBuffer(kernalID, "rightOverlayOutput", outputOverlayData);

        extractOverlays.Dispatch(kernalID, Mathf.CeilToInt((float)patchSize / 8), Mathf.CeilToInt((float)patchSize / 8), 1);
        float[] bottomOverlayCurrPatch = new float[truePatchSize * overlapSize * 4];
        // store bottom overlap pixel values into the array
        outputOverlayData.GetData(bottomOverlayCurrPatch);

        

        return bottomOverlayCurrPatch;
    }


    /*
    Functions Below are comparing overlays of either top and bottom patches or left and right patches. This is for calculating
    the distance metric for each patch. Patches lower than the maximum tolerance will be selected
    */

    public float[] compareOverlaysGPU(float[][] allLeftOverlays, float[] previousRightOverlay, int patchSize, int overlapSize, int totalPatches)
    {

        float[] distanceMetrics = new float[totalPatches];

        for (int i = 0; i < allLeftOverlays.Length; i++)
        {
            int truePatchSize = patchSize - overlapSize;
            int kernalID = differenceInOverlays.FindKernel("diffLeftRight");
            // pass in pixel data of previous right overlap to the shader
            rightOverlapData = new ComputeBuffer(truePatchSize * overlapSize * 4, sizeof(float));
            rightOverlapData.SetData(previousRightOverlay);
            differenceInOverlays.SetBuffer(kernalID, "rightOverlayData", rightOverlapData);

            // pass in pixel data of each left overlay to the shader
            leftOverlapData = new ComputeBuffer(truePatchSize * overlapSize * 4, sizeof(float));
            leftOverlapData.SetData(allLeftOverlays[i]);
           

            differenceInOverlays.SetBuffer(kernalID, "leftOverlayData", leftOverlapData);

            // store the difference in an array
            // for distance metric
            float[] pixelDifferences = new float[truePatchSize * overlapSize * 4];
            
            differenceOverlapData = new ComputeBuffer(truePatchSize * overlapSize * 4, sizeof(float));
            differenceInOverlays.SetBuffer(kernalID, "outputDifference", differenceOverlapData);


            differenceInOverlays.Dispatch(kernalID, Mathf.CeilToInt((float)(truePatchSize * overlapSize * 4) / 64), 1, 1);
            differenceOverlapData.GetData(pixelDifferences);


            // now we have the difference in pixel values, we now apply Distance Matrics and the distance tolerance (dmax)
            // given by the paper 
            float sumDifference = 0;


            for (int j = 0; j < pixelDifferences.Length; j++)
                sumDifference += pixelDifferences[j];

            float distanceMetric = Mathf.Pow(((1f / truePatchSize * overlapSize) * sumDifference), 0.5f);
            distanceMetrics[i] = distanceMetric;

        }
        return distanceMetrics;
    }

    public float[] computeMaxTolerance(float[] previousOverlay, int patchSize, int overlapSize)
    {
        int kernalID_max = differenceInOverlays.FindKernel("maxDifference");
        int truePatchSize = patchSize - overlapSize;

        rightOverlapData = new ComputeBuffer(truePatchSize * overlapSize * 4, sizeof(float));
        rightOverlapData.SetData(previousOverlay);
        differenceInOverlays.SetBuffer(kernalID_max, "rightOverlayData", rightOverlapData);

        

        float[] maxDifferences = new float[truePatchSize * overlapSize * 4];
        maxDiffOverlapData = new ComputeBuffer(truePatchSize * overlapSize * 4, sizeof(float));
        differenceInOverlays.SetBuffer(kernalID_max, "maxOutputDifference", maxDiffOverlapData);
        differenceInOverlays.Dispatch(kernalID_max, Mathf.CeilToInt((float)(truePatchSize * overlapSize) / 64), 1, 1);
        maxDiffOverlapData.GetData(maxDifferences);

        Debug.Log(":GH First 100 R/G/B/A values");
        for(int i = 0; i < 100; i++)
        {
            Debug.Log(":GH maxDifferences = " + maxDifferences[i]);
        }



        return maxDifferences;
    }


    public float[][] filterPatchMaxToleranceOne(float[] maxDifferences, float[] distanceMetrics, int patchSize, int overlapSize, float[][] allPatches, float lamda)
    {
        float maxDifference = 0;

        for (int j = 0; j < maxDifferences.Length; j++)
            maxDifference += maxDifferences[j];

        float truePatchSize = patchSize - overlapSize;
        float maxTolerance = lamda * Mathf.Pow(((1f / truePatchSize * overlapSize) * maxDifference), 0.5f);

        Debug.Log(":DF maxTolerance = " + maxTolerance);
        // store patches that fit the distance metric
        int numPossible = 0;
        for (int i = 0; i < distanceMetrics.Length; i++)
        {
            /*Debug.Log(":DF distanceMetrics = " + distanceMetrics[i]);
            Debug.Log(":DF maxTolerance = " + maxTolerance);*/
            if (distanceMetrics[i] < maxTolerance)
            {
                numPossible++;
            }

        }

        // if the set is empty (no patch meets the distance tolerance criteria)
        // get the closest one to the maximum tolerance
        if (numPossible == 0)
        {
            Debug.Log(":DF numPossible = None possible, chose closest one");

            // make sure numPossible isn't zero because we need to choose at least one patch regardless
            float[][] possiblePatches = new float[numPossible + 1][];

            int smallestIndex = Array.IndexOf(distanceMetrics, distanceMetrics.Min());
            //Debug.Log("Smallest Index = " + smallestIndex);
            possiblePatches[0] = allPatches[smallestIndex];
            Debug.Log(":DF Patch chosen = " + smallestIndex);
            // return all possible patches that can be used
            return possiblePatches;
        }
        else
        {
            float[] debugStore = new float[numPossible];

            float[][] possiblePatches = new float[numPossible][];
            int increment = 0;
            for (int i = 0; i < distanceMetrics.Length; i++)
            {
                if (distanceMetrics[i] < maxTolerance)
                {
                    debugStore[increment] = i;
                    possiblePatches[increment] = allPatches[i];
                    increment++;
                }

            }
            string debug = "";
            for (int i = 0; i < debugStore.Length; i++)
            {
                debug += (" ," + debugStore[i].ToString());
            }
            Debug.Log(":DF numPossible = " + numPossible);
            Debug.Log(":DF Patches to be considered = " + debug);

            // return all possible patches that can be used
            return possiblePatches;
        }
    }

    public float[][] filterPatchMaxToleranceBoth(float[] maxDifferencesLeft, float[] maxDifferencesTop, float[] distanceMetricsLeft, float[] distanceMetricsTop, int patchSize, int overlapSize, float[][] allPatches, float lamda)
    {
        float maxDifferenceLeft = 0;
        float maxDifferenceTop = 0;
        // calculate maximum distance tolerance
        for (int j = 0; j < maxDifferencesLeft.Length; j++)
            maxDifferenceLeft += maxDifferencesLeft[j];

        for (int k = 0; k < maxDifferencesTop.Length; k++)
            maxDifferenceTop += maxDifferencesTop[k];

        float truePatchSize = patchSize - overlapSize;
        float maxToleranceLeft = lamda * Mathf.Pow(((1f / truePatchSize * overlapSize) * maxDifferenceLeft), 0.5f);
        float maxToleranceTop = lamda * Mathf.Pow(((1f / truePatchSize * overlapSize) * maxDifferenceTop), 0.5f);

        // store patches that fit the distance metrices
        int numPossible = 0;
        for (int i = 0; i < distanceMetricsLeft.Length; i++)
        {
            /*Debug.Log(":DF distanceMetrics Left = " + distanceMetricsLeft[i]);
            Debug.Log(":DF distanceMetrics Top = " + distanceMetricsTop[i]);
            Debug.Log($":DF Averaged Left and Top = {(distanceMetricsLeft[i] + distanceMetricsTop[i]) / 2}");
            Debug.Log($":DF Averaged max Left and max Top = {(maxToleranceLeft + maxToleranceTop) / 2}");*/
            if ((distanceMetricsLeft[i] + distanceMetricsTop[i]) / 2 < (maxToleranceLeft + maxToleranceTop) / 2)
            {
                numPossible++;
            }
            Debug.Log(":DF numPossible = " + numPossible);

        }

        // if the set is empty (no patch meets the distance tolerance criteria)
        // get the closest one to the maximum tolerance
        if (numPossible == 0)
        {
            Debug.Log(":DF numPossible = None possible, chose closest one");
            // make sure numPossible isn't zero because we need to choose at least one patch regardless
            float[][] possiblePatches = new float[numPossible + 1][];

            int smallestIndex = Array.IndexOf(distanceMetricsLeft, distanceMetricsLeft.Min());
            possiblePatches[0] = allPatches[smallestIndex];
            Debug.Log(":DF Patch chosen = " + smallestIndex);
            return possiblePatches;
        }
        else
        {
            float[][] possiblePatches = new float[numPossible][];
            int increment = 0;
            float[] debugStore = new float[numPossible];

            for (int i = 0; i < distanceMetricsLeft.Length; i++)
            {
                if ((distanceMetricsLeft[i] + distanceMetricsTop[i]) / 2 < (maxToleranceLeft + maxToleranceTop) / 2)
                {
                    debugStore[increment] = i;
                    possiblePatches[increment] = allPatches[i];
                    increment++;
                }
            }
            string debug = "";
            for (int i = 1; i < debugStore.Length; i++)
            {
                debug += (" ," + debugStore[i].ToString());
            }
            Debug.Log(":DF numPossible = " + numPossible);
            Debug.Log(":DF Patches to be considered = " + debug);

            return possiblePatches;
        }
    }

}
