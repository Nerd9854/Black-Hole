using System;
using UnityEngine;
using Unity.Mathematics;

public class rayScript3D : MonoBehaviour
{
    public int pxi;
    public int pxj;


    public GameObject main;
    public Vector3Int index;
    public int maxStepCount;
    public Color color;
    public bool stopped = false;
    Vector3 startPos;
    public Texture2D texture;
    public Transform sphereTransform;
    public bool showRay;
    public Texture2D disk;
    //public Color StartDiskCol = new Color(1f, 0f, 0f);
    //public Color EndDiskCol = new Color(1f, 193f / 255f, 0f);

    Color spaceCol = new Color(15f / 255f, 0f, 50f / 255f, 1f);
    double scale;
    double r_s;
    double boundingSpere;
    double x; double y; double z;
    double r; double phi; double theta;
    double dr; double dphi; double dtheta;
    double diskHalfHeight;
    double L; double E;
    const double EPS = 1e-12;
    const double THETA_EPS = 1e-6;
    const double SIN_EPS = 1e-12;


    void Start()
    {
        if (Math.Abs(x) < 1e-6 && Math.Abs(y) < 1e-6) x += 1e-6;
        gameObject.name = $"Ray {index[0]}";

        transform.localScale = new Vector3(Consts.rayRad, Consts.rayRad, Consts.rayRad);

        scale = Consts.scale;
        r_s = Consts.r_s;
        diskHalfHeight = (0.1 * r_s) / scale;

        TrailRenderer trailRenderer = main.GetComponent<TrailRenderer>();
        trailRenderer.startWidth = main.transform.localScale.x;

        SpriteRenderer spriteRenderer= main.GetComponent<SpriteRenderer>();
        spriteRenderer.color = color;
        spriteRenderer.enabled = showRay;

        startPos = transform.position;

        x = ((double)transform.position.x * scale);
        y = ((double)transform.position.y * scale);
        z = ((double)transform.position.z * scale);

        r = Math.Sqrt(x * x + y * y + z * z);
        if (r < EPS) r = EPS;

        theta = Math.Acos(Math.Clamp(z / r, -1.0, 1.0));
        theta = Math.Clamp(theta, THETA_EPS, Math.PI - THETA_EPS);

        phi = Math.Atan2(y, x);

        Vector3 dir = transform.forward.normalized;

        double sinth = SafeSin(theta);

        double rx = sinth * Math.Cos(phi);
        double ry = sinth * Math.Sin(phi);
        double rz = Math.Cos(theta);

        double thetax = Math.Cos(theta) * Math.Cos(phi);
        double thetay = Math.Cos(theta) * Math.Sin(phi);
        double thetaz = -sinth;

        double phix = -Math.Sin(phi);
        double phiy = Math.Cos(phi);
        double phiz = 0.0;

        double velocityScale = Consts.C;

        double v_r     = dir.x * rx     + dir.y * ry     + dir.z * rz;
        double v_theta = dir.x * thetax + dir.y * thetay + dir.z * thetaz;
        double v_phi   = dir.x * phix   + dir.y * phiy   + dir.z * phiz;

        dr = v_r * velocityScale;
        dtheta = v_theta * velocityScale / r;
        dphi = v_phi * velocityScale / (r * sinth);

        L = r * r * Math.Pow(sinth, 2) * dphi;
        double f = 1.0 - r_s / Math.Max(r, r_s + EPS);
        double dt_dt = Math.Sqrt(Math.Max((dr * dr) / Math.Max(f, 1e-12) + r * r * dtheta * dtheta + r * r * sinth * sinth * dphi * dphi, 0.0));
        E = dt_dt;


        boundingSpere = Consts.boundingSpere;//r_s * 50.0;
    }

    public void DestroySelf(bool reset = false)
    {
        if (reset) Stopped(spaceCol, false);
        Destroy(gameObject);
    }

    public void updateSize() { if (!stopped) transform.localScale = new Vector3(Consts.rayRad, Consts.rayRad, Consts.rayRad); }
    double SafeSin(double th) { return Math.Max(Math.Abs(Math.Sin(th)), SIN_EPS) * Math.Sign(Math.Sin(th)); }


    bool Stopped(Color col, bool reset, bool getPx = false)
    {
        if (stopped) return true;

        SphereCollider collider = main.GetComponent<SphereCollider>();
        Destroy(collider);
        if (reset)
        {
            x = 0; y = 0; z = 0;
            transform.position = unitygrid(x, y, z);
            transform.position = startPos;
        }
        if (getPx) col = GetPixelFromWorldPos(texture, sphereTransform, transform.position);
        Consts.px[index.y, index.z] = col;

        SpriteRenderer spriteRenderer = main.GetComponent<SpriteRenderer>();
        spriteRenderer.color = col;
        spriteRenderer.enabled = true;
        stopped = true;
        return true;
    }

    Vector3 unitygrid(double x, double y, double z)
    {
        return new Vector3((float)(x / scale), (float)(y / scale), (float)(z / scale));
    }

    double[] geodesic(double[] temp, double r_sl, double dt)
    {
        double r = temp[0]; double theta = temp[1]; double phi = temp[2]; double dr = temp[3]; double dtheta = temp[4]; double dphi = temp[5];

        r = Math.Max(r, r_sl + 1e-12);
        double f = 1.0 - r_sl / r;
        double dt_dlambda = E / Math.Max(f, 1e-12);
        double cosTh = Math.Cos(theta);
        double sinth_safe = SafeSin(theta);
        double sinth2 = sinth_safe * sinth_safe;

        double d2r = -(r_sl / (2.0 * r * (r - r_sl + 1e-12))) * dr * dr
            +(r - r_sl) * (dtheta * dtheta + sinth2 * dphi * dphi)
            -(r_sl * (r - r_sl) / (2.0 * r * r * r + 1e-12)) * (dt_dlambda * dt_dlambda);

        double d2theta = -(2.0 / r) * dr * dtheta + sinth_safe * cosTh * dphi * dphi;

        double d2phi = -(2.0 / r) * dr * dphi - 2.0 * (cosTh / sinth_safe) * dtheta * dphi;

        return new double[6] { dr, dtheta, dphi, d2r, d2theta, d2phi };
    }


    double[] addState(double[] a, double[] b, double factor, double[] c)
    {
        for (int i = 0; i < 6; i++)
        {
            c[i] = a[i] + b[i] * factor;
        }
        return c;
    }

    void rk4Step(double dt, double r_s)
    {
        if (double.IsNaN(r) || double.IsInfinity(r) || double.IsNaN(theta) || double.IsInfinity(theta) || double.IsNaN(phi) || double.IsInfinity(phi)) return;

        double[] y0 = new double[6] { r, theta, phi, dr, dtheta, dphi };
        double[] k1 = new double[6]; double[] k2 = new double[6]; double[] k3 = new double[6]; double[] k4 = new double[6]; double[] temp = new double[6];

        k1 = geodesic(y0, r_s, dt);
        temp = addState(y0, k1, dt / 2.0, temp);
        k2 = geodesic(temp, r_s, dt);

        temp = addState(y0, k2, dt / 2.0, temp);
        k3 = geodesic(temp, r_s, dt);

        temp = addState(y0, k3, dt, temp);
        k4 = geodesic(temp, r_s, dt);

        dt /= 6.0;

        r      += dt * (k1[0] + 2 * k2[0] + 2 * k3[0] + k4[0]);
        theta  += dt * (k1[1] + 2 * k2[1] + 2 * k3[1] + k4[1]);
        phi    += dt * (k1[2] + 2 * k2[2] + 2 * k3[2] + k4[2]);
        dr     += dt * (k1[3] + 2 * k2[3] + 2 * k3[3] + k4[3]);
        dtheta += dt * (k1[4] + 2 * k2[4] + 2 * k3[4] + k4[4]);
        dphi   += dt * (k1[5] + 2 * k2[5] + 2 * k3[5] + k4[5]);

        theta = Math.Clamp(theta, THETA_EPS, Math.PI - THETA_EPS);
        phi = Math.IEEERemainder(phi, 2.0 * Math.PI);
        
        double sin2 = Math.Pow(SafeSin(theta), 2);
        if (sin2 < 1e-12) sin2 = 1e-12;
        if (sin2 < 1e-6) dphi = 0.0;
        else dphi = L / (r * r * sin2);

        r = Math.Max(r, r_s * 0.999999 + 1e-12);

    }

    static Color GetPixelFromWorldPos(Texture2D texture, Transform sphereTransform, Vector3 worldPos)
    {
        Vector3 localPos = sphereTransform.InverseTransformPoint(worldPos).normalized;

        float longitude = Mathf.Atan2(localPos.z, localPos.x);
        float latitude = Mathf.Acos(localPos.y);

        float u = (longitude + Mathf.PI) / (2f * Mathf.PI);
        float v = 1f - (latitude / Mathf.PI);

        int x = Mathf.Clamp(Mathf.FloorToInt(u * texture.width), 0, texture.width - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(v * texture.height), 0, texture.height - 1);

        return texture.GetPixel(x, y);
    }

    bool CheckAndColor()
    {
        if (r > 0)
        {
            double sinth = SafeSin(theta);
            double sin2 = sinth * sinth;

            dphi = L / (r * r * sin2);
            
            double f_local = 1.0 - r_s / Math.Max(r, r_s + 1e-12);
            double angularPart = r * r * dtheta * dtheta + r * r * sin2 * dphi * dphi;
            double inside = Math.Max(E * E - angularPart, 0.0);
            double new_dr_abs = Math.Sqrt(Math.Max(inside * f_local, 0.0));
            if (Math.Abs(dr) > 0) dr = Math.Sign(dr) * new_dr_abs; else dr = +new_dr_abs;
        }


        if (double.IsNaN(r) || double.IsNaN(theta) || double.IsNaN(phi) ||
            double.IsInfinity(r) || double.IsInfinity(theta) || double.IsInfinity(phi)) return Stopped(spaceCol, false);


        if (r <= r_s) return Stopped(Color.black, false);
        if (r >= boundingSpere) return Stopped(spaceCol, false, true);

        double r_scaled = r / scale;
        double dOut_scaled = Consts.dOut / 2d;
        double dIn_scaled = Consts.dIn / 2d;
        if (Math.Abs(transform.position.y) <= diskHalfHeight && r_scaled <= dOut_scaled && r_scaled >= dIn_scaled)
        {
            float t = Mathf.InverseLerp((float)dIn_scaled, (float)dOut_scaled, (float)r_scaled);
            //Color Grad = Color.Lerp(StartDiskCol, EndDiskCol, t);
            int py = (int)(t * disk.height) + UnityEngine.Random.Range(-1, 2);
            py = math.clamp(py, 0, disk.height);

            Color Grad = disk.GetPixel(0, py);
            return Stopped(Grad, false);

        }

        if (Math.Abs(x / scale) > 1e5 || Math.Abs(y / scale) > 1e5 || Math.Abs(z / scale) > 1e5) return Stopped(spaceCol, false);

        return false;
    }

    public bool Step(double dt)
    {
        if (stopped) return true;
        if (CheckAndColor()) return true;

        if (r < 4*r_s) dt /= 3;

        double f = 1.0 - r_s / Math.Max(r, r_s + 1e-12);
        double safety = Math.Clamp(f, 1e-3, 1.0);
        rk4Step(dt, r_s);

        x = r * SafeSin(theta) * Math.Cos(phi);
        y = r * SafeSin(theta) * Math.Sin(phi);
        z = r * Math.Cos(theta);

        if (Math.Abs(x / scale) > 1e5 || Math.Abs(y / scale) > 1e5 || Math.Abs(z / scale) > 1e5) return Stopped(spaceCol, false);

        try {transform.position = unitygrid(x, y, z);}
        catch (Exception) {x = 0; y = 0; z = 0; stopped = true; }
        return false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Star 1")
        {
            Stopped(Color.green, false);
        }
    }
}