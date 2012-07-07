using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace convexcad.Shapes
{
    public class Shape
    {
        public Mesh TriMesh;
        public Mesh PolyMesh;
        public List<Mesh> Convexes = new List<Mesh>();
        public List<Vertex> Vertices = new List<Vertex>();
        public bool Is2d;

        public Vertex CreateVertex()
        {
            Vertex v = new Vertex();
            v.OwnerShape = this;
            v.Idx = Vertices.Count;
            Vertices.Add(v);
            return v;
        }

        public Mesh CreateConvex()
        {
            Mesh m = new Mesh();
            m.Shape = this;
            Convexes.Add(m);
            return m;
        }

        public void BuildMeshGeometry3D(MeshGeometry3D trimesh)
        {
            Clean();
            Vector3D tinyoffset = new Vector3D(0, 0, 0);

            int first_vert = trimesh.Positions.Count;
            foreach (Vertex v in Vertices)
            {
                trimesh.Positions.Add(v.Pos + tinyoffset);
                trimesh.Normals.Add(new Vector3D(0, 1, 0));
            }

            foreach (Mesh c in Convexes)
            {
                tinyoffset.Z -= 0.0001;
                foreach (Face f in c.Faces)
                {
                    int centre_vert = trimesh.Positions.Count;
                    trimesh.Positions.Add(f.Centre);
                    trimesh.Normals.Add(new Vector3D(0,1,0));

                    foreach (Edge e in f.Edges)
                    {
                        trimesh.TriangleIndices.Add(centre_vert);
                        trimesh.TriangleIndices.Add(first_vert + e.Vertices[0].Idx);
                        trimesh.TriangleIndices.Add(first_vert + e.Vertices[1].Idx);
                    }
                }
            }       
        }

        public void BuildScreenSpaceLines(SafeScreenSpaceLines3D lines)
        {
            Clean();
            foreach (Edge e in Convexes.SelectMany(a => a.Edges))
            {
                lines.AddLine(e.Vertices[0].Pos, e.Vertices[1].Pos);
            }
        }

        public void BuildCrossesAtVertices(SafeScreenSpaceLines3D lines)
        {
            Clean();
            foreach (Edge e in Convexes.SelectMany(a => a.Edges))
            {
                lines.AddCross(e.Vertices[0].Pos, 0.5);
                lines.AddCross(e.Vertices[1].Pos, 0.5);
            }
        }

        public Shape Copy()
        {
            //create a new shape and copy the vertices
            Shape shape = new Shape();
            foreach (Vertex v in Vertices)
            {
                Vertex copy = shape.CreateVertex();
                copy.Pos = v.Pos;
            }

            //put new vertices in a quick look up table
            Vertex[] newverts = shape.Vertices.ToArray();

            //go over each convex
            foreach (Mesh c in Convexes)
            {
                //create a new convex in the new shape
                Mesh convexcopy = shape.CreateConvex();

                //copy edges
                foreach (Edge e in c.Edges)
                {
                    //create the edge and use look up table to build new vertex list
                    Edge edgecopy = convexcopy.CreateEdge();
                    edgecopy.Vertices = e.Vertices.Select(a => newverts[a.Idx]).ToArray();

                    //add this edge to each of the vertices it uses
                    foreach (Vertex v in edgecopy.Vertices) 
                        v.OwnerEdges.Add(edgecopy);
                }

                //build look up table for the edges
                Edge[] newedges = convexcopy.Edges.ToArray();

                //copy faces
                foreach (Face f in c.Faces)
                {
                    //create the face and use look up tables to build new vertex/edge list
                    Face facecopy = convexcopy.CreateFace();
                    facecopy.Vertices = f.Vertices.Select(a => newverts[a.Idx]).ToList();
                    facecopy.Edges = f.Edges.Select(a => newedges[a.Idx]).ToList();

                    //add this face to each of the vertices it uses
                    foreach (Vertex v in facecopy.Vertices)
                        v.OwnerFaces.Add(facecopy);

                    //add this face to each of the edges it uses
                    foreach (Edge e in facecopy.Edges)
                        e.OwnerFaces.Add(facecopy);

                }
            }

            return shape;
        }

        public Shape ApplyTransform(Matrix3D m)
        {
            foreach (Vertex v in Vertices) 
                v.Pos = m.Transform(v.Pos);
            return this;
        }

        public Shape Split2d(Point3D raystart, Vector3D raydir, Mesh.ESplitMode sm)
        {
            Mesh[] convexes = Convexes.ToArray();
            foreach (Mesh m in convexes)
            {
                Mesh newmesh = null;
                if (sm != Mesh.ESplitMode.KEEP_BOTH)
                    newmesh = CreateConvex();
                m.Split2d(raystart, raydir, sm, newmesh);
            }
            Convexes = convexes.ToList();
            return this;
        }

        public bool ContainsMesh(Mesh m)
        {
            if (Convexes != null && Convexes.Contains(m))
                return true;
            if (PolyMesh == m)
                return true;
            if (TriMesh == m)
                return true;
            return false;
        }

        public void Clean()
        {
            Vertices = Vertices.Where(a => a.OwnerFaces.Count > 0).ToList();
            for(int i = 0; i < Vertices.Count; i++)
                Vertices[i].Idx = i;
            foreach(Mesh m in Convexes)
            {
                m.Edges = m.Edges.Where(a => a.OwnerFaces.Count > 0).ToList();
                for(int i = 0; i < m.Edges.Count; i++)
                    m.Edges[i].Idx = i;
            }
        }

        public void CheckIntegrity()
        {
            StringBuilder builder = new StringBuilder(); 
            foreach (Vertex v in Vertices)
            {
                if (v.OwnerShape != this)
                    builder.Append(String.Format("Vertex {0} not in shape", v));

            }

            foreach (Mesh c in Convexes)
            {
                if(c.Shape != this)
                    builder.Append(String.Format("Convex {0} not in shape", c));

                foreach (Edge e in c.Edges)
                {
                    foreach (Vertex v in e.Vertices)
                    {

                    }
                }

                foreach (Face f in c.Faces)
                {

                }
            }
        }



    }
}
