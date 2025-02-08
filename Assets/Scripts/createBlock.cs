using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class createBlock : MonoBehaviour
{
    public int scaleCube = 1;

    private Bounds cubeBounds;
    void Start()
    {
        cubeBounds = createNewBlock(scaleCube);
    }
    private void Update()
    {
        createNewBlockUpdate(cubeBounds);
    }

    private Bounds createNewBlock(int cubeSize)
    {
        GameObject cubeInstance = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cubeInstance.transform.localScale = new Vector3(cubeSize, cubeSize, cubeSize);

        // get size of the cube in world space
        Renderer cubeRen = cubeInstance.GetComponent<Renderer>();
        Debug.Log("cube: cube size in world units = " + cubeRen.bounds.size);

        // get bounds of renderer
        Bounds bound = cubeRen.bounds;

        // get corners of one side of a cube
        // top right

        return bound;
        
    }

    private void createNewBlockUpdate(Bounds bound)
    {
        Vector3 faceTopRight = new Vector3(bound.max.x, bound.max.y, bound.max.z);
        Vector3 faceTopLeft = new Vector3(bound.min.x, bound.max.y, bound.max.z);
        Vector3 faceBottomRight = new Vector3(bound.max.x, bound.min.y, bound.max.z);
        Vector3 faceBottomLeft = new Vector3(bound.min.x, bound.min.y, bound.max.z);

        // get width and height of the sides of the cube
        float widthInPixels = Vector3.Distance(faceTopRight, faceTopLeft);
        float heightInPixels = Vector3.Distance(faceTopRight, faceBottomRight);

        Debug.Log($"cube: cube sides in pixels = ({widthInPixels},{heightInPixels})");
    }
}
