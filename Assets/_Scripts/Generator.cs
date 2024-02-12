using System.Collections.Generic;
using UnityEngine;

public class Branch
{
    public Vector3 start;
    public Vector3 end;
    public Vector3 direction;
    public Branch parent;
    public List<Branch> children = new List<Branch>();
    public List<Vector3> attractors = new List<Vector3>();

    public Branch(Vector3 start, Vector3 end, Vector3 direction, Branch parent = null) 
    {
        this.start = start;
        this.end = end;
        this.direction = direction;
        this.parent = parent;
    }
}

public class Generator : MonoBehaviour 
{
    [Header("Generation parameters")]
    public Vector3 startPosition = new Vector3(0, -4, 0);
    public int nbAttractors = 300;
    public float radius = 5;
    public float branchLength = 0.4f;
    public float timeBetweenIterations = 0.2f;
    public float attractionRange = 1;
    public float killRange = 0.5f;
    public float randomGrowth = 0.1f;

    private List<Vector3> attractors = new List<Vector3>();
    private List<int> activeAttractors = new List<int>();
    private Branch firstBranch;
    private List<Branch> branches = new List<Branch>();
    private List<Branch> extremities = new List<Branch>();
    private float timeSinceLastIteration = 0f;

    private void Start()
    {
        // create nodes
        GenerateAttractors(nbAttractors, radius / 2);

        // create starting point
        firstBranch = new Branch(startPosition, startPosition + new Vector3(0, branchLength, 0), new Vector3(0, 1, 0));
        branches.Add(firstBranch);
        extremities.Add(firstBranch);
    }

    private void Update() 
    {
        timeSinceLastIteration += Time.deltaTime;

        if (timeSinceLastIteration > timeBetweenIterations)
        {
            timeSinceLastIteration = 0f;

            for (int i = attractors.Count - 1; i >= 0; i--)
            {
                for (int j = 0; j < branches.Count; j++) 
                {
                    if (Vector3.Distance(branches[j].end, attractors[i]) < killRange) 
                    {
                        attractors.Remove(attractors[i]);
                        nbAttractors--;
                        break;
                    }
                }
            }

            if (attractors.Count > 0)
            {
                activeAttractors.Clear();

                for (int i = 0; i < branches.Count; i++) branches[i].attractors.Clear();

                for (int ia = 0; ia < attractors.Count; ia++) 
                {
                    float min = 999999f;
                    Branch closest = null;

                    for (int j = 0; j < branches.Count; j++) 
                    {
                        float d = Vector3.Distance(branches[j].end, attractors[ia]);
                        if (d < attractionRange && d < min) 
                        {
                            min = d;
                            closest = branches[j];
                        }
                    }

                    if (closest != null) 
                    {
                        closest.attractors.Add(attractors[ia]);
                        activeAttractors.Add(ia);
                    }
                }

                if (activeAttractors.Count != 0) 
                {
                    extremities.Clear();
                    List<Branch> newBranches = new List<Branch>();

                    for (int i = 0; i < branches.Count; i++) 
                    {
                        if (branches[i].attractors.Count > 0) 
                        {
                            Vector3 dir = new Vector3(0, 0, 0);
                            for (int j = 0; j < branches[i].attractors.Count; j++) dir += (branches[i].attractors[j] - branches[i].end).normalized;
                            dir /= branches[i].attractors.Count;
                            dir += RandomGrowthVector();
                            dir.Normalize();
                            Branch nb = new Branch(branches[i].end, branches[i].end + dir * branchLength, dir, branches[i]);
                            branches[i].children.Add(nb);
                            newBranches.Add(nb);
                            extremities.Add(nb);
                        } 
                        else if (branches[i].children.Count == 0) extremities.Add(branches[i]);
                    }

                    branches.AddRange(newBranches);
                } 
                else 
                {
                    for (int i = 0; i < extremities.Count; i++) 
                    {
                        Branch e = extremities[i];
                        Vector3 start = e.end;
                        Vector3 dir = e.direction + RandomGrowthVector();
                        Vector3 end = e.end + dir * branchLength;
                        Branch nb = new Branch(start, end, dir, e);
                        e.children.Add(nb);
                        branches.Add(nb);
                        extremities[i] = nb;
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

    private void GenerateAttractors(int n, float r) 
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
            attractors.Add(pt);
        }
    }

    private void OnDrawGizmos() 
    {
        for (int i = 0; i < branches.Count; i++) 
        {
            Branch b = branches[i];
            Gizmos.color = Color.green;
            Gizmos.DrawLine(b.start, b.end);
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(b.end, 0.05f);
            Gizmos.DrawSphere(b.start, 0.05f);
        }

        for (int i = 0; i < attractors.Count; i++) 
        {
            var a = attractors[i];
            Vector3 pos = new Vector3(a.x, a.y, a.z);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(pos, 0.05f);
        }
    }
}
