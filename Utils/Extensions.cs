using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaseMod.Utils;
public static class Extensions
{
    public static float Distance(this Vector thisVector, Vector vector2)
    {
        float deltaX = vector2.X - thisVector.X;
        float deltaY = vector2.Y - thisVector.Y;
        float deltaZ = vector2.Z - thisVector.Z;

        float distanceSquared = deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ;
        float distance = (float)Math.Sqrt(distanceSquared);

        return distance;
    }

}
