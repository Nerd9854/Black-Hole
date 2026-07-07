using UnityEngine;

public class SpawnRays : MonoBehaviour
{
    public int numRays = 10;
    public GameObject ray;
    public Camera Camera;
    private rayScript[] rays;

    void Start()
    {
        float aspect = Camera.aspect;
        float size = Camera.orthographicSize;
        float spacing = (size*2)/(numRays);
        float offset = size - (spacing / 2);

        rays = new rayScript[numRays];
        for (int i = 0; i < numRays; i++)
        {
            GameObject objRay = Instantiate(ray);
            objRay.transform.position = new Vector3(-(size * aspect), offset, -0.1f);
            objRay.transform.rotation = new Quaternion(0, 0, -0.0871557817f, 0.99619472f);
            rayScript rayScript = objRay.GetComponent<rayScript>();
            rayScript.index = i;
            rays[i] = rayScript;

            offset -= spacing;
        }
        foreach (rayScript i in rays)
        {
            Debug.Log(i.index);
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (rayScript i in rays)
        {
            i.Step(100d * Time.deltaTime);//Time.deltaTime);
        }
    }
}