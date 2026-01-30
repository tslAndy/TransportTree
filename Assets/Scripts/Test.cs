using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public NutrTree tree;
    public Material material;
    public int points;

    private Mesh mesh;

    private List<Vector3> verts;
    private List<int> trigs;

    private void AddVerts(int branch_index)
    {
        Branch branch = tree.branches[branch_index];
        Vector3 delta = branch.direction;
        Vector3 pos = tree.GetBranchPos(branch_index);

        float step = 2.0f * Mathf.PI / points;
        for (int j = 0; j < points; j++)
        {
            Vector3 a = Vector3.Cross(branch.direction, Vector3.right).normalized;
            Vector3 b = Vector3.Cross(a, branch.direction).normalized;
            Vector3 radVec = Mathf.Cos(j * step) * a + Mathf.Sin(j * step) * b;
            verts.Add(pos + radVec * branch.Radius);
        }
    }

    private void AddTrigs(int branch_index)
    {
        Branch branch = tree.branches[branch_index];

        if (branch.parent_id < 0)
            return;

        int own_offset = branch_index * points;
        int other_offset = branch.parent_id * points;
        for (int j = 0; j < points; j++)
        {
            int a = own_offset + j;
            int b = other_offset + j;
            int c = other_offset + ((j + 1) % points);
            int d = own_offset + ((j + 1) % points);

            trigs.Add(a);
            trigs.Add(b);
            trigs.Add(c);
            trigs.Add(a);
            trigs.Add(c);
            trigs.Add(d);
        }
    }

    void Start()
    {
        verts = new List<Vector3>(points * tree.branches.Count);
        trigs = new List<int>(points * 6 * tree.branches.Count);

        for (int i = 0; i < tree.branches.Count; i++)
            AddVerts(i);

        for (int i = 0; i < tree.branches.Count; i++)
            AddTrigs(i);

        mesh = new();
        mesh.SetVertices(verts);
        mesh.SetTriangles(trigs, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }

    void Update()
    {
        Graphics.DrawMesh(mesh, transform.position, Quaternion.identity, material, 0);
    }
}
