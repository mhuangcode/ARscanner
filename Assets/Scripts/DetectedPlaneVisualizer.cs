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
        private Vector3 m_PlaneCenter = new Vector3();

        private List<Color> m_MeshColors = new List<Color>();

        private List<int> m_MeshIndices = new List<int>();


        private Mesh m_Mesh;

        private MeshRenderer m_MeshRenderer;

        // 0 = top left, 1 = top right, 2 = bottom left, 3 = bottom right
        List<GameObject> markers = new List<GameObject>();
        public GameObject markerObject;
        public GameObject markerVolume;
        public Vector3 center = Vector3.zero;
        public Vector3 minDimentions = Vector3.zero;
        public Vector3 maxDimentions = Vector3.zero;
        public bool isFloor = false;
        // public GameObject LayerManager;
        GameObject pointOfInterest;
        public GameObject volumeInfoText;
        GameObject infoText;
        GameObject centerTracker;

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

            if (!isFloor) {
                pointOfInterest = Instantiate(markerVolume, Vector3.zero, Quaternion.identity);
                pointOfInterest.transform.localScale = Vector3.zero;
                pointOfInterest.transform.parent = gameObject.transform;
                infoText = Instantiate(volumeInfoText, Vector3.zero, Quaternion.identity);
                infoText.transform.parent = gameObject.transform;
            }
        }

        public void updateMarkers() {
            // Debug.Log("Min " + minDimentions);
            // Debug.Log("Max " + maxDimentions);
            // markers[0].transform.position = new Vector3(minDimentions.x, minDimentions.y, minDimentions.z);
            // markers[1].transform.position = new Vector3(minDimentions.x, minDimentions.y, maxDimentions.z);
            // markers[2].transform.position = new Vector3(maxDimentions.x, maxDimentions.y, minDimentions.z);
            // markers[3].transform.position = new Vector3(maxDimentions.x, maxDimentions.y, maxDimentions.z);

            markers[0].transform.position = new Vector3(center.x - (m_DetectedPlane.ExtentX / 2), minDimentions.y, center.z - (m_DetectedPlane.ExtentZ / 2));
            markers[1].transform.position = new Vector3(center.x - (m_DetectedPlane.ExtentX / 2), minDimentions.y, center.z + (m_DetectedPlane.ExtentZ / 2));
            markers[2].transform.position = new Vector3(center.x + (m_DetectedPlane.ExtentX / 2), minDimentions.y, center.z - (m_DetectedPlane.ExtentZ / 2));
            markers[3].transform.position = new Vector3(center.x + (m_DetectedPlane.ExtentX / 2), minDimentions.y, center.z + (m_DetectedPlane.ExtentZ / 2));
            centerTracker.transform.position =  m_PlaneCenter;
            centerTracker.transform.rotation = m_DetectedPlane.CenterPose.rotation;

            if (!isFloor && m_DetectedPlane.PlaneType == (DetectedPlaneType)0) {
                float distToFloor = getFloorDist();

                if (distToFloor == -1) {
                    return;
                }

                Vector3 poiCenter = center;
                poiCenter.y -=  distToFloor / 2;
                pointOfInterest.transform.position = poiCenter;
                pointOfInterest.transform.rotation = m_DetectedPlane.CenterPose.rotation;
                infoText.transform.position = poiCenter;

                Vector3 scale = maxDimentions - minDimentions;
                scale.x = Mathf.Abs(m_DetectedPlane.ExtentX);
                scale.y = Mathf.Abs(distToFloor);
                scale.z = Mathf.Abs(m_DetectedPlane.ExtentZ);
                pointOfInterest.transform.localScale = scale;

                infoText.GetComponent<TextBillboarding>().setText((scale.x * 100) + "cm x " + (scale.y * 100) + "cm x " + (scale.z * 100) + "cm");
            }
        }

        float getFloorDist() {
            int layer_mask = LayerMask.GetMask("Floor");
            RaycastHit hit;

            if (Physics.Raycast (center, pointOfInterest.transform.TransformDirection(Vector3.down), out hit, 100f, layer_mask))
            {
                Debug.Log(hit.distance);
                return hit.distance;
            }

            return -1;
        }

        public void setFloor(bool bFloor) {
            Debug.Log("Setting floor");
            isFloor = bFloor;
            setColorForOrientation();
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
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(m_PlaneCenter, 0.05f);
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
            else if (m_DetectedPlane.TrackingState != TrackingState.Tracking)
            {
                 m_MeshRenderer.enabled = false;
                 return;
            }

            m_MeshRenderer.enabled = true;

            _UpdateMeshIfNeeded();
        }

        /// <summary>
        /// Initializes the DetectedPlaneVisualizer with a DetectedPlane.
        /// </summary>
        /// <param name="plane">The plane to vizualize.</param>
        public void Initialize(DetectedPlane plane)
        {

            m_DetectedPlane = plane;
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

            m_PlaneCenter = m_DetectedPlane.CenterPose.position;

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
            center = new Vector3(m_PlaneCenter[0], m_PlaneCenter[1], m_PlaneCenter[2]);
            minDimentions = center;
            maxDimentions = center;

            // Add vertex 4 to 7.
            for (int i = 0; i < planePolygonCount; ++i)
            {
                Vector3 v = m_MeshVertices[i];



                // Vector from plane center to current point
                Vector3 d = v - m_PlaneCenter;

                float scale = 1.0f - Mathf.Min(featherLength / d.magnitude, featherScale);
                Vector3 newVert = (scale * d) + m_PlaneCenter;
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
