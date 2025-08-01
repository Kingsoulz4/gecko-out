using UnityEngine;

public class GridDrawer : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public float tileSize = 1f;
    public Color gridColor = Color.green;

    void OnDrawGizmos()
    {
        Gizmos.color = gridColor;

        Vector3 origin = transform.position;

        // Vẽ hàng ngang
        for (int y = 0; y <= height; y++)
        {
            Vector3 start = origin + new Vector3(0, 0, y * tileSize);
            Vector3 end = origin + new Vector3(width * tileSize, 0, y * tileSize);
            Gizmos.DrawLine(start, end);
        }

        // Vẽ cột dọc
        for (int x = 0; x <= width; x++)
        {
            Vector3 start = origin + new Vector3(x * tileSize, 0, 0);
            Vector3 end = origin + new Vector3(x * tileSize, 0, height * tileSize);
            Gizmos.DrawLine(start, end);
        }
    }
}
