using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class ScreenCap : MonoBehaviour
{
    public delegate void CaptureEvent(Vector2Int captureSize, Vector2Int captureStartLocation, int id);
    public static CaptureEvent onCapture;
    public Vector2Int CaptureSize;
    public Vector2Int CaptureLocation;
    // Start is called before the first frame update
    private void Start()
    {
        //yield return captureScreen(CaptureSize, CaptureLocation);
        onCapture += startCapture;

    }

    void startCapture(Vector2Int captureSize, Vector2Int captureStartLocation, int id) {
        StartCoroutine(captureScreen(captureSize, captureStartLocation, id));
    }

    public static IEnumerator captureScreen(Vector2Int captureSize, Vector2Int captureStartLocation, int id)
    {
        // We should only read the screen buffer after rendering is complete
        yield return new WaitForEndOfFrame();

        // Create a texture the size of the screen, RGB24 format
        int width = captureSize.x;
        int height = captureSize.y;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

        int startX = captureStartLocation.x;
        int startY = captureStartLocation.y;
        int endX = startX + width;
        int endY = startY + height;
        // Read screen contents into the texture
        tex.ReadPixels(new Rect(startX, startY, endX, endY), 0, 0);
        tex.Apply();

        // Encode texture into PNG
        byte[] bytes = tex.EncodeToPNG();
        Object.Destroy(tex);

        // For testing purposes, also write to a file in the project folder
        File.WriteAllBytes(Application.dataPath + "/../captures/" + id.ToString() + ".png", bytes);
        Debug.Log("cap");
    }
}
