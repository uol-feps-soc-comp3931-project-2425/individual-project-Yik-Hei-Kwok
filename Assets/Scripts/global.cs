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

}
