using UnityEngine;

public class Consts : MonoBehaviour
{
    public const double C = 299792458.0;
    public const double G = 6.67430e-11;

    public static double mass_bh = 8.54e36;
    public static double scale = 1e10;//1e9;
    public static double dIn;
    public static double dOut;
    public static double boundingSpere;
    public static float rayRad;
    public static rayScript3D[] rays;

    public static double r_s;

    public static Color[,] px;
}