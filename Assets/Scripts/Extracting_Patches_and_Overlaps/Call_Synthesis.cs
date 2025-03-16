using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class Call_Synthesis : MonoBehaviour
{
    public Texturesynthesis Synthesis;

    public void runSynthesis(string filename, int sourceImageWidth, int sourceImageHeight, string textureLocation, int sizeFinalImage, float lambdaValue, float PS, int OS)
    {
        // patch size is preset
        // delta is suggested to be between 0.25 and 0.5
        int patchSize = (int)(PS * Mathf.Min(sourceImageWidth, sourceImageHeight));
        Debug.Log("Run Synthesis Patch Size: " + patchSize);
        // set overlap size as 1/6 of patch size
        int overlapSize = patchSize / OS;
        Debug.Log("Run Synthesis Overlap Size" + overlapSize);

        // set loading animation

        // check if the image is saved
        if (textureLocation != null)
        {
            Texture2D texture = new Texture2D(2, 2);
            var fileContent = File.ReadAllBytes(textureLocation);
            texture.LoadImage(fileContent);
            // run the synthesis algorithm
            Synthesis.startSynthesis(texture, sizeFinalImage, patchSize, overlapSize, true,false, $"{global.blockCount}/{filename}", lambdaValue);

            // update the displayed texture



            Debug.Log("chooseTexController.processing_side = " + filename);
            // encode the texture
            var createdTexture = File.ReadAllBytes($"Assets/Saved/Final_Image/{global.blockCount}/{filename}.png");
            Texture2D newTexture = new Texture2D(2, 2);
            newTexture.LoadImage(createdTexture);

            // find the raw texture display and update it
            GameObject.Find(filename).GetComponentInChildren<RawImage>().texture = newTexture;
        }
    }

}
