using System.Collections.Generic;
using UnityEngine;

public class NutrTree : MonoBehaviour
{
    public int feed_iters = 100;
    public float feed = 10;
    public float ratio = 0.4f;
    public float spread = 0.3f;
    public float base_len = 20.0f;
    public float split_k = 0.98f;
    public float directedness = 0.8f;

    public List<Branch> branches = new();

    private void Awake()
    {
        Branch trunk = new Branch
        {
            parent_id = -1,
            direction = Vector3.up,
            length = 0.01f
        };

        branches.Add(trunk);

        for (int i = 0; i < feed_iters; i++)
            Step(feed, 0, 0);
    }

    public Vector3 GetBranchPos(int branch_index)
    {
        Vector3 result = Vector3.zero;
        Branch branch = branches[branch_index];
        while (branch.parent_id >= 0)
        {
            Branch parent = branches[branch.parent_id];
            result += parent.direction * parent.length;
            branch = parent;
        }
        return result;
    }

    private void Step(float feed, int index, int depth)
    {
        Branch branch = branches[index];

        if (!branch.IsLeaf)
        {
            float pass =
                (branches[branch.left_id].area + branches[branch.right_id].area) /
                (branches[branch.left_id].area + branches[branch.right_id].area + branch.area);

            branch.area += pass * feed / branch.length;
            feed *= 1.0f - pass;

            if (feed < 0.00001f)
                return;

            Step(feed * ratio, branch.left_id, depth + 1);
            Step(feed * (1.0f - ratio), branch.right_id, depth + 1);
            return;
        }

        float delta = Mathf.Pow(feed, 1f / 3f);
        branch.length += delta;
        feed -= delta * branch.area;
        branch.area += feed / branch.length;

        if (branch.length < base_len * Mathf.Pow(split_k, depth))
        {
            branches[index] = branch;
            return;
        }

        int left_id = branches.Count;
        int right_id = branches.Count + 1;

        branch.left_id = left_id;
        branch.right_id = right_id;

        Vector3 highest_density = GetLeafDensity(index, depth);
        Vector3 perpendicular = Vector3.Normalize(Vector3.Cross(branch.direction, highest_density));
        Vector3 reflected_perpendicular = -perpendicular;
        float flip = Random.Range(0, 2) * 2 - 1;

        Vector3 left_direction = Vector3.Lerp(flip * spread * perpendicular, branch.direction, ratio).normalized;
        Vector3 right_direction = Vector3.Lerp(flip * spread * reflected_perpendicular, branch.direction, 1.0f - ratio).normalized;

        Branch left = new Branch
        {
            parent_id = index,
            direction = left_direction,
        };

        Branch right = new Branch
        {
            parent_id = index,
            direction = right_direction,
        };

        branches[index] = branch;
        branches.Add(left);
        branches.Add(right);
    }

    private Vector3 GetLeafDensity(int index, int depth)
    {
        Vector3 rand_offset = Random.onUnitSphere;
        if (depth == 0)
            return rand_offset;

        Vector3 relative = Vector3.zero;
        Branch temp_branch = branches[index];
        while (true)
        {
            relative += temp_branch.direction * temp_branch.length;
            if (depth-- == 0)
                break;
            index = temp_branch.parent_id;
            temp_branch = branches[index];
        }

        return directedness * Vector3.Normalize(GetLeafAverage(index) - relative) + (1.0f - directedness) * rand_offset;
    }

    private Vector3 GetLeafAverage(int index)
    {
        Branch branch = branches[index];
        if (branch.IsLeaf)
            return branch.direction * branch.length;

        return
            branch.direction * branch.length +
            ratio * GetLeafAverage(branch.left_id) +
            (1.0f - ratio) * GetLeafAverage(branch.right_id);
    }
}

public struct Branch
{
    public int parent_id, left_id, right_id;
    public Vector3 direction;
    public float length, area;

    public bool IsLeaf => left_id == 0 && right_id == 0;
    public float Radius => Mathf.Sqrt(area / Mathf.PI);
}
