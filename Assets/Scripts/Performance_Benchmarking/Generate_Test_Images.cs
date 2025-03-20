using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class Generate_Test_Images : MonoBehaviour
{
    public Texturesynthesis Synthesis;
    // Start is called before the first frame update
    void Start()
    {
        string[] inputFileNames = new string[9];
        inputFileNames[0] = "R0.jpg"; inputFileNames[1] = "NR0.jpg"; inputFileNames[2] = "S0.jpg";
        inputFileNames[3] = "R1.jpg"; inputFileNames[4] = "NR1.jpg"; inputFileNames[5] = "S1.jpg";
        inputFileNames[6] = "R2.jpg"; inputFileNames[7] = "NR2.jpg"; inputFileNames[8] = "S2.jpg";

        string[] oututFileNames = new string[9];
        oututFileNames[0] = "Q1_500"; oututFileNames[1] = "Q2_500"; oututFileNames[2] = "Q3_500";
        oututFileNames[3] = "Q4_500"; oututFileNames[4] = "Q5_500"; oututFileNames[5] = "Q6_500";
        oututFileNames[6] = "Q7_500"; oututFileNames[7] = "Q8_500"; oututFileNames[8] = "Q9_500";

        for (int i = 0; i < 9; i++)
        {
            // create the corresponding texture
            Texture2D texture = new Texture2D(2, 2);
            var fileContent = File.ReadAllBytes($"Assets/Ref_Img/test/{inputFileNames[i]}");
            texture.LoadImage(fileContent);

            // for regular textures
            if(i == 0 || i == 3 || i == 6)
            {
                // generate 3 Non-KD images
                for (int j = 1; j < 4; j++) {
                    Synthesis.startSynthesis(texture, 500, 150, (int)(150 / 12), true, false, $"tests/{oututFileNames[i]}_NoKD{j}", 0.2f);
                }
                // generate 3 KD images
                for (int j = 1; j < 4; j++)
                {
                    Synthesis.startSynthesis(texture, 500, 150, (int)(150 / 12), true, true, $"tests/{oututFileNames[i]}_KD{j}", 0.2f);
                }
            }

            // for Non-regular textures
            if (i == 1 || i == 4 || i == 7)
            {
                // generate 3 Non-KD images
                for (int j = 1; j < 4; j++)
                {
                    Synthesis.startSynthesis(texture, 500, 150, (int)(150 / 12), true, false, $"tests/{oututFileNames[i]}_NoKD{j}", 0.3f);
                }
                // generate 3 KD images
                for (int j = 1; j < 4; j++)
                {
                    Synthesis.startSynthesis(texture, 500, 150, (int)(150 / 12), true, true, $"tests/{oututFileNames[i]}_KD{j}", 0.3f);
                }
            }

            // for Stochastic textures
            if (i == 2 || i == 5 || i == 8)
            {
                // generate 3 Non-KD images
                for (int j = 1; j < 4; j++)
                {
                    Synthesis.startSynthesis(texture, 500, 150, (int)(150 / 12), true, false, $"tests/{oututFileNames[i]}_NoKD{j}", 0.2f);
                }
                // generate 3 KD images
                for (int j = 1; j < 4; j++)
                {
                    Synthesis.startSynthesis(texture, 500, 150, (int)(150 / 12), true, true, $"tests/{oututFileNames[i]}_KD{j}", 0.2f);
                }
            }

        }
    }

    
}
