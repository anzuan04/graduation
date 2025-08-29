using UnityEngine;

public class Dominate : MonoBehaviour
{
    Collider2D[] hit_cd;
    Vector2 area_pt;
    Vector2 size;
    public float dm_percent;
    public int Conqueror_id;
    int layermask;

    public bool isConquer;
    public bool isConquerorIn;

    SpriteRenderer sr;
    public Color currentColor;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        PlaceManager.Instance.RegisterPlace(this);
        dm_percent = 0f;
        area_pt = transform.position;
        size = transform.localScale;
        isConquer = false;
        isConquerorIn = false;
        layermask = LayerMask.GetMask("Character", "Enemy");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isConquer == true)
            return;
        Conquer();
    }

    private void Conquer()
    {
        hit_cd = Physics2D.OverlapBoxAll(area_pt, size, 0f,layermask);
        
        if (hit_cd.Length > 0)
        {
            if (dm_percent == 0f)
            {
                Conqueror_id = hit_cd[0].gameObject.GetInstanceID();
                sr = hit_cd[0].gameObject.GetComponent<SpriteRenderer>();
                currentColor = sr.color;
            }
            if (dm_percent < 100f && hit_cd.Length == 1 && Conqueror_id == hit_cd[0].gameObject.GetInstanceID())
            {
                isConquerorIn = true;
                dm_percent += 0.2f;
                dm_percent = Mathf.Round(dm_percent * 10f) / 10f;

                // 최대값 초과 방지
                if (dm_percent >= 100f)
                {
                    dm_percent = 100f;
                    isConquer = true;
                }
            }
            else if(dm_percent >= 0f && hit_cd.Length >= 1)
            {
                foreach(Collider2D cd in hit_cd)
                {
                    if (Conqueror_id == cd.gameObject.GetInstanceID())
                    {
                        isConquerorIn = true;
                        return;
                    }
                    else
                        isConquerorIn = false;
                }
                if (!isConquerorIn)
                {
                    dm_percent -= 0.2f;
                    dm_percent = Mathf.Round(dm_percent * 10f) / 10f;

                    // 최소값 초과 방지
                    if (dm_percent <= 0f)
                    {
                        dm_percent = 0f;
                    }
                }

            }
        }
        else
        {
            if (dm_percent >= 0f)
            {
                isConquerorIn = false;
                dm_percent -= 0.2f;
                dm_percent = Mathf.Round(dm_percent * 10f) / 10f;

                // 최소값 초과 방지
                if (dm_percent <= 0f)
                {
                    dm_percent = 0f;
                }
            }
        }
    }
}
