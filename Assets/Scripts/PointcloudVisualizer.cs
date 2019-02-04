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
        List<Bounds> clusterBoxes = new List<Bounds>();

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

                List<Bounds> clusterBounds = new List<Bounds>();

                for (int i = 0; i < confidentPoints.Count; i++) {
                    Bounds potentialCluster = new Bounds(confidentPoints[i], Vector3.zero);

                    int congruentVecs = 0;

                    for (int n = i + 1; n < confidentPoints.Count; n++) {
                        if (Vector3.Distance(confidentPoints[n], confidentPoints[i]) <= 0.8f) {
                            potentialCluster.Encapsulate(confidentPoints[n]);
                            congruentVecs++;
                        }
                    }

                    if (congruentVecs > 3) {
                        clusterBounds.Add(potentialCluster);
                        // Debug.DrawLine(maxClusterExtent, new Vector3(minClusterExtent.x, maxClusterExtent.y, maxClusterExtent.z), Color.magenta, 0.3f);
                        // Debug.DrawLine(maxClusterExtent, new Vector3(maxClusterExtent.x, maxClusterExtent.y, minClusterExtent.z), Color.magenta, 0.3f);
                        // Debug.DrawLine(minClusterExtent, new Vector3(maxClusterExtent.x, maxClusterExtent.y, minClusterExtent.z), Color.magenta, 0.3f);
                        // Debug.DrawLine(minClusterExtent, new Vector3(minClusterExtent.x, maxClusterExtent.y, maxClusterExtent.z), Color.magenta, 0.3f);
                    }

                }

                clusterBoxes = mergeClusters(clusterBounds);

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

        List<Bounds> mergeClusters(List<Bounds> clusters) {

            for (int i = clusters.Count - 1; i >= 0; i--) {
                Bounds bound = clusters[i];

                for (int n = clusters.Count - 1; n >= 0; n--) {
                    if (n != i && (bound.Intersects(clusters[n]))) {
                        clusters[n].Encapsulate(bound);
                        clusters.RemoveAt(i);
                        break;
                    }
                }
            }

            return clusters;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;

            foreach (Bounds bound in clusterBoxes) {
                Gizmos.DrawWireCube(bound.center, bound.extents * 1.5f);
            }
        }
    }
}