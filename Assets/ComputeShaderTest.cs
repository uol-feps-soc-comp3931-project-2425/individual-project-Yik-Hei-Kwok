using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeShaderTest : MonoBehaviour
{
    public ComputeShader readPixels;

    public RenderTexture renderTexture;

    private ComputeBuffer outputBuffer;

    // the reference image for texture synthesis
    public Texture2D ref_texture;

    private int kernalID;

    // for storing all pixels of the reference image
    private float[] refPixelsRGB;

    // total size of the reference image
    private int sizeOfRefImage;

    // Start is called before the first frame update
    void Start()
    {
        
        int colourSize = sizeof(float)*4;
        Vector2 sizeOfRef = GetRefTextureProperties(ref_texture);

        // first argument is number of elements in buffer,
        // second argument is size of each element
        sizeOfRefImage = (int)(sizeOfRef.x * sizeOfRef.y);
        Debug.Log(sizeOfRefImage);
        Debug.Log(sizeOfRef.x);
        Debug.Log(sizeOfRef.y);
        outputBuffer = new ComputeBuffer(sizeOfRefImage, colourSize);

        // Find the compute shader responsible for reading pixel data,
        // and set variables in the shader
        kernalID = readPixels.FindKernel("ReadPixel");
        readPixels.SetTexture(kernalID, "inputTexture", ref_texture);
        readPixels.SetFloat("img_width", sizeOfRef.x);
        readPixels.SetFloat("img_height", sizeOfRef.y);
        readPixels.SetBuffer(kernalID, "outputBuffer", outputBuffer);
       
    }

    private Vector2 GetRefTextureProperties(Texture2D ref_texture)
    {
        int numOfPixels_height = ref_texture.height;
        int numOfPixels_width = ref_texture.width;
        Vector2 arrayTextureProp = new Vector2(numOfPixels_width, numOfPixels_height);
        return arrayTextureProp;
    }

    // function that stores the pixels of the reference image into a format
    // [ [ [r,g,b],[r,g,b],[r,g,b] ...... [r,g,b] ] ]
    private void StoreRefPixels()
    {
        readPixels.Dispatch(kernalID, ref_texture.width / 8, ref_texture.height / 8, 1);
        // initiate an array that stores all pixel values
        refPixelsRGB = new float[4 * sizeOfRefImage];
        // get the pixel data of the reference image in RGBA format calculated in shader
        outputBuffer.GetData(refPixelsRGB);

        int add = 0;
        for(int i = 0; i < sizeOfRefImage; i++)
        {
            Debug.Log($"Pixel colour {i}: R={refPixelsRGB[0 + add]}, G={refPixelsRGB[1 + add]}, B={refPixelsRGB[2 + add]}, A={refPixelsRGB[3 + add]}");
            add += 4;
        }


        //Debug.Log($"First pixel colour: R={refPixelsRGB[0]}, G={refPixelsRGB[1]}, B={refPixelsRGB[2]}, A={refPixelsRGB[3]}");
        //Debug.Log($"Last pixel colour: R={refPixelsRGB[65532]}, G={refPixelsRGB[65533]}, B={refPixelsRGB[65534]}, A={refPixelsRGB[65535]}");

        outputBuffer.Release();

    }

    // Update is called once per frame
    void Update()
    {
        StoreRefPixels();
    }
}
