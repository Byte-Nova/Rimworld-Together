using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace GameClient
{
    public class WorldObjectFakeOrbitingShip : WorldObject
    {
        public override Vector3 DrawPos
        {
            get
            {
                return drawPos;
            }
        }
        public override string Label
        {
            get
            {
                if (name == null)
                {
                    return base.Label;
                }
                return name;
            }
        }
        public string name;
        public Vector3 drawPos;
        Vector3 targetDrawPos = new Vector3(0, 0, 0);
        Vector3 originDrawPos = new Vector3(0, 0, 0);
        public float radius;
        public float phi;
        public float theta;
        public float altitude;

        public void OrbitSet() 
        {
            Vector3 v = Vector3.SlerpUnclamped(new Vector3(0, 0, 1) * radius, new Vector3(0, 0, 1) * radius * -1, theta * -1);
            drawPos = new Vector3(v.x, phi, v.z);
        }
    }
}
