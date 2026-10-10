using UnityEngine;

namespace MongeArcano
{
    /// <summary>Compatibilidade entre Unity 2022.3 (Rigidbody.velocity) e Unity 6+ (Rigidbody.linearVelocity).</summary>
    public static class FisicaCompat
    {
        public static Vector3 GetVel(this Rigidbody rb)
        {
#if UNITY_6000_0_OR_NEWER
            return rb.linearVelocity;
#else
            return rb.velocity;
#endif
        }

        public static void SetVel(this Rigidbody rb, Vector3 v)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = v;
#else
            rb.velocity = v;
#endif
        }
    }
}
