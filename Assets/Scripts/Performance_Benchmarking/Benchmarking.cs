using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.IO;

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

        /*
        int[] finalSizeValue = new int[6];
        finalSizeValue[0] = 580;
        finalSizeValue[1] = 780;
        finalSizeValue[2] = 1000;
        finalSizeValue[3] = 1500;
        finalSizeValue[4] = 2000;
        finalSizeValue[5] = 3000;
        */

        int[] finalSizeValue = new int[2];
        finalSizeValue[0] = 2000;
        finalSizeValue[1] = 3000;


        int patchSize = (int)(0.2f * Mathf.Min(texture.width, texture.height));
        // set overlap size as 1/6 of patch size
        int overlapSize = patchSize / 6;

        float lambdaValue = 0.4f;

        int numIncrements = 10;

        float[] saveKDTime = new float[finalSizeValue.Length];
        float[] saveNoKDTime = new float[finalSizeValue.Length];

        for (int i  = 0; i < finalSizeValue.Length; i++)
        {
            string filename_noKD = $"NoKD_Benchmark_Size{finalSizeValue[i]}";
            string filename_KD = $"KD_Benchmark_Size{finalSizeValue[i]}";

            // benchmark the execution time for using KD
            var watchKD = System.Diagnostics.Stopwatch.StartNew();
            Synthesis.startSynthesis(texture, finalSizeValue[i], patchSize, overlapSize, true, true, $"Benchmark_Saved/{filename_KD}", lambdaValue);
            watchKD.Stop();
            var elapsedMs_KD = watchKD.ElapsedMilliseconds;
            saveKDTime[i] = elapsedMs_KD;

            // benchmark the execution time for not using KD
            var watchNoKD = System.Diagnostics.Stopwatch.StartNew();
            Synthesis.startSynthesis(texture, finalSizeValue[i], patchSize, overlapSize, true, false, $"Benchmark_Saved/{filename_noKD}", lambdaValue);
            watchNoKD.Stop();
            var elapsedMs_noKD = watchNoKD.ElapsedMilliseconds;
            saveNoKDTime[i] = elapsedMs_noKD;
        }

        saveToCSV(finalSizeValue.Length, saveKDTime, saveNoKDTime, "Assets/Saved/Final_Image/Benchmark_Saved/KD_Compare.csv");
        


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
