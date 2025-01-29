using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Call_Synthesis : MonoBehaviour
{
    public Texturesynthesis Synthesis;

    public Texture2D ref_texture;

    private int outputSize = 1000;
    private int patchSize = 2;
    private int overlapSize = 10;

    private void Start()
    {
        Synthesis.startSynthesis(ref_texture, outputSize, patchSize, overlapSize);
    }


}
