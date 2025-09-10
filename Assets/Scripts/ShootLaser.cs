using UnityEngine;

public class ShootLaser : MonoBehaviour
{
    public Material beamMaterial;      // Material for the laser beam
    public LayerMask collisionMask;    // What layers the laser should hit (Mirror, Walls, etc.)
    //public float thickness;
    LaserBeam beam;
    public int beam_num;

    void Update()
    {
        // Destroy the old beam before redrawing
        GameObject oldBeam = GameObject.Find("LaserBeam"+beam_num);
        if (oldBeam != null) Destroy(oldBeam);

        // Fire new laser starting at this object's position & forward direction
        beam = new LaserBeam(transform.position, transform.right, beamMaterial, collisionMask, beam_num);
    }
}
