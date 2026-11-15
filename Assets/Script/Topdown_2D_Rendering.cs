using UnityEngine;


public class Topdown_2D_Rendering : MonoBehaviour
{

    private float SortMultiplier = 1000f;
    SpriteRenderer sr;
    Transform root;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        root = transform.childCount > 0 ? transform.GetChild(0) : transform;
    }

    void LateUpdate()
    {
        if(sr == null || root == null)
        {
            return;
        }
        int OrderInLayer = - Mathf.RoundToInt(root.position.y * SortMultiplier);

        sr.sortingOrder = OrderInLayer;
    }
}
