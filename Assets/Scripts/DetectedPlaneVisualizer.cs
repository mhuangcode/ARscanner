//-----------------------------------------------------------------------
// <copyright file="DetectedPlaneVisualizer.cs" company="Google">
//
// Copyright 2017 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace GoogleARCore.Examples.Common
{
    using System.Collections;
    using System.Collections.Generic;
    using GoogleARCore;
    using UnityEngine;

    /// <summary>
    /// Visualizes a single DetectedPlane in the Unity scene.
    /// </summary>
    public class DetectedPlaneVisualizer : MonoBehaviour
    {
        private static int s_PlaneCount = 0;

        private readonly Color[] k_PlaneColors = new Color[]
        {
            new Color(1.0f, 1.0f, 1.0f),
            new Color(0f, 0f, 1f),
            new Color(1f, 0f, 0f)
        };

        private DetectedPlane m_DetectedPlane;

        // Keep previous frame's mesh polygon to avoid mesh update every frame.
        private List<Vector3> m_PreviousFrameMeshVertices = new List<Vector3>();
        private List<Vector3> m_MeshVertices = new List<Vector3>();
        public Vector3 center = new Vector3();

        private List<Color> m_MeshColors = new List<Color>();

        private List<int> m_MeshIndices = new List<int>();


        private Mesh m_Mesh;

        private MeshRenderer m_MeshRenderer;

        // 0 = top left, 1 = top right, 2 = bottom left, 3 = bottom right
        List<GameObject> markers = new List<GameObject>();
        public GameObject markerObject;
        public GameObject markerVolume;
        public bool isFloor = false;
        // public GameObject LayerManager;
        GameObject pointOfInterest;
        public GameObject volumeInfoText;
        GameObject infoText;
        GameObject centerTracker;
        bool captured = false;

        /// <summary>
        /// The Unity Awake() method.
        /// </summary>
        public void Awake()
        {
            m_Mesh = GetComponent<MeshFilter>().mesh;
            m_MeshRenderer = GetComponent<UnityEngine.MeshRenderer>();
        }

        private void Start() {

            centerTracker = Instantiate(markerObject, Vector3.zero, Quaternion.identity);
            centerTracker.transform.parent = gameObject.transform;

            for (int i = 0; i < 4; i++) {
                GameObject newMarker = Instantiate(markerObject, Vector3.zero, Quaternion.identity);
                newMarker.transform.parent = centerTracker.transform;
                markers.Add(newMarker);
            }

            pointOfInterest = Instantiate(markerVolume, Vector3.zero, Quaternion.identity);
            pointOfInterest.transform.localScale = Vector3.zero;
            pointOfInterest.transform.parent = gameObject.transform;
            Color color = Random.ColorHSV(0f, 1f, 0.2f, 0.4f, 0.5f, 1f, 1.0f, 1.0f);
            pointOfInterest.GetComponent<Renderer>().material.SetColor("_Color1", color);
            color.a = 0.1f;
            pointOfInterest.GetComponent<Renderer>().material.SetColor("_Color", color);
            infoText = Instantiate(volumeInfoText, Vector3.zero, Quaternion.identity);
            infoText.transform.parent = gameObject.transform;

        }

        public void updateMarkers() {
            // Debug.Log("Min " + minDimentions);
            // Debug.Log("Max " + maxDimentions);
            // markers[0].transform.position = new Vector3(minDimentions.x, minDimentions.y, minDimentions.z);
            // markers[1].transform.position = new Vector3(minDimentions.x, minDimentions.y, maxDimentions.z);
            // markers[2].transform.position = new Vector3(maxDimentions.x, maxDimentions.y, minDimentions.z);
            // markers[3].transform.position = new Vector3(maxDimentions.x, maxDimentions.y, maxDimentions.z);

            // markers[0].transform.position = new Vector3(center.x - (m_DetectedPlane.ExtentX / 2), minDimentions.y, center.z - (m_DetectedPlane.ExtentZ / 2));
            // markers[1].transform.position = new Vector3(center.x - (m_DetectedPlane.ExtentX / 2), minDimentions.y, center.z + (m_DetectedPlane.ExtentZ / 2));
            // markers[2].transform.position = new Vector3(center.x + (m_DetectedPlane.ExtentX / 2), minDimentions.y, center.z - (m_DetectedPlane.ExtentZ / 2));
            // markers[3].transform.position = new Vector3(center.x + (m_DetectedPlane.ExtentX / 2), minDimentions.y, center.z + (m_DetectedPlane.ExtentZ / 2));


            markers[0].transform.localPosition = new Vector3(-m_DetectedPlane.ExtentX * 100, 0f, -m_DetectedPlane.ExtentZ * 100);
            markers[1].transform.localPosition = new Vector3(-m_DetectedPlane.ExtentX * 100, 0f, m_DetectedPlane.ExtentZ * 100);
            markers[2].transform.localPosition = new Vector3(m_DetectedPlane.ExtentX * 100, 0f, -m_DetectedPlane.ExtentZ * 100);
            markers[3].transform.localPosition = new Vector3(m_DetectedPlane.ExtentX * 100, 0f, m_DetectedPlane.ExtentZ * 100);

            centerTracker.transform.position = center;
            //centerTracker.transform.rotation = m_DetectedPlane.CenterPose.rotation;

            if (!isFloor && m_DetectedPlane.PlaneType == (DetectedPlaneType)0) {
                float distToFloor = getHeight();

                if (distToFloor == -1) {
                    return;
                }

                Vector3 poiCenter = center;
                poiCenter.y -=  distToFloor / 2;
                pointOfInterest.transform.position = poiCenter;
                pointOfInterest.transform.rotation = m_DetectedPlane.CenterPose.rotation;
                infoText.transform.position = poiCenter;

                Vector3 scale = new Vector3(Mathf.Abs(m_DetectedPlane.ExtentX) * 0.99f, distToFloor, Mathf.Abs(m_DetectedPlane.ExtentZ) * 0.99f);
                // scale.x = Mathf.Abs(m_DetectedPlane.ExtentX);
                // scale.y = Mathf.Abs(distToFloor);
                // scale.z = Mathf.Abs(m_DetectedPlane.ExtentZ);
                pointOfInterest.transform.localScale = scale;

                infoText.GetComponent<TextBillboarding>().setText(Mathf.Round(scale.x * 100) + "cm x " + Mathf.Round(scale.y * 100) + "cm x " + Mathf.Round(scale.z * 100) + "cm");
            }

            centerTracker.transform.rotation = m_DetectedPlane.CenterPose.rotation;
            //ScreenCap.captureScreen(new Vector2Int(500, 500), new Vector2Int(100, 300));
            captured = false;
        }

        bool getMarkerScreenSpace(out Vector2Int size, out Vector2Int start) {
            Vector2Int end = Vector2Int.zero;
            start = new Vector2Int(Screen.width, Screen.height);
            size = Vector2Int.zero;
            int missingPoints = 0;

            foreach (GameObject marker in markers) {
                Camera cam = Camera.main;

                Vector3 pointOnScreen = cam.WorldToScreenPoint(marker.transform.position);

                if ((pointOnScreen.x < 0) || (pointOnScreen.x > Screen.width) || (pointOnScreen.y < 0) || (pointOnScreen.y > Screen.height))
                {
                    missingPoints++;

                    if (missingPoints > 1) {
                        return false;
                    }
                }

                start.x = Mathf.RoundToInt(Mathf.Min(start.x, pointOnScreen.x));
                start.y = Mathf.RoundToInt(Mathf.Min(start.y, pointOnScreen.y));
                end.x = Mathf.RoundToInt(Mathf.Max(end.x, pointOnScreen.x));
                end.y = Mathf.RoundToInt(Mathf.Max(end.y, pointOnScreen.y));
            }

            foreach (GameObject marker in markers) {
                Camera cam = Camera.main;

                Vector3 pointOnScreen = cam.WorldToScreenPoint(marker.transform.position);
                pointOnScreen.y -= pointOfInterest.transform.localScale.y;

                if ((pointOnScreen.x < 0) || (pointOnScreen.x > Screen.width) || (pointOnScreen.y < 0) || (pointOnScreen.y > Screen.height))
                {
                    missingPoints++;

                    if (missingPoints > 4) {
                        return false;
                    }
                }

                start.x = Mathf.RoundToInt(Mathf.Min(start.x, pointOnScreen.x));
                start.y = Mathf.RoundToInt(Mathf.Min(start.y, pointOnScreen.y));
                end.x = Mathf.RoundToInt(Mathf.Max(end.x, pointOnScreen.x));
                end.y = Mathf.RoundToInt(Mathf.Max(end.y, pointOnScreen.y));

                 size = new Vector2Int((end.x - start.x), (end.y - start.y));

                Debug.Log("Visible");

                return true;
            }


            return false;
        }

        float getHeight() {
            int layer_mask = LayerMask.GetMask("Floor", "POI");
            RaycastHit hit;

            List<float> distances = new List<float>();

            for (int i = 0; i < markers.Count; i++) {
                if (Physics.Raycast(markers[i].transform.position, new Vector3(0, -1, 0), out hit, 100f, layer_mask))
                {
                    distances.Add(hit.distance);
                    //Debug.DrawRay(markers[i].transform.position, new Vector3(0, -1, 0) * hit.distance, Color.yellow, 5f);
                }
            }

            if (distances.Count < 1) {
                return -1;
            }

            int maxCount = 1;
            int count = 1;
            float ret = distances[0];

            for (int i = 1; i < distances.Count; i++) {
                if (distances[i] == distances[i - 1]) {
                    count++;
                } else {
                    if (count > maxCount) {
                        maxCount = count;
                        ret = distances[i - 1];
                    }

                    count = 1;
                }
            }

            if (count > maxCount) {
                ret = distances[distances.Count - 1];
            }

            if (ret < 0.01f) {
                return -1;
            }

            return ret;
        }

        public void setColorForOrientation() {
            if (isFloor) {
                m_MeshRenderer.material.SetColor("_GridColor", k_PlaneColors[0]);
            } else if (m_DetectedPlane.PlaneType == (DetectedPlaneType)0) { // downward facing plane (floor)
                 m_MeshRenderer.material.SetColor("_GridColor", k_PlaneColors[1]);
            } else if (m_DetectedPlane.PlaneType == (DetectedPlaneType)1) { // upward facing plane (ceiling)
                m_MeshRenderer.material.SetColor("_GridColor", k_PlaneColors[1]);
            } else { // verticle plane
                m_MeshRenderer.material.SetColor("_GridColor", k_PlaneColors[2]);
            }
        }
        void OnDrawGizmos()
        {
        // Draw a yellow sphere at the transform's position
            // Gizmos.color = Color.yellow;
            // Gizmos.DrawSphere(center, 0.05f);
        }

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            if (m_DetectedPlane == null)
            {
                return;
            }
            else if (m_DetectedPlane.SubsumedBy != null)
            {
                Destroy(gameObject);
                return;
            }
            else if (m_DetectedPlane.TrackingState != TrackingState.Tracking || isFloor)
            {
                 setRenderer(false);
                 return;
            }

        //    if (!captured) {
        //         Vector2Int size;
        //         Vector2Int start;

        //         if (getMarkerScreenSpace(out size, out start)) {
        //             ScreenCap.onCapture(size, start, gameObject.GetInstanceID());
        //             captured = true;
        //         }
        //    }
            //setRenderer(true);
            _UpdateMeshIfNeeded();
        }

        void setRenderer(bool isEnabled) {
            m_MeshRenderer.enabled = isEnabled;

            for (int i = 0; i < markers.Count; i++) {
                markers[i].GetComponent<MeshRenderer>().enabled = isEnabled;
            }

            if (!isFloor) {
                pointOfInterest.GetComponent<MeshRenderer>().enabled = isEnabled;
            }
        }

        /// <summary>
        /// Initializes the DetectedPlaneVisualizer with a DetectedPlane.
        /// </summary>
        /// <param name="plane">The plane to vizualize.</param>
        public void Initialize(DetectedPlane plane)
        {
            m_DetectedPlane = plane;
            m_MeshRenderer.enabled = false;
            // float distToFloor = getFloorDist();

            // if (Mathf.Approximately(0.01f, distToFloor) || distToFloor < 0.01f) {
            //     isFloor = true;
            // }

            setColorForOrientation();
            m_MeshRenderer.material.SetFloat("_UvRotation", Random.Range(0.0f, 360.0f));

            //Update();
            // LayerManager.GetComponent<LayerManager>().onInitiated(this);
        }

        /// <summary>
        /// Update mesh with a list of Vector3 and plane's center position.
        /// </summary>
        private void _UpdateMeshIfNeeded()
        {
            m_DetectedPlane.GetBoundaryPolygon(m_MeshVertices);

            if (_AreVerticesListsEqual(m_PreviousFrameMeshVertices, m_MeshVertices))
            {

                return;
            }

            m_PreviousFrameMeshVertices.Clear();
            m_PreviousFrameMeshVertices.AddRange(m_MeshVertices);

            center = m_DetectedPlane.CenterPose.position;

            Vector3 planeNormal = m_DetectedPlane.CenterPose.rotation * Vector3.up;

            m_MeshRenderer.material.SetVector("_PlaneNormal", planeNormal);

            int planePolygonCount = m_MeshVertices.Count;

            // The following code converts a polygon to a mesh with two polygons, inner
            // polygon renders with 100% opacity and fade out to outter polygon with opacity 0%, as shown below.
            // The indices shown in the diagram are used in comments below.
            // _______________     0_______________1
            // |             |      |4___________5|
            // |             |      | |         | |
            // |             | =>   | |         | |
            // |             |      | |         | |
            // |             |      |7-----------6|
            // ---------------     3---------------2
            m_MeshColors.Clear();

            // Fill transparent color to vertices 0 to 3.
            for (int i = 0; i < planePolygonCount; ++i)
            {
                m_MeshColors.Add(Color.clear);
            }

            // Feather distance 0.2 meters.
            const float featherLength = 0.2f;

            // Feather scale over the distance between plane center and vertices.
            const float featherScale = 0.2f;
            // center = new Vector3(center[0], center[1], center[2]);
            // minDimentions = center;
            // maxDimentions = center;

            // Add vertex 4 to 7.
            for (int i = 0; i < planePolygonCount; ++i)
            {
                Vector3 v = m_MeshVertices[i];



                // Vector from plane center to current point
                Vector3 d = v - center;

                float scale = 1.0f - Mathf.Min(featherLength / d.magnitude, featherScale);
                scale = 1f;
                Vector3 newVert = (scale * d) + center;
                m_MeshVertices.Add(newVert);

                // maxDimentions.x = Mathf.Max(newVert.x, maxDimentions.x);
                // minDimentions.x = Mathf.Min(newVert.x, minDimentions.x);
                // maxDimentions.z = Mathf.Max(newVert.z, maxDimentions.z);
                // minDimentions.z = Mathf.Min(newVert.z, minDimentions.z);
                // maxDimentions.y = Mathf.Max(newVert.y, maxDimentions.y);
                // minDimentions.y = Mathf.Min(newVert.y, minDimentions.y);

                m_MeshColors.Add(Color.white);
            }

            updateMarkers();


            m_MeshIndices.Clear();
            int firstOuterVertex = 0;
            int firstInnerVertex = planePolygonCount;

            // Generate triangle (4, 5, 6) and (4, 6, 7).
            for (int i = 0; i < planePolygonCount - 2; ++i)
            {
                m_MeshIndices.Add(firstInnerVertex);
                m_MeshIndices.Add(firstInnerVertex + i + 1);
                m_MeshIndices.Add(firstInnerVertex + i + 2);
            }


            // Generate triangle (0, 1, 4), (4, 1, 5), (5, 1, 2), (5, 2, 6), (6, 2, 3), (6, 3, 7)
            // (7, 3, 0), (7, 0, 4)
            for (int i = 0; i < planePolygonCount; ++i)
            {
                int outerVertex1 = firstOuterVertex + i;
                int outerVertex2 = firstOuterVertex + ((i + 1) % planePolygonCount);
                int innerVertex1 = firstInnerVertex + i;
                int innerVertex2 = firstInnerVertex + ((i + 1) % planePolygonCount);

                m_MeshIndices.Add(outerVertex1);
                m_MeshIndices.Add(outerVertex2);
                m_MeshIndices.Add(innerVertex1);

                m_MeshIndices.Add(innerVertex1);
                m_MeshIndices.Add(outerVertex2);
                m_MeshIndices.Add(innerVertex2);
            }

            m_Mesh.Clear();
            m_Mesh.SetVertices(m_MeshVertices);
            m_Mesh.SetTriangles(m_MeshIndices, 0);
            m_Mesh.SetColors(m_MeshColors);
        }

        private bool _AreVerticesListsEqual(List<Vector3> firstList, List<Vector3> secondList)
        {
            if (firstList.Count != secondList.Count)
            {
                return false;
            }

            for (int i = 0; i < firstList.Count; i++)
            {
                if (firstList[i] != secondList[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
