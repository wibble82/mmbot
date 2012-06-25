using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace convexcad.Shapes
{
    public class Scene
    {
        public Node Root = null;

        public static List<string> Stages = new List<string>();
        public static Node LastNode = null;
        public static int TargetStage = -1;
        public static SafeScreenSpaceLines3D DebugLines = null;

        public static void AddDebugLine(Point3D a, Point3D b)
        {
            if(DebugLines != null)
                DebugLines.AddLine(a,b);
        }

        public static void AddDebugCross(Point3D a, double sz)
        {
            if (DebugLines != null)
                DebugLines.AddCross(a, sz);
        }

        public static bool NextStage(string stage_name)
        {
            if (TargetStage == -1 || Stages.Count <= TargetStage)
            {
                Stages.Add(stage_name);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsCurrentStage()
        {
            return TargetStage >= 0 && TargetStage == Stages.Count - 1;
        }

        public Scene Run()
        {
            Stages.Clear();
            LastNode = null;
            Root = Create();
            if (Root != null)
                Root.Run();
            return this;
        }

        public virtual Node Create()
        {
            return null;
        }

        public Node Rectangle(double x, double y)
        {
            return new RectangleNode(x, y);
        }

        public Node Box(double x, double y, double z)
        {
            return new BoxNode(x, y, z);
        }

        public Node Translate(double x, double y, double z, params Node[] nodes)
        {
            return new TranslateNode(x, y, z, nodes);
        }

        public Node Rotate(double axisx, double axisy, double axisz, double angle, params Node[] nodes)
        {
            return new RotateNode(new Vector3D(axisx, axisy, axisz), angle, nodes);
        }

        public Node Union(params Node[] nodes)
        {
            return new UnionNode(nodes);
        }
        public Node Difference(params Node[] nodes)
        {
            return new DifferenceNode(nodes);
        }
        public Node Intersect(params Node[] nodes)
        {
            return new IntersectNode(nodes);
        }
        public Node SplitByRay(double raystart_x, double raystart_y, double raystart_z, double raydir_x, double raydir_y, double raydir_z, params Node[] nodes)
        {
            return new SplitByRayNode(new Point3D(raystart_x,raystart_y,raystart_z), new Vector3D(raydir_x,raydir_y,raydir_z), nodes);
        }


    }
}
