using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.HableCurve;

public class LaserBeam
{
    Vector3 pos, dir;
    GameObject beamParent;   // Holds all beam cylinders
    List<GameObject> segments = new List<GameObject>(); // Store each cylinder

    // Limit reflections (avoid infinite bouncing between mirrors)
    int maxReflections = 10;

    public LaserBeam(Vector3 pos, Vector3 dir, Material material, LayerMask mask, int beam_num)
    {
        this.pos = pos;
        this.dir = dir;

        // Create parent object for all beam segments
        beamParent = new GameObject("LaserBeam"+ beam_num);

        // Start casting the laser
        CastRay(pos, dir, material, mask, 0);
    }

    void CastRay(Vector3 pos, Vector3 dir, Material mat, LayerMask mask, int reflections)
    {
        if (reflections > maxReflections) return; // safety cutoff

        Ray ray = new Ray(pos, dir);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, mask))
        {
            // Draw beam up to hit point
            CreateBeamSegment(pos, hit.point, mat);

            if (hit.collider.CompareTag("Mirror"))
            {
                // Reflect direction using hit surface normal
                Vector3 reflectDir = Vector3.Reflect(dir, hit.normal);

                // Small offset so ray doesn’t re-hit same collider
                Vector3 newPos = hit.point + reflectDir * 0.01f;

                // Keep casting recursively
                CastRay(newPos, reflectDir, mat, mask, reflections + 1);
            }
            else
            {
                // Hit a non-mirror object → stop laser here
                return;
            }
        }
        else
        {
            // If nothing hit → extend beam max length
            Vector3 end = ray.GetPoint(100f);
            CreateBeamSegment(pos, end, mat);
        }
    }

    void CreateBeamSegment(Vector3 start, Vector3 end, Material mat)
    {
        // Create cylinder for this beam segment
        GameObject cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cyl.transform.SetParent(beamParent.transform);
        cyl.GetComponent<Renderer>().material = mat;

        // ✅ Keep collider and make it trigger
        Collider col = cyl.GetComponent<Collider>();
        col.isTrigger = true;

        // Position cylinder halfway between start & end
        Vector3 mid = (start + end) / 2f;
        cyl.transform.position = mid;

        // Scale cylinder to match distance
        float length = Vector3.Distance(start, end);
        cyl.transform.localScale = new Vector3(0.5f, length / 2f, 0.5f);

        // Rotate cylinder so its "up" axis points along beam direction
        cyl.transform.up = (end - start).normalized;

        // ✅ Set tag for the laser segment
        cyl.tag = "LaserBeam"; // Make sure you created this tag in Unity

        segments.Add(cyl);
    }
}
