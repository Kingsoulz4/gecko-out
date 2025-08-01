using UnityEngine;
using System.Collections.Generic;

public class SnakeController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float tileSize = 1f;

    [Header("Snake Settings")]
    public GameObject bodySegmentPrefab;
    public int initialBodyCount = 3;

    [Header("Timing Settings")]
    public float moveCooldown = 0.1f;
    private float moveTimer = 0f;

    [Header("Collision Settings")]
    public LayerMask wallLayer;

    private List<Transform> segments = new List<Transform>();
    private List<Vector3> targetPositions = new List<Vector3>();
    private Vector3 currentDirection = Vector3.forward;
    public List<Transform> Segments => segments;

    // Trượt
    private Vector3 lastSlideDirection = Vector3.zero;
    private float slideDuration = 0.3f;
    private float slideTimer = 0f;
    private bool isSliding = false;

    void Start()
    {
        segments.Add(transform);
        targetPositions.Add(transform.position);

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
        moveTimer -= Time.deltaTime;

        if (moveTimer <= 0f)
        {
            HandleInput();
            moveTimer = moveCooldown;

            if (isSliding)
            {
                slideTimer -= moveCooldown;
                if (slideTimer > 0f)
                {
                    TrySlideStep();
                }
                else
                {
                    isSliding = false;
                    lastSlideDirection = Vector3.zero;
                }
            }
        }

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
                Vector3 nextStep = GetNextStepTowards(targetPositions[0], target);
                if (!IsOccupied(nextStep, 0) && !IsWall(nextStep) && (targetPositions.Count <= 1 || nextStep != targetPositions[1]))
                {
                    MoveHeadTo(nextStep);
                    lastSlideDirection = (nextStep - targetPositions[0]).normalized;
                    isSliding = false;
                }
                else
                {
                    TryTurnAtObstacle();
                }
            }
            else
            {
                int last = targetPositions.Count - 1;
                Vector3 nextStep = GetNextStepTowards(targetPositions[last], target);
                if (!IsOccupied(nextStep, last) && !IsWall(nextStep) && (targetPositions.Count <= 1 || nextStep != targetPositions[last - 1]))
                {
                    MoveTailTo(nextStep);
                    lastSlideDirection = (targetPositions[last] - nextStep).normalized;
                    isSliding = false;
                }
                else
                {
                    TryTailTurnAtObstacle();
                }
            }
        }
        else
        {
            if (!isSliding && lastSlideDirection != Vector3.zero)
            {
                isSliding = true;
                slideTimer = slideDuration;
            }
        }
#endif
    }

    void TrySlideStep()
    {
        Vector3 next = targetPositions[0] + lastSlideDirection * tileSize;

        if (!IsOccupied(next, 0) && !IsWall(next))
        {
            MoveHeadTo(next);
        }
        else
        {
            isSliding = false;
            lastSlideDirection = Vector3.zero;
        }
    }

    Vector3 GetMouseTilePosition()
    {
        if (Camera.main == null) return transform.position;

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

    Vector3 GetNextStepTowards(Vector3 from, Vector3 to)
    {
        Vector3 dir = to - from;
        Vector3Int stepDir = Vector3Int.zero;

        float dx = Mathf.Abs(dir.x);
        float dz = Mathf.Abs(dir.z);

        if (dx >= dz)
            stepDir.x = (int)Mathf.Sign(dir.x);
        else
            stepDir.z = (int)Mathf.Sign(dir.z);

        return from + new Vector3(stepDir.x * tileSize, 0, stepDir.z * tileSize);
    }

    void TryTurnAtObstacle()
    {
        Vector3 head = targetPositions[0];

        Vector3[] directions = new Vector3[]
        {
            new Vector3(currentDirection.z, 0, -currentDirection.x),
            new Vector3(-currentDirection.z, 0, currentDirection.x)
        };

        foreach (var dir in directions)
        {
            Vector3 next = head + dir * tileSize;
            if (!IsOccupied(next, 0) && !IsWall(next))
            {
                MoveHeadTo(next);
                lastSlideDirection = dir.normalized;
                isSliding = false;
                return;
            }
        }
    }

    void TryTailTurnAtObstacle()
    {
        int last = targetPositions.Count - 1;
        Vector3 tail = targetPositions[last];
        Vector3 backDir = (tail - targetPositions[last - 1]).normalized;

        Vector3[] directions = new Vector3[]
        {
            new Vector3(backDir.z, 0, -backDir.x),
            new Vector3(-backDir.z, 0, backDir.x)
        };

        foreach (var dir in directions)
        {
            Vector3 next = tail + dir * tileSize;
            if (!IsOccupied(next, last) && !IsWall(next))
            {
                MoveTailTo(next);
                lastSlideDirection = -dir.normalized;
                isSliding = false;
                return;
            }
        }
    }

    bool IsWall(Vector3 pos)
    {
        return Physics.CheckBox(pos, Vector3.one * tileSize * 0.4f, Quaternion.identity, wallLayer);
    }

    void MoveHeadTo(Vector3 targetPos)
    {
        Vector3 dir = targetPos - targetPositions[0];
        if (dir.sqrMagnitude < 0.01f) return;

        currentDirection = dir.normalized;
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
        float thresholdSqr = 0.001f;
        for (int i = 0; i < targetPositions.Count; i++)
        {
            if (i == ignoreIndex) continue;
            if ((targetPositions[i] - pos).sqrMagnitude < thresholdSqr)
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
                if (currentDirection != Vector3.zero)
                    segments[0].rotation = Quaternion.LookRotation(currentDirection);
            }
            else
            {
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
