using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace convexcad
{
    static class MathUtils
    {
        public static bool IntersectRayRay2d(ref Point3D hitpoint, ref double hitu, ref double hitv, Point3D ray0start, Vector3D ray0dir, Point3D ray1start, Vector3D ray1dir)
        {
            // Denominator for ua and ub are the same, so store this calculation
            double d =
               (ray1dir.Y) * (ray0dir.X)
               -
               (ray1dir.X) * (ray0dir.Y);

            //calc offset of lines
            Vector3D offset = ray0start - ray1start;

            //n_a and n_b are calculated as separate values for readability
            double n_a =
               ray1dir.X * offset.Y
               -
               ray1dir.Y * offset.X;

            double n_b =
               ray0dir.X * offset.Y
               -
               ray0dir.Y * offset.X;

            // Make sure there is not a division by zero - this also indicates that
            // the lines are parallel.  
            // If n_a and n_b were both equal to zero the lines would be on top of each 
            // other (coincidental).  This check is not done because it is not 
            // necessary for this implementation (the parallel check accounts for this).
            if (d == 0)
                return false;

            // Calculate the intermediate fractional point that the lines potentially intersect.
            double ua = n_a / d;
            double ub = n_b / d;

            hitu = ua;
            hitv = ub;
            hitpoint = ray0start + ua * ray0dir;
            return true;
        }

        public static bool IntersectRayLine2d(ref Point3D hitpoint, ref double hitu, ref double hitv, Point3D raystart, Vector3D raydir, Point3D linea, Point3D lineb)
        {
            Vector3D linedir = lineb - linea;

            if (IntersectRayRay2d(ref hitpoint, ref hitu, ref hitv, raystart, raydir, linea, linedir))
            {
                if (hitv > 0d && hitv < 1d)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IntersectLineLine2d(ref Point3D hitpoint, ref double hitu, ref double hitv, Point3D line0a, Point3D line0b, Point3D line1a, Point3D line1b)
        {
            Vector3D line0dir = line0b - line0a;
            Vector3D line1dir = line1b - line1a;

            if (IntersectRayRay2d(ref hitpoint, ref hitu, ref hitv, line0a, line0dir, line1a, line1dir))
            {
                if (hitu > 0d && hitu < 1d && hitv > 0d && hitv < 1d)
                {
                    return true;
                }
            }
 
            return false;
        }
    }
}
