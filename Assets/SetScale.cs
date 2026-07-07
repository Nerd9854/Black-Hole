using UnityEngine;

public class SetScale : MonoBehaviour
{
    const double C = 299792458.0;
    const double G = 6.67430e-11;

    private double mass_bh;
    private double scale;

    private float r_sScaled;
    private float dOUTScaled;

    public int index;
    public GameObject Sphere;
    public bool is2D;
    public double zScale;

    void Awake()
    {
        mass_bh = Consts.mass_bh;
        scale = Consts.scale;
        double r_s = 2 * G * mass_bh / (C * C);

        Consts.r_s = r_s;
        float boundingSpere = 60.0f;
        Consts.boundingSpere = (double)(boundingSpere / 2) * scale;
        boundingSpere -= 1;
        double dOUT = 1.8 * r_s;
        float z = (float)((-zScale * r_s) / scale);
        float y = (float)(r_s / scale) * 1f;//* 0.7f;

        r_sScaled = (float)((r_s / scale)*2);
        dOUTScaled = (float)((dOUT / scale)*2);
        Consts.dOut = dOUTScaled;

        if (index == 2 && is2D) Sphere.transform.position = new Vector3(0, 0, z);
        else
        {
            Sphere.transform.localScale = (index == 1)? new Vector3(r_sScaled, r_sScaled, r_sScaled) : 
                                          (index == 0)? new Vector3(dOUTScaled, dOUTScaled, dOUTScaled): 
                                                        new Vector3(boundingSpere, boundingSpere, boundingSpere);
        }
    }



    //this is spagetti as hell and ik it shoudlnet be here but i just want to ship this porject and move on 😭😭
    public static Texture textureToRender;
    public static bool shouldRenderTexture = false;

    public static void ShowTexture(Texture texture)
    {
        textureToRender = texture;
        shouldRenderTexture = true;
    }

    public static void ResetV()
    {
        shouldRenderTexture = false;
    }

    private void OnPostRender()
    {
        if (shouldRenderTexture && textureToRender != null)
        {
            Graphics.Blit(textureToRender, (RenderTexture)null);
        }
    }
}