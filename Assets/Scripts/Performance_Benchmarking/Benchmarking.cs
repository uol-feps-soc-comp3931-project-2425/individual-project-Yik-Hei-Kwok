using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.IO;
using static global;

public class Benchmarking : MonoBehaviour
{
    // Start is called before the first frame update

    //Synthesis.startSynthesis(texture, sizeFinalImage, patchSize, overlapSize, true,true, $"{global.blockCount}/{filename}", lambdaValue);

    // the texture chosen for benchmarking
    public Texture2D texture;

    public Texturesynthesis Synthesis;
    void Start()
    {
        // test for time used with and without using KD for different output sizes
        debugKD.debug = true;


        int[] finalSizeValue = new int[10];
        finalSizeValue[0] = 100;
        finalSizeValue[1] = 200;
        finalSizeValue[2] = 300;
        finalSizeValue[3] = 400;
        finalSizeValue[4] = 500;
        finalSizeValue[5] = 600;
        finalSizeValue[6] = 700;
        finalSizeValue[7] = 800;
        finalSizeValue[8] = 900;
        finalSizeValue[9] = 1000;


        // for texture 2, patch size = 40 and boundary size = 6
        int patchSize = (int)(0.27f * Mathf.Min(texture.width, texture.height));
        // set overlap size as 1/6 of patch size
        int overlapSize = patchSize / 6;

        float lambdaValue = 0.4f;

        //float[] saveKDTime = new float[finalSizeValue.Length];
        //float[] saveNoKDTime = new float[finalSizeValue.Length];

        for (int i  = 0; i < finalSizeValue.Length; i++)
        {
            string filename_noKD = $"NoKD_Benchmark_Size{finalSizeValue[i]}";
            string filename_KD = $"KD_Benchmark_Size{finalSizeValue[i]}";

            // benchmark the execution time for using KD
            Synthesis.startSynthesis(texture, finalSizeValue[i], patchSize, overlapSize, true, true, $"Benchmark_Saved/{filename_KD}", lambdaValue);

            // benchmark the execution time for not using KD
            Synthesis.startSynthesis(texture, finalSizeValue[i], patchSize, overlapSize, true, false, $"Benchmark_Saved/{filename_noKD}", lambdaValue);
        }

        saveToCSV(finalSizeValue.Length, debugKD.time_NoKD, debugKD.time_KD, "Assets/Saved/Final_Image/Benchmark_Saved/KD_Compare.csv");


        debugKD.debug = false;
        //var watch = System.Diagnostics.Stopwatch.StartNew();
    }

    private void saveToCSV(int increment, float[] saveKDTime, float[] saveNoKDTime, string filePath)
    {
        var csv = new StringBuilder();

        for (int i = 0; i < increment; i++)
        {
            //in your loop
            //float first = saveKDTime;
            //float second = image.ToString();
            //Suggestion made by KyleMit
            var newLine = string.Format("{0},{1}", saveKDTime[i], saveNoKDTime[i]);
            csv.AppendLine(newLine);
        }
        
        
        //after your loop
        File.WriteAllText(filePath, csv.ToString());
    }

    
}
