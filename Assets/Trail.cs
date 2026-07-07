using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Trail : MonoBehaviour
{
    public GameObject main;

    void Start()
    {
        TrailRenderer trailRenderer = main.GetComponent<TrailRenderer>();
        trailRenderer.startWidth = main.transform.localScale.x;
    }
}
/*
 using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Trail : MonoBehaviour
{
    public bool trail = true;
    public GameObject trailPrefab;
    public float fade = 0.05f;
    public float Scale = 1f;
    public GameObject main;
    Trail rayTrailScript;
    SpriteRenderer trailRenderer;

    void Start()
    {
        main.transform.localScale = new Vector2 (Scale, Scale);
    }

    void Update()
    {
        if (trail)
        {
            GameObject rayTrail = Instantiate(trailPrefab);
            rayTrail.transform.position = transform.position;
            rayTrailScript = rayTrail.GetComponent<Trail>();
            rayTrailScript.trail = false;
            rayTrailScript.main = rayTrail;
            rayTrailScript.trailPrefab = trailPrefab;
            rayTrailScript.fade = fade;
            rayTrailScript.Scale = Scale;
            rayTrailScript.trailRenderer = rayTrail.GetComponent<SpriteRenderer>();

        }
        else
        {
            Color currCol = trailRenderer.color;
            currCol.a -= fade;
            Color newCol = currCol;

            if (newCol.a == 0)
            {
                Destroy(main);
            }
            else
            {
                trailRenderer.color = newCol;
            }
        }
    }
}
 */