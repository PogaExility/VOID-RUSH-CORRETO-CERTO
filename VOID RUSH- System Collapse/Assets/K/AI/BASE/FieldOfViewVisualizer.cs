using UnityEngine;
using System.Collections.Generic;

// Versão FINAL com a variável 'viewMeshFilter' pública.
public class FieldOfViewVisualizer : MonoBehaviour
{
    [Header("Referências")]
    public AIController aiController;

    // --- VARIÁVEL CORRIGIDA ---
    [Tooltip("Arraste o componente Mesh Filter (do mesmo objeto) para cá.")]
    public MeshFilter viewMeshFilter; // <-- ESTA LINHA ESTAVA FALTANDO

    private Mesh viewMesh;

    [Header("Qualidade da Mesh")]
    public int meshResolution = 2;
    public int edgeResolveIterations = 4;
    public float edgeDistanceThreshold = 0.5f;

    void Start()
    {
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";

        // Se o viewMeshFilter foi atribuído no Inspector, use-o.
        if (viewMeshFilter != null)
        {
            viewMeshFilter.mesh = viewMesh;
        }
        else
        {
            Debug.LogError("A referência do 'View Mesh Filter' não foi atribuída no Inspector!", this);
        }

        if (aiController == null)
        {
            aiController = GetComponentInParent<AIController>();
            if (aiController == null) Debug.LogError("FieldOfViewVisualizer não conseguiu encontrar um AIController!", this);
        }
    }

    void LateUpdate()
    {
        if (aiController != null && viewMeshFilter != null)
        {
            DrawFieldOfView();
        }
    }

    void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(aiController.visionAngle * meshResolution);
        float stepAngleSize = aiController.visionAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo oldViewCast = new ViewCastInfo();

        for (int i = 0; i <= stepCount; i++)
        {
            float angle = -transform.eulerAngles.z - aiController.visionAngle / 2 + stepAngleSize * i;
            if (aiController.transform.localScale.x < 0)
            {
                angle = 180 - angle;
            }

            ViewCastInfo newViewCast = ViewCast(angle);

            if (i > 0)
            {
                bool edgeDstThresholdExceeded = Mathf.Abs(oldViewCast.dst - newViewCast.dst) > edgeDistanceThreshold;
                if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && edgeDstThresholdExceeded))
                {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
                    if (edge.pointA != Vector3.zero) viewPoints.Add(edge.pointA);
                    if (edge.pointB != Vector3.zero) viewPoints.Add(edge.pointB);
                }
            }
            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);
            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }
        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;
        for (int i = 0; i < edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle);
            bool edgeDstThresholdExceeded = Mathf.Abs(minViewCast.dst - newViewCast.dst) > edgeDistanceThreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }
        return new EdgeInfo(minPoint, maxPoint);
    }

    ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, aiController.visionRange, aiController.visionBlockers);
        if (hit.collider != null)
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + dir * aiController.visionRange, aiController.visionRange, globalAngle);
        }
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.z;
        }
        return new Vector3(Mathf.Cos(angleInDegrees * Mathf.Deg2Rad), Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0);
    }

    public struct ViewCastInfo
    {
        public bool hit; public Vector3 point; public float dst; public float angle;
        public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle) { hit = _hit; point = _point; dst = _dst; angle = _angle; }
    }
    public struct EdgeInfo
    {
        public Vector3 pointA; public Vector3 pointB;
        public EdgeInfo(Vector3 _pointA, Vector3 _pointB) { pointA = _pointA; pointB = _pointB; }
    }
}