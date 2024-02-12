using System.Collections.Generic;
using UnityEngine;

public class Passage
{
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
    }
}

public class Generator : MonoBehaviour 
{
    public Vector3 entrance = new Vector3(0, 4, 0);
    public int nodesAmount = 300;
    public int nodesLeft = 20;
    public float caveRadius = 5;
    public float passageLength = 0.4f;
    public float timeBetweenIterations = 0.2f;
    public float attractionRange = 1;
    public float killRange = 0.6f;
    public float randomGrowth = 0.1f;

    private List<Vector3> nodes = new List<Vector3>();
    private List<int> activeNodes = new List<int>();
    private Passage firstPassage;
    private List<Passage> passages = new List<Passage>();
    private List<Passage> extremities = new List<Passage>();
    private float timeSinceLastIteration = 0f;

    private void Start()
    {
        // create nodes
        GenerateNodes(nodesAmount, caveRadius / 2);

        // create cave entrance passage
        firstPassage = new Passage(entrance, entrance - new Vector3(0, passageLength, 0), new Vector3(0, -1, 0));
        passages.Add(firstPassage);
        extremities.Add(firstPassage);
    }

    private void Update() 
    {
        if (nodes.Count <= nodesLeft) nodes.Clear();
        
        timeSinceLastIteration += Time.deltaTime;

        if (timeSinceLastIteration > timeBetweenIterations)
        {
            timeSinceLastIteration = 0f;

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

            if (nodes.Count > 0)
            {
                activeNodes.Clear();

                for (int i = 0; i < passages.Count; i++) passages[i].attractors.Clear();

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
                else 
                {
                    for (int i = 0; i < extremities.Count; i++) 
                    {
                        Passage extremity = extremities[i];
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
    }

    private Vector3 RandomGrowthVector()
    {
        float alpha = Random.Range(0f, Mathf.PI);
        float theta = Random.Range(0f, Mathf.PI * 2f);
        Vector3 pt = new Vector3(Mathf.Cos(theta) * Mathf.Sin(alpha), Mathf.Sin(theta) * Mathf.Sin(alpha), Mathf.Cos(alpha));
        return pt * randomGrowth;
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

    private void OnDrawGizmos() 
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(new Vector3(node.x, node.y, node.z), 0.05f);
        }
        
        for (int i = 0; i < passages.Count; i++)
        {
            Passage passage = passages[i];

            if (extremities.Contains(passage) && nodes.Count > nodesLeft) // extremities during generation
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(passage.start, passage.end);
            }
            else // other passages
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(passage.start, passage.end);
            }
        }
    }
}
