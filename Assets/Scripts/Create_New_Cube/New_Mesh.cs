using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;


public class New_Mesh : MonoBehaviour
{
    public Texturesynthesis Synthesis;

    private string textureLocation;
    private int sourceImageWidth;
    private int sourceImageHeight;
    private int sizeFinalImage;

    // for switching controls
    private PC_ChooseSize chooseSizeController;
    private PC_Choose_Textures chooseTexController;
   
    public void Start()
    {
        GameObject player_controls = GameObject.Find("Player_Controller");
        chooseSizeController = player_controls.GetComponent<PC_ChooseSize>();
        chooseTexController = player_controls.GetComponent<PC_Choose_Textures>();

    }

    

    // for setting size of final image
    public void outputSizeConfirm()
    {
        // get the slider object and get its value
        GameObject sliderObject = GameObject.Find("Size_Slider");
        sizeFinalImage = (int)sliderObject.GetComponent<Slider>().value;

        // disable the sizing menu
        enableOrDisableChildren("Size_Input", false);
        chooseSizeController.enabled = false;
        chooseTexController.enabled = true;
    }

    // for importing the image from user's computer and running the synthesis algorithm
    public void Apply(string filename)
    {
        // open prompt for inputing image and save image to path
        string path = EditorUtility.OpenFilePanel("Overwrite with png", "", "png");
        if (path.Length != 0)
        {
            var fileContent = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(fileContent);

            sourceImageWidth = tex.width;
            sourceImageHeight = tex.height;

            Debug.Log("New1 sourceImageWidth = " + sourceImageWidth);

            byte[] bytes = tex.EncodeToPNG();
            string savePath = $"Assets/Saved/Loaded_Images/{filename}.png";

            // save path of the texture
            textureLocation = savePath;

            File.WriteAllBytes(savePath, bytes);

            // switch controls
            chooseSizeController.enabled = true;
            chooseTexController.enabled = false;

            // run the synthesis algorithm
            runSynthesis(filename);
        }
    }


    private void runSynthesis(string filename)
    {
        // patch size is preset
        // delta is suggested to be between 0.25 and 0.5
        int patchSize = (int)(0.5 * Mathf.Min(sourceImageWidth, sourceImageHeight));
        // set overlap size as 1/6 of patch size
        int overlapSize = patchSize / 6;

        // set loading animation

        // check if the image is saved
        if (textureLocation != null)
        {
            Texture2D texture = new Texture2D(2, 2);
            var fileContent = File.ReadAllBytes(textureLocation);
            texture.LoadImage(fileContent);
            // run the synthesis algorithm
            Synthesis.startSynthesis(texture, sizeFinalImage, patchSize, overlapSize, true, $"{global.blockCount}/{filename}");

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

    

    /*public void outputSizeCancel()
    {
        // switch controls
        chooseSizeController.enabled = false;
        chooseTexController.enabled = true;

        // disable the sizing menu
        enableOrDisableChildren("Size_Input", false);
    }*/

    private void enableOrDisableChildren(string objectName, bool active)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj != null)
        {   
        foreach (Transform t in obj.transform)
                t.gameObject.SetActive(active);
        }
    }




    public void createNewTextureMesh()
    {
        // create new cube for display
        GameObject cubeInstance = GameObject.CreatePrimitive(PrimitiveType.Cube);


        // since Unity by default only supports the same texture on each side of cube,
        // need to make sure we set the UV mapping for customization on each side
        setUVMapping();
    }

    private void setUVMapping()
    {

    }
}
