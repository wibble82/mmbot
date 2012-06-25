using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace convexcad.Shapes
{

    public class Edge
    {

        public Mesh OwnerMesh;
        public List<Face> OwnerFaces = new List<Face>();
        public Vertex[] Vertices = new Vertex[2];
        public int Idx = -1;

        public Edge()
        {
        }

        public override string ToString()
        {
            return String.Format("[{0},{1}] [{2},{3}])",Vertices[0].Idx,Vertices[1].Idx,Vertices[0].Pos,Vertices[1].Pos);
        }

        public Vector3D Offset { get { return Vertices[1].Pos - Vertices[0].Pos; } }
        public Vector3D Direction { get { return MathUtils.Normalize(Offset); } }
        public Vector3D TangentXY { get { return MathUtils.TangentXY(Direction); } }
        public Vector3D TangentYZ { get { return MathUtils.TangentYZ(Direction); } }
        public Vector3D TangentZX { get { return MathUtils.TangentZX(Direction); } }
        public Point3D Centre { get { return MathUtils.Centre(Vertices.Select(a=>a.Pos)); } }
        public Point3D Min { get { return MathUtils.Min(Vertices.Select(a => a.Pos)); } }
        public Point3D Max { get { return MathUtils.Max(Vertices.Select(a => a.Pos)); } }


        /*public Point3D AABBMin { get { return Vertices.}*/

        public void RemoveVertex(int idx)
        {
            Vertices[idx].OwnerEdges.Remove(this);
            Vertices[idx] = null;
        }

        public void SetVertex(int idx, Vertex v)
        {
            if (Vertices[idx] != v)
            {
                if (Vertices[idx] != null)
                {
                    Vertices[idx].OwnerEdges.Remove(this);
                    Vertices[idx] = null;
                }
                if (v != null)
                {
                    Vertices[idx] = v;
                    Vertices[idx].OwnerEdges.Add(this);
                }
            }
        }
        
        public Edge Split(double split_param)
        {
            //calculate position of new vertex
            Point3D p0 = Vertices[0].Pos;
            Point3D p1 = Vertices[1].Pos;
            Point3D splitp = p0 + (p1 - p0) * split_param;

            //create the new vertex and set it up
            Vertex first_vertex = Vertices[0];
            Vertex last_vertex = Vertices[1];
            Vertex new_vertex = OwnerMesh.CreateVertex();
            new_vertex.Pos = splitp;                        //it's at the split point

            //create new edge
            Edge new_edge = OwnerMesh.CreateEdge();

            //link new vert/edge into owning faces
            foreach (Face f in OwnerFaces)
            {
                f.AddVertex(new_vertex);
                f.InsertEdge(new_edge, this);
            }

            //fix up the new edges
            SetVertex(0, first_vertex);
            SetVertex(1, new_vertex);
            new_edge.SetVertex(0, new_vertex);
            new_edge.SetVertex(1, last_vertex);

            return new_edge;
        }

        public MathUtils.RayLineResult ClosestPointRay(out double closestdist, out double closestu, out double closestv, Point3D raystart, Vector3D raydir)
        {
            return MathUtils.ClosestPointRayLine(out closestdist, out closestu, out closestv, raystart, raydir, Vertices[0].Pos, Vertices[1].Pos);
        }
    }
}
