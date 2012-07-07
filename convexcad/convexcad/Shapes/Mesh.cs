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
        public bool IsConvex;

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

        public enum ESplitMode
        {
            KEEP_INSIDE,
            KEEP_OUTSIDE,
            KEEP_BOTH
        }

        public Mesh Split2d(Point3D raystart, Vector3D raydir, ESplitMode split_mode, ref Mesh target_mesh)
        {
            Face[] tosplit = Faces.ToArray();
            foreach (Face f in tosplit)
            {
                Face inside_face,outside_face;
                f.SplitByRay(raystart, raydir, out inside_face, out outside_face);
                if (split_mode == ESplitMode.KEEP_INSIDE)
                {
                    if(outside_face != null)
                        TransferFaceTo(outside_face, target_mesh);
                }
                else if (split_mode == ESplitMode.KEEP_OUTSIDE)
                {
                    if(inside_face != null)
                        TransferFaceTo(inside_face, target_mesh);
                }
            }
            return this;
        }

        public void Clip2d(Mesh clip_mesh, List<Mesh> outside)
        {
            Mesh new_mesh = Shape.CreateConvex();
            foreach (Edge clip_edge in clip_mesh.Edges)
            {
                Point3D raystart = clip_edge.Vertices[0].Pos;
                Vector3D raydir = clip_edge.Direction;
                Split2d(raystart, raydir, ESplitMode.KEEP_INSIDE, new_mesh);
                if (new_mesh.Faces.Count != 0)
                {
                }
            }
        }

        public void TransferFaceTo(Face f, Mesh mesh)
        {
            if (mesh.Shape != Shape)
                throw new System.ApplicationException("Can not transfer faces between shapes");

            //transfer the face
            Faces.Remove(f);
            mesh.Faces.Add(f);
            f.OwnerMesh = mesh;

            //go over each edge in the face and copy it over
            for(int edgeidx = 0; edgeidx < f.Edges.Count; edgeidx++)
            {
                //get the edge and remove the face from it
                Edge e = f.Edges[edgeidx];
                e.OwnerFaces.Remove(f);

                //create a new edge and replace it
                Edge newedge = mesh.CreateEdge();
                newedge.OwnerFaces.Add(f);
                f.Edges[edgeidx] = newedge;

                //set the 2 vertex references
                newedge.SetVertex(0, e.Vertices[0]);
                newedge.SetVertex(1, e.Vertices[1]);
            }
        }
    }
}
