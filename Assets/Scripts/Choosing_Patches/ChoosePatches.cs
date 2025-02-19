using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class ChoosePatches : MonoBehaviour
{
    // compute buffer for passing in array data of patches into shader
    private ComputeBuffer inputPatchData;
    // compute buffer for storing overlay data
    private ComputeBuffer outputOverlayData;
    // compute buffer for storing patch data without overlapping reigions
    private ComputeBuffer outputPatchData;


    // compute buffer for storing right and left overlap data
    private ComputeBuffer rightOverlapData;
    private ComputeBuffer leftOverlapData;
    // compute buffer for storing the difference in values
    private ComputeBuffer differenceOverlapData;
    // compute buffer storing values for calculating maximum distance tolerance
    private ComputeBuffer maxDiffOverlapData;


    [Header("For extracting overlays of each patch")]
    public ComputeShader extractOverlays;

    [Header("For getting patch data without the overlapping regions")]
    public ComputeShader getWithoutOverlays;

    [Header("For obtaining difference of pixel values between left and right / bottom and top overlays")]
    public ComputeShader differenceInOverlays;

    

    public void startChoosePatches(float[][] allPatches, float[][][] allOverlaps, int resultImageSize, int patchSize, int overlapSize)
    {
        // determine how many patches are needed to make up result image 
        // ( + 1 used for cases where result image not completely filled by)
        // need to subtract overlapSize from patchSize because its the actual size of patch which will be placed into result image 
        int totalPatchesNeeded = (int)Mathf.Ceil(Mathf.Pow((resultImageSize/(patchSize-overlapSize)) + 1,2f));
        // how many patches per row in the result( image
        int patchesPerRow = (int)Mathf.Ceil(resultImageSize / (patchSize - overlapSize)) + 1;
        Debug.Log("D:: this = " + resultImageSize / (patchSize - overlapSize));
        Debug.Log("D:: resultImageSize = " + resultImageSize);
        Debug.Log("D:: patchSize = " + patchSize);
        Debug.Log("D:: overlapSize = " + overlapSize);
        Debug.Log("D:: totalPatchesNeeded = " + totalPatchesNeeded);
        Debug.Log("D:: patchesPerRow = " + patchesPerRow);

        // for placing patches onto a new canvas
        int k = 0;
        // for saving the patch number that is being extracted
        int currIterationPatchNum = 0;

        // chosen patches wil be placed in this array
        float[][] chosenPatches = new float[totalPatchesNeeded][];
        // we only need to save the bottom overlay patches in the long run
        float[][] previousRowBottomPatches = new float[patchesPerRow][];
        // we only need to save the left overlay of the previously chosen patch
        float[] previousRightPatch = new float[patchSize * overlapSize * 4];

        // loop through all patches needed
        for (int i = 0;  i < totalPatchesNeeded; i++) {
            (currIterationPatchNum, previousRightPatch, previousRowBottomPatches[k]) = placeNewPatch(k, allPatches, allOverlaps, chosenPatches, patchesPerRow, previousRightPatch, previousRowBottomPatches);
            k += 1;
        }
    }

    // for placing a new patch at a new position of the result image
    private (int, float[], float[]) placeNewPatch(int k, float[][] allPatches, float[][][] allOverlaps, float[][] chosenPatches, int patchesPerRow, float[] prevRightPatch, float[][] previousRowBottomPatches)
    {
        int patchSize = global.patchData.patchSize;
        int overlapSize = global.patchData.overlapSize;

        // for storing the chosen patch at this iteration
        int chosenPatchNum;
        // get the right and bottom overlay of the extracted patch, will be used for comparing with next patch
        float[] chosenRightOverlay = new float[patchSize * overlapSize * 4];
        float[] chosenBottomOverlay = new float[patchSize * overlapSize * 4];

        // chosenPatches array denotes 
        // if k = 0, it means there isn't a single patch placed, place the first patch
        if (k == 0)
        {
            // place the first chosen patch into the chosenPatches array.
            (chosenPatches[0], chosenPatchNum, chosenRightOverlay, chosenBottomOverlay) = placeFirstPatch(allPatches, k);
            
        }
        // if k != 0, then a patch is already placed. Look at previous patch and compare
        // compare
        else
        {
            (chosenPatches[k], chosenPatchNum, chosenRightOverlay, chosenBottomOverlay) = placeNextPatch(allPatches, allOverlaps, k, patchesPerRow, prevRightPatch, previousRowBottomPatches);
        }
        return (chosenPatchNum, chosenRightOverlay, chosenBottomOverlay);
    }


    public (float[],int, float[], float[]) placeFirstPatch(float[][] allPatches, int k)
    {
        int patchSize = global.patchData.patchSize;
        int overlapSize = global.patchData.overlapSize;

        // choose a random patch as start patch 
        System.Random ran = new System.Random();
        // ranPatch describes which patch is chosen
        int ranPatch = ran.Next(allPatches.Length);
        ranPatch = 0;
        float[] chosenPatch = allPatches[ranPatch];

        // for storing the current patch's bottom and right overlaps
        float[] overlapBottom = new float[patchSize * overlapSize * 4];
        float[] overlapRight = new float[patchSize * overlapSize * 4];
        float[] toBePlacedPatch = new float[(patchSize - overlapSize)^2 * 4];

        // save right and bottom overlays of the chosen patch
        overlapBottom = saveBottomOverlay(chosenPatch);
        overlapRight = saveRightOverlay(chosenPatch);

        // need to get the patch pixel data without the overlap areas (this will be the data to be placed in the image)
        toBePlacedPatch = getPatchWithoutOverlap(chosenPatch);

        // for debug only
        DebugFunctions.showData_CustomizedWH(patchSize, overlapSize, overlapBottom, $"Saved/Save_Overlay_Bottom/bottom_{k}.png");
        DebugFunctions.showData_CustomizedWH(overlapSize, patchSize, overlapRight, $"Saved/Save_Overlay_Right/right_{k}.png");
        DebugFunctions.showData_CustomizedWH(patchSize - overlapSize, patchSize - overlapSize, toBePlacedPatch, $"Saved/To_Be_Placed_Patches/to_be_placed_{k}.png");
        
        return (toBePlacedPatch, ranPatch, overlapRight, overlapBottom);
    }

    private (float[],int, float[], float[]) placeNextPatch(float[][] allPatches, float[][][] allOverlaps, int patchNumber, int patchesPerRow, float[] prevRightPatch, float[][] previousRowBottomPatches)
    {
        int patchSize = global.patchData.patchSize;
        int overlapSize = global.patchData.overlapSize;

        // to be returned chosen patch
        float[] toBePlacedPatch = new float[(patchSize - overlapSize) ^ 2 * 4];
        int ranPatch = 0;
        // for storing the current patch's bottom and right overlaps
        float[] overlapBottom = new float[patchSize * overlapSize * 4];
        float[] overlapRight = new float[patchSize * overlapSize * 4];

        Debug.Log("K1 = " + patchNumber);
        Debug.Log("K12 = " + patchesPerRow);

        // if we are still at the first row of the result image, we don't need to compare overlap top region
        if (patchNumber < patchesPerRow)
        {
            // function for comparing left overlays
            float[][] possiblePatches = compareOneOverlays(prevRightPatch, allOverlaps[1], global.patchData.totalNumPatches, allPatches);
            // choose a random patch from the list
            System.Random ran = new System.Random();
            ranPatch = ran.Next(possiblePatches.Length);
            float[] chosenPatch = possiblePatches[ranPatch];
           

            // save right and bottom overlays of the chosen patch
            overlapBottom = saveBottomOverlay(chosenPatch);
            overlapRight = saveRightOverlay(chosenPatch);

            // need to get the patch pixel data without the overlap areas (this will be the data to be placed in the image)
            toBePlacedPatch = getPatchWithoutOverlap(chosenPatch);

            DebugFunctions.showData_CustomizedWH(patchSize, overlapSize, overlapBottom, $"Saved/Save_Overlay_Bottom/bottom_{patchNumber}.png");
            DebugFunctions.showData_CustomizedWH(overlapSize, patchSize, overlapRight, $"Saved/Save_Overlay_Right/right_{patchNumber}.png");
            DebugFunctions.showData_CustomizedWH(patchSize - overlapSize, patchSize - overlapSize, toBePlacedPatch, $"Saved/To_Be_Placed_Patches/to_be_placed_{patchNumber}.png");
            
        }
        // if we are at the first patch of the row, we don't need to compare left overlays
        else if (patchNumber % (patchesPerRow) == 0)
        {
            Debug.Log($"D:: enter patchNum = {patchNumber}");
            // get the previous bottom overlay (since we are choosing patch for start of row, always compare with patch chosen at start of previous row)
            float[] prevBottomPatch = previousRowBottomPatches[0];
            // function for comparing top overlays
            float[][] possiblePatches = compareOneOverlays(prevBottomPatch, allOverlaps[0], global.patchData.totalNumPatches, allPatches);

            System.Random ran = new System.Random();
            ranPatch = ran.Next(possiblePatches.Length);
            float[] chosenPatch = possiblePatches[ranPatch];

            // save right and bottom overlays of the chosen patch
            overlapBottom = saveBottomOverlay(chosenPatch);
            overlapRight = saveRightOverlay(chosenPatch);

            // need to get the patch pixel data without the overlap areas (this will be the data to be placed in the image)
            toBePlacedPatch = getPatchWithoutOverlap(chosenPatch);

            DebugFunctions.showData_CustomizedWH(patchSize, overlapSize, overlapBottom, $"Saved/Save_Overlay_Bottom/bottom_{patchNumber}.png");
            DebugFunctions.showData_CustomizedWH(overlapSize, patchSize, overlapRight, $"Saved/Save_Overlay_Right/right_{patchNumber}.png");
            DebugFunctions.showData_CustomizedWH(patchSize - overlapSize, patchSize - overlapSize, toBePlacedPatch, $"Saved/To_Be_Placed_Patches/to_be_placed_{patchNumber}.png");


            //float possiblePatches = compareTopOverlays();


        }
        // otherwise, both left and bottom have to be compared with right and up respectively
        else
        {
            // get current bottomPatch
            float[] prevBottomPatch = previousRowBottomPatches[patchNumber % (patchesPerRow)];
            // function for comparing top overlays
            //float[][] possiblePatchesTop = compareLeftOverlays(prevBottomPatch, allOverlaps[0], global.patchData.totalNumPatches, allPatches);
            // function for  comparing left overlays
            //float[][] possiblePatchesLeft = compareLeftOverlays(prevRightPatch, allOverlaps[1], global.patchData.totalNumPatches, allPatches);


        }
        //Debug.Log("patch number = " +  patchNumber);
        //Debug.Log("actual patch number = " + allPatches.Length);
        return (toBePlacedPatch, ranPatch, overlapRight, overlapBottom);
    }

    // for comparing difference values of each patch left overlay with the previous patch right overlay 
    private float[][] compareOneOverlays(float[] previousOverlay, float[][] allOverlays, int totalPatches, float[][] allPatches)
    {
        int patchSize = global.patchData.patchSize;
        int overlapSize = global.patchData.overlapSize;

        // loop through each patch
        float[] distanceMetrics = compareOverlaysGPU(allOverlays, previousOverlay, patchSize, overlapSize, totalPatches);
        // calculate distance tolerance (dmax)
        int kernalID_max = differenceInOverlays.FindKernel("maxDifference");

        rightOverlapData = new ComputeBuffer(patchSize * overlapSize * 4, sizeof(float));
        rightOverlapData.SetData(previousOverlay);
        differenceInOverlays.SetBuffer(kernalID_max, "rightOverlayData", rightOverlapData);

        float[] maxDifferences = new float[patchSize * overlapSize * 4];
        maxDiffOverlapData = new ComputeBuffer(patchSize * overlapSize * 4, sizeof(float));
        differenceInOverlays.SetBuffer(kernalID_max, "maxOutputDifference", maxDiffOverlapData);
        differenceInOverlays.Dispatch(kernalID_max, Mathf.CeilToInt((float)(patchSize * overlapSize) / 64), 1, 1);
        maxDiffOverlapData.GetData(maxDifferences);

        float maxDifference = 0;

        for (int j = 0;  j < maxDifferences.Length; j++)
            maxDifference += maxDifferences[j];

        float maxTolerance = 0.2f * Mathf.Pow(((1f / patchSize * overlapSize) * maxDifference), 1f / 2);
        Debug.Log(":DF maxTolerance = " + maxTolerance);
        // store patches that fit the distance metric
        int numPossible = 0;
        for (int i = 0; i < distanceMetrics.Length; i++)
        {
            Debug.Log(":DF distanceMetrics = " + distanceMetrics[i]);
            if (distanceMetrics[i] < maxTolerance)
            {
                numPossible++;
            }
            
        }

        float[][] possiblePatches = new float[numPossible][];
        int increment = 0;
        for (int i = 0; i < distanceMetrics.Length; i++)
        {
            if (distanceMetrics[i] < maxTolerance)
            {
                possiblePatches[increment] = allPatches[increment];
                increment++;
            }
            
        }
        // if the set is empty (no patch meets the distance tolerance criteria)

        // return all possible patches that can be used
        return possiblePatches;
    }


    /*private float[][] compareTopOverlays(float[] previousBottomOverlay, float[][] allTopOverlays, int totalPatches, float[][] allPatches)
    {
        int patchSize = global.patchData.patchSize;
        int overlapSize = global.patchData.overlapSize;
        float[] distanceMetrics = new float[totalPatches];

        for (int i = 0; i < allTopOverlays.Length; i++)
        {

        }
    }*/

    private float[] compareOverlaysGPU(float[][] allLeftOverlays, float[] previousRightOverlay, int patchSize, int overlapSize, int totalPatches)
    {

        float[] distanceMetrics = new float[totalPatches];

        for (int i = 0; i < allLeftOverlays.Length; i++)
        {

            int kernalID = differenceInOverlays.FindKernel("diffLeftRight");
            // pass in pixel data of previous right overlap to the shader
            rightOverlapData = new ComputeBuffer(patchSize * overlapSize * 4, sizeof(float));
            rightOverlapData.SetData(previousRightOverlay);
            differenceInOverlays.SetBuffer(kernalID, "rightOverlayData", rightOverlapData);

            // pass in pixel data of each left overlay to the shader
            leftOverlapData = new ComputeBuffer(patchSize * overlapSize * 4, sizeof(float));
            leftOverlapData.SetData(allLeftOverlays[i]);
            differenceInOverlays.SetBuffer(kernalID, "leftOverlayData", leftOverlapData);

            // store the difference in an array
            // for distance metric
            float[] pixelDifferences = new float[patchSize * overlapSize * 4];
            differenceOverlapData = new ComputeBuffer(patchSize * overlapSize * 4, sizeof(float));
            differenceInOverlays.SetBuffer(kernalID, "outputDifference", differenceOverlapData);



            differenceInOverlays.Dispatch(kernalID, Mathf.CeilToInt((float)(patchSize * overlapSize) / 64), 1, 1);
            differenceOverlapData.GetData(pixelDifferences);



            // now we have the difference in pixel values, we now apply Distance Matrics and the distance tolerance (dmax)
            // given by the paper 
            float sumDifference = 0;


            for (int j = 0; j < pixelDifferences.Length; j++)
                sumDifference += pixelDifferences[j];


            float distanceMetric = Mathf.Pow(((1f / patchSize * overlapSize) * sumDifference), 1f / 2);
            distanceMetrics[i] = distanceMetric;

        }
        return distanceMetrics;
    }


    private float[] saveBottomOverlay(float[] chosenPatch)
    {
        int patchSize = global.patchData.patchSize;
        int overlapSize = global.patchData.overlapSize;

        int kernalID = extractOverlays.FindKernel("extractBottOverlays");

        // use data in the patch array for the shader
        inputPatchData = new ComputeBuffer(chosenPatch.Length, sizeof(float));
        inputPatchData.SetData(chosenPatch);
        // set the patch data that will be inputted into GPU processing
        extractOverlays.SetBuffer(kernalID, "patchData", inputPatchData);

        // data for extracting overlap regions Top
        extractOverlays.SetInt("overlapSize", overlapSize);
        extractOverlays.SetInt("patchSize", patchSize);
        extractOverlays.SetInt("imgHeight", global.refImgData.refImgHeight);


        outputOverlayData = new ComputeBuffer(patchSize * overlapSize, sizeof(float) * 4);
        extractOverlays.SetBuffer(kernalID, "bottomOverlayOutput", outputOverlayData);

        extractOverlays.Dispatch(kernalID, Mathf.CeilToInt((float)patchSize / 8), Mathf.CeilToInt((float)patchSize / 8), 1);
        float[] bottomOverlayCurrPatch = new float[patchSize * overlapSize * 4];
        // store bottom overlap pixel values into the array
        outputOverlayData.GetData(bottomOverlayCurrPatch);

        return bottomOverlayCurrPatch;
    }

    private float[] saveRightOverlay(float[] chosenPatch)
    {
        int patchSize = global.patchData.patchSize;
        int overlapSize = global.patchData.overlapSize;

        int kernalID = extractOverlays.FindKernel("extractRightOverlays");

        // use data in the patch array for the shader
        inputPatchData = new ComputeBuffer(chosenPatch.Length, sizeof(float));
        inputPatchData.SetData(chosenPatch);
        // set the patch data that will be inputted into GPU processing
        extractOverlays.SetBuffer(kernalID, "patchData", inputPatchData);

        // data for extracting overlap regions Top
        extractOverlays.SetInt("overlapSize", overlapSize);
        extractOverlays.SetInt("patchSize", patchSize);
        extractOverlays.SetInt("imgHeight", global.refImgData.refImgHeight);


        outputOverlayData = new ComputeBuffer(patchSize * overlapSize, sizeof(float) * 4);
        extractOverlays.SetBuffer(kernalID, "rightOverlayOutput", outputOverlayData);

        extractOverlays.Dispatch(kernalID, Mathf.CeilToInt((float)patchSize / 8), Mathf.CeilToInt((float)patchSize / 8), 1);
        float[] bottomOverlayCurrPatch = new float[patchSize * overlapSize * 4];
        // store bottom overlap pixel values into the array
        outputOverlayData.GetData(bottomOverlayCurrPatch);

        return bottomOverlayCurrPatch;
    }

    private float[] getPatchWithoutOverlap(float[] chosenPatch)
    {
        int patchSize = global.patchData.patchSize;
        int overlapSize = global.patchData.overlapSize;

        int kernalID = getWithoutOverlays.FindKernel("GetPatchWithoutOverlap");

        // use data in the patch array for the shader
        inputPatchData = new ComputeBuffer(chosenPatch.Length, sizeof(float));
        inputPatchData.SetData(chosenPatch);
        // set the patch data that will be inputted into GPU processing
        getWithoutOverlays.SetBuffer(kernalID, "patchData", inputPatchData);

        getWithoutOverlays.SetInt("overlapSize", overlapSize);
        getWithoutOverlays.SetInt("patchSize", patchSize);


        outputPatchData = new ComputeBuffer((patchSize - overlapSize) * (patchSize - overlapSize), sizeof(float) * 4);
        getWithoutOverlays.SetBuffer(kernalID, "withoutOverlaps", outputPatchData);

        //debugging
        ComputeBuffer debug = new ComputeBuffer((patchSize - overlapSize) * (patchSize - overlapSize), sizeof(int));
        getWithoutOverlays.SetBuffer(kernalID, "x_or_y_values", debug);
        int[] debugValues = new int[(patchSize - overlapSize) * (patchSize - overlapSize)];

        getWithoutOverlays.Dispatch(kernalID, Mathf.CeilToInt((float)patchSize / 8), Mathf.CeilToInt((float)patchSize / 8), 1);
        
        float[] patch = new float[(patchSize - overlapSize) * (patchSize - overlapSize) * 4];
        // store bottom overlap pixel values into the array
        outputPatchData.GetData(patch);
        debug.GetData(debugValues);


        /*for (int i = 0; i < debugValues.Length; i ++)
        {
            Debug.Log($"debugValues[{i}] = {debugValues[i]}");
        }
        int countPixel = 0;
        for (int i = 0; i < patch.Length; i += 4)
        {
            Debug.Log($"no overlay pixel {countPixel}: R = {patch[i + 0]}, G = {patch[i + 1]}, B = {patch[i + 2]}, A = {patch[i + 3]}");
            Debug.Log($"chosen patch pixel {countPixel}: R = {chosenPatch[i + 0]}, G = {chosenPatch[i + 1]}, B = {chosenPatch[i + 2]}, A = {chosenPatch[i + 3]}");
            countPixel++;
        }*/

        return patch;

    }

    // int imageSize = patchSize - overlay;
    
}
