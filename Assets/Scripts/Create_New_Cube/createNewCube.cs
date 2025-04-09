using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEngine.UI;
//using static System.Net.Mime.MediaTypeNames;
using UnityEngine.Playables;
using UnityEngine.InputSystem;
using TMPro;
using SimpleFileBrowser;

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

    // for checking if all textures have been filled
    public string[] textures_added;
    public int inc_texture_added = 0;

    // determine if we are creating a new cube as a block, or creating a new cube for terrain
    private bool isTerrain = false;

    public void initializeCubeMenu(bool terrain, bool cancelPressed)
    {
        
        GameObject player_controls = GameObject.Find("Player_Controller");
        chooseSizeController = player_controls.GetComponent<PC_ChooseSize>();
        chooseTexController = player_controls.GetComponent<PC_Choose_Textures>();

        size_text = sizeSlider.gameObject.transform.Find("Slider_Value").Find("Value").gameObject.GetComponent<TextMeshProUGUI>();
        randomness_text = lambdaSlider.gameObject.transform.Find("Slider_Value").Find("Value").gameObject.GetComponent<TextMeshProUGUI>();
        patchSize_text = patchSlider.gameObject.transform.Find("Slider_Value").Find("Value").gameObject.GetComponent<TextMeshProUGUI>();
        boundary_text = boundarySlider.gameObject.transform.Find("Slider_Value").Find("Value").gameObject.GetComponent<TextMeshProUGUI>();

        isTerrain = terrain;

        Debug.Log("Application.persistentDataPath = " + Application.persistentDataPath);

        if(cancelPressed == false)
        {
            setSize();
        }
        else
        {
            sizeConfirm();
        }

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
        invokeSynthesis.runSynthesis(currentProcessingName, sourceImageWidth, sourceImageHeight, textureLocation, sizeFinalImage, lambda, PS_point, BS_point,isTerrain);
        // add the name to this array to inform that this texture side is added
        bool same_side = false;
        foreach(string side in textures_added)
        {
            if(side == currentProcessingName)
                same_side = true;
        }
        if (!same_side)
        {
            textures_added[inc_texture_added] = currentProcessingName;
            inc_texture_added++;
        }
                
        
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


    IEnumerator ShowLoadDialogCoroutine(System.Action<string> callback)
    {
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, true, null, null, "Select Files", "Load");

        Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
            //OnFilesSelected(FileBrowser.Result); // FileBrowser.Result is null, if FileBrowser.Success is false
            callback(FileBrowser.Result[0]);
        else
            callback(null);
    }


    // for importing the image from user's computer and running the synthesis algorithm
    public void Apply(string filename)
    {
        // open prompt for inputing image and save image to path
        //string path = EditorUtility.OpenFilePanel("Overwrite with png", "", "png");
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Images", ".jpg", ".png"));
        FileBrowser.AddQuickLink("Users", "C:\\Users", null);

        StartCoroutine(ShowLoadDialogCoroutine((path) => OnFileSelected(path, filename)));
    }

    private void OnFileSelected(string path, string filename)
    {
        Debug.Log("path = " + path);
        if (path.Length != 0 && path != null)
        {
            var fileContent = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(fileContent);

            sourceImageWidth = tex.width;
            sourceImageHeight = tex.height;

            Debug.Log("New1 sourceImageWidth = " + sourceImageWidth);

            byte[] bytes = tex.EncodeToPNG();

            string savePath = $"{global.rootPath}/Saved/Loaded_Images/{filename}.png";

            // save path of the texture
            textureLocation = savePath;

            bool exists = System.IO.Directory.Exists($"{global.rootPath}/Saved/Loaded_Images");
            if (!exists)
                System.IO.Directory.CreateDirectory($"{global.rootPath}/Saved/Loaded_Images");

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


    public void deleteTextureAltas(string atlas_index)
    {
        if(File.Exists($"{global.rootPath}/Saved/Final_Image/{atlas_index}/atlas.png"))
            File.Delete($"{global.rootPath}/Saved/Final_Image/{atlas_index}/atlas.png");
    }


}
