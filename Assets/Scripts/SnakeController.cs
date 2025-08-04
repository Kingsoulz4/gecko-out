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

    [Header("Raycast Settings")]
    public LayerMask groundLayer;
    public List<Transform> Segments => segments;


    private List<Transform> segments = new List<Transform>();
    private List<Vector3> targetPositions = new List<Vector3>();
    private Vector3 currentDirection = Vector3.forward;

    private Vector3? lastTouchTile = null;
    private Vector3 lastSlideDirection = Vector3.zero;
    private bool shouldSlide = false;
    private bool hasSlid = false;
    private bool isClickingOnSnake = false;

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

            if (shouldSlide && !isClickingOnSnake && !hasSlid)
            {
                if (TrySlideStep())
                    hasSlid = true;
                else
                    lastSlideDirection = Vector3.zero;
            }
        }

        MoveSegmentsSmoothly();
    }

    void HandleInput()
    {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS
        if (Input.GetMouseButton(0))
        {
            isClickingOnSnake = false;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Snake"))
                {
                    isClickingOnSnake = true;
                    lastSlideDirection = Vector3.zero;
                    shouldSlide = false;
                    return;
                }

                if ((groundLayer.value & (1 << hit.collider.gameObject.layer)) == 0)
                    return;
            }
            else return;

            Vector3 target = GetMouseTilePosition();
            if (lastTouchTile.HasValue && Vector3.Distance(lastTouchTile.Value, target) < 0.01f)
                return;

            lastTouchTile = target;

            float distToHead = Vector3.Distance(target, targetPositions[0]);
            float distToTail = Vector3.Distance(target, targetPositions[^1]);

            if (distToHead < distToTail)
            {
                Vector3 next = GetNextStepTowards(targetPositions[0], target);
                if (!IsOccupied(next, 0) && !IsWall(next) && (targetPositions.Count <= 1 || next != targetPositions[1]))
                {
                    MoveHeadTo(next);
                    lastSlideDirection = (next - targetPositions[0]).normalized;
                    shouldSlide = false;
                }
                else
                {
                    TryTurnAtObstacle();
                }
            }
            else
            {
                int last = targetPositions.Count - 1;
                Vector3 next = GetNextStepTowards(targetPositions[last], target);
                if (!IsOccupied(next, last) && !IsWall(next) && (targetPositions.Count <= 1 || next != targetPositions[last - 1]))
                {
                    MoveTailTo(next);
                    lastSlideDirection = (targetPositions[last] - next).normalized;
                    shouldSlide = false;
                }
                else
                {
                    TryTailTurnAtObstacle();
                }
            }
        }
        else
        {
            lastTouchTile = null;
            isClickingOnSnake = false;

            if (lastSlideDirection != Vector3.zero)
            {
                shouldSlide = true;
                hasSlid = false;
            }
        }
#endif
    }

    bool TrySlideStep()
    {
        Vector3 next = targetPositions[0] + lastSlideDirection * tileSize;
        if (!IsOccupied(next, 0) && !IsWall(next))
        {
            MoveHeadTo(next);
            return true;
        }
        return false;
    }

    Vector3 GetMouseTilePosition()
    {
        if (Camera.main == null) return transform.position;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        if (plane.Raycast(ray, out float enter))
        {
            Vector3 point = ray.GetPoint(enter);
            float x = Mathf.Round(point.x / tileSize) * tileSize;
            float z = Mathf.Round(point.z / tileSize) * tileSize;
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

        Vector3[] dirs =
        {
            new Vector3(currentDirection.z, 0, -currentDirection.x),
            new Vector3(-currentDirection.z, 0, currentDirection.x)
        };

        foreach (var dir in dirs)
        {
            Vector3 next = head + dir * tileSize;
            if (!IsOccupied(next, 0) && !IsWall(next))
            {
                MoveHeadTo(next);
                lastSlideDirection = dir.normalized;
                shouldSlide = false;
                return;
            }
        }
    }

    void TryTailTurnAtObstacle()
    {
        int last = targetPositions.Count - 1;
        Vector3 tail = targetPositions[last];
        Vector3 backDir = (tail - targetPositions[last - 1]).normalized;

        Vector3[] dirs =
        {
            new Vector3(backDir.z, 0, -backDir.x),
            new Vector3(-backDir.z, 0, backDir.x)
        };

        foreach (var dir in dirs)
        {
            Vector3 next = tail + dir * tileSize;
            if (!IsOccupied(next, last) && !IsWall(next))
            {
                MoveTailTo(next);
                lastSlideDirection = -dir.normalized;
                shouldSlide = false;
                return;
            }
        }
    }

    bool IsWall(Vector3 pos)
    {
        return Physics.CheckBox(pos, Vector3.one * tileSize * 0.4f, Quaternion.identity, wallLayer);
    }

    void MoveHeadTo(Vector3 target)
    {
        Vector3 dir = target - targetPositions[0];
        if (dir.sqrMagnitude < 0.01f) return;

        currentDirection = dir.normalized;
        ShiftPositionsForward(target);
    }

    void MoveTailTo(Vector3 target)
    {
        Vector3 dir = target - targetPositions[^1];
        if (dir.sqrMagnitude < 0.01f) return;

        ShiftPositionsBackward(target);
    }

    bool IsOccupied(Vector3 pos, int ignoreIndex = -1)
    {
        float threshold = 0.001f;
        for (int i = 0; i < targetPositions.Count; i++)
        {
            if (i == ignoreIndex) continue;
            if ((targetPositions[i] - pos).sqrMagnitude < threshold)
                return true;
        }
        return false;
    }

    void ShiftPositionsForward(Vector3 newHead)
    {
        for (int i = targetPositions.Count - 1; i > 0; i--)
            targetPositions[i] = targetPositions[i - 1];
        targetPositions[0] = newHead;
    }

    void ShiftPositionsBackward(Vector3 newTail)
    {
        for (int i = 0; i < targetPositions.Count - 1; i++)
            targetPositions[i] = targetPositions[i + 1];
        targetPositions[^1] = newTail;
    }

    void MoveSegmentsSmoothly()
    {
        float headSpeed = 10f;
        float bodySpeed = 8f;

        for (int i = 0; i < segments.Count; i++)
        {
            Vector3 target = targetPositions[i];
            Transform segment = segments[i];
            float speed = (i == 0) ? headSpeed : bodySpeed;

            segment.position = Vector3.Lerp(segment.position, target, Time.deltaTime * speed);

            if (i == 0)
            {
                if (currentDirection != Vector3.zero)
                    segment.rotation = Quaternion.Lerp(segment.rotation, Quaternion.LookRotation(currentDirection), Time.deltaTime * 10f);
            }
            else
            {
                Vector3 dir = target - segment.position;
                if (dir != Vector3.zero)
                    segment.forward = Vector3.Lerp(segment.forward, dir.normalized, Time.deltaTime * 10f);
            }
        }
    }

    public void AddBodySegment()
    {
        Transform tail = segments[^1];
        Vector3 dir = (tail.position - segments[^2].position).normalized;
        Vector3 newPos = tail.position + dir * tileSize;

        GameObject newSeg = Instantiate(bodySegmentPrefab, newPos, Quaternion.identity);
        segments.Add(newSeg.transform);
        targetPositions.Add(newPos);
    }
}
