using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace convexcad.Shapes
{
    public class Mesh
    {
        public Shape Shape;
        public List<Edge> Edges = new List<Edge>();
        public List<Face> Faces = new List<Face>();

        //public Point3D Centre { get { return MathUtils.Centre(Vertices.Select(a => a.Pos)); } }
        //public Point3D Min { get { return MathUtils.Min(Vertices.Select(a => a.Pos)); } }
        //public Point3D Max { get { return MathUtils.Max(Vertices.Select(a => a.Pos)); } }

        public Vertex CreateVertex()
        {
            Vertex v = Shape.CreateVertex();
            return v;
        }
        public Vertex[] CreateVertices(int count)
        {
            Vertex[] buff = new Vertex[count];
            for(int i = 0; i < count; i++)
                buff[i] = CreateVertex();
            return buff;
        }

        public Edge CreateEdge()
        {
            Edge e = new Edge();
            e.Idx = Edges.Count;
            Edges.Add(e);
            e.OwnerMesh = this;
            return e;
        }
        public Edge[] CreateEdges(int count)
        {
            Edge[] buff = new Edge[count];
            for (int i = 0; i < count; i++)
                buff[i] = CreateEdge();
            return buff;
        }

        public Face CreateFace()
        {
            Face f = new Face();
            f.Idx = Faces.Count;
            Faces.Add(f);
            f.OwnerMesh = this;
            return f;
        }
        public Face[] CreateFaces(int count)
        {
            Face[] buff = new Face[count];
            for (int i = 0; i < count; i++)
                buff[i] = CreateFace();
            return buff;
        }

        public Face CreateFace(int vertex_count)
        {
            Face f = CreateFace();
            Vertex[] verts; Edge[] edges;
            f.CreateLinkedEdgeAndVertexRing(vertex_count, out verts, out edges);
            return f;
        }
        public Mesh SplitFacesByRay(Point3D raystart, Vector3D raydir)
        {
            Face[] tosplit = Faces.ToArray();
            foreach (Face f in tosplit)
            {
                Face inside_face,outside_face;
                f.SplitByRay(raystart, raydir, out inside_face, out outside_face);
            }
            return this;
        }
    }
}
