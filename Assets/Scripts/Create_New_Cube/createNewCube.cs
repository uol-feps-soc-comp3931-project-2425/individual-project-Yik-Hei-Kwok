using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using UnityEngine.Playables;
using UnityEngine.InputSystem;
using TMPro;

public class createNewCube : MonoBehaviour
{
    
    public Call_Synthesis invokeSynthesis;
    public int sizeFinalImage;

    private string textureLocation;
    private int sourceImageWidth;
    private int sourceImageHeight;
    private string currentProcessingName;
    private bool showingSizingMenu = false;
    // denoting which menu is being shown
    private bool sizeMenu = false;
    private bool settingsMenu = false;
    // get text value of the slider
    private TextMeshProUGUI size_text;
    private TextMeshProUGUI randomness_text;
    private TextMeshProUGUI patchSize_text;
    private TextMeshProUGUI boundary_text;

    // for switching controls
    private PC_ChooseSize chooseSizeController;
    private PC_Choose_Textures chooseTexController;

    // the sliders to control parameters of texture synthesised 
    public Slider sizeSlider;
    public Slider lambdaSlider;
    public Slider patchSlider;
    public Slider boundarySlider;

    public GameObject cubeView;

    public void Start()
    {
        GameObject player_controls = GameObject.Find("Player_Controller");
        chooseSizeController = player_controls.GetComponent<PC_ChooseSize>();
        chooseTexController = player_controls.GetComponent<PC_Choose_Textures>();
        
        size_text = sizeSlider.gameObject.transform.Find("Slider_Value").Find("Value").gameObject.GetComponent<TextMeshProUGUI>();
        randomness_text = lambdaSlider.gameObject.transform.Find("Slider_Value").Find("Value").gameObject.GetComponent<TextMeshProUGUI>();
        patchSize_text = patchSlider.gameObject.transform.Find("Slider_Value").Find("Value").gameObject.GetComponent<TextMeshProUGUI>();
        boundary_text = boundarySlider.gameObject.transform.Find("Slider_Value").Find("Value").gameObject.GetComponent<TextMeshProUGUI>();

        setSize();

    }
    private void Update()
    {

        // update the text value of size slider
        if(sizeMenu == true)
        {
            size_text.text = sizeSlider.value.ToString();
        }else if(settingsMenu == true)
        {
            randomness_text.text = lambdaSlider.value.ToString();

            float PS = patchSlider.value;
            int actualPatchSize = (int)(PS * Mathf.Min(sourceImageWidth, sourceImageHeight));
            patchSize_text.text = actualPatchSize.ToString();

            int OS = (int)boundarySlider.value;
            int overlapSize = actualPatchSize / OS;
            boundary_text.text = overlapSize.ToString();

        }
    }

    public void setSize()
    {
        // show the sizing menu
        sizeMenu = true;
        showSizingMenu(true);
    }
    public void sizeConfirm()
    {
        // get the size 
        sizeFinalImage = (int)sizeSlider.value;
        Debug.Log("GOT VALUE = " + sizeFinalImage);
        // unshow the menu
        sizeMenu = false;
        showSizingMenu(false);
    }
    // for setting size of final image
    public void settingsConfirm()
    {
        float lambda = lambdaSlider.value;
        float PS_point = patchSlider.value;
        int BS_point = (int)boundarySlider.value;
        // disable the sizing menu
        settingsMenu = false;
        showSettingsMenu(false);

        // run the synthesis algorithm
        invokeSynthesis.runSynthesis(currentProcessingName, sourceImageWidth, sourceImageHeight, textureLocation, sizeFinalImage, lambda, PS_point, BS_point);
    }

    private void showSettingsMenu(bool show)
    {
        enableOrDisableChildren("Settings_Input", show);
        chooseSizeController.enabled = show;
        chooseTexController.enabled = !show;
    }

    private void showSizingMenu(bool show)
    {
        enableOrDisableChildren("Actual Size Input", show);
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
            settingsMenu = true;
            showSettingsMenu(true);


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
