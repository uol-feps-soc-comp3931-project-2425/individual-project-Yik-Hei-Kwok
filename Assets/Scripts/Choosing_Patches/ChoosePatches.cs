using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Rendering;
using System.Linq;
using System;
using Supercluster.KDTree;
using UnityEngine.UIElements;


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

    // compute buffer for storing all pixel values of a row of patches to be placed
    private ComputeBuffer inputPixelData;
    // compute buffer for storing pixel values of row of patches in the correct sequence
    private ComputeBuffer outputPixelData;

    [Header("For extracting overlays of each patch")]
    public ComputeShader extractOverlays;

    [Header("For getting patch data without the overlapping regions")]
    public ComputeShader getWithoutOverlays;

    [Header("For obtaining difference of pixel values between left and right / bottom and top overlays")]
    public ComputeShader differenceInOverlays;

    [Header("For combining patches into a single image")]
    public ComputeShader combinePatches;

    // contains lot of operations which involves in choosing which patch to process
    public ProcessPatches ProcessPatch;

    // lower if want more regular textures, higher if want more randomness
    private float lambda;

    public void startChoosePatches(float[][] allPatches, float[][][] allOverlaps, int resultImageSize, int patchSize, int overlapSize, string finalImageLocation, float lambdaValue)
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

        // set the  lamda value
        lambda = lambdaValue;

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

        // which patch of the row we are on
        int patchNumInRow = 0;

        // loop through all patches needed
        for (int i = 0;  i < totalPatchesNeeded; i++) {
            (currIterationPatchNum, previousRightPatch, previousRowBottomPatches[patchNumInRow]) = placeNewPatch(k, allPatches, allOverlaps, chosenPatches, patchesPerRow, previousRightPatch, previousRowBottomPatches);
            k += 1;
            patchNumInRow += 1;
            // reset patch count when we processed the final patch of the row
            if (patchNumInRow == patchesPerRow)
                patchNumInRow = 0;
        }

        // actual patch size (the size of patch when placed into the result)
        int truePatchSize = patchSize - overlapSize;

        // calculate how many rows of patches make up the image
        int numOfRows = totalPatchesNeeded / patchesPerRow;

        // after choosing all the patches, combine the patches into one final image
        float[][] finalImage = new float[numOfRows][];

        // denote which row we are on
        int processingRow = 0;

        // loop through each row of patches
        for (int i = 0; i < numOfRows; i++)
        {
            
            // store all the pixels of the current row of patches
            float[] allPixelsInPatchRow = new float[patchesPerRow * truePatchSize * truePatchSize * 4];
            // for each patch in the row 
            for (int j = 0; j < patchesPerRow; j++)
            {
                chosenPatches[j + processingRow * patchesPerRow].CopyTo(allPixelsInPatchRow, j * truePatchSize * truePatchSize * 4);

            }

            // initialize array to prepare store row pixel values in correct sequence
            finalImage[processingRow] = new float[resultImageSize * truePatchSize * 4];

            // pass a whole row of patch pixel data into the compute buffer
            int kernalID = combinePatches.FindKernel("CombinePatches");

            inputPixelData = new ComputeBuffer(patchesPerRow * truePatchSize * truePatchSize * 4, sizeof(float));
            inputPixelData.SetData(allPixelsInPatchRow);
            combinePatches.SetBuffer(kernalID, "patchData", inputPixelData);

            combinePatches.SetInt("patchSize", truePatchSize);
            combinePatches.SetInt("numPatchesPerRow", patchesPerRow);
            combinePatches.SetInt("lengthImage", resultImageSize);

            // store pixel values of each row of patches in correct order to the final iamge
            float[] pixelsInPatchRow = new float[resultImageSize * truePatchSize * 4];
            outputPixelData = new ComputeBuffer(resultImageSize * truePatchSize * 4, sizeof(float));
            combinePatches.SetBuffer(kernalID, "outputRowOfPatchPixels", outputPixelData);

            combinePatches.Dispatch(kernalID, Mathf.CeilToInt((float)(patchesPerRow * truePatchSize * truePatchSize * 4) / 256), 1, 1);

            outputPixelData.GetData(finalImage[processingRow]);

            processingRow++;
        }
        for (int j = 0; j < finalImage.Length; j++)
        {
            DebugFunctions.showData_CustomizedWH(resultImageSize, truePatchSize, finalImage[j], $"Saved/Final_Image_Per_Row/patch_row_{j}.png");
        }

        string fileName = finalImageLocation.Split('/')[1];
        createFinalImage(resultImageSize, finalImage, numOfRows, truePatchSize , fileName, finalImageLocation);


    }

    // create final image which is a combination of all patches
    private void createFinalImage (int resultImageSize, float[][] finalImage, int numOfRows, int patchSize, string fileName, string checkLocation)
    {
        // create new texture 
        Texture2D texture = new Texture2D(resultImageSize, resultImageSize, TextureFormat.RGBA32, false);
        // store all pixel colours of the final image
        Color[] allPixelRGBAs = new Color[resultImageSize * resultImageSize];
        // process the pixels per row of patches
        
        int pixelIndent = 0;
        int totalPixelsAccumulated = 0;
        for (int i = numOfRows - 1; i >= 0; i--)
        {
            for(int j = 0; j < finalImage[i].Length; j+=4)
            {
                
                totalPixelsAccumulated++;
                if (totalPixelsAccumulated >= resultImageSize * resultImageSize)
                    break;

                // create the colour
                float R = finalImage[i][j];
                float G = finalImage[i][j + 1];
                float B = finalImage[i][j + 2];
                float A = finalImage[i][j + 3];

                allPixelRGBAs[pixelIndent] = new Color(R, G, B, A);
                pixelIndent++;

            }
        }
        // Apply colors to the texture
        texture.SetPixels(allPixelRGBAs);
        texture.Apply();

        // encode and save the texture
        byte[] bytes = texture.EncodeToPNG();

        string checkPath = checkLocation.Split('/')[0];
        // check if the path exist, if not, create the path
        bool exists = System.IO.Directory.Exists($"Assets/Saved/Final_Image/{checkPath}");
        if (!exists)
            System.IO.Directory.CreateDirectory($"Assets/Saved/Final_Image/{checkPath}");
            File.WriteAllBytes($"Assets/Saved/Final_Image/{checkPath}/{fileName}.png", bytes);
    }

    // for placing a new patch at a new position of the result image
    private (int, float[], float[]) placeNewPatch(int k, float[][] allPatches, float[][][] allOverlaps, float[][] chosenPatches, int patchesPerRow, float[] prevRightPatch, float[][] previousRowBottomPatches)
    {
        int patchSize = global.patchData.patchSize;
        int overlapSize = global.patchData.overlapSize;

        // for storing the chosen patch at this iteration
        int chosenPatchNum;
        // get the right and bottom overlay of the extracted patch, will be used for comparing with next patch
        float[] chosenRightOverlay;
        float[] chosenBottomOverlay;

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
        ranPatch = 7;
        Debug.Log(":DF ranPatch = " + ranPatch);
        float[] chosenPatch = allPatches[ranPatch];

        // for storing the current patch's bottom and right overlaps
        float[] overlapBottom;
        float[] overlapRight;
        float[] toBePlacedPatch;

        // save right and bottom overlays of the chosen patch
        overlapBottom = ProcessPatch.saveBottomOverlay(chosenPatch);
        overlapRight = ProcessPatch.saveRightOverlay(chosenPatch);

       
        

        // need to get the patch pixel data without the overlap areas (this will be the data to be placed in the image)
        toBePlacedPatch = getPatchWithoutOverlap(chosenPatch);

        // for debug only
        DebugFunctions.showData_CustomizedWH(patchSize-overlapSize, overlapSize, overlapBottom, $"Saved/Save_Overlay_Bottom/bottom_{k}.png");
        DebugFunctions.showData_CustomizedWH(overlapSize, patchSize - overlapSize, overlapRight, $"Saved/Save_Overlay_Right/right_{k}.png");
        DebugFunctions.showData_CustomizedWH(patchSize - overlapSize, patchSize - overlapSize, toBePlacedPatch, $"Saved/To_Be_Placed_Patches/to_be_placed_{k}.png");
        
        return (toBePlacedPatch, ranPatch, overlapRight, overlapBottom);
    }

    private (float[],int, float[], float[]) placeNextPatch(float[][] allPatches, float[][][] allOverlaps, int patchNumber, int patchesPerRow, float[] prevRightPatch, float[][] previousRowBottomPatches)
    {
        int patchSize = global.patchData.patchSize;
        int overlapSize = global.patchData.overlapSize;

        bool useKD = false;
        // to be returned chosen patch
        float[] toBePlacedPatch;
        int ranPatch = 0;
        // for storing the current patch's bottom and right overlaps
        float[] overlapBottom;
        float[] overlapRight;

        // if we are still at the first row of the result image, we don't need to compare overlap top region
        if (patchNumber < patchesPerRow)
        {
            float[][] possiblePatches;
            // function for comparing left overlays
            if (useKD == false)
                possiblePatches = compareOneOverlays(prevRightPatch, allOverlaps[1], global.patchData.totalNumPatches, allPatches);
            else
                possiblePatches = compareOverlaysKD(prevRightPatch, allOverlaps[1], allPatches);

            // choose a random patch from the list
            System.Random ran = new System.Random();
            ranPatch = ran.Next(possiblePatches.Length);
            float[] chosenPatch = possiblePatches[ranPatch];
           

            // save right and bottom overlays of the chosen patch
            overlapBottom = ProcessPatch.saveBottomOverlay(chosenPatch);
            overlapRight = ProcessPatch.saveRightOverlay(chosenPatch);

            // need to get the patch pixel data without the overlap areas (this will be the data to be placed in the image)
            toBePlacedPatch = getPatchWithoutOverlap(chosenPatch);

            DebugFunctions.showData_CustomizedWH(patchSize - overlapSize, overlapSize, overlapBottom, $"Saved/Save_Overlay_Bottom/bottom_{patchNumber}.png");
            DebugFunctions.showData_CustomizedWH(overlapSize, patchSize - overlapSize, overlapRight, $"Saved/Save_Overlay_Right/right_{patchNumber}.png");
            DebugFunctions.showData_CustomizedWH(patchSize - overlapSize, patchSize - overlapSize, toBePlacedPatch, $"Saved/To_Be_Placed_Patches/to_be_placed_{patchNumber}.png");
            
        }
        // if we are at the first patch of the row, we don't need to compare left overlays
        else if (patchNumber % (patchesPerRow) == 0)
        {
            // get the previous bottom overlay (since we are choosing patch for start of row, always compare with patch chosen at start of previous row)
            float[] prevBottomPatch = previousRowBottomPatches[0];
            float[][] possiblePatches;
            // function for comparing top overlays
            if (useKD == false)
                possiblePatches = compareOneOverlays(prevBottomPatch, allOverlaps[0], global.patchData.totalNumPatches, allPatches);
            else
                possiblePatches = compareOverlaysKD(prevBottomPatch, allOverlaps[0], allPatches);

            System.Random ran = new System.Random();
            ranPatch = ran.Next(possiblePatches.Length);
            float[] chosenPatch = possiblePatches[ranPatch];

            // save right and bottom overlays of the chosen patch
            overlapBottom = ProcessPatch.saveBottomOverlay(chosenPatch);
            overlapRight = ProcessPatch.saveRightOverlay(chosenPatch);

            // need to get the patch pixel data without the overlap areas (this will be the data to be placed in the image)
            toBePlacedPatch = getPatchWithoutOverlap(chosenPatch);

            DebugFunctions.showData_CustomizedWH(patchSize - overlapSize, overlapSize, overlapBottom, $"Saved/Save_Overlay_Bottom/bottom_{patchNumber}.png");
            DebugFunctions.showData_CustomizedWH(overlapSize, patchSize - overlapSize, overlapRight, $"Saved/Save_Overlay_Right/right_{patchNumber}.png");
            DebugFunctions.showData_CustomizedWH(patchSize - overlapSize, patchSize - overlapSize, toBePlacedPatch, $"Saved/To_Be_Placed_Patches/to_be_placed_{patchNumber}.png");


            //float possiblePatches = compareTopOverlays();


        }
        // otherwise, both left and bottom have to be compared with right and up respectively
        else
        {
            // get current bottomPatch
            float[] prevBottomPatch = previousRowBottomPatches[patchNumber % (patchesPerRow)];
            float[][] possiblePatches;
            if (useKD == false)
                possiblePatches = compareBothOverlays(prevRightPatch, prevBottomPatch, allOverlaps[0], allOverlaps[1], global.patchData.totalNumPatches, allPatches);
            else
                possiblePatches = compareBothOverlaysKD(prevRightPatch, prevBottomPatch, allOverlaps[1], allOverlaps[0], allPatches);


            System.Random ran = new System.Random();
            ranPatch = ran.Next(possiblePatches.Length);
            float[] chosenPatch = possiblePatches[ranPatch];
            //Debug.Log(":DF possiblePatches.Length = " + possiblePatches.Length);
            //Debug.Log(":DF ranPatch = " + ranPatch);
            //Debug.Log(":DF possiblePatches[ranPatch] = " + possiblePatches[ranPatch]);
            // save right and bottom overlays of the chosen patch
            overlapBottom = ProcessPatch.saveBottomOverlay(chosenPatch);
            overlapRight = ProcessPatch.saveRightOverlay(chosenPatch);

            // need to get the patch pixel data without the overlap areas (this will be the data to be placed in the image)
            toBePlacedPatch = getPatchWithoutOverlap(chosenPatch);

            DebugFunctions.showData_CustomizedWH(patchSize - overlapSize, overlapSize, overlapBottom, $"Saved/Save_Overlay_Bottom/bottom_{patchNumber}.png");
            DebugFunctions.showData_CustomizedWH(overlapSize, patchSize - overlapSize, overlapRight, $"Saved/Save_Overlay_Right/right_{patchNumber}.png");
            DebugFunctions.showData_CustomizedWH(patchSize - overlapSize, patchSize - overlapSize, toBePlacedPatch, $"Saved/To_Be_Placed_Patches/to_be_placed_{patchNumber}.png");

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
        float[] distanceMetrics = ProcessPatch.compareOverlaysGPU(allOverlays, previousOverlay, patchSize, overlapSize, totalPatches);
        
        // calculate distance tolerance (dmax)
        float[] maxDifferences = ProcessPatch.computeMaxTolerance(previousOverlay,patchSize, overlapSize);

        // filter out the patches which overlap distance doesn't exceed max tolerance
        float[][] possiblePatches = ProcessPatch.filterPatchMaxToleranceOne(maxDifferences, distanceMetrics,patchSize,overlapSize,allPatches, lambda);


        return possiblePatches;
    }

    private float[][] compareBothOverlays(float[] previousRightOverlay, float[] previousBottOverlay, float[][] overlaysTop, float[][] overlaysLeft , int totalPatches, float[][] allPatches)
    {
        int patchSize = global.patchData.patchSize;
        int overlapSize = global.patchData.overlapSize;

        // calculate distance metrics of each patch based on left right comparison
        float[] distanceMetricsLeft = ProcessPatch.compareOverlaysGPU(overlaysLeft, previousRightOverlay, patchSize, overlapSize, totalPatches);
        // calculate distance tolerance (dmax) for right overlays
        float[] maxDifferencesLeft = ProcessPatch.computeMaxTolerance(previousRightOverlay, patchSize, overlapSize);


        // calculate distance metrics of each patch based on top bottom comparison
        float[] distanceMetricsTop = ProcessPatch.compareOverlaysGPU(overlaysTop, previousBottOverlay, patchSize, overlapSize, totalPatches);
        // calculate distance tolerance (dmax) for bottom overlays
        float[] maxDifferencesTop = ProcessPatch.computeMaxTolerance(previousBottOverlay, patchSize, overlapSize);

        float[][] possiblePatches = ProcessPatch.filterPatchMaxToleranceBoth(maxDifferencesLeft,maxDifferencesTop,distanceMetricsLeft,distanceMetricsTop,patchSize,overlapSize,allPatches, lambda);

        return possiblePatches;
    }

    

    private float[][] compareOverlaysKD(float[] targetPatchOverlay, float[][] allOverlays, float[][] allPatches)
    {
        // construct the KD Tree
        (var tree, double[][] allFlattened) = createTree(allOverlays);
        // flatten the values in the target patch
        double[] flattenedTarget = flattenArray(targetPatchOverlay);
        double sumTarget = 0;
        for (int j = 0; j < flattenedTarget.Length; j++)
            sumTarget += flattenedTarget[j];
        double fA_Target = sumTarget / flattenedTarget.Length;

        //allFlattened[i] = flattened;
        double[] targetFlattenedAverage = new double[1];
        targetFlattenedAverage[0] = fA_Target;
        /*Debug.Log($"DDPRF Target Patch average value = {targetFlattenedAverage[0]}");

        Debug.Log(":P flattenedTarget.Length = " + flattenedTarget.Length);
        Debug.Log(":P allFlattened[0].Length = " + allFlattened[0].Length);*/

        // choose the 10 nearest patches if possible
        if (allFlattened.Length > 4)
        {
            var test = tree.NearestNeighbors(targetFlattenedAverage, 4);
            int patchSize = global.patchData.patchSize;
            int overlapSize = global.patchData.overlapSize;

            // store each chosen overlap into an array
            float[][] filteredOverlaps = new float[4][];
            int inc = 0;
            for (int i = 0; i < filteredOverlaps.Length; i++)
            {
                int filteredIndex;
                int.TryParse(test[inc].Item2, out filteredIndex);
                filteredOverlaps[i] = allOverlays[filteredIndex];

                inc++;
            }

            // loop through each filtered patch
            // calculate distance metrics of each filtered patch
            float[] distanceMetrics = ProcessPatch.compareOverlaysGPU(filteredOverlaps, targetPatchOverlay, patchSize, overlapSize, filteredOverlaps.Length);
            // calculate max difference for max tolerance calculation
            float[] maxDifferences = ProcessPatch.computeMaxTolerance(targetPatchOverlay, patchSize, overlapSize);

            float[][] possiblePatches = ProcessPatch.filterPatchMaxToleranceOne(maxDifferences, distanceMetrics, patchSize, overlapSize, allPatches, lambda);

            return possiblePatches;

        }
        else
        {
            // just call the normal compare overlap function
            float[][] possiblePatches = compareOneOverlays(targetPatchOverlay, allOverlays, global.patchData.totalNumPatches, allPatches);
            return possiblePatches;

        }
    }

    private float[][] compareBothOverlaysKD(float[] targetPatchRightOverlay, float[] targetPatchBottomOverlay, float[][] allOverlaysLeft, float[][] allOverlaysTop, float[][] allPatches)
    {
        // combine the top and left overlays of each patch into one array
        float[][] allOverlays = new float[allOverlaysLeft.Length][];
        for(int i = 0; i < allOverlaysLeft.Length; i++)
        {
            allOverlays[i] = new float[allOverlaysLeft[i].Length * 2];
            float[] combine = allOverlaysLeft[i].Concat(allOverlaysTop[i]).ToArray();
            allOverlays[i] = combine;
        }
        // construct the KD Tree
        (var tree, double[][] allFlattened) = createTree(allOverlays);

        // flatten the values in the target patch
        float[] targetCombinedOverlay = targetPatchRightOverlay.Concat(targetPatchBottomOverlay).ToArray();
        double[] flattenedTarget = flattenArray(targetCombinedOverlay);
        double sumTarget = 0;
        for (int j = 0; j < flattenedTarget.Length; j++)
            sumTarget += flattenedTarget[j];
        double fA_Target = sumTarget / flattenedTarget.Length;

        //allFlattened[i] = flattened;
        double[] targetFlattenedAverage = new double[1];
        targetFlattenedAverage[0] = fA_Target;
        

        // choose the 10 nearest patches if possible
        if (allFlattened.Length > 4)
        {
            var test = tree.NearestNeighbors(targetFlattenedAverage, 4);
            int patchSize = global.patchData.patchSize;
            int overlapSize = global.patchData.overlapSize;

            // store each chosen overlap into an array
            float[][] filteredOverlapsLeft = new float[4][];
            float[][] filteredOverlapsTop = new float[4][];
            int inc = 0;
            for (int i = 0; i < filteredOverlapsLeft.Length; i++)
            {
                int filteredIndex;
                int.TryParse(test[inc].Item2, out filteredIndex);
                filteredOverlapsLeft[i] = allOverlaysLeft[filteredIndex];
                filteredOverlapsTop[i] = allOverlaysTop[filteredIndex];
                inc++;


            }

            // loop through each filtered patch
            // calculate distance metrics of each patch based on left right comparison
            float[] distanceMetricsLeft = ProcessPatch.compareOverlaysGPU(filteredOverlapsLeft, targetPatchRightOverlay, patchSize, overlapSize, filteredOverlapsLeft.Length);
            // calculate distance tolerance (dmax) for right overlays
            float[] maxDifferencesLeft = ProcessPatch.computeMaxTolerance(targetPatchRightOverlay, patchSize, overlapSize);

            // calculate distance metrics of each filtered patch
            float[] distanceMetricsTop = ProcessPatch.compareOverlaysGPU(filteredOverlapsTop, targetPatchBottomOverlay, patchSize, overlapSize, filteredOverlapsTop.Length);
            // calculate max difference for max tolerance calculation
            float[] maxDifferencesTop = ProcessPatch.computeMaxTolerance(targetPatchBottomOverlay, patchSize, overlapSize);

            float[][] possiblePatches = ProcessPatch.filterPatchMaxToleranceBoth(maxDifferencesLeft, maxDifferencesTop, distanceMetricsLeft,distanceMetricsTop,patchSize,overlapSize,allPatches, lambda);

            //float[][] possiblePatches = filterPatchMaxToleranceOne(maxDifferences, distanceMetrics, patchSize, overlapSize, allPatches);

            return possiblePatches;

        }
        else
        {
            // just call the normal compare overlap function
            float[][] possiblePatches = compareBothOverlays(targetPatchRightOverlay, targetPatchBottomOverlay, allOverlaysTop, allOverlaysLeft, global.patchData.totalNumPatches, allPatches);
            //float[][] possiblePatches = compareOneOverlays(targetPatchOverlay, allOverlays, global.patchData.totalNumPatches, allPatches);
            return possiblePatches;

        }
    }


    private (KDTree<double, string>,double[][]) createTree(float[][] allOverlays)
    {
        //Debug.Log("Size of allOverlays = " + allOverlays[0].Length);
        // store all flattened overlap values
        double[][] allFlattened = new double[allOverlays.Length][];
        // compare each 4 elements (RGBA values) of Left/Top overlays into 1 element by averaging
        for (int i = 0; i < allOverlays.Length; i++)
        {
            // do this each patch overlay
            double[] flattened = flattenArray(allOverlays[i]);
            //get average of the averages
            double sum = 0;
            for (int j = 0; j < flattened.Length; j++)
                sum += flattened[j];

            double fA_Compare = sum / flattened.Length;

            //allFlattened[i] = flattened;
            allFlattened[i] = new double[1];
            allFlattened[i][0] = fA_Compare;

        }
        // debug the averaged value of each patch
        /*for (int i = 0; i < allFlattened.Length; i++)
        {
            Debug.Log($"DDPRF Patch {i} average value = {allFlattened[i][0]}");
        }*/

        // allFlattened.Select(p => p.ToString()).ToArray();
        // name each node
        string[] treeNodes = new string[allFlattened.Length];
        for (int i = 0; i < allFlattened.Length; i++)
            treeNodes[i] = i.ToString();
        // create the KD Tree
        var tree = new KDTree<double, string>(1, points: allFlattened, nodes: treeNodes, metric: averageDistance);

        return (tree, allFlattened);
    }

    private double[] flattenArray(float[] overlayArray)
    {
        float[] flattened = new float[overlayArray.Length / 4];

        // Flatten the array of each overlay patch
        int kernalID = differenceInOverlays.FindKernel("flattenValues");

        ComputeBuffer overlapData = new ComputeBuffer(overlayArray.Length, sizeof(float));
        overlapData.SetData(overlayArray);
        differenceInOverlays.SetBuffer(kernalID, "overlayData", overlapData);

        ComputeBuffer flattenedBuffer = new ComputeBuffer(overlayArray.Length / 4, sizeof(float));
        differenceInOverlays.SetBuffer(kernalID, "flattenedData", flattenedBuffer);
        differenceInOverlays.Dispatch(kernalID, Mathf.CeilToInt((float)(overlayArray.Length / 4) / 64), 1, 1);
        flattenedBuffer.GetData(flattened);

        double[] dFlattened = flattened.Select(f => (double)f).ToArray();

        /*for (int j = 0; j < dFlattened.Length; j++)
        {
            Debug.Log($"Pixel {j} dFlattened average = " + dFlattened[j]);
        }*/

        return dFlattened;
    }

    // calculate the eculidean distance of flattened overlap arrays (metric for kd tree)
    private double averageDistance(double[] overlay1, double[] overlay2)
    {
        int distance = 0;
        for (int i = 0; i < overlay1.Length; i++)
        {
            distance += (int)((overlay1[i] - overlay2[i]) * (overlay1[i] - overlay2[i]));
        }

        return distance;
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
