using System;
using UnityEngine;

public class rayScript : MonoBehaviour
{
    public GameObject main;
    public int index;

    double C;
    double G;
    double mass_bh;
    double scale;
    double r_s;
    double x; double y;
    double r; double phi;
    double dr; double dphi;

    void Start()
    {
        C = Consts.C;
        G = Consts.G;
        mass_bh = Consts.mass_bh;
        scale = Consts.scale;
        r_s = Consts.r_s;

        TrailRenderer trailRenderer = main.GetComponent<TrailRenderer>();
        trailRenderer.startWidth = main.transform.localScale.x;
    }
    Vector2 realworld(double dt)
    {
        x = (transform.position.x * scale) - (-C * dt);
        y = (transform.position.y * scale);
        return new Vector2((float)(transform.position.x*scale), (float)(transform.position.y*scale));
    }

    Vector2 unitygrid(double x, double y)
    {
        return new Vector2((float)(x / scale), (float)(y / scale));
    }

    double[] geodesic(double[] temp, double r_s, double dt)
    {
        double r = temp[0]; double phi = temp[1]; double dr = temp[2]; double dphi = temp[3];

        double d2r = r * dphi * dphi - (C * C * r_s) / (2.0 * r * r);
        double d2phi = -2.0 * dr * dphi / r;

        return new double[4] { dr, dphi, d2r, d2phi };
    }

    double[] addState(double[] a, double[] b, double factor, double[] c)
    {
        for (int i = 0; i < 4; i++)
        {
            c[i] = a[i] + b[i] * factor;
        }
        return c;
    }

    void rk4Step (double dt, double r_s)
    {
        double[] y0 = new double[4] { r, phi, dr, dphi };
        double[] k1 = new double[4]; double[] k2 = new double[4]; double[] k3 = new double[4]; double[] k4 = new double[4]; double[] temp = new double[4];

        k1 = geodesic(new double[] { r, phi, dr, dphi }, r_s, dt);
        temp = addState(y0, k1, dt / 2.0, temp);
        k2 = geodesic(temp, r_s, dt);

        temp = addState(y0, k2, dt / 2.0, temp);
        k3 = geodesic(temp, r_s, dt);

        temp = addState(y0, k3, dt / 2.0, temp);
        k4 = geodesic(temp, r_s, dt);

        r    += (dt / 6.0) * (k1[0] + 2 * k2[0] + 2 * k3[0] + k4[0]);
        phi  += (dt / 6.0) * (k1[1] + 2 * k2[1] + 2 * k3[1] + k4[1]);
        dr   += (dt / 6.0) * (k1[2] + 2 * k2[2] + 2 * k3[2] + k4[2]);
        dphi += (dt / 6.0) * (k1[3] + 2 * k2[3] + 2 * k3[3] + k4[3]);
    }

    public void Step(double dt)
    {
        realworld(dt);

        r = Math.Sqrt(x*x + y*y);
        phi = Math.Atan2(y, x);
        rk4Step(dt, r_s);

        if (r < r_s)
        {
            return;
        }

        x = Math.Cos(phi) * r;
        y = Math.Sin(phi) * r;
        transform.position = unitygrid(x, y);
    }
}