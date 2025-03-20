using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Merge_Textures : MonoBehaviour
{
    // for obtaining pixels from the faces that has been made into textures
    [Header("For extracting pixels from extracted face textures")]
    public ComputeShader readPixels;
    // for creating the texture atlas
    [Header("For combining all 6 face images into a single texture")]
    public ComputeShader createAtlas;

    public void createNewAtlas(int sizeFinalImage, bool isTerrain)
    {
        float[][] finalTexture = new float[3][];
        string path;
        if (isTerrain)
            path = "Terrain";
        else
            path = $"{global.blockCount}";
        for (int i = 0; i < 3; i++)
        {
            // create new 1d array that stores all pixels of the 3 faces in chronological order (wrong sequence)
            // row 0
            if (i == 0)
            {
                // row 0 do not have any pixel values, so just default them to white
                finalTexture[0] = new float[sizeFinalImage * sizeFinalImage * 3 * 4];
            }
            // row 1 
            else if (i == 1)
            {
                // row 1 is to deal with Bottom Left Right

                // get the pixel values of each individual face
                Texture2D bottomTexture = new Texture2D(2, 2);
                bottomTexture.LoadImage(File.ReadAllBytes($"Assets/Saved/Final_Image/{path}/Bottom.png"));
                float[] bottomPixels = readPixelsOneTexture(bottomTexture,sizeFinalImage);

                Texture2D leftTexture = new Texture2D(2, 2);
                leftTexture.LoadImage(File.ReadAllBytes($"Assets/Saved/Final_Image/{path}/Side1.png"));
                float[] leftPixels = readPixelsOneTexture(leftTexture, sizeFinalImage);

                Texture2D rightTexture = new Texture2D(2, 2);
                rightTexture.LoadImage(File.ReadAllBytes($"Assets/Saved/Final_Image/{path}/Side2.png"));
                float[] rightPixels = readPixelsOneTexture(rightTexture, sizeFinalImage);


                // put all of the pixels from these 3 faces into a single array
                float[] unorderedPixels = new float[sizeFinalImage * sizeFinalImage * 3 * 4];
                bottomPixels.CopyTo(unorderedPixels, 0);
                leftPixels.CopyTo(unorderedPixels, sizeFinalImage * sizeFinalImage * 4);
                rightPixels.CopyTo(unorderedPixels, sizeFinalImage * sizeFinalImage * 4 * 2);

                // put the pixels into correct order
                float[] orderedPixels = orderPixels(unorderedPixels, sizeFinalImage);
                finalTexture[1] = orderedPixels;

                DebugFunctions.showData_CustomizedWH(sizeFinalImage * 3, sizeFinalImage, finalTexture[1], "Saved/bottomleftright.png");
            }
            // row 2
            else if (i == 2)
            {
                // row 2 is to deal with Front Top Back

                // get the pixel values of each individual face
                Texture2D frontTexture = new Texture2D(2, 2);
                frontTexture.LoadImage(File.ReadAllBytes($"Assets/Saved/Final_Image/{path}/Side3.png"));
                float[] frontPixels = readPixelsOneTexture(frontTexture,sizeFinalImage);

                Texture2D topTexture = new Texture2D(2, 2);
                topTexture.LoadImage(File.ReadAllBytes($"Assets/Saved/Final_Image/{path}/Top.png"));
                float[] topPixels = readPixelsOneTexture(topTexture, sizeFinalImage);

                Texture2D backTexture = new Texture2D(2, 2);
                backTexture.LoadImage(File.ReadAllBytes($"Assets/Saved/Final_Image/{path}/Side4.png"));
                float[] backPixels = readPixelsOneTexture(backTexture, sizeFinalImage);

                // put all of the pixels from these 3 faces into a single array
                float[] unorderedPixels = new float[sizeFinalImage * sizeFinalImage * 3 * 4];
                frontPixels.CopyTo(unorderedPixels, 0);
                topPixels.CopyTo(unorderedPixels, sizeFinalImage * sizeFinalImage * 4);
                backPixels.CopyTo(unorderedPixels, sizeFinalImage * sizeFinalImage * 4 * 2);

                // put the pixels into correct order
                float[] orderedPixels = orderPixels(unorderedPixels,sizeFinalImage);
                finalTexture[2] = orderedPixels;

                DebugFunctions.showData_CustomizedWH(sizeFinalImage * 3, sizeFinalImage, finalTexture[2], "Saved/fronttopback.png");
            }
        }

        // create new texture 
        Texture2D texture = new Texture2D(sizeFinalImage * 3, sizeFinalImage * 3, TextureFormat.RGBA32, false);
        // now that we have all the pixels in the correct order, turn it into one image
        Color[] allPixelRGBAs = new Color[sizeFinalImage * sizeFinalImage * 9];
        int pixelIndent = 0;
        for (int i = 2; i >= 0; i--)
        {
            for (int j = 0; j < finalTexture[i].Length; j += 4)
            {
                float R = finalTexture[i][j];
                float G = finalTexture[i][j + 1];
                float B = finalTexture[i][j + 2];
                float A = finalTexture[i][j + 3];
                allPixelRGBAs[pixelIndent] = new Color(R, G, B, A);
                pixelIndent += 1;
            }
        }

        // Apply colors to the texture
        texture.SetPixels(allPixelRGBAs);
        texture.Apply();

        // encode and save the texture atlas
        byte[] bytes = texture.EncodeToPNG();
        // save the atlas
        File.WriteAllBytes($"Assets/Saved/Final_Image/{path}/atlas.png", bytes);
    }


    private float[] readPixelsOneTexture(Texture2D texture, int sizeFinalImage)
    {
        ComputeBuffer outputBuffer = new ComputeBuffer(sizeFinalImage * sizeFinalImage, sizeof(float) * 4);

        // Find the compute shader responsible for reading pixel data,
        // and set variables in the shader
        int kernalID = readPixels.FindKernel("ReadPixel");
        readPixels.SetTexture(kernalID, "inputTexture", texture);
        readPixels.SetFloat("img_width", sizeFinalImage);
        readPixels.SetFloat("img_height", sizeFinalImage);
        readPixels.SetBuffer(kernalID, "outputBuffer", outputBuffer);

        // size of input image must be at least 8x8 or else thread group will be 0
        readPixels.Dispatch(kernalID, Mathf.CeilToInt(sizeFinalImage / 8.0f), Mathf.CeilToInt(sizeFinalImage / 8.0f), 1);
        // initiate an array that stores all pixel values
        float[] pixelValues = new float[4 * sizeFinalImage * sizeFinalImage];
        // get the pixel data of the reference image in RGBA format calculated in shader
        outputBuffer.GetData(pixelValues);

        outputBuffer.Release();

        return pixelValues;
    }

    private float[] orderPixels(float[] unorderedPixels, int sizeFinalImage)
    {
        float[] pixelValuesInRow = new float[sizeFinalImage * sizeFinalImage * 3 * 4];

        int kernalID = createAtlas.FindKernel("textureAtlas");
        createAtlas.SetInt("imageLength", sizeFinalImage * 3);
        createAtlas.SetInt("faceSize", sizeFinalImage);

        // use data in the unordered array as input
        ComputeBuffer inputPixelData = new ComputeBuffer(sizeFinalImage * sizeFinalImage * 3 * 4, sizeof(float));
        inputPixelData.SetData(unorderedPixels);
        createAtlas.SetBuffer(kernalID, "faceData", inputPixelData);

        // set output
        ComputeBuffer outputOrdered = new ComputeBuffer(sizeFinalImage * sizeFinalImage * 3 * 4, sizeof(float));
        createAtlas.SetBuffer(kernalID, "outputOrder", outputOrdered);

        createAtlas.Dispatch(kernalID, Mathf.CeilToInt((float)(sizeFinalImage * sizeFinalImage * 3 * 4) / 256), 1, 1);

        outputOrdered.GetData(pixelValuesInRow);

        inputPixelData.Release();
        outputOrdered.Release();

        return pixelValuesInRow;
    }

}
