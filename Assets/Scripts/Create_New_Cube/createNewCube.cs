using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using UnityEngine.Playables;
using UnityEngine.InputSystem;

public class createNewCube : MonoBehaviour
{
    
    public Call_Synthesis invokeSynthesis;
    public int sizeFinalImage;

    private string textureLocation;
    private int sourceImageWidth;
    private int sourceImageHeight;
    

    private string currentProcessingName;

    private bool showingSizingMenu = false;

    // for switching controls
    private PC_ChooseSize chooseSizeController;
    private PC_Choose_Textures chooseTexController;

    // the sliders to control parameters of texture synthesised 
    public Slider sizeSlider;
    public Slider lambdaSlider;

    public GameObject cubeView;

    public void Start()
    {
        GameObject player_controls = GameObject.Find("Player_Controller");
        chooseSizeController = player_controls.GetComponent<PC_ChooseSize>();
        chooseTexController = player_controls.GetComponent<PC_Choose_Textures>();

    }
    private void Update()
    {

    }

    // for setting size of final image
    public void outputSizeConfirm()
    {
        // get the slider objects and get its value
        
        sizeFinalImage = (int)sizeSlider.value;
        float lambda = lambdaSlider.value;
        Debug.Log("GOT VALUE = " + sizeFinalImage);
        // disable the sizing menu
        showSizingMenu(false);

        // run the synthesis algorithm
        invokeSynthesis.runSynthesis(currentProcessingName, sourceImageWidth, sourceImageHeight, textureLocation, sizeFinalImage, lambda);
    }

    private void showSizingMenu(bool show)
    {
        enableOrDisableChildren("Size_Input", show);
        chooseSizeController.enabled = show;
        chooseTexController.enabled = !show;
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

            // Show the menu indicating 
            showSizingMenu(true);

            currentProcessingName = filename;


        }
    }

    public void enableOrDisableChildren(string objectName, bool active)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj != null)
        {
            foreach (Transform t in obj.transform)
                t.gameObject.SetActive(active);
        }
    }


}
