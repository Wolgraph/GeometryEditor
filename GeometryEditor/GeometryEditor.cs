namespace GeometryEditor
{
    public struct SimplifiedMesh
    {
        public int[] indices;
        public Vector3[] points;
    }
    public static class MeshEditor
    {
        public static SimplifiedMesh Unify(Vector3[] vertices, int[] triangles)
        {
            // Definizione variabili
            int[] IndexList = new int[triangles.Length];
            for (int i = 0; i < triangles.Length; i++) IndexList[i] = triangles[i];
            // Calcola il baricentro della Mesh
            Vector3 CentreofMesh = new Vector3(0, 0, 0);
            for (int i = 0; i < vertices.Length; i++)
            {

                CentreofMesh.x += vertices[i].x;
                CentreofMesh.y += vertices[i].y;
                CentreofMesh.z += vertices[i].z;
            }
            CentreofMesh /= vertices.Length;
            // Determina il vertice più distante
            int Mostdistant = 0; // L'indice da ritornare
            float maxDistance = 0; // Valore di confronto
            for (int i = 0; i < vertices.Length; i++)
            {
                float distance = Mathf.Sqrt(
                    (Mathf.Pow(vertices[i].x - CentreofMesh.x, 2) +
                    (Mathf.Pow(vertices[i].y - CentreofMesh.y, 2)) +
                    (Mathf.Pow(vertices[i].z - CentreofMesh.z, 2))));
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    Mostdistant = i;
                }
            }
            // Trova la prima terna di vertici da valutare
            int[] currentindices = new int[3];
            for (int i = 0; i < triangles.Length; i += 3)
            {
                if (triangles[i] == Mostdistant | triangles[i + 1] == Mostdistant | triangles[i + 2] == Mostdistant)
                { // Controllare che indice viene salvato #############################################################################################################################################
                    currentindices[0] = i;             // #############################################################################################################################################
                    currentindices[1] = i + 1;         // #############################################################################################################################################
                    currentindices[2] = i + 2;         // #############################################################################################################################################
                    break;
                }
            }
            // Esegue il controllo sulla prima terna
            Vector3[] FirstVertices = new Vector3[3] {
                vertices[triangles[currentindices[0]]],
                vertices[triangles[currentindices[1]]],
                vertices[triangles[currentindices[2]]]};

            Vector3 CentreFace = new Vector3
            {
                x = (FirstVertices[0].x + FirstVertices[1].x + FirstVertices[2].x),
                y = (FirstVertices[0].y + FirstVertices[1].y + FirstVertices[2].y),
                z = (FirstVertices[0].z + FirstVertices[1].z + FirstVertices[2].z)
            };
            CentreFace /= 3;
            // Normalizzo il vettore centrale
            Vector3 u = new Vector3
            {
                x = FirstVertices[1].x - FirstVertices[0].x,
                y = FirstVertices[1].y - FirstVertices[0].y,
                z = FirstVertices[1].z - FirstVertices[0].z
            };
            Vector3 v = new Vector3
            {
                x = FirstVertices[2].x - FirstVertices[0].x,
                y = FirstVertices[2].y - FirstVertices[0].y,
                z = FirstVertices[2].z - FirstVertices[0].z
            };

            Vector3 Normal = Vector3.Cross(u, v).normalized;

            Vector3 Projection = new Vector3();
            if (Normal.x == 1 && Normal.y == 0 && Normal.z == 0)
            {
                Projection.x = CentreofMesh.x;
                Projection.y = CentreFace.y;
                Projection.z = CentreFace.z;
            }
            else if (Normal.x == 0 && Normal.y == 1 && Normal.z == 0)
            {
                Projection.x = CentreFace.x;
                Projection.y = CentreofMesh.y;
                Projection.z = CentreFace.z;
            }
            else if (Normal.x == 0 && Normal.y == 0 && Normal.z == 1)
            {
                Projection.x = CentreFace.x;
                Projection.y = CentreFace.y;
                Projection.z = CentreofMesh.z;
            }
            else
            {
                Projection = ProjectedPoint(CentreofMesh, Normal, CentreFace);
            }
            Vector3 CentralVector = new Vector3//Determino il vettore passante per il centro della faccia
            {
                x = (CentreFace.x - Projection.x),
                y = (CentreFace.y - Projection.y),
                z = (CentreFace.z - Projection.z)
            }; CentralVector.Normalize(); // Normalizzo il vettore centrale

            // Effettivo controllo dei due vettori
            if (CentralVector == -Normal)
            {
                int tmp = triangles[currentindices[0]];
                triangles[currentindices[0]] = triangles[currentindices[1]];
                triangles[currentindices[1]] = tmp;

                int tmpi = IndexList[currentindices[0]];
                IndexList[currentindices[0]] = IndexList[currentindices[1]];
                IndexList[currentindices[1]] = tmpi;
            }
            else if (!(CentralVector == -Normal) && !(CentralVector == Normal))
            {
                float Dotproduct = CentralVector.x * Normal.x + CentralVector.y * Normal.y + CentralVector.z * Normal.z;
                //Se il prodotto scalare è nullo, vuol dire che i vettori sono ortogonali e non non si tratta al momento questo caso.
                if (Dotproduct != 0)
                {
                    float x = CentralVector.x * Normal.x;
                    float y = CentralVector.y * Normal.y;
                    float z = CentralVector.z * Normal.z;
                    if (x <= 0 && y <= 0 && z <= 0)
                    {
                        int tmp = triangles[currentindices[0]];
                        triangles[currentindices[0]] = triangles[currentindices[1]];
                        triangles[currentindices[1]] = tmp;
                    }
                }
            }
            // Spunto la terna appena considerata
            IndexList[currentindices[0]] = IndexList[currentindices[0]] * -1 - IndexList.Length;
            IndexList[currentindices[1]] = IndexList[currentindices[1]] * -1 - IndexList.Length;
            IndexList[currentindices[2]] = IndexList[currentindices[2]] * -1 - IndexList.Length;
            int Count = 0;
            int icount = 0;
            int checkedtriangles = 1;
            bool foundsomething = false;
            do
            {
                for (int i = 0; i < triangles.Length;) // i rappresenta l'indice del vertice da valutare
                {
                    if (IndexList[i] < 0)
                    {
                        i += 3 - i % 3; // Non scrivo (3 - i % 3) perché la variabile viene incrementata di 1 al ciclo successivo
                        continue;
                    }
                    int j = -1; // Rappresenta il numero della terna del triangolo corrente
                    int r = -1;
                    if (IndexList[i] == triangles[currentindices[0]]) j = 0;
                    else if (IndexList[i] == triangles[currentindices[1]]) j = 1;
                    else if (IndexList[i] == triangles[currentindices[2]]) j = 2;
                    else
                    {
                        i++;
                        continue; // Qua si finisce se non si trova corrispondenza dell'indice con la terna corrente
                    }
                    r = i % 3;
                    switch (r)
                    {
                        case 0: // Corrispondenza al primo termine della terna dell'IndexList 
                            switch (j)
                            {
                                case 0: // IndexList[i] == triangles[currentindices[0]]
                                    if (IndexList[i + 1] == triangles[currentindices[1]] || IndexList[i + 2] == triangles[currentindices[2]])
                                    {
                                        int tmp = triangles[i];
                                        triangles[i] = triangles[i + 1];
                                        triangles[i + 1] = tmp;
                                        int tmpi = IndexList[i];
                                        IndexList[i] = IndexList[i + 1];
                                        IndexList[i + 1] = tmpi;

                                        IndexList[i] = IndexList[i] * -1;
                                        IndexList[i + 1] = IndexList[i + 1] * -1;
                                        IndexList[i + 2] = IndexList[i + 2] * -1;
                                        checkedtriangles++;
                                        foundsomething = true;
                                    }
                                    else if (IndexList[i + 1] == triangles[currentindices[2]] || IndexList[i + 2] == triangles[currentindices[1]])
                                    {
                                        IndexList[i] = IndexList[i] * -1;
                                        IndexList[i + 1] = IndexList[i + 1] * -1;
                                        IndexList[i + 2] = IndexList[i + 2] * -1;
                                        checkedtriangles++;
                                        foundsomething = true;

                                    };
                                    break;
                                case 1: // IndexList[i] == triangles[currentindices[1]]
                                    if (IndexList[i + 1] == triangles[currentindices[2]] || IndexList[i + 2] == triangles[currentindices[0]])
                                    {
                                        int tmp = triangles[i];
                                        triangles[i] = triangles[i + 1];
                                        triangles[i + 1] = tmp;
                                        int tmpi = IndexList[i];
                                        IndexList[i] = IndexList[i + 1];
                                        IndexList[i + 1] = tmpi;

                                        IndexList[i] = IndexList[i] * -1;
                                        IndexList[i + 1] = IndexList[i + 1] * -1;
                                        IndexList[i + 2] = IndexList[i + 2] * -1;
                                        checkedtriangles++;
                                        foundsomething = true;
                                    }
                                    else if (IndexList[i + 1] == triangles[currentindices[0]] || IndexList[i + 2] == triangles[currentindices[2]])
                                    {
                                        IndexList[i] = IndexList[i] * -1;
                                        IndexList[i + 1] = IndexList[i + 1] * -1;
                                        IndexList[i + 2] = IndexList[i + 2] * -1;
                                        checkedtriangles++;
                                        foundsomething = true;
                                    };
                                    break;
                                case 2: // IndexList[i] == triangles[currentindices[2]]
                                    if (IndexList[i + 1] == triangles[currentindices[0]] || IndexList[i + 2] == triangles[currentindices[1]])
                                    {
                                        int tmp = triangles[i];
                                        triangles[i] = triangles[i + 1];
                                        triangles[i + 1] = tmp;
                                        int tmpi = IndexList[i];
                                        IndexList[i] = IndexList[i + 1];
                                        IndexList[i + 1] = tmpi;

                                        IndexList[i] = IndexList[i] * -1;
                                        IndexList[i + 1] = IndexList[i + 1] * -1;
                                        IndexList[i + 2] = IndexList[i + 2] * -1;
                                        checkedtriangles++;
                                        foundsomething = true;

                                    }
                                    else if (IndexList[i + 1] == triangles[currentindices[2]] || IndexList[i + 2] == triangles[currentindices[0]])
                                    {
                                        IndexList[i] = IndexList[i] * -1;
                                        IndexList[i + 1] = IndexList[i + 1] * -1;
                                        IndexList[i + 2] = IndexList[i + 2] * -1;
                                        checkedtriangles++;
                                        foundsomething = true;

                                    };
                                    break;
                            }
                            break;
                        case 1: // Corrispondenza al primo termine della terna dell'IndexList
                            switch (j)
                            {
                                case 0: // Corrispondenza al primo termine
                                    if (IndexList[i + 1] == triangles[currentindices[1]])
                                    {
                                        int tmp = triangles[i];
                                        triangles[i] = triangles[i + 1];
                                        triangles[i + 1] = tmp;
                                        int tmpi = IndexList[i];
                                        IndexList[i] = IndexList[i + 1];
                                        IndexList[i + 1] = tmpi;

                                        IndexList[i] = IndexList[i] * -1;
                                        IndexList[i + 1] = IndexList[i + 1] * -1;
                                        IndexList[i - 1] = IndexList[i - 1] * -1;
                                        checkedtriangles++;
                                        foundsomething = true;
                                    }
                                    else if (IndexList[i + 1] == triangles[currentindices[2]])
                                    {
                                        IndexList[i] = IndexList[i] * -1;
                                        IndexList[i + 1] = IndexList[i + 1] * -1;
                                        IndexList[i - 1] = IndexList[i - 1] * -1;
                                        checkedtriangles++;
                                        foundsomething = true;
                                    }
                                    break;
                                case 1: // IndexList[i] == triangles[currentindices[1]]
                                    if (IndexList[i + 1] == triangles[currentindices[2]])
                                    {
                                        int tmp = triangles[i];
                                        triangles[i] = triangles[i + 1];
                                        triangles[i + 1] = tmp;
                                        int tmpi = IndexList[i];
                                        IndexList[i] = IndexList[i + 1];
                                        IndexList[i + 1] = tmpi;

                                        IndexList[i] = IndexList[i] * -1;
                                        IndexList[i + 1] = IndexList[i + 1] * -1;
                                        IndexList[i - 1] = IndexList[i - 1] * -1;
                                        checkedtriangles++;
                                        foundsomething = true;

                                    }
                                    else if (IndexList[i + 1] == triangles[currentindices[0]])
                                    {
                                        IndexList[i] = IndexList[i] * -1;
                                        IndexList[i + 1] = IndexList[i + 1] * -1;
                                        IndexList[i - 1] = IndexList[i - 1] * -1;
                                        checkedtriangles++;
                                        foundsomething = true;
                                    }
                                    break;
                                case 2: // IndexList[i] == triangles[currentindices[2]]
                                    if (IndexList[i + 1] == triangles[currentindices[0]])
                                    {
                                        int tmp = triangles[i];
                                        triangles[i] = triangles[i + 1];
                                        triangles[i + 1] = tmp;
                                        int tmpi = IndexList[i];
                                        IndexList[i] = IndexList[i + 1];
                                        IndexList[i + 1] = tmpi;

                                        IndexList[i] = IndexList[i] * -1;
                                        IndexList[i + 1] = IndexList[i + 1] * -1;
                                        IndexList[i - 1] = IndexList[i - 1] * -1;
                                        checkedtriangles++;
                                        foundsomething = true;

                                    }
                                    else if (IndexList[i + 1] == triangles[currentindices[1]])
                                    {
                                        IndexList[i] = IndexList[i] * -1;
                                        IndexList[i + 1] = IndexList[i + 1] * -1;
                                        IndexList[i - 1] = IndexList[i - 1] * -1;
                                        checkedtriangles++;
                                        foundsomething = true;
                                    }
                                    break;
                            }
                            break;
                    }
                    i += 3 - i % 3;
                }
                if (foundsomething == false) Count++;
                else
                {
                    currentindices[0] -= triangles.Length;
                    currentindices[1] -= triangles.Length;
                    currentindices[2] -= triangles.Length;
                    Count = 0;
                    foundsomething = false;
                }
                icount = 0;
                for (int j = 0; j < triangles.Length; j += 3)
                {
                    if (icount >= Count && (IndexList[j] > -triangles.Length && IndexList[j] < 0))
                    {
                        currentindices[0] = j;
                        currentindices[1] = j + 1;
                        currentindices[2] = j + 2;
                        break;
                    }
                    else if (icount < Count && (IndexList[j] > -triangles.Length && IndexList[j] < 0))
                    {
                        icount++;
                    }
                }
                if (Count > triangles.Length + 1)
                {
                    UnityEngine.Debug.Log("Ciclo do...While bloccato");
                    break;
                }
            } while (checkedtriangles < triangles.Length / 3);
            Vector3[] Uvertices = new Vector3[triangles.Length];
            int[] Utriangles = new int[triangles.Length];
            Vector2[] uvs = new Vector2[triangles.Length];
            Vector2 scaleFactor = new Vector2 { x = 0.1f, y = 0.1f };
            for (int i = 0; i < triangles.Length; i++)
            {
                Uvertices[i] = vertices[triangles[i]];
                Utriangles[i] = i;
                // Get the three vertices bounding this triangle.
                Vector3 v1 = vertices[triangles[i]];
                Vector3 v2 = vertices[triangles[i + 1]];
                Vector3 v3 = vertices[triangles[i + 2]];

                // Compute a vector perpendicular to the face.
                Vector3 normal = Vector3.Cross(v3 - v1, v2 - v1);

                // Form a rotation that points the z+ axis in this perpendicular direction.
                // Multiplying by the inverse will flatten the triangle into an xy plane.
                Quaternion rotation = Quaternion.Inverse(Quaternion.LookRotation(normal));

                // Assign the uvs, applying a scale factor to control the texture tiling.
                uvs[triangles[i]] = (Vector2)(rotation * v1) * scaleFactor;
                uvs[triangles[i + 1]] = (Vector2)(rotation * v2) * scaleFactor;
                uvs[triangles[i + 2]] = (Vector2)(rotation * v3) * scaleFactor;
            }
            SimplifiedMesh Umesh;
            Umesh.indices = Utriangles;
            Umesh.points = Uvertices;

            return Umesh;
        }

        // Calcola la proiezione del punto centrale delle facce sulla retta normale (TERMINATO) ############################################
        static Vector3 ProjectedPoint(Vector3 Ptp, Vector3 nv, Vector3 Cf)
        {
            Vector3 v = new Vector3
            { x = nv.y, y = nv.x, z = 0 };// Primo vettore direttivo che identifica la retta normale
            Vector3 w = new Vector3
            { x = 0, y = nv.z, z = -nv.y };// Secondo vettore direttivo che identifica la retta normale
            Vector3 cp = new Vector3(); cp = CrossProduct(v, w);

            Vector3 Projection = new Vector3();
            Projection.x = (cp.x * Ptp.x - cp.y * (Cf.y - Ptp.y) - cp.z * ((Cf.z - nv.z * Ptp.z) / nv.z)) / (cp.x + cp.y * (nv.y / nv.x) + (cp.z * (1 / nv.x)));
            Projection.y = (Projection.x * nv.y / nv.x) + Cf.y;
            Projection.z = (Projection.x / nv.x) + (Cf.z / nv.z);
            return Projection;
        }

        // Prodotto Vettoriale tra i vettori direttori della retta normale (TERMINATO) #####################################################
        static Vector3 CrossProduct(Vector3 v, Vector3 w)
        {
            return new Vector3
            {
                x = v.y * w.z - v.z * w.y,
                y = v.z * w.x - v.x * w.z,
                z = v.x * w.y - v.y * w.x
            };
        }

        public static int[] FlipNormals(int[] triangles)
        {
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int tmp = triangles[i];
                triangles[i] = triangles[i + 1];
                triangles[i + 1] = tmp;
            }
            return triangles;
        }
    }

}
