using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;


public class New_Mesh : MonoBehaviour
{
    public void loadImage(string filename)
    {
        // open prompt for inputing image and save image to path
        Apply(filename);
        

        
        
    }

   
    private void Apply(string filename)
    {
        string path = EditorUtility.OpenFilePanel("Overwrite with png", "", "png");
        if (path.Length != 0)
        {
            var fileContent = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(fileContent);

            byte[] bytes = tex.EncodeToPNG();
            string savePath = $"Assets/Saved/Loaded_Images/{filename}.png";

            File.WriteAllBytes(savePath, bytes);
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
