using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RingCreator : MonoBehaviour
{
    [Range(0f, 360f)] public float angle = 360f;
    [Range(0f, 10f)] public float radius = 1f;
    public int segmentsPerRadius = 32;


    LineRenderer lr;

    void Start()
    {
        lr = gameObject.GetComponent<LineRenderer>();
    }

    void Update()
    {
        int segCount = Mathf.RoundToInt(angle / 360f * segmentsPerRadius * radius) + 1;
        lr.positionCount = segCount + 1;

        for (int i = 0; i < lr.positionCount; i++)
        {
            Vector3 pos = transform.position + Quaternion.AngleAxis(angle / segCount * i, transform.up) * transform.forward * radius;
            lr.SetPosition(i, pos);
        }
    }

    private void OnDrawGizmosSelected()
    {
        int segCount = Mathf.RoundToInt(segmentsPerRadius * radius);

        for (int i = 0; i < segCount; i++)
        {
            Vector3 pos1 = transform.position + Quaternion.AngleAxis(360f / segCount * i, transform.up) * transform.forward * radius;
            Vector3 pos2 = transform.position + Quaternion.AngleAxis(360f / segCount * (i + 1), transform.up) * transform.forward * radius;

            float p = (float)i / segCount;
            Gizmos.color = p * Color.blue + (1f - p) * Color.red;
            Gizmos.DrawLine(pos1, pos2);
        }
    }
}