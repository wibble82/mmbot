﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using _3DTools;

namespace convexcad
{
    namespace Geometry
    {
        public class Vertex
        {
            public Point3D Pos = new Point3D();
            public List<int> VertIndices = new List<int>();
            public List<int> EdgeIndices = new List<int>();

            public Vertex(double x, double y)
            {
                Pos = new Point3D(x, y, 0);
            }
            public Vertex(double x, double y, double z)
            {
                Pos = new Point3D(x, y, z);
            }
            public Vertex(Point3D pos)
            {
                Pos = pos;
            }
            public Vertex(Vertex v)
            {
                Pos = v.Pos;
                VertIndices = new List<int>(v.VertIndices);
                EdgeIndices = new List<int>(v.EdgeIndices);
            }
        }
        public class Edge
        {
            public int[] VertIndices = new int[2];
            public int[] FaceIndices = new int[2];

            public Edge(int a, int b)
            {
                VertIndices[0] = a;
                VertIndices[1] = b;
            }
            public Edge(Edge e)
            {
                e.VertIndices.CopyTo(VertIndices,0);
                e.FaceIndices.CopyTo(FaceIndices,0);
            }
        }
        public class Face
        {
            public int[] VertIndices = null;
            public int[] EdgeIndices = null;

            public Face(params int[] verts)
            {
                VertIndices = verts;
            }
            public Face(Face e)
            {
                VertIndices = e.VertIndices.ToArray();
                EdgeIndices = e.EdgeIndices.ToArray();
            }
       }

        public class Convex
        {
            public bool Is3d = false;
            public List<Vertex> Vertices = new List<Vertex>();
            public List<Edge> Edges = new List<Edge>();
            public List<Face> Faces = new List<Face>();

            public Point3D GetVertPos(int idx) { return Vertices[idx].Pos; }

            public Vertex GetEdgeVert(int edge, int idx) { return Vertices[Edges[edge].VertIndices[idx]]; }
            public Point3D GetEdgeVertPos(int edge, int idx) { return GetEdgeVert(edge, idx).Pos; }

            public int GetFaceVertCount(int face) { return Faces[face].VertIndices.Length; }
            public Vertex GetFaceVert(int face, int idx) { return Vertices[Faces[face].VertIndices[idx]]; }
            public Point3D GetFaceVertPos(int face, int idx) { return GetFaceVert(face, idx).Pos; }

            public Point3D GetCentre()
            {
                Vector3D ctr = new Vector3D();
                foreach (Vertex v in Vertices)
                {
                    ctr.X += v.Pos.X;
                    ctr.Y += v.Pos.Y;
                    ctr.Z += v.Pos.Z;
                }
                ctr /= Vertices.Count;
                return new Point3D(ctr.X,ctr.Y,ctr.Z);
            }

            public void BuildFromVertsAndFaces()
            {
                Edges = new List<Edge>();

                if (Is3d)
                {

                }
                else
                {
                    if (Faces.Count != 1)
                        throw new System.ApplicationException("2d convex should only have 1 face");

                    Face f = Faces[0];
                    int vcnt = f.VertIndices.Length;
                    f.EdgeIndices = new int[vcnt];
                    for (int i = 0; i < vcnt; i++)
                    {
                        int v0 = f.VertIndices[i];
                        int v1 = f.VertIndices[(i + 1) % vcnt];

                        Edge e = new Edge(v0, v1);

                        f.EdgeIndices[i] = Edges.Count;

                        Edges.Add(e);
                    }
                }
            }

            public Convex Copy()
            {
                Convex cvx = new Convex();
                foreach (Vertex v in Vertices) cvx.Vertices.Add(new Vertex(v));
                foreach (Edge v in Edges) cvx.Edges.Add(new Edge(v));
                foreach (Face v in Faces) cvx.Faces.Add(new Face(v));
                return cvx;
            }

            public Convex ApplyTransform(Matrix3D m)
            {
                foreach (Vertex v in Vertices) v.Pos = m.Transform(v.Pos);
                return this;
            }

            public bool RayIntersect2d(Point3D raystart, Vector3D raydir, ref Point3D hitpos0, ref int hitedge0, ref Point3D hitpos1, ref int hitedge1 )
            {
                hitedge0 = hitedge1 = -1;
                int edgeidx = 0;
                double u,v;
                u = v = 0;
                for(; edgeidx < Edges.Count; edgeidx++)
                {
                    Point3D p0 = Vertices[Edges[edgeidx].VertIndices[0]].Pos;
                    Point3D p1 = Vertices[Edges[edgeidx].VertIndices[1]].Pos;
                    if(Math.IntersectRayLine2d(ref hitpos0,ref u, ref v, raystart, raydir, p0, p1))
                    {
                        hitedge0 = edgeidx;
                        break;
                    }
                }
                edgeidx++;
                for (; edgeidx < Edges.Count; edgeidx++)
                {
                    Point3D p0 = Vertices[Edges[edgeidx].VertIndices[0]].Pos;
                    Point3D p1 = Vertices[Edges[edgeidx].VertIndices[1]].Pos;
                    if (Math.IntersectRayLine2d(ref hitpos1, ref u, ref v, raystart, raydir, p0, p1))
                    {
                        hitedge1 = edgeidx;
                        break;
                    }
                }
                if ((hitedge0 < 0 && hitedge1 >= 0) || (hitedge0 >= 0 && hitedge1 < 0))
                    throw new System.ApplicationException("Ray only intersected 1 edge of polygon - something dodgy going on");
                return hitedge0 >= 0;
            }

            public static void Split2d(Convex convex, Point3D point0, int edge0, Point3D point1, int edge1, ref Convex newcvxa, ref Convex newcvxb)
            {
                newcvxa = new Convex();
                newcvxb = new Convex();

                Face facea = new Face();
                Face faceb = new Face();
                newcvxa.Faces.Add(facea);
                newcvxb.Faces.Add(faceb);

                int edgecount = convex.Edges.Count;

                List<int> usedindicesa = new List<int>();
                List<int> usedindicesb = new List<int>();


                newcvxa.Vertices.Add(new Vertex(point0));
                for (int i = edge0; i != edge1; i = (i + 1) % edgecount)
                {
                    usedindicesa.Add(convex.Edges[i].VertIndices[1]);
                    newcvxa.Vertices.Add(new Vertex(convex.GetEdgeVert(i, 1)));
                }
                newcvxa.Vertices.Add(new Vertex(point1));

                newcvxb.Vertices.Add(new Vertex(point1));
                for (int i = edge1; i != edge0; i = (i + 1) % edgecount)
                {
                    usedindicesb.Add(convex.Edges[i].VertIndices[1]);
                    newcvxb.Vertices.Add(new Vertex(convex.GetEdgeVert(i, 1)));
                }
                newcvxb.Vertices.Add(new Vertex(point0));

                facea.VertIndices = new int[newcvxa.Vertices.Count];
                faceb.VertIndices = new int[newcvxb.Vertices.Count];

                for (int i = 0; i < facea.VertIndices.Length; i++) 
                    facea.VertIndices[i] = i;
                for (int i = 0; i < faceb.VertIndices.Length; i++)
                    faceb.VertIndices[i] = i;

                newcvxa.BuildFromVertsAndFaces();
                newcvxb.BuildFromVertsAndFaces();
            }

            public static bool CalculateClippedConvexes2d(Convex a, Convex b, List<Convex> a_only, ref Convex overlap)
            {


                overlap = a.Copy();

                Point3D hp0, hp1;
                int hedge0, hedge1;
                hp0 = new Point3D();
                hp1 = new Point3D();
                hedge0 = hedge1 = -1;

                bool any_split = false;
                foreach (Edge bedge in b.Edges)
                {
                    Point3D raystart = b.Vertices[bedge.VertIndices[0]].Pos;
                    Vector3D raydir = b.Vertices[bedge.VertIndices[1]].Pos - raystart;
                    
                    if (overlap.RayIntersect2d(raystart, raydir, ref hp0, ref hedge0, ref hp1, ref hedge1))
                    {
                        //CSGScene.DebugLines.AddCross(hp0, 1);
                        //CSGScene.DebugLines.AddCross(hp1, 1);
                        //CSGScene.DebugLines.AddLine(hp0, hp1);
                        Convex newa, newb;
                        newa = newb = null;
                        Split2d(overlap, hp0, hedge0, hp1, hedge1, ref newa, ref newb);

                        Vector3D rayorth = new Vector3D(-raydir.Y, raydir.X, 0);
                        if (Vector3D.DotProduct(rayorth, newa.GetCentre() - raystart) < 0)
                        {
                            a_only.Add(newb);
                            overlap = newa;
                        }
                        else if (Vector3D.DotProduct(rayorth, newb.GetCentre() - raystart) < 0)
                        {
                            a_only.Add(newa);
                            overlap = newb;
                        }
                        else
                        {
                            throw new System.ApplicationException("Double degerate or something!");
                        }
                        any_split = true;
                    }
                }

                if (!any_split)
                {
                    a_only.Add(overlap);
                    overlap = null;
                }
                return any_split;
            }

            /*public MeshGeometry3D GetGeometry()
            {
                MeshGeometry3D trimesh = new MeshGeometry3D();

                foreach (Vertex v in Vertices)
                {
                    trimesh.Positions.Add(v.Pos);
                    trimesh.Normals.Add(new Vector3D(0, 1, 0));
                }
                foreach (Face f in Faces)
                {
                    int vcnt = f.VertIndices.Length;
                    for (int i = 2; i < vcnt; i++)
                    {
                        trimesh.TriangleIndices.Add(f.VertIndices[0]);
                        trimesh.TriangleIndices.Add(f.VertIndices[i]);
                        trimesh.TriangleIndices.Add(f.VertIndices[i - 1]);
                    }
                }

                return trimesh;
            }*/
        }
    }
}