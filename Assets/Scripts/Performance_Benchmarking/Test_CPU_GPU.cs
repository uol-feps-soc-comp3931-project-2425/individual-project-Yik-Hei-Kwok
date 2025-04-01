using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static global;
using System.IO;
using System.Text;

public class Test_CPU_GPU : MonoBehaviour
{
    // Use same texture of different sizes, test using CPU and GPU for texture segmentation
    string[] inputFileNames = new string[4];
    int fixedFinalSize = 300;
    string[] inputSizes = new string[4];
    
    public Texturesynthesis Synthesis;
    void Start()
    {
        inputSizes[0] = "500"; inputSizes[1] = "1000"; inputSizes[2] = "1500"; inputSizes[3] = "2000";
        inputFileNames[0] = "500x500.png";
        inputFileNames[1] = "1000x1000.png";
        inputFileNames[2] = "1500x1500.png";
        inputFileNames[3] = "2000x2000.png";


        debugCPU_GPU.debug = true;

        for (int i = 0; i < inputFileNames.Length; i++)
        {
            Texture2D texture = new Texture2D(2, 2);
            var fileContent = File.ReadAllBytes($"Assets/Ref_Img/Test_CPU_GPU/{inputFileNames[i]}");
            texture.LoadImage(fileContent);


            // for texture 2, patch size = 40 and boundary size = 6
            int patchSize = (int)(0.27f * Mathf.Min(texture.width, texture.height));
            // set overlap size as 1/6 of patch size
            int overlapSize = patchSize / 6;
            float lambdaValue = 0.4f;


            string filename_CPU = $"CPU_Benchmark_Input_Size{inputSizes[i]}";
            string filename_GPU = $"GPU_Benchmark_Input_Size{inputSizes[i]}";

            // benchmark the execution time for using CPU
            Synthesis.startSynthesis(texture, fixedFinalSize, patchSize, overlapSize, false, true, $"Benchmark_CPU_GPU_Saved/{filename_CPU}", lambdaValue);

            // benchmark the execution time for using GPU
            Synthesis.startSynthesis(texture, fixedFinalSize, patchSize, overlapSize, true, true, $"Benchmark_CPU_GPU_Saved/{filename_GPU}", lambdaValue);
        }

        saveToCSV(inputFileNames.Length, debugCPU_GPU.time_CPU, debugCPU_GPU.time_GPU, "Assets/Saved/Final_Image/Benchmark_CPU_GPU_Saved/CPU_GPU_Compare.csv");

        debugCPU_GPU.debug = false;

    }



    private void saveToCSV(int increment, float[] saveCPUTime, float[] saveGPUTime, string filePath)
    {
        var csv = new StringBuilder();

        for (int i = 0; i < increment; i++)
        {
            //in your loop
            //float first = saveKDTime;
            //float second = image.ToString();
            //Suggestion made by KyleMit
            var newLine = string.Format("{0},{1}", saveCPUTime[i], saveGPUTime[i]);
            csv.AppendLine(newLine);
        }


        //after your loop
        File.WriteAllText(filePath, csv.ToString());
    }
}
