using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class global : MonoBehaviour
{
    public struct patchData
    {
        public static int patchSize;
        public static int overlapSize;
        public static int totalNumPatches;
    }

    public struct refImgData
    {
        public static int refImgSize;
        public static int refImgWidth;
        public static int refImgHeight;
    }

    // number of unique blocks made by the player
    public static int blockCount = 0;


    // for indicating if user is holding down the mouse
    public static bool mouseHoldLeft = false;
    public static bool mouseHoldRight = false;

    // keep track which state we are in
    public static State_List.States current_state;


    // track if we want to debug
    public struct debugKD
    {
        public static bool debug = false;
        public static int index_KD = 0;
        public static int index_NoKD = 0;
        public static float[] time_KD = new float[10];
        public static float[] time_NoKD = new float[10];
    }


    public struct debugCPU_GPU
    {
        public static bool debug = false;
        public static int index_CPU = 0;
        public static int index_GPU = 0;
        public static float[] time_CPU = new float[10];
        public static float[] time_GPU = new float[10];
    }
}
