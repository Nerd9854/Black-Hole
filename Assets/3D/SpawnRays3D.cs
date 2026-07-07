using System;
using System.IO;
using UnityEngine;

public class SpawnRays3D : MonoBehaviour
{
    public int numRaysX = 100;
    public int numRaysY = 100;
    public bool showRay = false;
    public float rayRad;
    public bool multiFrame;
    public int maxStepCount;
    public float destroyThresh;
    public double dtScale;
    public GameObject ray;
    public Camera Camera;
    public GameObject rayParent;
    private rayScript3D[] rays;
    private int[] indexes;
    public Texture2D texture;
    public int frameNum;
    public Transform sphere;
    public Texture2D sky;
    public Texture2D disk;

    public int frameCount = 0;
    Vector3 camPos;
    Quaternion rotation;
    int destroyNum;

    void Start()
    {
        Consts.rayRad = rayRad;
        Consts.dIn = ((Consts.r_s/*1.3*/)/Consts.scale) * 2;
        numRaysY = (int)(numRaysX * Camera.aspect);
        Consts.px = new Color[numRaysX, numRaysY];

        texture = new Texture2D(numRaysY, numRaysX);
        destroyNum = (int)(destroyThresh * ((float)numRaysY * (float)numRaysX));


        camPos = Camera.transform.position;
        rotation = Camera.transform.rotation;

        rayParent.transform.position = camPos;

        resetCam(true);

        spawn();

    }

    void spawn()
    {
        int count = 0;
        rays = new rayScript3D[numRaysX * numRaysY];
        indexes = new int[numRaysX * numRaysY];
        for (int i = 0; i < numRaysX; i++)
        {
            for (int j = 0; j < numRaysY; j++)
            {
                float u = (float)j / (numRaysY - 1);
                float v = (float)i / (numRaysX - 1);

                Ray camRay = Camera.ViewportPointToRay(new Vector3(u, v, 0));
                Vector3 origin = camRay.origin;
                Vector3 direction = camRay.direction;

                GameObject objRay = Instantiate(ray, origin, Quaternion.LookRotation(direction), rayParent.transform);

                rayScript3D rayScript = objRay.GetComponent<rayScript3D>();

                rayScript.pxi = i;
                rayScript.pxj = j;
                rayScript.index = new Vector3Int(count, i, j);
                rayScript.maxStepCount = maxStepCount;
                rayScript.color = Color.Lerp(Color.red, Color.green, (float)count / ((float)numRaysX * (float)numRaysY));
                rayScript.texture = sky;
                rayScript.sphereTransform = sphere;
                rayScript.showRay = showRay;
                rayScript.disk = disk;

                Consts.px[i, j] = rayScript.color;

                rays[count] = rayScript;
                count++;
            }
        }
        updateTexture();
    }

    void updateTexture()
    {
        for (int i = 0; i < numRaysX; i++)
        {
            for (int j = 0; j < numRaysY; j++)
            {
                texture.SetPixel(j, (numRaysX - 1 - i), Consts.px[i, j]);
            }
        }
        texture.Apply();
    }

    void resetRays()
    {
        for (int i = 0; i < indexes.Length; i++)
        {
            if (indexes[i] != -1)
            {
                indexes[i] = -1;
                rays[i].DestroySelf(true);
            }
        }
    }

    void resetCam(bool start = false)
    {
        if (!multiFrame) return;
        float radius = -20f;
        float deg = start == false ? Mathf.Lerp(90f, -90f, (float)frameCount / (float)frameNum) : 0;

        camPos = new Vector3(radius * Mathf.Sin(deg * Mathf.PI / 180), 4,
                             radius * Mathf.Cos(deg * Mathf.PI / 180));
        rotation = Quaternion.Euler(10, deg, 0);

        Camera.transform.position = camPos;
        Camera.transform.rotation = rotation;

        if (!start) spawn();
        if (frameCount == frameNum) multiFrame = false;
    }

    void resetAll()
    {
        resetRays();
        updateTexture();

        byte[] bytes = texture.EncodeToPNG();
        var dirPath = Application.dataPath + "/../SaveImages/";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllBytes($"{dirPath}frame{frameCount}.png", bytes);

        resetCam();
        destroyed = 0;
        frameCount++;
        Debug.Log(timer);
    }


    int textureCount = 0;
    int destroyed = 0;
    float timer = 0;
    int textureApply = 0;


    private float timer2 = 0f;
    public float updateInterval = 1f;

    void Update()
    {
        for (int i = 0; i < indexes.Length; i++)
        {
            if (indexes[i] != -1)
            {
                rays[i].Step(Time.deltaTime * dtScale);
                if (rays[i].stopped)
                {
                    if (SetScale.shouldRenderTexture == false)
                    {
                        SetScale.ShowTexture(texture);
                    }
                    texture.SetPixel(rays[i].pxj, (numRaysX - 1 - rays[i].pxi), Consts.px[rays[i].pxi, rays[i].pxj]);
                    indexes[i] = -1;
                    rays[i].DestroySelf();
                    destroyed++;
                }
            }
        }

        if (textureCount >= 50)
        {
            if (destroyed >= destroyNum) resetAll();
            destroyNum = (int)(destroyThresh * ((float)numRaysY * (float)numRaysX));
            textureCount = -1;
        }
        textureCount++;

        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            texture.Apply();
            timer2 = 0f;
        }
    }
}