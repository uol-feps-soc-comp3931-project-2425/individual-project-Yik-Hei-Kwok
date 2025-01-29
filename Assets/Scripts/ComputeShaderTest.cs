using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Texturesynthesis : MonoBehaviour
{
    
    public ComputeShader readPixels;

    public RenderTexture renderTexture;

    private ComputeBuffer outputBuffer;

    private int kernalID;

    // for storing all pixels of the reference image
    private float[] refPixelsRGB;

    // total size of the reference image
    private int sizeOfRefImage;


    public void startSynthesis(Texture2D texture, int outputSize, int patchSize, int overlapSize)
    {
        // -----------------------------------------------------------------------------------------------
        // for reading pixel data of reference image

        int colourSize = sizeof(float) * 4;
        Vector2 sizeOfRef = GetRefTextureProperties(texture);

        // first argument is number of elements in buffer, second argument is size of each element
        sizeOfRefImage = (int)(sizeOfRef.x * sizeOfRef.y);
        outputBuffer = new ComputeBuffer(sizeOfRefImage, colourSize);

        // Find the compute shader responsible for reading pixel data,
        // and set variables in the shader
        kernalID = readPixels.FindKernel("ReadPixel");
        readPixels.SetTexture(kernalID, "inputTexture", texture);
        readPixels.SetFloat("img_width", sizeOfRef.x);
        readPixels.SetFloat("img_height", sizeOfRef.y);
        readPixels.SetBuffer(kernalID, "outputBuffer", outputBuffer);

        // call function to activate Compute Shader and store pixel data of reference image in array
        float[] allPixelDataRef = StoreRefPixels(texture);

        // -----------------------------------------------------------------------------------------------

        // number of patches that should be resulted from the reference image


        // for storing rgba values of each row 
        // might change: Current implementation obtains patches by ignoring extra pixels that cannot make up a patch
        int numRowPatches = Mathf.FloorToInt(sizeOfRef.x / patchSize);
        int numColPatches = Mathf.FloorToInt(sizeOfRef.y / patchSize);
        int numOfPatches = numRowPatches * numColPatches;
        Debug.Log("numOfPatches = " + numOfPatches);


        
        float[][] storePatches = new float[numOfPatches+1][];
        // for segmenting array of pixels into patches
        // number of elements in a patch = patchSize * patchSize * 4

        // get number of rgba elements of each row (after this many elements, a new row is started)
        float elementsPerRow = numRowPatches * patchSize * 4;
        Debug.Log("elementsPerRow = " + elementsPerRow);

        // number of elements in a row in a patch
        int rowElementsInPatch = patchSize * 4;


        // number of elements in a column in a patch
        int totalColElements = patchSize * numColPatches;

        Debug.Log("rowElementsInPatch = " + rowElementsInPatch);

        // will be incremented every time rowElementsInPatch amount of times is looped
        int nextHPatch = 0;
        // will be incremented every time to check if we reached end of all patches in a row
        int checkEndofRow = 0;
        // incremented to represent which row of a patch is being managed
        int currentPatchRow = 0;
        // indicates which patch is being managed
        int patchNum = 0;
        int savePatchNum = 0;

        int loopInPatch = 0;
        int finalLoopValue = 0;
        int saveLoopValue = 0;

        // initialize the array
        for (int i = 0; i < numOfPatches + 1; i++)
        {
            storePatches[i] = new float[patchSize * patchSize * 4 + 1];
        }

        Debug.Log("allPixelDataRef.Length = " + allPixelDataRef.Length);
        for (int i = 0; i < allPixelDataRef.Length; i++) {
            //Debug.Log("i = " + i);
            //Debug.Log("allPixelDataRef[i] = " + allPixelDataRef[i]);
            Debug.Log("patchNum = " + patchNum);
            //Debug.Log("loopInPatch = " + loopInPatch);
            // store element in target patch
            storePatches[savePatchNum][loopInPatch] = allPixelDataRef[i];

            //Debug.Log("storePatches[patchNum][loopInPatch] = " + storePatches[patchNum][loopInPatch]);
            // never reset looElements
            loopInPatch++;
            nextHPatch++;
            checkEndofRow++;
            // when reached end of row in a single patch, move to the next patch
            if (nextHPatch == rowElementsInPatch)
            {
                //Debug.Log("checkEndofRow = " + checkEndofRow);
                // Are we at the end of the row for all horizontal patches?
                if (checkEndofRow == elementsPerRow)
                {
                //    Debug.Log("currentPatchRow = " + currentPatchRow);
                //    Debug.Log("patchSize - 1 = " + (patchSize - 1));
                    // if we have not reached last row of the patch
                    if (currentPatchRow != patchSize - 1)
                    {
                        Debug.Log("Add line");
                        // indicate we are in next row of the patch, and reset nextHPatch
                        currentPatchRow += 1;
                        nextHPatch = 0;
                        // also reset patch number 
                        patchNum = savePatchNum;
                        // set loopInPatch to where we left off
                        loopInPatch = finalLoopValue + 1;
                        // save it for future use
                        saveLoopValue = loopInPatch;
                        // reset to start of row
                        checkEndofRow = 0;
                    }
                    // if we reached last row of the patch
                    else
                    {
                        // time to manage patch below previously computed patches
                        // set the default start patch number
                        Debug.Log("Next Patch Vertical");
                        patchNum += 1;
                        savePatchNum = patchNum + 1;
                        nextHPatch = 0;
                        loopInPatch = 0;
                        saveLoopValue = 0;
                        checkEndofRow = 0;
                        currentPatchRow = 0;

                    }

                }
                // move to the next patch horizontal to current patch
                else
                {
                    Debug.Log("Next Patch Horizontal");
                    patchNum += 1;
                    nextHPatch = 0;

                    finalLoopValue = loopInPatch - 1;
                    loopInPatch = saveLoopValue;
                }

                
            }
            //Debug.Log("checkEndofRow = " + checkEndofRow);
            //Debug.Log("elementsPerRow = " + elementsPerRow);
            // when reached end of row in all row patches
            
        }
        
        



        //Debug.Log(rowsOfPixels[0].Length);




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
        readPixels.Dispatch(kernalID, ref_texture.width / 8, ref_texture.height / 8, 1);
        // initiate an array that stores all pixel values
        refPixelsRGB = new float[4 * sizeOfRefImage];
        // get the pixel data of the reference image in RGBA format calculated in shader
        outputBuffer.GetData(refPixelsRGB);

        int add = 0;
        for (int i = 0; i < sizeOfRefImage; i++)
        {
            Debug.Log($"Pixel colour {i}: R={refPixelsRGB[0 + add]}, G={refPixelsRGB[1 + add]}, B={refPixelsRGB[2 + add]}, A={refPixelsRGB[3 + add]}");
            add += 4;
        }

        //outputBuffer.Release();

        return refPixelsRGB;
    }

    // Update is called once per frame
    void Update()
    {
        //StoreRefPixels();
    }
}