using UnityEngine;

public static class FindUlti
{
    public static GameObject FindAllChild(this Component transform, string name, bool isContain = false)
    {
        if (transform == null || string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("Transform or name is null/empty!");
            return null;
        }

        Transform[] children = transform.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (isContain)
            {
                if (child.name.Contains(name))
                {
                    return child.gameObject;
                }
            }
            else if (child.name == name)
            {
                return child.gameObject;
            }
        }

        return null;
    }

    public static GameObject FindChildDirect(this Component transform, string name, bool includeInactive = true, bool isContain = false)
    {
        if (transform == null || string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("Transform or name is null/empty!");
            return null;
        }

        return FindChildRecursiveHelper(transform, name, includeInactive);
    }

    private static GameObject FindChildRecursiveHelper(Component parent, string name, bool includeInactive, bool isContain = false)
    {
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            Transform child = parent.transform.GetChild(i);

            if (!includeInactive && !child.gameObject.activeSelf)
            {
                continue;
            }

            if (isContain)
            {
                if (child.name.Contains(name))
                {
                    return child.gameObject;
                }
            }
            else if (child.name == name)
            {
                return child.gameObject;
            }

            GameObject foundInDepth = FindChildRecursiveHelper(child, name, includeInactive);
            if (foundInDepth != null)
            {
                return foundInDepth;
            }
        }

        return null;
    }
}