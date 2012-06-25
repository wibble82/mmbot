using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using _3DTools;
using System.Windows.Media;

namespace convexcad
{
    namespace Geometry
    {
        [Serializable]
        public class Node
        {
            public CSGScene Scene = null;
            public Node Parent = null;
            public List<Node> Children = new List<Node>();
            //public Matrix3D Transform = Matrix3D.Identity;
            public List<Convex> Convexes = new List<Convex>();
            public bool Is3d = false;

            public void Run()
            {
                foreach (Node n in Children)
                    n.Run();

                if (CSGScene.NextStage("NodeCreate"))
                {
                    Create();
                    CSGScene.LastNode = this;
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

            public MeshGeometry3D GetGeometry()
            {
                MeshGeometry3D trimesh = new MeshGeometry3D();

                int first_vert = 0;
                Vector3D tinyoffset = new Vector3D(0, 0, 0);
                foreach (Convex c in Convexes)
                {
                    foreach (Vertex v in c.Vertices)
                    {
                        trimesh.Positions.Add(v.Pos+tinyoffset);
                        trimesh.Normals.Add(new Vector3D(0, 1, 0));
                    }
                    tinyoffset.Z -= 0.0001;
                    foreach (Face f in c.Faces)
                    {
                        int vcnt = f.VertIndices.Length;
                        for (int i = 2; i < vcnt; i++)
                        {
                            trimesh.TriangleIndices.Add(first_vert+f.VertIndices[0]);
                            trimesh.TriangleIndices.Add(first_vert + f.VertIndices[i - 1]);
                            trimesh.TriangleIndices.Add(first_vert + f.VertIndices[i]);
                            trimesh.TriangleIndices.Add(first_vert + f.VertIndices[i]);
                            trimesh.TriangleIndices.Add(first_vert + f.VertIndices[i - 1]);
                            trimesh.TriangleIndices.Add(first_vert + f.VertIndices[0]);
                             
                        }
                    }
                    first_vert += c.Vertices.Count;
                }

                return trimesh;
            }

            class VertexPosComparer : IEqualityComparer<Vertex>
            {
                public bool Equals(Vertex a, Vertex b)
                {
                    return a.Pos == b.Pos;
                }
                public int GetHashCode(Vertex a)
                {
                    return a.Pos.GetHashCode();
                }
            }
            class EdgeIdxComparer : IEqualityComparer<Edge>
            {
                public bool Equals(Edge a, Edge b)
                {
                    return a.VertIndices[0] == b.VertIndices[0] && a.VertIndices[1] == b.VertIndices[1];
                }
                public int GetHashCode(Edge e)
                {
                    return e.VertIndices[0].GetHashCode() ^ e.VertIndices[1].GetHashCode();
                }
            }

            /// <summary>
            /// Removes t junctions or overlapping edges by building a list of unique vertices and then
            /// detecting which vertices lie on each edge
            /// </summary>
            public void JoinEdges2d()
            {
                Point3D[] vertices = Convexes.SelectMany(a => a.Vertices).Distinct(new VertexPosComparer()).Select(a=>a.Pos).ToArray();
                foreach (Convex c in Convexes)
                {
                    List<Point3D> allnewverts = new List<Point3D>();
                    Face f = new Face(c);
                    foreach (Edge e in c.Edges)
                    {
                        Point3D p0 = c.GetEdgeVertPos(e, 0);
                        Point3D p1 = c.GetEdgeVertPos(e, 1);
                        Vector3D dir = p1 - p0; dir.Normalize();
                        Vector3D orth = new Vector3D(-dir.Y, dir.X, 0);
                        double t0 = Vector3D.DotProduct((Vector3D)p0, dir);
                        double t1 = Vector3D.DotProduct((Vector3D)p1, dir);
                        double s = Vector3D.DotProduct((Vector3D)p0, orth);
                        //allnewverts.Add(p0);
                        List<Point3D> edgenewverts = new List<Point3D>();
                        foreach (Point3D v in vertices)
                        {
                            double vs = Vector3D.DotProduct((Vector3D)v,orth);
                            if (vs == s)
                            {
                                double vt = Vector3D.DotProduct((Vector3D)v, dir);
                                if (vt >= t0 && vt < t1)
                                {
                                    edgenewverts.Add(v);
                                }
                            }
                        }
                        allnewverts.AddRange(edgenewverts.OrderBy(a => Vector3D.DotProduct((Vector3D)a, dir)));
                    }

                    c.Vertices = allnewverts.Select(a => new Vertex(c, a)).ToList();

                    f.VertIndices = new int[c.Vertices.Count];
                    for (int i = 0; i < f.VertIndices.Length; i++ )
                        f.VertIndices[i] = i;

                    c.Faces = new List<Face>();
                    c.Faces.Add(f);

                    c.BuildFromVertsAndFaces();
                }
            }

            //System.Windows.Point
            public void GetWeldedGeometry(out Vertex[] vertices, out Edge[] edges)
            {
                JoinEdges2d();

                Point3D[] points = Convexes.SelectMany(a => a.Vertices).Select(a => a.Pos).ToArray();
                int[] hashes = Convexes.SelectMany(a => a.Vertices).Select(a => a.GetHashCode()).ToArray();

                vertices = Convexes.SelectMany(a => a.Vertices).Distinct(new VertexPosComparer()).ToArray();

                Dictionary<Point3D, int> vert_idx_dict = new Dictionary<Point3D, int>();
                for (int i = 0; i < vertices.Length; i++)
                {
                    vert_idx_dict.Add(vertices[i].Pos, i);
                    //CSGScene.DebugLines.AddCross(vertices[i].Pos,0.25);
                }


                Dictionary<Edge, int> edge_dict = new Dictionary<Edge, int>(new EdgeIdxComparer());

                foreach (Convex c in Convexes)
                {
                    foreach (Edge e in c.Edges)
                    {
                        int idxa = vert_idx_dict[c.GetEdgeVertPos(e,0)];
                        int idxb = vert_idx_dict[c.GetEdgeVertPos(e,1)];
                        Edge newedge0 = new Edge(null, idxa, idxb);
                        Edge newedge1 = new Edge(null, idxb, idxa);
                        if (edge_dict.ContainsKey(newedge0))
                            edge_dict[newedge0]++;
                        else if (edge_dict.ContainsKey(newedge1))
                            edge_dict[newedge1]++;
                        else
                        {
                            //CSGScene.DebugLines.AddLine(c.GetEdgeVertPos(e, 0), c.GetEdgeVertPos(e, 1));
                            edge_dict.Add(newedge0, 1);
                        }
                    }
                }

                Edge[] unique_edges = edge_dict.Where(a => a.Value == 1).Select(a => a.Key).ToArray();

                for (int i = 0; i < unique_edges.Length; i++)
                {
                    Point3D p0 = vertices[unique_edges[i].VertIndices[0]].Pos;
                    Point3D p1 = vertices[unique_edges[i].VertIndices[1]].Pos;
                    //CSGScene.DebugLines.AddLine(p0,p0+(p1-p0)*1);
                }

                edges = unique_edges;

                foreach (Edge e in edges)
                {
                }

            }

            public void GetWireFrame(SafeScreenSpaceLines3D line)
            {
                foreach (Convex c in Convexes)
                {
                    foreach (Face f in c.Faces)
                    {
                       int vcnt = f.VertIndices.Length;
                        for (int i = 1; i < vcnt; i++)
                        {
                            line.Points.Add(c.Vertices[f.VertIndices[i-1]].Pos);
                            line.Points.Add(c.Vertices[f.VertIndices[i]].Pos);
                        }
                        line.Points.Add(c.Vertices[f.VertIndices[0]].Pos);
                        line.Points.Add(c.Vertices[f.VertIndices[vcnt-1]].Pos);
                    }
                }
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
                Convex cvx = new Convex();
                cvx.Is3d = false;

                cvx.Vertices.Add(new Vertex(cvx,-Size.X * 0.5, -Size.Y * 0.5));
                cvx.Vertices.Add(new Vertex(cvx,-Size.X * 0.5,  Size.Y * 0.5));
                cvx.Vertices.Add(new Vertex(cvx, Size.X * 0.5,  Size.Y * 0.5));
                cvx.Vertices.Add(new Vertex(cvx, Size.X * 0.5, -Size.Y * 0.5));

                cvx.Faces.Add(new Face(cvx,0, 1, 2, 3));

                cvx.BuildFromVertsAndFaces();

                Convexes.Add(cvx);
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
                    Convexes.AddRange(n.Convexes.Select(a=>a.Copy().ApplyTransform(m)));
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
                    Convexes.AddRange(n.Convexes.Select(a => a.Copy().ApplyTransform(m)));
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
                if (Is3d)
                {

                }
                else
                {
                    //this'll be the clever algorithm. it takes advantage of the fact that we always maintain:
                    //- for any given node, none of it's child convexes overlap
                    //therefore:
                    //- no node should test it's own convexes against each other
            



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
                            for (int j = i+1; j < Convexes.Count; j++)
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
                    }
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
                    Convexes.AddRange(n.Convexes.Select(a => a.Copy()));
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
                    Convexes.AddRange(n.Convexes.Select(a => a.Copy()));
                }
            }
        }

    }
}
