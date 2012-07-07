using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace convexcad.Shapes
{
    public class Node
    {
        public List<Shape> Shapes = new List<Shape>();
        public Node Parent = null;
        public List<Node> Children = new List<Node>();
        public bool Is3d = false;

        public Node()
        {

        }


        public void Run()
        {
            foreach (Node n in Children)
                n.Run();

            if (Scene.NextStage("NodeCreate"))
            {
                Create();
                Scene.LastNode = this;
            }
        }

        public virtual void Create() { }

        public void SetChildren(params Node[] nodes)
        {
            Children = nodes.ToList();
            bool any_2d = false;
            bool any_3d = false;
            foreach (Node n in nodes)
            {
                n.Parent = this;
                any_2d = any_2d || !n.Is3d;
                any_3d = any_3d || n.Is3d;
            }
            if (any_2d && any_3d)
                throw new System.ApplicationException("Error - node must be either 2d or 3d, not both!");
            Is3d = any_3d;
        }
        
    }


    [Serializable]
    public class BoxNode : Node
    {
        Vector3D Size = new Vector3D();

        public BoxNode(double x, double y, double z)
        {
            Size = new Vector3D(x, y, z);
            Is3d = true;
        }

        public override void Create()
        {
        }
    }

    [Serializable]
    public class RectangleNode : Node
    {
        Vector3D Size = new Vector3D();

        public RectangleNode(double x, double y)
        {
            Size = new Vector3D(x, y, 0);
        }

        public override void Create()
        {
            Shapes.Clear();
            Shapes.Add(Primitives.Rectangle(Size));
        }
    }

    [Serializable]
    public class TranslateNode : Node
    {
        Vector3D Translation;

        public TranslateNode(double x, double y, double z, params Node[] nodes)
        {
            Translation = new Vector3D(x, y, z);

            SetChildren(nodes);

        }

        public override void Create()
        {
            Matrix3D m = new TranslateTransform3D(Translation).Value;
            foreach (Node n in Children)
            {
                Shapes.AddRange(n.Shapes.Select(a => a.Copy().ApplyTransform(m)));
            }

        }
    }

    [Serializable]
    public class RotateNode : Node
    {
        Vector3D Axis;
        double Angle;

        public RotateNode(Vector3D axis, double angle, params Node[] nodes)
        {
            Axis = axis;
            Angle = angle;

            SetChildren(nodes);

        }

        public override void Create()
        {
            Matrix3D m = new RotateTransform3D(new AxisAngleRotation3D(Axis, Angle)).Value;
            foreach (Node n in Children)
            {
                Shapes.AddRange(n.Shapes.Select(a => a.Copy().ApplyTransform(m)));
            }

        }
    }

    [Serializable]
    public class UnionNode : Node
    {
        public UnionNode(params Node[] nodes)
        {
            SetChildren(nodes);
        }

        public override void Create()
        {
            /*foreach (Node n in Children)
            {
                Shapes.AddRange(n.Shapes.Select(a => a.Copy()));
            }
            return;*/
            
            if (Is3d)
            {

            }
            else
            {
                Shapes.AddRange(Children[0].Shapes.Select(a => a.Copy()));

                Shapes[0].Convexes[0].Faces[0].Split(Shapes[0].Convexes[0].Edges[1], 0.3, Shapes[0].Convexes[0].Edges[3], 0.25);
                Shapes[0].Convexes[0].Faces[0].IntegrityCheck();
                Shapes[0].Convexes[0].Faces[1].IntegrityCheck();
                double closestdist, closestu, closestv;
                Point3D raystart=new Point3D(0,0,0);
                Vector3D raydir = new Vector3D(5,1,0);
                MathUtils.RayLineResult res = MathUtils.ClosestPointRayLine(out closestdist, out closestu, out closestv, raystart, raydir, Shapes[0].Convexes[0].Edges[1].Vertices[0].Pos, Shapes[0].Convexes[0].Edges[1].Vertices[1].Pos);
                Scene.AddDebugLine(raystart - raydir * 10, raystart + raydir * 10);
                Scene.AddDebugCross(raystart + raydir * closestu, 0.5);
                Scene.AddDebugCross(Shapes[0].Convexes[0].Edges[1].Vertices[0].Pos + Shapes[0].Convexes[0].Edges[1].Offset * closestv, 0.5);
              
                //this'll be the clever algorithm. it takes advantage of the fact that we always maintain:
                //- for any given node, none of it's child convexes overlap
                //therefore:
                //- no node should test it's own convexes against each other


/*

                //this is the simplest algorithm - we assume every convex could potentially overlap every other convex
                //and keep iterating until no more splits occur. it works, but involves a lot of unnecessary overlap tests
                Convexes = new List<Convex>();
                foreach (Node n in Children)
                    Convexes.AddRange(n.Convexes.Select(a => a.Copy()));

                //draw all initial convexes if this is the current stage
                if (!CSGScene.NextStage("Begin union"))
                {
                    foreach (Convex c in Convexes)
                        c.DebugDraw();
                    return;
                }

                //now do the iterative splitting
                //loop until no splits done
                bool done_split = true;
                while (done_split)
                {
                    //spin over every convex
                    done_split = false;
                    for (int i = 0; i < Convexes.Count; i++)
                    {
                        //go over every other convex
                        for (int j = i + 1; j < Convexes.Count; j++)
                        {
                            //get the 2 convexes to compare
                            Convex acvx = Convexes[i];
                            Convex bcvx = Convexes[j];

                            //do a clip test
                            List<Convex> otherconvexsplit = new List<Convex>();
                            Convex overlap = null;
                            if (Convex.CalculateClippedConvexes2d(acvx, bcvx, otherconvexsplit, ref overlap))
                            {
                                //got a split, so remove the convex that was split (cvx a), and re-add the sections
                                //that didn't overlap
                                Convexes.RemoveAt(i);
                                Convexes.AddRange(otherconvexsplit);
                                done_split = true;

                                //if last stage, draw the convex we were splitting and then bail
                                if (!CSGScene.NextStage("Done a split"))
                                {
                                    return;
                                }
                                break;
                            }
                        }

                        //break out (so we iterate round again) if a split happened
                        if (done_split)
                            break;
                    }
                }*/
            }
        }
    }

    [Serializable]
    public class DifferenceNode : Node
    {

        public DifferenceNode(params Node[] nodes)
        {
            SetChildren(nodes);
        }

        public override void Create()
        {
            foreach (Node n in Children)
            {
                Shapes.AddRange(n.Shapes.Select(a => a.Copy()));
            }
        }
    }

    [Serializable]
    public class IntersectNode : Node
    {

        public IntersectNode(params Node[] nodes)
        {
            SetChildren(nodes);
        }

        public override void Create()
        {
            foreach (Node n in Children)
            {
                Shapes.AddRange(n.Shapes.Select(a => a.Copy()));
            }
        }
    }

    [Serializable]
    public class SplitByRayNode : Node
    {
        Point3D RayStart;
        Vector3D RayDir;
        Mesh.ESplitMode SplitMode;

        public SplitByRayNode(Point3D raystart, Vector3D raydir, Mesh.ESplitMode sm, params Node[] nodes)
        {
            SetChildren(nodes);
            RayStart = raystart;
            RayDir = raydir;
            SplitMode = sm;
        }

        public override void Create()
        {
            if (!Scene.NextStage("Create SplitByRayNode"))
            {
                Shapes.AddRange(Children.SelectMany(a=>a.Shapes).Select(a => a.Copy())); // if not doing this stage, just copy over the none split convexes
                return;
            }
            else if (Scene.IsCurrentStage())
            {
                Scene.AddDebugLine(RayStart - RayDir * 10, RayStart + RayDir * 10);
            }

            Shapes.AddRange(Children.SelectMany(a=>a.Shapes).Select(a => a.Copy().Split2d(RayStart,RayDir,SplitMode))); //do full copy then split
        }
    }
}
