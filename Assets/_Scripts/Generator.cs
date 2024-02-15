using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Passage
{
    public bool lit;
    public int id;
    public int verticesid;
    public Vector3 start;
    public Vector3 end;
    public Vector3 direction;
    public Passage parent;
    public List<Passage> children = new List<Passage>();
    public List<Vector3> attractors = new List<Vector3>();
    
    public Passage(Vector3 start, Vector3 end, Vector3 direction, Passage parent = null)
    {
        this.start = start;
        this.end = end;
        this.direction = direction;
        this.parent = parent;
        lit = false;
    }
}

public class Generator : MonoBehaviour
{
    [Header("Mesh Generation Settings")]
    public Material caveMaterial;
    public float passageWidth = 0.01f;
    public int subdivisions = 6;

    [Header("Space Colonization Settings")]
    public Vector3 entrance = new Vector3(0, -6, 0);
    public int nodesAmount = 1000;
    public int nodesLeft = 100;
    public float caveSize = 10;
    public float passageLength = 0.1f;
    public float attractionRange = 1.2f;
    public float killRange = 0.4f;
    public float randomGrowth = 0.4f;

    private List<Vector3> nodes = new List<Vector3>();
    private List<int> activeNodes = new List<int>();
    private Passage firstPassage;
    private List<Passage> passages = new List<Passage>();
    private List<Passage> extremities = new List<Passage>();
    private bool finished = false;
    private int indexToLight = 0;
    
    private void Start()
    {
        // create nodes
        GenerateNodes(nodesAmount, caveSize / 2);

        // create entrance
        firstPassage = new Passage(entrance, entrance + new Vector3(0, passageLength, 0), new Vector3(0, 1, 0));
        passages.Add(firstPassage);
        extremities.Add(firstPassage);

        // slowly lights up each passage
        InvokeRepeating("SlowlyLightPassages", 0.0f, 1f / 200f);
    }

    private void SlowlyLightPassages()
    {
        if (!finished || indexToLight > passages.Count - 1) return;
        SetPassageLight(true, passages[passages.Count - indexToLight - 1]);
        indexToLight++;
    }

    private void Update()
    {
        // iterate
        IterateSpaceColonization();

        // run once when done iterating
        if (!finished && nodes.Count == 0)
        {
            // generate mesh
            GenerateMesh();

            // assign id's
            for (int i = 0; i < passages.Count; i++) passages[i].id = i;

            // set finished to true
            finished = true;
        }
    }

    private void IterateSpaceColonization()
    {
        // cleanup leftover nodes
        if (nodes.Count > 0 && nodes.Count <= nodesLeft)
        {
            nodes.Clear();
            nodesAmount = nodes.Count;
        }

        // return if there are no nodes
        if (nodes.Count == 0) return;

        // cleanup
        for (int i = nodes.Count - 1; i >= 0; i--)
        {
            for (int j = 0; j < passages.Count; j++)
            {
                if (Vector3.Distance(passages[j].end, nodes[i]) < killRange)
                {
                    nodes.Remove(nodes[i]);
                    nodesAmount--;
                    break;
                }
            }
        }
        activeNodes.Clear();
        for (int i = 0; i < passages.Count; i++) passages[i].attractors.Clear();

        // calculate active nodes
        for (int ia = 0; ia < nodes.Count; ia++)
        {
            float min = 999999f;
            Passage closest = null;

            for (int j = 0; j < passages.Count; j++) 
            {
                float d = Vector3.Distance(passages[j].end, nodes[ia]);
                if (d < attractionRange && d < min) 
                {
                    min = d;
                    closest = passages[j];
                }
            }

            if (closest != null)
            {
                closest.attractors.Add(nodes[ia]);
                activeNodes.Add(ia);
            }
        }

        // if nodes are active
        if (activeNodes.Count != 0)
        {
            extremities.Clear();
            List<Passage> newBranches = new List<Passage>();

            for (int i = 0; i < passages.Count; i++)
            {
                if (passages[i].attractors.Count > 0)
                {
                    Vector3 dir = new Vector3(0, 0, 0);
                    for (int j = 0; j < passages[i].attractors.Count; j++) dir += (passages[i].attractors[j] - passages[i].end).normalized;
                    dir /= passages[i].attractors.Count;
                    dir += RandomGrowthVector();
                    dir.Normalize();
                    Passage passage = new Passage(passages[i].end, passages[i].end + dir * passageLength, dir, passages[i]);
                    passages[i].children.Add(passage);
                    newBranches.Add(passage);
                    extremities.Add(passage);
                } 
                else if (passages[i].children.Count == 0) extremities.Add(passages[i]);
            }

            passages.AddRange(newBranches);
        } 
        else // if nodes arent active
        {
            for (int i = 0; i < extremities.Count; i++)
            {
                Passage extremity = extremities[i];
                bool extremityInRadius = Vector3.Distance(extremity.start, Vector3.zero) < caveSize / 2;
                bool beginning = passages.Count < 20;

                if (extremityInRadius || beginning)
                {
                    Vector3 start = extremity.end;
                    Vector3 dir = extremity.direction + RandomGrowthVector();
                    Vector3 end = extremity.end + dir * passageLength;
                    Passage passage = new Passage(start, end, dir, extremity);
                    
                    extremity.children.Add(passage);
                    passages.Add(passage);
                    extremities[i] = passage;
                }
            }
        }
    }

    private void GenerateMesh()
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[(passages.Count + 1) * subdivisions * 2];
        int[] triangles = new int[passages.Count * subdivisions * 6];

        // Construct vertices
        for (int i = 0; i < passages.Count; i++)
        {
            Passage passage = passages[i];
            int id = subdivisions * i;
            passage.verticesid = id;

            for (int j = 0; j < subdivisions; j++)
            {
                float alpha = (float)j / subdivisions * Mathf.PI * 2f;
                Quaternion orientationRing = Quaternion.FromToRotation(Vector3.up, passage.direction);
                Vector3 aroundRing = new Vector3(Mathf.Cos(alpha) * passageWidth, 0, Mathf.Sin(alpha) * passageWidth);
                Vector3 offset = orientationRing * aroundRing;
                Vector3 extra = passage.direction * passageWidth / 4;
                
                Vector3 firstRingVertex = passage.end + extra + offset;
                Vector3 secondRingVertex = passage.start - extra + offset;

                // set vertices
                int half = (passages.Count + 1) * subdivisions;
                vertices[id + j] = firstRingVertex - transform.position;
                vertices[id + j + half] = secondRingVertex - transform.position;

                // first passage vertices
                if (passage.parent == null) vertices[passages.Count * subdivisions + j] = passage.start + new Vector3(Mathf.Cos(alpha) * passageWidth, 0, Mathf.Sin(alpha) * passageWidth) - transform.position;
            }
        }

        // Construct faces
        for (int i = 0; i < passages.Count; i++)
        {
            Passage passage = passages[i];
            int half = (passages.Count + 1) * subdivisions;
            int triangleID = i * subdivisions * 2 * 3;

            int startVertexID = passage.verticesid + half;
            int endVertexID = passage.verticesid;

            for (int j = 0; j < subdivisions; j++)
            {
                // start of triangles
                triangles[triangleID + j * 6] = startVertexID + j;
                triangles[triangleID + j * 6 + 1] = endVertexID + j;

                if (j == subdivisions - 1) // last triangles
                {
                    triangles[triangleID + j * 6 + 2] = endVertexID;
                    triangles[triangleID + j * 6 + 3] = startVertexID + j;
                    triangles[triangleID + j * 6 + 4] = endVertexID;
                    triangles[triangleID + j * 6 + 5] = startVertexID;
                }
                else // other triangles
                {
                    triangles[triangleID + j * 6 + 2] = endVertexID + j + 1;
                    triangles[triangleID + j * 6 + 3] = startVertexID + j;
                    triangles[triangleID + j * 6 + 4] = endVertexID + j + 1;
                    triangles[triangleID + j * 6 + 5] = startVertexID + j + 1;
                }
            }
        }
        
        

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = InitialVertexColors(vertices.Length);
        mesh.RecalculateNormals();
        gameObject.AddComponent<MeshFilter>().mesh = mesh;
        gameObject.AddComponent<MeshRenderer>().material = caveMaterial;
    }

    public void SetPassageLight(bool lit, Passage passage)
    {
        passage.lit = lit;
        Color color = passage.lit ? Color.yellow * 2 : Color.black;
        int half = (passages.Count + 1) * subdivisions;

        Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh;
        Color[] colors = mesh.colors;
        for (int i = 0; i < subdivisions; i++)
        {
            colors[passage.verticesid + i] = color;
            colors[passage.verticesid + i + half] = color;
        }
        mesh.colors = colors;
    }

    private Color[] InitialVertexColors(int amount)
    {
        Color[] colors = new Color[amount];
        for (int i = 0; i < passages.Count; i++)
        {
            Passage passage = passages[i];
            Color color = passage.lit ? Color.yellow * 2 : Color.black;
            int half = (passages.Count + 1) * subdivisions;
            for (int j = 0; j < subdivisions; j++)
            {
                colors[passage.verticesid + j] = color;
                colors[passage.verticesid + j + half] = color;
            }
        }
        return colors;
    }

    private void GenerateNodes(int n, float r)
    {
        for (int i = 0; i < n; i++)
        {
            float radius = Random.Range(0f, 1f);
            radius = Mathf.Pow(Mathf.Sin(radius * Mathf.PI / 2f), 0.8f);
            radius *= r;
            float alpha = Random.Range(0f, Mathf.PI);
            float theta = Random.Range(0f, Mathf.PI * 2f);
            Vector3 pt = new Vector3(radius * Mathf.Cos(theta) * Mathf.Sin(alpha), radius * Mathf.Sin(theta) * Mathf.Sin(alpha), radius * Mathf.Cos(alpha));
            pt += transform.position;
            nodes.Add(pt);
        }
    }

    private Vector3 RandomGrowthVector()
    {
        float alpha = Random.Range(0f, Mathf.PI);
        float theta = Random.Range(0f, Mathf.PI * 2f);
        Vector3 pt = new Vector3(Mathf.Cos(theta) * Mathf.Sin(alpha), Mathf.Sin(theta) * Mathf.Sin(alpha), Mathf.Cos(alpha));
        return pt * randomGrowth;
    }

    static bool CheckAngleDifference(Vector3 vector1, Vector3 vector2, float thresholdDegrees)
    {
        vector1 = Vector3.Normalize(vector1);
        vector2 = Vector3.Normalize(vector2);
        float dotProduct = Vector3.Dot(vector1, vector2);
        float angle = Mathf.Acos(dotProduct);
        float angleDegrees = angle * Mathf.Rad2Deg;
        return angleDegrees > thresholdDegrees;
    }
}
