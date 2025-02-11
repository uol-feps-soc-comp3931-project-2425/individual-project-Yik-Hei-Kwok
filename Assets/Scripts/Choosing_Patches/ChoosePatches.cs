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

    [Header("For extracting overlays of each patch")]
    public ComputeShader extractOverlays;

    [Header("For getting patch data without the overlapping regions")]
    public ComputeShader getWithoutOverlays;



    public void startChoosePatches(float[][] allPatches, float[][][] allOverlaps, int resultImageSize, int patchSize, int overlapSize, float[][] chosenPatches)
    {
        // determine how many patches are needed to make up result image 
        // ( + 1 used for cases where result image not completely filled by)
        // need to subtract overlapSize from patchSize because its the actual size of patch which will be placed into result image 
        int totalPatchesNeeded = ((resultImageSize/(patchSize-overlapSize)) + 1) * 2;
        // how many patches per row in the result image
        int patchesPerRow = (resultImageSize / patchSize) + 1;

        // for placing patches onto a new canvas
        int k = 0;

        // loop through all patches needed
        for (int i = 0;  i < totalPatchesNeeded; i++) {
            for (int j = 0;  j < patchesPerRow; j++)
            {

            }
        }
    }

    // for placing a new patch at a new position of the result image
    private (int, int) placeNewPatch(int k, float[][] allPatches, float[][][] allOverlaps, float[][] chosenPatches, int patchesPerRow)
    {
        // for storing the chosen patch at this iteration
        int chosenPatch;
        // chosenPatches array denotes 
        // if k = 0, it means there isn't a single patch placed, place the first patch
        if (k == 0)
        {
            // place the first chosen patch into the chosenPatches array.
            (chosenPatches[0], chosenPatch) = placeFirstPatch(allPatches);
            k += 1;
        }
        // if k != 0, then a patch is already placed. Look at previous patch and compare
        // compare
        else
        {
            (chosenPatches[k], chosenPatch) = placeNextPatch(allPatches, allOverlaps, k, patchesPerRow);
            k += 1;

        }
        return (k, chosenPatch);
    }


    public (float[],int) placeFirstPatch(float[][] allPatches)
    {
        int patchSize = global.patchData.patchSize;
        int overlapSize = global.patchData.overlapSize;

        // choose a random patch as start patch 
        System.Random ran = new System.Random();
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
        getPatchWithoutOverlap(chosenPatch);

        // for debug only
        float[][] overlapBottoms = new float[1][];
        float[][] overlapRights = new float[1][];
        float[][] patchesWithoutOverlap = new float[1][];
        overlapBottoms[0] = saveBottomOverlay(chosenPatch);
        overlapRights[0] = saveRightOverlay(chosenPatch);
        patchesWithoutOverlap[0] = getPatchWithoutOverlap(chosenPatch);
        Texturesynthesis.showData(overlapBottoms, patchSize, "Saved/Save_Overlay_Bottom");
        Texturesynthesis.showData_left(overlapRights, patchSize, overlapSize, "Saved/Save_Overlay_Right");
        
        debugScript(patchSize, overlapSize, patchesWithoutOverlap[0], "Saved");
        return (chosenPatch,ranPatch);
    }

    private (float[],int) placeNextPatch(float[][] allPatches, float[][][] allOverlaps, int patchNumber, int patchesPerRow)
    {
        // if we are still at the first row of the result image, we don't need to compare overlap top region
        if (patchNumber < patchesPerRow)
        {
            // function for comparing left overlays
        }
        else
        {
            // function for comparing left overlays

            // function for comparing top overlays
        }
        return (allPatches[patchNumber], 0);
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


    private void debugScript(int patchSize, int overlay, float[] pixelData, string dir)
    {
        // Calculate image size
        int imageSize = patchSize - overlay;

        // Check if the pixelData array is correctly sized
        if (pixelData == null || pixelData.Length != imageSize * imageSize * 4)
        {
            Debug.LogError("Pixel data size does not match the expected image dimensions!");
            return;
        }

        // Create a new texture
        Texture2D texture = new Texture2D(imageSize, imageSize, TextureFormat.RGBA32, false);

        // Fill the texture with the 1D pixel data
        Color[] colors = new Color[imageSize * imageSize];
        for (int i = 0; i < colors.Length; i++)
        {
            // Extract RGBA values from the 1D array
            float r = pixelData[i * 4 + 0];
            float g = pixelData[i * 4 + 1];
            float b = pixelData[i * 4 + 2];
            float a = pixelData[i * 4 + 3];

            // Assign the color
            colors[i] = new Color(r, g, b, a);
        }

        // Apply colors to the texture
        texture.SetPixels(colors);
        texture.Apply();

        byte[] bytes = texture.EncodeToPNG();

        // delete directory contents before appending new patches to the folder
        //deleteDirContents("Assets/Save_Patches");

        string savePath = $"Assets/{dir}/withoutOverlays.png";



        File.WriteAllBytes(savePath, bytes);
    }
}
