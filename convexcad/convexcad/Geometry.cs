using System;
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
                Pos = new Point3D(x, 0, y);
            }
            public Vertex(double x, double y, double z)
            {
                Pos = new Point3D(x, y, z);
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
                VertIndices.CopyTo(e.VertIndices,0);
                FaceIndices.CopyTo(e.FaceIndices,0);
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

            public bool RayIntersect2d(Point3D raypoint, Vector3D raydir, out Point3D hitpos0, out int hitedge0, out Point3D hitpos1, out int hitedge1 )
            {
                hitpos0 = hitpos1 = new Point3D();
                hitedge0 = hitedge1 = -1;
                return false;
            }

            public static void CalculateClippedConvexes2d(Convex a, Convex b, out List<Convex> a_only, out Convex overlap)
            {
                a_only = new List<Convex>();
                overlap = a.Copy();

                foreach(Edge bedge in b.Edges)
                {
                    Point3D raypos = b.Vertices[bedge.VertIndices[0]].Pos;
                    Vector3D raydir = b.Vertices[bedge.VertIndices[1]].Pos - raypos;
                    raydir.Normalize();
                    Point3D hp0, hp1;
                    int hi0, hi1;
                    if (a.RayIntersect2d(raypos, raydir, out hp0, out hi0, out hp1, out hi1))
                    {
                        
                    }
                }

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
