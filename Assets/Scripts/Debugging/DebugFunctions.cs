using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DebugFunctions : MonoBehaviour
{
    public static void debugImage(float[] array, int img_width, int img_height)
    {
        int add = 0;
        Color[] pixels = new Color[img_width * img_height];
        for (int j = 0; j < array.Length; j += 4)
        {
            float R = array[j + 0];
            float G = array[j + 1];
            float B = array[j + 2];
            float A = array[j + 3];

            pixels[add] = new Color(R, G, B, A);
            add++;
        }

        Texture2D patch = new Texture2D(img_width, img_height);
        patch.SetPixels(pixels);
        patch.Apply();

        byte[] bytes = patch.EncodeToPNG();

        // delete directory contents before appending new patches to the folder
        //deleteDirContents("Assets/Save_Patches");

        string savePath = $"Assets/image_produced.png";



        File.WriteAllBytes(savePath, bytes);
    }


    public static void showData(float[][] patchesValues, int patchSize, string dir)
    {


        int add = 0;
        Debug.Log("number of patches eeee = " + patchesValues.Length);
        Debug.Log("number of patches eeee = " + patchesValues.Length);

        for (int i = 0; i < patchesValues.Length; i += 1)
        {
            Color[] pixels = new Color[patchSize * patchSize];
            for (int j = 0; j < patchesValues[i].Length; j += 4)
            {


                float R = patchesValues[i][j + 0];
                float G = patchesValues[i][j + 1];
                float B = patchesValues[i][j + 2];
                float A = patchesValues[i][j + 3];

                pixels[add] = new Color(R, G, B, A);

                add++;
            }
            Texture2D patch = new Texture2D(patchSize, patchSize);
            patch.SetPixels(pixels);
            patch.Apply();

            byte[] bytes = patch.EncodeToPNG();

            // delete directory contents before appending new patches to the folder
            //deleteDirContents("Assets/Save_Patches");

            string savePath = $"Assets/{dir}/Patch{i}.png";



            File.WriteAllBytes(savePath, bytes);
            add = 0;

        }

    }


    public static void showData_left(float[][] patchesValues, int patchSize, int overlapSize, string dir)
    {
        Debug.Log("Number of patches = " + patchesValues.Length);

        // Loop through each patch's left overlay data
        for (int i = 0; i < patchesValues.Length; i++)
        {
            // Create an array for the pixels of the overlay image
            Color[] pixels = new Color[overlapSize * patchSize];

            // Map the RGBA values from the data to the overlay image
            for (int y = 0; y < patchSize; y++) // Iterate over rows
            {
                for (int x = 0; x < overlapSize; x++) // Iterate over the left overlap columns
                {
                    // Calculate the index in the flat array (RGBA values)
                    int index = (y * overlapSize + x) * 4;

                    // Extract RGBA values
                    float R = patchesValues[i][index + 0];
                    float G = patchesValues[i][index + 1];
                    float B = patchesValues[i][index + 2];
                    float A = patchesValues[i][index + 3];

                    // Map to the pixel array for the image
                    pixels[y * overlapSize + x] = new Color(R, G, B, A);
                }
            }

            // Create a Texture2D to represent the overlay image
            Texture2D overlayTexture = new Texture2D(overlapSize, patchSize);
            overlayTexture.SetPixels(pixels);
            overlayTexture.Apply();

            // Encode the texture to a PNG file
            byte[] bytes = overlayTexture.EncodeToPNG();

            // Save the overlay image to the specified directory
            string savePath = $"Assets/{dir}/LeftOverlay_Patch{i}.png";
            File.WriteAllBytes(savePath, bytes);

            Debug.Log($"Saved left overlay image for patch {i} at {savePath}");
        }
    }


    public static void showData_CustomizedWH(int imageSizeWidth, int imageSizeHeight, float[] pixelData, string dir)
    {
        // Calculate image size


        // Check if the pixelData array is correctly sized
        if (pixelData == null || pixelData.Length != imageSizeWidth * imageSizeHeight * 4)
        {
            Debug.LogError("Pixel data size does not match the expected image dimensions!");
            return;
        }

        // Create a new texture
        Texture2D texture = new Texture2D(imageSizeWidth, imageSizeHeight, TextureFormat.RGBA32, false);

        // Fill the texture with the 1D pixel data
        Color[] colors = new Color[imageSizeWidth * imageSizeHeight];
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

        string savePath = $"Assets/{dir}";



        File.WriteAllBytes(savePath, bytes);
    }
}
