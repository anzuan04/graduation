using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]

public class FieldOfView : MonoBehaviour
{
    [Range(0, 360)] public float fov = 360f;     
    public int rayCount = 360;                   
    public float viewDistance = 10f;            
    public LayerMask layerMask;                 

    private Camera mainCamera;                  
    private Mesh mesh;                          

    private void Start()
    {
        mainCamera = Camera.main;
        mesh = new Mesh();
        mesh.name = "FOV_Effect";
        GetComponent<MeshFilter>().mesh = mesh;
    }

    private void LateUpdate()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        transform.eulerAngles = new Vector3(0, 0, GetAngleFromVector(new Vector3(worldPos.x - transform.position.x, worldPos.y - transform.position.y, 0)));
        DrawFOV();
    }

    private void DrawFOV()
    {
        float angle = fov * 0.5f;
        float angleIncrease = fov / rayCount;

        Vector3[] vertices = new Vector3[rayCount + 1 + 1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = Vector3.zero;

        int vertexIndex = 1;
        int triangleIndex = 0;
        for (int i = 0; i <= rayCount; i++)
        {
            Vector3 vertex;
            Debug.DrawRay(transform.position, GetVectorFromAngle(transform.eulerAngles.z + angle)*viewDistance, Color.blue);
            RaycastHit2D raycastHit2D = Physics2D.Raycast(transform.position, GetVectorFromAngle(transform.eulerAngles.z + angle), viewDistance, layerMask);
            if (raycastHit2D.collider == null)
            {
                vertex = GetVectorFromAngle(angle) * viewDistance;
            }
            else
            {
                vertex = GetVectorFromAngle(angle) * raycastHit2D.distance;
            }
            vertices[vertexIndex] = vertex;

            if (0 < i)
            {
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;

                triangleIndex += 3;
            }

            ++vertexIndex;
            angle -= angleIncrease;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
    }
    private Vector3 GetVectorFromAngle(float angle)
    {
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }
    private int GetAngleFromVector(Vector3 dir)
    {
        dir = dir.normalized;
        float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (n < 0) n += 360;
        int angle = Mathf.RoundToInt(n);

        return angle;
    }

    public void setTransform(Vector3 trans)
    {
        transform.position = trans;
    }
}
