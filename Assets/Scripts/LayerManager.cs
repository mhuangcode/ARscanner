using System.Collections;
using System.Collections.Generic;
using GoogleARCore.Examples.Common;
using GoogleARCore;
using UnityEngine;

public class LayerManager : MonoBehaviour
{

    // Start is called before the first frame update

    List<DetectedPlaneVisualizer> LayerVisualizers = new List<DetectedPlaneVisualizer>();
    DetectedPlaneVisualizer floorLayer;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void onInitiated(DetectedPlaneVisualizer newLayer) {
        // if (floorLayer == null || newLayer.center.y < floorLayer.center.y) {
        //     floorLayer = newLayer;
        //     newLayer.setFloor(true);
        // }

        // LayerVisualizers.Add(newLayer);
    }
}
