using UnityEngine;


public class Topdown_2D_Rendering : MonoBehaviour
{

    private float SortMultiplier = 1000f;
    SpriteRenderer sr;
    Transform root;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        root = this.gameObject.transform.GetChild(0);
    }

    void LateUpdate()
    {
        if(sr == null  )
        {
            return;
        }
        int OrderInLayer = - Mathf.RoundToInt(root.position.y * SortMultiplier);

        sr.sortingOrder = OrderInLayer;
    }
}
