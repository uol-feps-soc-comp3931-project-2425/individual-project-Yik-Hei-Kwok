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

    // for switching controls
    private PC_ChooseSize chooseSizeController;
    private PC_Choose_Textures chooseTexController;
   
    public void Start()
    {
        GameObject player_controls = GameObject.Find("Player_Controller");
        chooseSizeController = player_controls.GetComponent<PC_ChooseSize>();
        chooseTexController = player_controls.GetComponent<PC_Choose_Textures>();

    }

    public void loadImage(string filename)
    {
        // open prompt for inputing image and save image to path
        Apply(filename);

    }


    // button functions
    // ------------------------------------------------------------------------------------------
    // for importing the image from user's computer
    private void Apply(string filename)
    {
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

            // disable the sizing menu
            enableOrDisableChildren("Size_Input", true);
        }
    }
    

    // for setting size
    public void outputSizeConfirm()
    {
        // patch size is preset
        // delta is suggested to be between 0.25 and 0.5
        int patchSize = (int)(0.5 * Mathf.Min(sourceImageWidth, sourceImageHeight));
        // set overlap size as 1/6 of patch size
        int overlapSize = patchSize / 6;

        Debug.Log("New sourceImageWidth = " + sourceImageWidth);
        Debug.Log("New Patch Size = " + patchSize);
        Debug.Log("New Overlap Size = " + overlapSize);

        // get the slider object and get its value
        GameObject sliderObject = GameObject.Find("Size_Slider");
        int size_value = (int)sliderObject.GetComponent<Slider>().value;

        // set loading animation

        // check if the image is saved
        if (textureLocation !=  null)
        {
            Texture2D texture = new Texture2D(2, 2);
            var fileContent = File.ReadAllBytes(textureLocation);
            texture.LoadImage(fileContent);
            // run the synthesis algorithm
            Synthesis.startSynthesis(texture, size_value, patchSize, overlapSize, true,$"{global.blockCount}/{chooseTexController.processing_side}");

            // switch controls
            chooseSizeController.enabled = false;
            chooseTexController.enabled = true;

            // disable the sizing menu
            enableOrDisableChildren("Size_Input", false);
        }
        

    }

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
