using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Call_Synthesis : MonoBehaviour
{
    public Texturesynthesis Synthesis;

    public Texture2D ref_texture;

    public RawImage ref_texture2;

    private int outputSize = 1000;
    private int patchSize = 100;
    private int overlapSize = 23;

    private void Start()
    {
        Synthesis.startSynthesis(ref_texture, outputSize, patchSize, overlapSize,true);
    }


}
