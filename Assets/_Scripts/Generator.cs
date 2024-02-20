using System.Collections.Generic;
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
    public int initialNodeAmount = 1000;
    public int nodesLeft = 100;
    public float caveSize = 10;
    public float passageLength = 0.1f;
    public float attractionRange = 1.2f;
    public float killRange = 0.4f;
    public float randomGrowth = 0.4f;
    public float surfaceHeight = 2;

    [Header("Camera & Player Settings")]
    [Range(0.1f, 10)] public float cameraOffset = 2;
    [Range(0.001f, 0.1f)] public float cameraSpeed = 0.1f;
    [Range(0.1f, 1)] public float playerSpeed = 0.2f;

    private List<Vector3> nodes = new List<Vector3>();
    private List<int> activeNodes = new List<int>();
    private Passage firstPassage;
    private List<Passage> passages = new List<Passage>();
    private List<Passage> extremities = new List<Passage>();
    private bool finished = false;
    private int indexToLight = 0;
    private Passage playerEntrance;
    private Passage playerPassage;
    
    private void Start()
    {
        // create nodes
        GenerateNodes(initialNodeAmount, caveSize / 2);

        // create entrance
        firstPassage = new Passage(entrance, entrance + new Vector3(0, passageLength, 0), new Vector3(0, 1, 0));
        passages.Add(firstPassage);
        extremities.Add(firstPassage);

        // slowly lights up each passage
        InvokeRepeating("TestPlayer", 0.0f, playerSpeed);
    }

    private void TestPlayer()
    {
        if (!finished || indexToLight > passages.Count - 1) return;
        var current = GetParentByIndex(playerEntrance, indexToLight);
        if (current != null)
        {
            SetPassageLight(true, current);
            playerPassage = current;
        }
        indexToLight++;
    }

    Passage GetParentByIndex(Passage passage, int index)
    {
        if (passage.parent == null) return null;

        Passage parent = passage;
        for (int i = 0; i < index + 1; i++) 
            if (parent.parent != null) parent = parent.parent;
            else return null;

        return parent;
    }

    private void Update()
    {
        // move camera
        if (playerEntrance != null && playerPassage != null)
        {
            var current = Camera.main.transform.position;
            var next = playerPassage.end + Vector3.back * cameraOffset;
            Camera.main.transform.position = Vector3.Lerp(current, next, cameraSpeed);
        }

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

            // set player entrance
            playerEntrance = GetHighest();
            SetPassageLight(true, playerEntrance);
        }
    }

    Passage GetHighest()
    {
        Passage highest = null;
        foreach (var ex in extremities) if (highest == null || ex.end.y > highest.end.y) highest = ex;
        return highest;
    }

    private void IterateSpaceColonization()
    {
        // cleanup leftover nodes and return
        if (nodes.Count > 0 && nodes.Count <= nodesLeft) nodes.Clear();
        if (nodes.Count == 0) return;

        // remove nodes in killrange
        List<Vector3> toRemove = new List<Vector3>();
        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = 0; j < passages.Count; j++)
            {
                float distance = Vector3.Distance(nodes[i], passages[j].end);
                if (distance < killRange) toRemove.Add(nodes[i]);
            }
        }
        foreach (var node in toRemove) nodes.Remove(node);

        // reset attractors and active nodes
        for (int i = 0; i < passages.Count; i++) passages[i].attractors.Clear();
        activeNodes.Clear();

        // calculate active nodes and attractors
        for (int i = 0; i < nodes.Count; i++)
        {
            float lastDist = 10000;
            Passage closest = null;

            for (int j = 0; j < passages.Count; j++) 
            {
                float distance = Vector3.Distance(passages[j].end, nodes[i]);
                if (distance < attractionRange && distance < lastDist) 
                {
                    closest = passages[j];
                    lastDist = distance;
                }
            }

            if (closest != null)
            {
                closest.attractors.Add(nodes[i]);
                activeNodes.Add(i);
            }
        }

        // if there are nodes in attraction range
        if (activeNodes.Count != 0)
        {
            extremities.Clear();
            List<Passage> newPassages = new List<Passage>();

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
                    newPassages.Add(passage);
                    extremities.Add(passage);
                } 
                else if (passages[i].children.Count == 0) extremities.Add(passages[i]);
            }

            passages.AddRange(newPassages);
        }

        // if no active nodes
        if (activeNodes.Count == 0)
        {
            for (int i = 0; i < extremities.Count; i++)
            {
                Passage current = extremities[i];
                bool extremityInRadius = Vector3.Distance(current.start, Vector3.zero) < caveSize / 2;
                bool beginning = passages.Count < 20;

                if (extremityInRadius || beginning)
                {
                    Vector3 raw = current.direction + RandomGrowthVector();
                    Vector3 dir = raw.normalized;
                    Passage next = new Passage(current.end, current.end + dir * passageLength, dir, current);
                    current.children.Add(next);
                    passages.Add(next);
                    extremities.Remove(current);
                    extremities.Add(next);
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
                int half = (passages.Count + 1) * subdivisions;
                float part = (float)j / subdivisions * Mathf.PI * 2f;
                Quaternion ringRotation = Quaternion.FromToRotation(Vector3.up, passage.direction);
                Vector3 vertexRotation = new Vector3(Mathf.Cos(part) * passageWidth, 0, Mathf.Sin(part) * passageWidth);
                Vector3 offset = ringRotation * vertexRotation;
                Vector3 extra = passage.direction * passageWidth / 4;
                
                vertices[id + j] = passage.start - extra + offset;
                vertices[id + j + half] = passage.end + extra + offset;
            }
        }

        // Construct faces
        for (int i = 0; i < passages.Count; i++)
        {
            Passage passage = passages[i];
            int half = (passages.Count + 1) * subdivisions;

            int triangle = i * subdivisions * 6;
            int startVertex = passage.verticesid;
            int endVertex = passage.verticesid + half;

            // setup all triangles
            for (int j = 0; j < subdivisions; j++)
            {
                int offset = j == subdivisions - 1 ? 0 : j + 1;
                triangles[triangle + j * 6] = startVertex + j;
                triangles[triangle + j * 6 + 1] = endVertex + j;
                triangles[triangle + j * 6 + 2] = endVertex + offset;
                triangles[triangle + j * 6 + 3] = startVertex + j;
                triangles[triangle + j * 6 + 4] = endVertex + offset;
                triangles[triangle + j * 6 + 5] = startVertex + offset;
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
        Color color = passage.lit ? Color.yellow * 2 : Color.grey;
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
            Color color = passage.lit ? Color.yellow * 2 : Color.grey;
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
            if (pt.y <= surfaceHeight) nodes.Add(pt);
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
