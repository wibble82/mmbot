using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace convexcad
{
    namespace Geometry
    {
        public class CSGScene
        {
            public Node Root = null;
            public static SafeScreenSpaceLines3D DebugLines = null;
            public static int TargetStage = -1;
            public static List<string> Stages = new List<string>();
            public static Node LastNode = null;

            public static bool NextStage(string stage_name)
            {
                if (TargetStage==-1 || Stages.Count <= TargetStage)
                {
                    Stages.Add(stage_name);
                    return true;
                }
                else
                {
                    return false;
                }
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
        }
    }
}
