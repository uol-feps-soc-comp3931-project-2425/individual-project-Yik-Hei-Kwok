using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoosePatches : MonoBehaviour
{
    // compute buffer for passing in array data of patches into shader
    private ComputeBuffer inputPatchData;
    // compute buffer for storing overlay data
    private ComputeBuffer outputOverlayData;

    [Header("For extracting overlays of each patch")]
    public ComputeShader extractOverlays;




    public void startChoosePatches(float[][] allPatches, float[][][] allOverlaps, int resultImageSize, int patchSize, int overlapSize, float[][] chosenPatches)
    {
        // determine how many patches are needed to make up result image 
        // ( + 1 used for cases where result image not completely filled by)
        int totalPatchesNeeded = ((resultImageSize/patchSize) + 1) * 2;
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


        float[][] overlapBottoms = new float[1][];
        // save right and bottom overlays of the chosen patch
        overlapBottoms[0] = saveBottomOverlay(chosenPatch);

        Texturesynthesis.showData(overlapBottoms, patchSize, "Saved/Save_Overlay_Bottom");

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
}
