using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using UnityEngine.Playables;


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

    // for obtaining pixels from the faces that has been made into textures
    [Header("For extracting pixels from extracted face textures")]
    public ComputeShader readPixels;
    // for creating the texture atlas
    [Header("For combining all 6 face images into a single texture")]
    public ComputeShader createAtlas;

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
        Debug.Log("GOT VALUE = " + sizeFinalImage);
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

    // for allowing user to view how the cube looks like, and confirm if that is what they want
    public void viewCubeResult()
    {
        // disable all stuff in the first screen
        enableOrDisableChildren("Screen_1", false);
        // enable all stuff in the second screen
        enableOrDisableChildren("Screen_2", true);

        // all pixels will end up being in this array (3 rows, row 1 is empty, row 2 and 3 each have three faces)
        float[][] finalTexture = new float[3][];

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
                bottomTexture.LoadImage(File.ReadAllBytes($"Assets/Saved/Final_Image/{global.blockCount}/Bottom.png"));
                float[] bottomPixels = readPixelsOneTexture(bottomTexture);

                Texture2D leftTexture = new Texture2D(2, 2);
                leftTexture.LoadImage(File.ReadAllBytes($"Assets/Saved/Final_Image/{global.blockCount}/Side1.png"));
                float[] leftPixels = readPixelsOneTexture(leftTexture);

                Texture2D rightTexture = new Texture2D(2, 2);
                rightTexture.LoadImage(File.ReadAllBytes($"Assets/Saved/Final_Image/{global.blockCount}/Side2.png"));
                float[] rightPixels = readPixelsOneTexture(rightTexture);


                // put all of the pixels from these 3 faces into a single array
                float[] unorderedPixels = new float[sizeFinalImage * sizeFinalImage * 3 * 4];
                bottomPixels.CopyTo(unorderedPixels, 0);
                leftPixels.CopyTo(unorderedPixels, sizeFinalImage * sizeFinalImage * 4);
                rightPixels.CopyTo(unorderedPixels, sizeFinalImage * sizeFinalImage * 4 * 2);

                // put the pixels into correct order
                float[] orderedPixels = orderPixels(unorderedPixels);
                finalTexture[1] = orderedPixels;

                DebugFunctions.showData_CustomizedWH(sizeFinalImage * 3 , sizeFinalImage, finalTexture[1], "Saved/bottomleftright.png");
            }
            // row 2
            else if (i == 2)
            {
                // row 2 is to deal with Front Top Back

                // get the pixel values of each individual face
                Texture2D frontTexture = new Texture2D(2, 2);
                frontTexture.LoadImage(File.ReadAllBytes($"Assets/Saved/Final_Image/{global.blockCount}/Side3.png"));
                float[] frontPixels = readPixelsOneTexture(frontTexture);

                Texture2D topTexture = new Texture2D(2, 2);
                topTexture.LoadImage(File.ReadAllBytes($"Assets/Saved/Final_Image/{global.blockCount}/Top.png"));
                float[] topPixels = readPixelsOneTexture(topTexture);

                Texture2D backTexture = new Texture2D(2, 2);
                backTexture.LoadImage(File.ReadAllBytes($"Assets/Saved/Final_Image/{global.blockCount}/Side4.png"));
                float[] backPixels = readPixelsOneTexture(backTexture);

                // put all of the pixels from these 3 faces into a single array
                float[] unorderedPixels = new float[sizeFinalImage * sizeFinalImage * 3 * 4];
                frontPixels.CopyTo(unorderedPixels, 0);
                topPixels.CopyTo(unorderedPixels, sizeFinalImage * sizeFinalImage * 4);
                backPixels.CopyTo(unorderedPixels, sizeFinalImage * sizeFinalImage * 4 * 2);

                // put the pixels into correct order
                float[] orderedPixels = orderPixels(unorderedPixels);
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
            for (int j = 0; j < finalTexture[i].Length; j+=4)
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
        File.WriteAllBytes($"Assets/Saved/Final_Image/{global.blockCount}/atlas2.png", bytes);




        // modify the texture UV mapping
        Mesh meshCube = GameObject.Find("Screen_2").GetComponentInChildren<MeshFilter>().mesh;
        Vector2[] UVs = new Vector2[meshCube.vertices.Length];

        // Front
        UVs[0] = new Vector2(0.0f, 0.0f);
        UVs[1] = new Vector2(0.333f, 0.0f);
        UVs[2] = new Vector2(0.0f, 0.333f);
        UVs[3] = new Vector2(0.333f, 0.333f);

        // Top
        UVs[4] = new Vector2(0.334f, 0.333f);
        UVs[5] = new Vector2(0.666f, 0.333f);
        UVs[8] = new Vector2(0.334f, 0.0f);
        UVs[9] = new Vector2(0.666f, 0.0f);

        // Back
        UVs[6] = new Vector2(1.0f, 0.0f);
        UVs[7] = new Vector2(0.667f, 0.0f);
        UVs[10] = new Vector2(1.0f, 0.333f);
        UVs[11] = new Vector2(0.667f, 0.333f);

        // Bottom
        UVs[12] = new Vector2(0.0f, 0.334f);
        UVs[13] = new Vector2(0.0f, 0.666f);
        UVs[14] = new Vector2(0.333f, 0.666f);
        UVs[15] = new Vector2(0.333f, 0.334f);

        // Left
        UVs[16] = new Vector2(0.334f, 0.334f);
        UVs[17] = new Vector2(0.334f, 0.666f);
        UVs[18] = new Vector2(0.666f, 0.666f);
        UVs[19] = new Vector2(0.666f, 0.334f);

        // Right        
        UVs[20] = new Vector2(0.667f, 0.334f);
        UVs[21] = new Vector2(0.667f, 0.666f);
        UVs[22] = new Vector2(1.0f, 0.666f);
        UVs[23] = new Vector2(1.0f, 0.334f);

        meshCube.uv = UVs;

        var createdTexture = File.ReadAllBytes($"Assets/Ref_Img/AZY1a.png");
        Texture2D newTexture = new Texture2D(2, 2);
        newTexture.LoadImage(createdTexture);

        Material material = new Material(Shader.Find("Unlit/Texture"));
        material.mainTexture = newTexture;

        //AssetDatabase.CreateAsset(material, "Assets/Saved/Final_Image/0/atlas.png");

        //AssetDatabase.SaveAssets();

        GameObject.Find("Screen_2").transform.GetChild(0).GetComponent<MeshRenderer>().material = material;

    }

    private float[] readPixelsOneTexture(Texture2D texture)
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

        return pixelValues;
    }

    private float[] orderPixels(float[] unorderedPixels)
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

        createAtlas.Dispatch(kernalID, Mathf.CeilToInt((float)(sizeFinalImage * sizeFinalImage * 3 * 4) / 64), 1, 1);

        outputOrdered.GetData(pixelValuesInRow);

        return pixelValuesInRow;
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
