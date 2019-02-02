// <copyright file="PointcloudVisualizer.cs" company="Google">
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
    /// Visualize the point cloud.
    /// </summary>
    public class PointcloudVisualizer : MonoBehaviour
    {
        private const int k_MaxPointCount = 61440;

        private Mesh m_Mesh;

        private Vector3[] m_Points = new Vector3[k_MaxPointCount];

        private Vector3 center;
        private float boxExtent;
        public GameObject focusMarker;

        /// <summary>
        /// Unity start.
        /// </summary>
        public void Start()
        {
            m_Mesh = GetComponent<MeshFilter>().mesh;
            m_Mesh.Clear();
        }

        /// <summary>
        /// Unity update.
        /// </summary>
        public void Update()
        {
            // Fill in the data to draw the point cloud.
            if (Frame.PointCloud.IsUpdatedThisFrame)
            {
                center = Vector3.zero;
                List<float> distances = new List<float>();
                List<Vector3> points = new List<Vector3>();
                List<Vector3> confidentPoints = new List<Vector3>();
                boxExtent = 0f;
                // Copy the point cloud points for mesh verticies.
                for (int i = 0; i < Frame.PointCloud.PointCount; i++)
                {
                    Vector3 pointPos = Frame.PointCloud.GetPointAsStruct(i).Position;
                    float confidence = Frame.PointCloud.GetPointAsStruct(i).Confidence;
                    m_Points[i] = Frame.PointCloud.GetPointAsStruct(i);
                    points.Add(pointPos);

                    int layer_mask = LayerMask.GetMask("POI");
                    RaycastHit hit;

                    if (confidence > 0.3f && Physics.Raycast(pointPos, new Vector3(0, -1, 0), out hit, 0.5f))
                    {
                        distances.Add(hit.distance);
                        confidentPoints.Add(pointPos);
                    }
                }

                for (int i = 0; i < confidentPoints.Count; i++) {
                    Vector3 v = confidentPoints[i];

                    for (int n = i + 1; n < confidentPoints.Count; n++) {

                        if (Vector3.Distance(confidentPoints[n], v) <= 0.1f) {
                            Debug.DrawLine(v, confidentPoints[n], Color.red, 0.3f);
                        }

                    }

                }

                // Update the mesh indicies array.
                int[] indices = new int[Frame.PointCloud.PointCount];
                for (int i = 0; i < Frame.PointCloud.PointCount; i++)
                {
                    indices[i] = i;

                    Vector3 pointPos = Frame.PointCloud.GetPointAsStruct(i).Position;
                    distances.Add(Vector3.Distance(pointPos, center));
                }

                m_Mesh.Clear();
                m_Mesh.vertices = m_Points;
                m_Mesh.SetIndices(indices, MeshTopology.Points, 0);


                // if (Frame.PointCloud.PointCount < 30) {
                //     return;
                // }

                // distances.Sort();
                // points.Sort((a, b) => a.x.CompareTo(b.x));

                // int trim = Mathf.FloorToInt(distances.Count * 0.4f);

                // for (int i = trim; i < distances.Count - trim; i++) {
                //     boxExtent += distances[i];
                //     center += points[i];
                // }

                // boxExtent /= 100 * (distances.Count);
                // center = center / (Frame.PointCloud.PointCount - (trim * 2));
                // if (!float.IsNaN(center.x)) {
                //     focusMarker.transform.position = center;
                // }
            }
        }


        // void OnDrawGizmos()
        // {
        // // Draw a yellow sphere at the transform's position
        //     Gizmos.color = Color.yellow;
        //     Gizmos.DrawWireSphere(center, 0.05f);
        //     //Gizmos.DrawWireCube(center, new Vector3(boxExtent, boxExtent, boxExtent));
        // }
    }
}