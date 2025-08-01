using UnityEngine;
using System.Collections.Generic;

public class SnakeController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float tileSize = 1f;

    [Header("Snake Settings")]
    public GameObject bodySegmentPrefab;
    public int initialBodyCount = 3;

    private List<Transform> segments = new List<Transform>();
    private List<Vector3> targetPositions = new List<Vector3>();
    private Vector3 currentDirection = Vector3.forward; // default hướng lên

    void Start()
    {
        // Đầu rắn
        segments.Add(transform);
        targetPositions.Add(transform.position);

        // Thêm thân ban đầu
        for (int i = 1; i <= initialBodyCount; i++)
        {
            Vector3 pos = transform.position - new Vector3(0, 0, tileSize * i);
            GameObject segment = Instantiate(bodySegmentPrefab, pos, Quaternion.identity);
            segments.Add(segment.transform);
            targetPositions.Add(pos);
        }
    }

    void Update()
    {
        HandleInput();
        MoveSegmentsSmoothly();
    }

    void HandleInput()
    {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS
        if (Input.GetMouseButton(0))
        {
            Vector3 target = GetMouseTilePosition();

            float distToHead = Vector3.Distance(target, targetPositions[0]);
            float distToTail = Vector3.Distance(target, targetPositions[targetPositions.Count - 1]);

            if (distToHead < distToTail)
            {
                Vector3 nextStep = GetNextStep(targetPositions[0], target);
                if (!IsOccupied(nextStep, 0))
                    MoveHeadTo(nextStep);
            }
            else
            {
                Vector3 nextStep = GetNextStep(targetPositions[targetPositions.Count - 1], target);
                if (!IsOccupied(nextStep, segments.Count - 1))
                    MoveTailTo(nextStep);
            }
        }
#endif
    }

    Vector3 GetMouseTilePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        if (plane.Raycast(ray, out float enter))
        {
            Vector3 worldPoint = ray.GetPoint(enter);
            float x = Mathf.Round(worldPoint.x / tileSize) * tileSize;
            float z = Mathf.Round(worldPoint.z / tileSize) * tileSize;
            return new Vector3(x, transform.position.y, z);
        }
        return transform.position;
    }

    Vector3 GetNextStep(Vector3 from, Vector3 to)
    {
        Vector3 dir = to - from;
        Vector3 step = Vector3.zero;

        float dx = Mathf.Abs(dir.x);
        float dz = Mathf.Abs(dir.z);

        if (dx > dz && dx >= tileSize)
            step = new Vector3(Mathf.Sign(dir.x) * tileSize, 0, 0);
        else if (dz >= tileSize)
            step = new Vector3(0, 0, Mathf.Sign(dir.z) * tileSize);

        return from + step;
    }

    void MoveHeadTo(Vector3 targetPos)
    {
        Vector3 dir = targetPos - targetPositions[0];
        if (dir.sqrMagnitude < 0.01f) return;

        currentDirection = dir.normalized; // Lưu hướng đi mới
        ShiftPositionsForward(targetPos);
    }

    void MoveTailTo(Vector3 targetPos)
    {
        Vector3 dir = targetPos - targetPositions[targetPositions.Count - 1];
        if (dir.sqrMagnitude < 0.01f) return;

        ShiftPositionsBackward(targetPos);
    }

    bool IsOccupied(Vector3 pos, int ignoreIndex = -1)
    {
        for (int i = 0; i < targetPositions.Count; i++)
        {
            if (i == ignoreIndex) continue;
            if (Vector3.Distance(targetPositions[i], pos) < 0.01f)
                return true;
        }
        return false;
    }

    void ShiftPositionsForward(Vector3 newHeadPos)
    {
        for (int i = targetPositions.Count - 1; i > 0; i--)
        {
            targetPositions[i] = targetPositions[i - 1];
        }
        targetPositions[0] = newHeadPos;
    }

    void ShiftPositionsBackward(Vector3 newTailPos)
    {
        for (int i = 0; i < targetPositions.Count - 1; i++)
        {
            targetPositions[i] = targetPositions[i + 1];
        }
        targetPositions[targetPositions.Count - 1] = newTailPos;
    }

    void MoveSegmentsSmoothly()
    {
        float speed = 30f;

        for (int i = 0; i < segments.Count; i++)
        {
            Vector3 target = targetPositions[i];
            segments[i].position = Vector3.MoveTowards(segments[i].position, target, speed * Time.deltaTime);

            if (i == 0)
            {
                // Đầu rắn: quay cứng theo hướng đi
                if (currentDirection != Vector3.zero)
                    segments[0].rotation = Quaternion.LookRotation(currentDirection);
            }
            else
            {
                // Thân: quay mượt
                Vector3 dir = target - segments[i].position;
                if (dir != Vector3.zero)
                    segments[i].forward = Vector3.Lerp(segments[i].forward, dir.normalized, Time.deltaTime * 10f);
            }
        }
    }

    public void AddBodySegment()
    {
        Transform tail = segments[segments.Count - 1];
        Vector3 dir = (tail.position - segments[segments.Count - 2].position).normalized;
        Vector3 newPos = tail.position + dir * tileSize;

        GameObject newSeg = Instantiate(bodySegmentPrefab, newPos, Quaternion.identity);
        segments.Add(newSeg.transform);
        targetPositions.Add(newPos);
    }
}
