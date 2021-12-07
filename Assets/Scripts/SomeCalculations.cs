using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SomeCalculations
{
    public static Vector3 RightDirection (Vector3 forward)
    {
        // forward.z = cos(a)
        // forward.x = sin(a)

        // res.z = - forward.x
        // res.x = forward.z

        //sin(a - 90) = -cos(a)
        //cos(a - 90) = sin(a)
        return new Vector3(forward.z, forward.y, -forward.x);
    }
}
