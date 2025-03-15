using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

public class Texturesynthesis : MonoBehaviour
{
    [Header("For reading all pixels of a given reference image")]
    public ComputeShader readPixels;
    [Header("For storing all pixels of reference image into patches")]
    public ComputeShader segmentToPatches;
    [Header("For extracting overlays of each patch")]
    public ComputeShader extractOverlays;

    public RenderTexture renderTexture;

    [Header("Get the method for starting the patch choosing algorithm")]
    public ChoosePatches choosePatches;

    // compute buffer for reading pixels of reference image
    private ComputeBuffer outputBuffer;

    // compute buffer for passing in pixel inputs into shader
    private ComputeBuffer inputPixelData;
    // compute buffer for storing patches data
    private ComputeBuffer outputPatches;

    // compute buffer for passing in array data of patches into shader
    private ComputeBuffer inputPatchData;
    // compute buffer for storing overlay data
    private ComputeBuffer outputOverlayData;
   

    private int kernalID;

    // for storing all pixels of the reference image
    private float[] refPixelsRGB;

    // total size of the reference image
    private int sizeOfRefImage;

    // to be used in choosing patches to be used on new image
    float[][] patches;
    public float[][][] overlayPixels;

    // store all pixels that are in the referenced image
    private float[] allPixelDataRef;

    public void startSynthesis(Texture2D texture, int outputSize, int patchSize, int overlapSize, bool useGPU, bool useKD, string finalImageLocation, float lambdaValue)
    {
        // save data to struct for use in other scripts
        global.patchData.patchSize = patchSize;
        global.patchData.overlapSize = overlapSize;
        
        // -----------------------------------------------------------------------------------------------
        // for reading pixel data of reference image

        int colourSize = sizeof(float) * 4;
        Vector2 sizeOfRef = GetRefTextureProperties(texture);

        // first argument is number of elements in buffer, second argument is size of each element
        sizeOfRefImage = (int)(sizeOfRef.x * sizeOfRef.y);

        // save data to struct for use in other scripts
        global.refImgData.refImgSize = sizeOfRefImage;
        global.refImgData.refImgHeight = (int)sizeOfRef.y;
        global.refImgData.refImgWidth = (int)sizeOfRef.x;

        outputBuffer = new ComputeBuffer(sizeOfRefImage, colourSize);

        // Find the compute shader responsible for reading pixel data,
        // and set variables in the shader
        kernalID = readPixels.FindKernel("ReadPixel");
        readPixels.SetTexture(kernalID, "inputTexture", texture);
        readPixels.SetFloat("img_width", sizeOfRef.x);
        readPixels.SetFloat("img_height", sizeOfRef.y);
        readPixels.SetBuffer(kernalID, "outputBuffer", outputBuffer);
        // call function to activate Compute Shader and store pixel data of reference image in array
        allPixelDataRef = StoreRefPixels(texture);

        outputBuffer.Release();

        // save the image for showing purposes
        DebugFunctions.debugImage(allPixelDataRef, (int)sizeOfRef.x, (int)sizeOfRef.y);

        // -----------------------------------------------------------------------------------------------
        // for segementing all pixel data in the 1d array into patches of indicated size
        // choice of algorithm based on if user wants to use CPU or GPU
        if (useGPU)
        {
            // GPU
            var watch2 = System.Diagnostics.Stopwatch.StartNew();
            patches = SegmentPatches_GPU(allPixelDataRef, sizeOfRef, patchSize, texture);
            watch2.Stop();
            Debug.Log("Time: Segmenting Patches = " + watch2.ElapsedMilliseconds);
        }
        else
        {
            // CPU
            patches = SegmentPatches_CPU(allPixelDataRef, sizeOfRef, patchSize);
        }


        // save the patches as images for showing purposes
        //DebugFunctions.showData(patches, patchSize, "Saved/Save_Patches");

        //choosePatches.placeFirstPatch(patches, 0);
        // ----------------------------------------------------------------------------------------------
        // from each patch, extract top and left overlaps
        var watch = System.Diagnostics.Stopwatch.StartNew();
        overlayPixels = SegmentOverlays_GPU(patches, overlapSize, sizeOfRef, patchSize,overlapSize);
        watch.Stop();
        Debug.Log("Time: Segmenting Overlays = " + watch.ElapsedMilliseconds);

        // save the top overlap as images for showing purposes
        //DebugFunctions.showData(overlayPixels[0], patchSize-overlapSize, "Saved/Save_Overlay_Top");
        //DebugFunctions.showData_left(overlayPixels[1], patchSize-overlapSize, overlapSize , "Saved/Save_Overlay_Left");
        //DebugFunctions.showData_CustomizedWH(patchSize - overlapSize, overlapSize, overlayPixels[0][0], "Saved/Save_Overlay_Top/overlapPatch0.png");
        //DebugFunctions.showData_CustomizedWH(overlapSize, patchSize - overlapSize, overlayPixels[1][0], "Saved/Save_Overlay_Left/overlapPatch0.png");

        // after getting all information needed, call the choose patch function to start choosing patches
        choosePatches.startChoosePatches(patches, overlayPixels, outputSize, patchSize, overlapSize, finalImageLocation, lambdaValue, useKD);

    }
    private Vector2 GetRefTextureProperties(Texture2D ref_texture)
    {
        int numOfPixels_height = ref_texture.height;
        int numOfPixels_width = ref_texture.width;
        Vector2 arrayTextureProp = new Vector2(numOfPixels_width, numOfPixels_height);
        return arrayTextureProp;
    }

    // function that stores the pixels of the reference image into a format
    // [ [ [r,g,b],[r,g,b],[r,g,b] ...... [r,g,b] ] ]
    private float[] StoreRefPixels(Texture2D ref_texture)
    {
        // size of input image must be at least 8x8 or else thread group will be 0
        readPixels.Dispatch(kernalID, Mathf.CeilToInt(ref_texture.width / 8.0f), Mathf.CeilToInt(ref_texture.height / 8.0f), 1);
        // initiate an array that stores all pixel values
        refPixelsRGB = new float[4 * sizeOfRefImage];
        // get the pixel data of the reference image in RGBA format calculated in shader
        outputBuffer.GetData(refPixelsRGB);
      
        outputBuffer.Release();

        return refPixelsRGB;
    }


    // function for segementing the stored pixels into different patches in a format
    // [ [ patch 1 pixels ] , [ patch 2 pixels ] ....... ]
    private float[][] SegmentPatches_CPU(float[] allPixelDataRef, Vector2 sizeOfRef, int patchSize)
    {
        // R G B A values are items

        // number of items in a patch 
        int rowItemsPerPatch = patchSize * 4;
        int totalItemsPerPatch = patchSize * patchSize * 4;

        // get total number of derived patches 
        int rowPatches = Mathf.FloorToInt(sizeOfRef.x / patchSize);
        int colPatches = Mathf.FloorToInt(sizeOfRef.y / patchSize);
        int totalPatches = rowPatches * colPatches;

        // get total number of items in each row 
        int rowItemsTotal = rowPatches * rowItemsPerPatch;


        // initialize array for storing patches
        float[][] storePatches = new float[totalPatches][];
        for (int i = 0; i < totalPatches; i++)
        {
            storePatches[i] = new float[totalItemsPerPatch];
        }


        int currentPatch = 0; // indicate which patch we are working on
        int currentItem = 0; // indicate which item position of the patch is being dealt with
        int currentRow = 1; // indicate which row of the patch are we dealing with
        int startingPatch = 0; // for saving which patch is at the start of the set of horizontal patches
        int startingItem = 0; // for saving what position we are starting for each patch

        int itemInRow = 0; // track which item are we in, in the row

        int ignoreItemsWidth = (int)(sizeOfRef.x * 4) - (rowPatches * rowItemsPerPatch); // number of items in each row to ignore
        int ignoreItemsHeight = (int)(sizeOfRef.y) - (colPatches * patchSize); // number of items in each column to ignore
        int totalHeightIgnore = allPixelDataRef.Length - (int)(ignoreItemsHeight * sizeOfRef.x * 4);

        // segment the pixel data of the reference image into patches
        for (int i = 0; i < allPixelDataRef.Length; i++)
        {
            //Debug.Log("i = " + i);
            storePatches[currentPatch][currentItem] = allPixelDataRef[i];
            //Debug.Log("storePatches[currentPatch][currentItem] = " + storePatches[currentPatch][currentItem]);
            currentItem++;
            itemInRow++;
            // if we reached the end of a row in the patch
            if (currentItem == rowItemsPerPatch * currentRow)
            {
                // check if we are at the final item of the entire reference image row
                if (itemInRow == rowItemsTotal)
                {
                    // skip through the set amount of width pixels
                    i += ignoreItemsWidth;

                    // we check if we are at the final row of the patch
                    // if yes, then we start working on a new set of patches below current patch
                    if (currentRow == patchSize)
                    {
                        currentRow = 1;
                        currentItem = 0;
                        startingItem = 0;
                        // set starting patch as the patch at leftmost new row, and set it as current patch
                        startingPatch = currentPatch + 1;
                        currentPatch = startingPatch;
                        itemInRow = 0;
                    }
                    // we continue working on our current set of patches
                    else
                    {
                        currentRow++;
                        // we set the starting item
                        startingItem = currentItem;

                        // since we are not starting a new set of patches, go back to our current starting patch
                        currentPatch = startingPatch;
                        itemInRow = 0;
                    }
                }
                // we are only at the final item in the row of our current patch, not the entire reference image
                else
                {
                    currentItem = startingItem;
                    // move to the patch next to our current patch
                    currentPatch += 1;
                }

            }

            // we skip through the column items that we want to ignore
            if (i == totalHeightIgnore - 1)
                break;
        }
        //Debug.Log($"First patch first item CPU = {storePatches[0][0]} , {storePatches[0][1]} , {storePatches[0][2]} , {storePatches[0][3]}");
        //Debug.Log($"First patch second item CPU = {storePatches[0][4]} , {storePatches[0][5]} , {storePatches[0][6]} , {storePatches[0][7]}");
        //Debug.Log($"First patch third item CPU = {storePatches[0][8]} , {storePatches[0][9]} , {storePatches[0][10]} , {storePatches[0][11]}");
        //Debug.Log($"Last patch last item = {storePatches[3][9996]} , {storePatches[3][9997]} , {storePatches[3][9998]} , {storePatches[3][9999]}");

        return storePatches;
    }

    private float[][] SegmentPatches_GPU(float[] allPixelDataRef, Vector2 sizeOfRef, int patchSize, Texture2D texture)
    {
        // number of items in a patch 
        int rowItemsPerPatch = patchSize * 4;
        int totalItemsPerPatch = patchSize * patchSize * 4;

        // get total number of derived patches 
        int rowPatches = Mathf.FloorToInt(sizeOfRef.x / patchSize);
        int colPatches = Mathf.FloorToInt(sizeOfRef.y / patchSize);
        int totalPatches = rowPatches * colPatches;
        global.patchData.totalNumPatches = totalPatches;

        // get total number of items in each row 
        int rowItemsTotal = rowPatches * rowItemsPerPatch;

        int ignoreItemsWidth = (int)(sizeOfRef.x * 4) - (rowPatches * rowItemsPerPatch); // number of items in each row to ignore
        int ignoreItemsHeight = (int)(sizeOfRef.y) - (colPatches * patchSize); // number of items in each column to ignore
        int totalHeightIgnore = allPixelDataRef.Length - (int)(ignoreItemsHeight * sizeOfRef.x * 4);

       

        float[][] storePatches = new float[totalPatches][];
        // dispatch the compute shader for each single patch
        for (int y = 0; y < colPatches; y++)
        {
            for(int x = 0; x < rowPatches; x++)
            {

                // use data in the pixel array for the shader
                inputPixelData = new ComputeBuffer(allPixelDataRef.Length, sizeof(float));
                inputPixelData.SetData(allPixelDataRef);

                // compute colour size
                int colourSize = sizeof(float) * 4;

                // define output patches for storing segmented data
                outputPatches = new ComputeBuffer(totalItemsPerPatch, colourSize);
                // solely for debugging
                ComputeBuffer currentPatches = new ComputeBuffer(totalPatches * totalItemsPerPatch, sizeof(float) * 4);
                
                kernalID = segmentToPatches.FindKernel("SegmentPatches");

                // set the variables
                segmentToPatches.SetInt("imgHeight", (int)sizeOfRef.y);
                segmentToPatches.SetInt("imgWidth", (int)sizeOfRef.x);
                segmentToPatches.SetInt("heightIgnore", totalHeightIgnore);
                segmentToPatches.SetInt("widthIgnore", ignoreItemsWidth);
                segmentToPatches.SetInt("numRowPatches", rowPatches);
                segmentToPatches.SetInt("numColPatches", colPatches);
                segmentToPatches.SetInt("sizePatch", patchSize);

                // for pinpointing current patch to be processed
                segmentToPatches.SetInt("currentPatchVertical", y);
                segmentToPatches.SetInt("currentPatchHorizontal", x);


                // set the pixel data that will be inputted into GPU processing
                segmentToPatches.SetBuffer(kernalID, "pixelData", inputPixelData);

                // buffer which stores the output
                segmentToPatches.SetBuffer(kernalID, "outputPatches", outputPatches);

                segmentToPatches.Dispatch(kernalID, Mathf.CeilToInt((float)patchSize / 8), Mathf.CeilToInt((float)patchSize / 8), 1);

                float[] segmentedPatch = new float[patchSize * patchSize * 4];
                // get the pixel data of the reference image in RGBA format calculated in shader
                outputPatches.GetData(segmentedPatch);

                inputPixelData.Release();
                outputPatches.Release();

                // store the segment
                storePatches[rowPatches * y + x] = segmentedPatch;
            }
        }

        return storePatches;
    }

    

    


    


    private float[][][] SegmentOverlays_GPU(float[][] patches, int overlaySize, Vector2 sizeOfRef, int patchSize, int overlapSize)
    {

        // get total number of derived patches 
        int rowPatches = Mathf.FloorToInt(sizeOfRef.x / patchSize);
        int colPatches = Mathf.FloorToInt(sizeOfRef.y / patchSize);
        int totalPatches = rowPatches * colPatches;

        // first segment the top overlays
        int kernalID = extractOverlays.FindKernel("extractTopOverlays");

        float[][] storeTopOverlays = new float[totalPatches][];
        // call GPU on each patch to extract overlays
        for (int i = 0; i < patches.Length; i++ )
        {
            // use data in the patch array for the shader
            inputPatchData = new ComputeBuffer(patches[i].Length, sizeof(float));
            inputPatchData.SetData(patches[i]);
            // set the patch data that will be inputted into GPU processing
            extractOverlays.SetBuffer(kernalID, "patchData", inputPatchData);
            int truePatchSize = patchSize - overlapSize;
            // data for extracting overlap regions Top
            extractOverlays.SetInt("overlapSize", overlapSize);
            extractOverlays.SetInt("patchSize", patchSize);
            extractOverlays.SetInt("truePatchSize", truePatchSize);
            extractOverlays.SetInt("imgHeight", (int)sizeOfRef.y);

            // set output buffer for overlay of patches (each storeTopOverlays[i] will represent all top overlay pixels extracted in the ith row
            outputOverlayData = new ComputeBuffer(truePatchSize * overlaySize, sizeof(float) * 4);
            extractOverlays.SetBuffer(kernalID, "topOverlayOutput", outputOverlayData);
            
            // for debugging
            ComputeBuffer condition_debug = new ComputeBuffer(patches[i].Length, sizeof(int));
            extractOverlays.SetBuffer(kernalID, "condition_debug", condition_debug);

            extractOverlays.Dispatch(kernalID, Mathf.CeilToInt((float)patchSize / 8), Mathf.CeilToInt((float)patchSize / 8), 1);
            float[] topOverlayOfAPatch = new float[truePatchSize * overlapSize * 4];
            outputOverlayData.GetData(topOverlayOfAPatch);

            storeTopOverlays[i] = topOverlayOfAPatch;

            // for debugging
            int[] debug = new int[patchSize * patchSize];
            condition_debug.GetData(debug);

            inputPatchData.Release();
            outputOverlayData.Release();
        }
        // --------------------------------------------------------------------------------------------------------------------------------------------
        // now deal with left overlays

        kernalID = extractOverlays.FindKernel("extractLeftOverlays");

        float[][] storeLeftOverlays = new float[totalPatches][];

        for (int i = 0; i < patches.Length; i++)
        {
            // use data in the patch array for the shader
            inputPatchData = new ComputeBuffer(patches[i].Length, sizeof(float));
            inputPatchData.SetData(patches[i]);
            // set the patch data that will be inputted into GPU processing
            extractOverlays.SetBuffer(kernalID, "patchData", inputPatchData);

            int truePatchSize = patchSize - overlapSize;
            extractOverlays.SetInt("overlapSize", overlapSize);
            extractOverlays.SetInt("patchSize", patchSize);
            extractOverlays.SetInt("truePatchSize", truePatchSize);

            // set output buffer for overlay of patches (each storeLeftOverlays[i] will represent all left overlay pixels extracted in the ith row
            outputOverlayData = new ComputeBuffer(truePatchSize * overlaySize, sizeof(float) * 4);
            extractOverlays.SetBuffer(kernalID, "leftOverlayOutput", outputOverlayData);

            // for debugging
            ComputeBuffer condition_debug = new ComputeBuffer(patches[i].Length, sizeof(int));
            extractOverlays.SetBuffer(kernalID, "condition_debug", condition_debug);

            extractOverlays.Dispatch(kernalID, Mathf.CeilToInt((float)patchSize / 8), Mathf.CeilToInt((float)patchSize / 8), 1);
            float[] LeftOverlayOfAPatch = new float[truePatchSize * overlapSize * 4];
            outputOverlayData.GetData(LeftOverlayOfAPatch);
            storeLeftOverlays[i] = LeftOverlayOfAPatch;

            // for debugging
            int[] debug = new int[patchSize * patchSize];
            condition_debug.GetData(debug);

            inputPatchData.Release();
            outputOverlayData.Release();
            condition_debug.Release();
        }

        // put it in a single array
        float[][][] topAndLeftOverlays = new float[2][][];
        topAndLeftOverlays[0] = storeTopOverlays; 
        topAndLeftOverlays[1] = storeLeftOverlays;

        return topAndLeftOverlays;
    }

    private void deleteDirContents(string Path)
    {
        if (Directory.Exists(Path))
        {
            Directory.Delete(Path, true);
            Directory.CreateDirectory(Path);
        }
    }

    

    // Update is called once per frame

    private void debugCPU_andGPU(float[][] patchesCPU, float[][] patchesGPU)
    {
        for (int i = 0; i < patchesCPU.Length; i++)
        {
            for (int j = 0; j < patchesCPU[i].Length; j+= 4)
            {
                //Debug.Log($"Pixel of GPU in Patch {i} = {patchesGPU[i * patchesCPU[i].Length + j + 0]} , {patchesGPU[i * patchesCPU[i].Length + j + 1]} , {patchesGPU[i * patchesCPU[i].Length + j + 2]} , {patchesGPU[i * patchesCPU[i].Length + j + 3]}");
                Debug.Log($"Pixel of CPU in Patch {i} = {patchesCPU[i][j + 0]} , {patchesCPU[i][j + 1]} , {patchesCPU[i][j + 2]} , {patchesCPU[i][j + 3]}");
                Debug.Log($"Pixel of GPU in Patch {i} = {patchesGPU[i][j + 0]} , {patchesGPU[i][j + 1]} , {patchesGPU[i][j + 2]} , {patchesGPU[i][j + 3]}");

                /*if (patchesCPU[i][j] != patchesGPU[i * patchesCPU[i].Length + j])
                {
                    Debug.Log($"Start Diverging at patch {i} in pixel number {(i * patchesCPU[i].Length + j)/4}");
                }*/
            }
        }
    }
    void Update()
    {
        //StoreRefPixels();
    }
}