﻿using System;
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

                Create();
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
                foreach (Convex c in Convexes)
                {
                    foreach (Vertex v in c.Vertices)
                    {
                        trimesh.Positions.Add(v.Pos);
                        trimesh.Normals.Add(new Vector3D(0, 1, 0));
                    }
                    foreach (Face f in c.Faces)
                    {
                        int vcnt = f.VertIndices.Length;
                        for (int i = 2; i < vcnt; i++)
                        {
                            trimesh.TriangleIndices.Add(first_vert+f.VertIndices[0]);
                            trimesh.TriangleIndices.Add(first_vert + f.VertIndices[i - 1]);
                            trimesh.TriangleIndices.Add(first_vert + f.VertIndices[i]);
                            
                        }
                    }
                    first_vert += c.Vertices.Count;
                }

                return trimesh;
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
                        line.Thickness = 2;
                        line.Color = Colors.Black;
                    }
                }
            }
        }

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

                cvx.Vertices.Add(new Vertex(-Size.X * 0.5, -Size.Y * 0.5));
                cvx.Vertices.Add(new Vertex(-Size.X * 0.5,  Size.Y * 0.5));
                cvx.Vertices.Add(new Vertex( Size.X * 0.5,  Size.Y * 0.5));
                cvx.Vertices.Add(new Vertex( Size.X * 0.5, -Size.Y * 0.5));

                cvx.Faces.Add(new Face(0, 1, 2, 3));

                cvx.BuildFromVertsAndFaces();

                Convexes.Add(cvx);
            }
        }

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
                    foreach (Node n in Children)
                        foreach (Convex c in n.Convexes)
                            for (int i = 0; i < c.Edges.Count; i++)
                                CSGScene.DebugLines.AddLine(c.GetEdgeVertPos(i, 0), c.GetEdgeVertPos(i, 1));

                    //copy all convexes from first node
                    Convexes.AddRange(Children[0].Convexes.Select(a => a.Copy()));

                    //now progressively union extra nodes in
                    for (int i = 1; i < Children.Count; i++)
                    {
                        //go through each current convex
                        //and each convex in current child
                        foreach (Convex otherconvex in Children[i].Convexes)
                        {
                            //start with the other child's convex (this will be a list of none overlapping convexes)
                            List<Convex> newlist = new List<Convex>();
                            newlist.Add(otherconvex);

                            //now iterate over all the current convexes (note: we know these do not overlap)
                            for (int cidx = 0; cidx < Convexes.Count; cidx++)
                            {
                                //
                                List<Convex> otherconvexsplit = new List<Convex>();
                                foreach (Convex newcvx in newlist)
                                {
                                    Convex overlap = null;
                                    Convex.CalculateClippedConvexes2d(newcvx, Convexes[cidx], otherconvexsplit, ref overlap);                                    
                                }
                                newlist = otherconvexsplit;
                                 
                            }
                            Convexes.AddRange(newlist);
                        }
                    }
                }
//                 foreach (Node n in Children)
//                 {
//                     Convexes.AddRange(n.Convexes.Select(a => a.Copy()));
//                 }
            }
        }

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