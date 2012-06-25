using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace convexcad
{
    public static class MathUtils
    {
        public static double MAX_VALUE = 999999999;
        public static double EPSILON   = 0.000000001;

        public static Vector3D Normalize(Vector3D val) { val.Normalize(); return val; }

        public static Vector3D LeftXY(Vector3D val) { double x = val.X; val.X = val.Y; val.Y = -x; return val; }
        public static Vector3D LeftYZ(Vector3D val) { double y = val.Y; val.Y = val.Z; val.Z = -y; return val; }
        public static Vector3D LeftZX(Vector3D val) { double z = val.Z; val.Z = val.X; val.X = -z; return val; }
        public static Vector3D RightXY(Vector3D val) { double x = val.X; val.X = -val.Y; val.Y = x; return val; }
        public static Vector3D RightYZ(Vector3D val) { double y = val.Y; val.Y = -val.Z; val.Z = y; return val; }
        public static Vector3D RightZX(Vector3D val) { double z = val.Z; val.Z = -val.X; val.X = z; return val; }

        public static Vector3D TangentXY(Vector3D val) { return LeftXY(val); }
        public static Vector3D TangentYZ(Vector3D val) { return LeftYZ(val); }
        public static Vector3D TangentZX(Vector3D val) { return LeftZX(val); }

        public static double CrossXY(Vector3D u, Vector3D v) { return u.Y * v.X - u.X * v.Y; }
        public static double CrossYZ(Vector3D u, Vector3D v) { return u.Z * v.Y - u.Y * v.Z; }
        public static double CrossZX(Vector3D u, Vector3D v) { return u.X * v.Z - u.Z * v.X; }

        /*public static bool IsInsideXY(Point3D point, Point3D raystart, Point3D raydir)
        {
            return CrossXY(point - raystart, raydir) >= 0;
        }*/

        public static Point3D Centre(IEnumerable<Point3D> points)
        {
            Vector3D psum = new Vector3D(0, 0, 0);
            int count = 0;
            foreach (Point3D p in points)
            {
                psum.X += p.X;
                psum.Y += p.Y;
                psum.Z += p.Z;
                count++;
            }
            if (count != 0)
                return (Point3D)(psum / count);
            else
                return new Point3D(0, 0, 0);
        }
        public static Point3D Sum(IEnumerable<Point3D> points)
        {
            Vector3D psum = new Vector3D(0,0,0);
            foreach (Point3D p in points)
            {
                psum.X += p.X;
                psum.Y += p.Y;
                psum.Z += p.Z;
            }
            return (Point3D)psum;
        }
        public static Point3D Min(IEnumerable<Point3D> points)
        {
            Point3D pmin = new Point3D(MAX_VALUE,MAX_VALUE,MAX_VALUE);
            foreach(Point3D p in points)
            {
                pmin.X = Math.Min(pmin.X, p.X);
                pmin.Y = Math.Min(pmin.Y, p.Y);
                pmin.Z = Math.Min(pmin.Z, p.Z);
            }
            return pmin;
        }
        public static Point3D Max(IEnumerable<Point3D> points)
        {
            Point3D pmax = new Point3D(-MAX_VALUE, -MAX_VALUE, -MAX_VALUE);
            foreach (Point3D p in points)
            {
                pmax.X = Math.Max(pmax.X, p.X);
                pmax.Y = Math.Max(pmax.Y, p.Y);
                pmax.Z = Math.Max(pmax.Z, p.Z);
            }
            return pmax;
        }
        public static void MinMax(IEnumerable<Point3D> points, ref Point3D pmin, ref Point3D pmax)
        {
            pmin.X = pmin.Y = pmin.Z = MAX_VALUE;
            pmax.X = pmax.Y = pmax.Z = -MAX_VALUE;
            foreach (Point3D p in points)
            {
                pmin.X = Math.Min(pmin.X, p.X);
                pmin.Y = Math.Min(pmin.Y, p.Y);
                pmin.Z = Math.Min(pmin.Z, p.Z);
                pmax.X = Math.Max(pmax.X, p.X);
                pmax.Y = Math.Max(pmax.Y, p.Y);
                pmax.Z = Math.Max(pmax.Z, p.Z);
            }
        }
        

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

        public enum RayRayResult
        {
            UNKOWN,
            CLOSEST,
            PARALLEL,
            PARALLEL_OVERLAPPING,
            INTERSECTING
        }

        /// <summary>
        /// Finds closest point between 2 rays and returns 1 of 4 results:
        /// - NO_INTERSECTION 
        /// </summary>
        public static RayRayResult ClosestPointRayRay(out double closestdist, out double closestu, out double closestv, Point3D ray0start, Vector3D ray0dir, Point3D ray1start, Vector3D ray1dir)
        {
            Vector3D u = ray0dir;
            Vector3D v = ray1dir;
            Vector3D w = ray0start - ray1start; ;
            double    a = Vector3D.DotProduct(u,u);        // always >= 0
            double    b = Vector3D.DotProduct(u,v);
            double    c = Vector3D.DotProduct(v,v);        // always >= 0
            double    d = Vector3D.DotProduct(u,w);
            double    e = Vector3D.DotProduct(v,w);
            double    D = a*c - b*b;       // always >= 0
            double    sc, tc;

            // compute the line parameters of the two closest points
            RayRayResult res_positive, res_negative;
            if (D <= EPSILON) 
            {         
                // the lines are almost parallel
                sc = 0.0;
                tc = (b>c ? d/b : e/c);   // use the largest denominator

                res_positive = RayRayResult.PARALLEL_OVERLAPPING;
                res_negative = RayRayResult.PARALLEL;
            }
            else 
            {
                sc = (b*e - c*d) / D;
                tc = (a*e - b*d) / D;

                res_positive = RayRayResult.INTERSECTING;
                res_negative = RayRayResult.CLOSEST;
            }

            Vector3D dP = w + (sc * u) - (tc * v);  // = L1(sc) - L2(tc)

            closestdist = dP.Length;
            closestu = sc;
            closestv = tc;
            
            return closestdist <= EPSILON ? res_positive : res_negative;
        }

        public enum RayLineResult
        {
            UNKOWN,

            CLOSEST_POS0,
            CLOSEST_POS1,
            CLOSEST_LINE,

            PARALLEL,
            PARALLEL_OVERLAPPING,
            
            INTERSECTING_POS0,
            INTERSECTING_POS1,
            INTERSECTING_LINE
        }

        /// <summary>
        /// Finds closest point between 2 rays and returns 1 of 4 results:
        /// - NO_INTERSECTION 
        /// </summary>
        public static RayLineResult ClosestPointRayLine(out double closestdist, out double closestu, out double closestv, Point3D raystart, Vector3D raydir, Point3D linea, Point3D lineb)
        {
            Vector3D u = raydir;
            Vector3D v = lineb-linea;
            Vector3D w = raystart-linea;
            double a = Vector3D.DotProduct(u, u);        // always >= 0
            double b = Vector3D.DotProduct(u, v);
            double c = Vector3D.DotProduct(v, v);        // always >= 0
            double d = Vector3D.DotProduct(u, w);
            double e = Vector3D.DotProduct(v, w);

            double D = a * c - b * b;       // always >= 0
            double sc, sN, sD = D;      // sc = sN / sD, default sD = D >= 0
            double tc, tN, tD = D;      // tc = tN / tD, default tD = D >= 0

            RayLineResult res = RayLineResult.UNKOWN;

	        // compute the line parameters of the two closest points
	        if (D <= EPSILON) 
            {	
                // the lines are almost parallel
		        tN = 0.0;			// force using point P0 on the line
		        tD = 1.0;			// to prevent possible division by 0.0 later
		        sN = -d;
		        sD = a;
                res = RayLineResult.PARALLEL;
	        }
	        else 
            {				
                // get the closest points on the infinite lines
                sN = (b * e - c * d);
                tN = (a * e - b * d);

                res = RayLineResult.CLOSEST_LINE;

	            if (tN <= 0.0) 
                {		
                    // tc < 0 => the t=0 edge is visible
		            tN = 0.0;
                    res = RayLineResult.CLOSEST_POS0;
		        
                    // recompute sc for this edge
                    sN = -d;
                    sD = a;
	            }
	            else if (tN >= tD) 
                {	
                    // tc > 1 => the t=1 edge is visible
		            tN = tD;
                    res = RayLineResult.CLOSEST_POS1;
		        
                    // recompute sc for this edge
                    sN = (-d + b);
                    sD = a;
	            }
	        }

	        // finally do the division to get sc and tc
	        sc = (Math.Abs(sN) <= EPSILON ? 0.0 : sN / sD);
	        tc = (Math.Abs(tN) <= EPSILON ? 0.0 : tN / tD);

	        // get the difference of the two closest points
            Vector3D dP = w + (sc * u) - (tc * v);  // = L1(sc) - L2(tc)

            closestdist = dP.Length;
            closestu = sc;
            closestv = tc;

            if (closestdist <= EPSILON)
            {
                switch (res)
                {
                    case RayLineResult.CLOSEST_LINE:
                        return RayLineResult.INTERSECTING_LINE;
                    case RayLineResult.CLOSEST_POS0:
                        return RayLineResult.INTERSECTING_POS0;
                    case RayLineResult.CLOSEST_POS1:
                        return RayLineResult.INTERSECTING_POS1;
                    case RayLineResult.PARALLEL:
                        return RayLineResult.PARALLEL_OVERLAPPING;
                    default:
                        throw new System.ApplicationException("Unexpected result");
                }
            }
            else
            {
                return res;
            }
        }

        /*double IntersectLineLine(Segment S1, Segment S2)
        {
            Vector   u = S1.P1 - S1.P0;
            Vector   v = S2.P1 - S2.P0;
            Vector   w = S1.P0 - S2.P0;
            float    a = dot(u,u);        // always >= 0
            float    b = dot(u,v);
            float    c = dot(v,v);        // always >= 0
            float    d = dot(u,w);
            float    e = dot(v,w);
            float    D = a*c - b*b;       // always >= 0
            float    sc, sN, sD = D;      // sc = sN / sD, default sD = D >= 0
            float    tc, tN, tD = D;      // tc = tN / tD, default tD = D >= 0

            // compute the line parameters of the two closest points
            if (D < SMALL_NUM) { // the lines are almost parallel
                sN = 0.0;        // force using point P0 on segment S1
                sD = 1.0;        // to prevent possible division by 0.0 later
                tN = e;
                tD = c;
            }
            else {                // get the closest points on the infinite lines
                sN = (b*e - c*d);
                tN = (a*e - b*d);
                if (sN < 0.0) {       // sc < 0 => the s=0 edge is visible
                    sN = 0.0;
                    tN = e;
                    tD = c;
                }
                else if (sN > sD) {  // sc > 1 => the s=1 edge is visible
                    sN = sD;
                    tN = e + b;
                    tD = c;
                }
            }

            if (tN < 0.0) {           // tc < 0 => the t=0 edge is visible
                tN = 0.0;
                // recompute sc for this edge
                if (-d < 0.0)
                    sN = 0.0;
                else if (-d > a)
                    sN = sD;
                else {
                    sN = -d;
                    sD = a;
                }
            }
            else if (tN > tD) {      // tc > 1 => the t=1 edge is visible
                tN = tD;
                // recompute sc for this edge
                if ((-d + b) < 0.0)
                    sN = 0;
                else if ((-d + b) > a)
                    sN = sD;
                else {
                    sN = (-d + b);
                    sD = a;
                }
            }
            // finally do the division to get sc and tc
            sc = (abs(sN) < SMALL_NUM ? 0.0 : sN / sD);
            tc = (abs(tN) < SMALL_NUM ? 0.0 : tN / tD);

            // get the difference of the two closest points
            Vector   dP = w + (sc * u) - (tc * v);  // = S1(sc) - S2(tc)

            return norm(dP);   // return the closest distance
        }
        //===================================================================*/
        
    }
}
