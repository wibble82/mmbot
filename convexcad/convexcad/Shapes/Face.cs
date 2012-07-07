using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace convexcad.Shapes
{
    public class Face
    {
        public Mesh OwnerMesh;
        public List<Vertex> Vertices = new List<Vertex>();
        public List<Edge> Edges = new List<Edge>();

        public Point3D Centre { get { return MathUtils.Centre(Vertices.Select(a => a.Pos)); } }
        public Point3D Min { get { return MathUtils.Min(Vertices.Select(a => a.Pos)); } }
        public Point3D Max { get { return MathUtils.Max(Vertices.Select(a => a.Pos)); } }

        public void AddVertex(Vertex v)
        {
            v.OwnerFaces.Add(this);
            Vertices.Add(v);
        }
        public void RemoveVertex(Vertex v)
        {
            v.OwnerFaces.Remove(this);
            Vertices.Remove(v);
        }
        public Vertex CreateVertex()
        {
            Vertex v = OwnerMesh.CreateVertex();
            AddVertex(v);
            return v;
        }
        public Vertex[] CreateVertices(int count)
        {
            Vertex[] buff = new Vertex[count];
            for (int i = 0; i < count; i++)
                buff[i] = CreateVertex();
            return buff;
        }

        public Face AddEdge(Edge e)
        {
            e.OwnerFaces.Add(this);
            Edges.Add(e);
            return this;
        }
        public Face RemoveEdge(Edge e)
        {
            e.OwnerFaces.Remove(this);
            Edges.Remove(e);
            return this;
        }
        public Edge CreateEdge()
        {
            Edge e = OwnerMesh.CreateEdge();
            AddEdge(e);
            return e;
        }
        public Edge CreateJoiningEdge()
        {
            //make sure shape needs joining before doing so
            if (Edges.Last().Vertices[1] == Edges.First().Vertices[0])
                throw new System.ApplicationException("Face is already closed, so can't add a joining edge");

            //create an edge, initially only linked to the main mesh
            Edge e = OwnerMesh.CreateEdge();

            //link the new edge to the last and first vertices to join them together
            e.SetVertex(0,Edges.Last().Vertices[1]);
            e.SetVertex(1,Edges.First().Vertices[0]);

            //append and link the new edge
            AddEdge(e);
            return e;
        }
        public Edge[] CreateEdges(int count)
        {
            Edge[] buff = new Edge[count];
            for (int i = 0; i < count; i++)
                buff[i] = CreateEdge();
            return buff;
        }
        public Face InsertEdge(Edge new_edge, int idx)
        {
            new_edge.OwnerFaces.Add(this);
            Edges.Insert(idx + 1, new_edge);
            return this;
        }

        public Face InsertEdge(Edge new_edge, Edge prev_edge)
        {
            int idx = Edges.IndexOf(prev_edge);
            InsertEdge(new_edge, idx);
            return this;
        }

        /// <summary>
        /// Creates N vertices and N edges, joined in a ring
        /// </summary>
        /// <param name="num_centre_verts"></param>
        public void CreateLinkedEdgeAndVertexRing(int num_centre_verts, out Vertex[] vertices, out Edge[] edges)
        {
            edges = CreateEdges(num_centre_verts);
            vertices = CreateVertices(num_centre_verts);

            for (int i = 0; i < (vertices.Length-1); i++)
            {
                edges[i].SetVertex(0, vertices[i]);
                edges[i].SetVertex(1, vertices[i + 1]);
            }

            edges.Last().SetVertex(0, vertices.Last());
            edges.Last().SetVertex(1, vertices.First());
        }

        public Face Split(Edge edge0, Edge edge1)
        {
            //get indices
            int idx0 = Edges.IndexOf(edge0);
            int idx1 = Edges.IndexOf(edge1);

            //verify indices
            if (idx0 == idx1)
                throw new System.ApplicationException("Can not split face between 2 vertices");
            if (((idx0 + 1) % Edges.Count) == idx1)
                throw new System.ApplicationException("Edge 1 is next edge after edge 0 - edges must be at least 1 vertex apart");
            if (((idx1 + 1) % Edges.Count) == idx0)
                throw new System.ApplicationException("Edge 0 is next edge after edge 1 - edges must be at least 1 vertex apart");

            //we have a list of edges in the face, and are transferring those between edge0 and edge1 to new face
            //so if keeping 'a' on this face, and transfer 'b' to new face
            //will be in one of these 2 orders, depending on which comes first - edge0 or edge1
            //b-b-b-e1-a-a-a-e0-b-b-b
            //a-a-a-e0-b-b-b-e1-a-a-a

            //so, check indices to work out if edge0 comes first or edge1 comes first
            List<Edge> my_edges, other_edges;
            if (idx0 < idx1)
            {
                //edge0 is first, so have:
                //a-a-a-e0-b-b-b-e1-a-a-a
                //after splitting it should be:
                //a-a-a-e0(b)-b-b-b-e1(a)-a-a-a
                my_edges = Edges.Skip(idx0).Take(idx1 - idx0).ToList();
                other_edges = Edges.Skip(idx1).Concat(Edges.Take(idx0)).ToList();
            }
            else
            {
                //edge1 is first, so have:
                //b-b-b-e1-a-a-a-e0-b-b-b
                //after splitting it should be:
                //b-b-b-e1(a)-a-a-a-e0(b)-b-b-b
                other_edges = Edges.Skip(idx1).Take(idx0 - idx1).ToList();
                my_edges = Edges.Skip(idx0).Concat(Edges.Take(idx1)).ToList();
            }

            //store new list of edges (effectively linking face->edge for all edges we keep)
            Edges = my_edges;

            //creatre new face
            Face new_face = OwnerMesh.CreateFace();

            //transfer all other edges from this face to new face
            foreach (Edge e in other_edges)
            {
                e.OwnerFaces.Remove(this);          //unlink edge->face (face->edge was automatically unlinked when changed edge list)
                new_face.AddEdge(e);                //add edge to new face (adds it to list, linking edge->face and face->edge)
                RemoveVertex(e.Vertices[0]);        //unlink vertex0<->face 
                new_face.AddVertex(e.Vertices[0]);  //link vertex0<->newface 
            }

            //add last vertex in other face tot the other face
            new_face.AddVertex(other_edges.Last().Vertices[1]);

            //re-add just the first vert in the other face, which will have been incorrectly removed from this one
            AddVertex(other_edges.First().Vertices[0]);

            //edge transfer should have left a gap in the edge list for current and new face, so create new edges to fill gap
            CreateJoiningEdge();                    //create edge from end to start (sets up face->edge and edge->face)
            new_face.CreateJoiningEdge();           //other create edge from end to start (sets up face->edge and edge->face)

            return new_face;
        }

        public Face Split(Edge edge0, Edge edge1, double edgeparam1)
        {
            Edge new_edge = edge1.Split(edgeparam1);
            return Split(edge0, new_edge);
        }

        public Face Split(Edge edge0, double edgeparam0, Edge edge1)
        {
            Edge new_edge = edge0.Split(edgeparam0);
            return Split(new_edge, edge1);
        }

        public Face Split(Edge edge0, double edgeparam0, Edge edge1, double edgeparam1)
        {
            Edge new_edge0 = edge0.Split(edgeparam0);
            Edge new_edge1 = edge1.Split(edgeparam1);
            return Split(new_edge0,new_edge1);
        }

        public Edge NextEdge(int edgeidx)
        {
            return Edges[(edgeidx + 1) % Edges.Count];
        }
        public Edge PrevEdge(int edgeidx)
        {
            return Edges[(edgeidx + Edges.Count - 1) % Edges.Count];
        }
        public Edge NextEdge(Edge e)
        {
            return NextEdge(Edges.IndexOf(e));
        }
        public Edge PrevEdge(Edge e)
        {
            return PrevEdge(Edges.IndexOf(e));
        }

        void GetRaySplitParams(Point3D raystart, Vector3D raydir, out Edge edge0, out double param0, out MathUtils.RayLineResult res0, out Edge edge1, out double param1, out MathUtils.RayLineResult res1)
        {
            edge0 = null;
            edge1 = null;
            param0 = 0;
            param1 = 0;
            res0 = MathUtils.RayLineResult.UNKOWN;
            res1 = MathUtils.RayLineResult.UNKOWN;

            //find first intersecting edge
            int i = 0;
            for (; i < Edges.Count; i++)
            {
                double closestdist, closestu;
                MathUtils.RayLineResult res = Edges[i].ClosestPointRay(out closestdist, out closestu, out param0, raystart, raydir);
                edge0 = Edges[i];

                //valid intersection if we touch the line, the first vertex or fully overlap the edge
                if (res == MathUtils.RayLineResult.INTERSECTING_LINE || res == MathUtils.RayLineResult.INTERSECTING_POS0 || res == MathUtils.RayLineResult.PARALLEL_OVERLAPPING)
                {
                    res0 = res;
                    i++;
                    break;
                }
            }

            //find second intersecting edge
            for (; i < Edges.Count; i++)
            {
                double closestdist, closestu;
                MathUtils.RayLineResult res = Edges[i].ClosestPointRay(out closestdist, out closestu, out param1, raystart, raydir);
                edge1 = Edges[i];

                //valid intersection if we touch the line, the first vertex or fully overlap the edge
                if (res == MathUtils.RayLineResult.INTERSECTING_LINE || res == MathUtils.RayLineResult.INTERSECTING_POS0 || res == MathUtils.RayLineResult.PARALLEL_OVERLAPPING)
                {
                    res1 = res;
                    i++;
                    break;
                }
            }
        }

        public void SplitByRay(Point3D raystart, Vector3D raydir, out Face inside_face, out Face outside_face)
        {
            //default to just returning this as inside and outside
            inside_face = this;
            outside_face = null;
            raydir.Normalize();

            if (!Scene.NextStage("SplitByRay"))
                return;
            else if (Scene.IsCurrentStage())
                Scene.AddDebugLine(raystart-raydir*10,raystart+raydir*10);

            //get the edges, feature info and params that describe how the ray intersects the face
            Edge edge0, edge1;
            double param0, param1;
            MathUtils.RayLineResult res0, res1;
            GetRaySplitParams(raystart, raydir, out edge0, out param0, out res0, out edge1, out param1, out res1);

            //debug draw the results
            if (Scene.IsCurrentStage())
            {
                if(res0 == MathUtils.RayLineResult.INTERSECTING_LINE)
                    Scene.AddDebugLine(edge0.Vertices[0].Pos,edge0.Vertices[1].Pos);
                else if(res0 == MathUtils.RayLineResult.INTERSECTING_POS0)
                    Scene.AddDebugCross(edge0.Vertices[0].Pos, 0.5);
                else if (res0 == MathUtils.RayLineResult.PARALLEL_OVERLAPPING)
                {
                    Scene.AddDebugCross(edge0.Vertices[0].Pos, 0.5);
                    Scene.AddDebugCross(edge0.Vertices[1].Pos, 0.5);
                }
                if (res1 == MathUtils.RayLineResult.INTERSECTING_LINE)
                    Scene.AddDebugLine(edge1.Vertices[0].Pos, edge1.Vertices[1].Pos);
                else if (res1 == MathUtils.RayLineResult.INTERSECTING_POS0)
                    Scene.AddDebugCross(edge1.Vertices[0].Pos, 0.5);
                else if (res1 == MathUtils.RayLineResult.PARALLEL_OVERLAPPING)
                {
                    Scene.AddDebugCross(edge1.Vertices[1].Pos, 0.5);
                    Scene.AddDebugCross(edge1.Vertices[1].Pos, 0.5);
                }
            }

            //check if centre is initially inside ray
            Vector3D centre_offset = Centre - raystart;
            double centre_cp = MathUtils.CrossXY(centre_offset, raydir);

            //valid results for closed, convex, clockwise polygon are:
            //  res0 == no intersection                         res1 == no intersection
            //when res0 is parallel:
            //  res0 == parallel overlapping                    res1 == intersecting pos0 of next edge (this is treated as no intersection as there is no split required)
            //  res0 == parallel overlapping                    res1 == parallel overlapping next edge IF next edge is colinear
            //when res0 is vertex cases
            //  res0 == intersecting pos0 of first edge         res1 == parallel overlapping last edge (note: in any other scenario res0 would detect the parallel line, and res1 will be the vertex)
            //  res0 == intersecting pos0 of any edge           res1 == no intersection (i.e. we just touched 1 vertex)
            //  res0 == intersecting pos0 of any edge           res1 == intersecting pos0 of any none-neighour edge
            //  res0 == intersecting pos0 of any edge           res1 == intersecting line of any edge other than previous neighbour
            //when res0 is line cases
            //  res0 == intersecting line of any edge           res1 == intersecting pos0 of any edge other than next neighbour
            //  res0 == intersecting line of any edge           res1 == intersecting line of any edge

            //check intersection results for each valid combination of res0 and res1
            if (res0 == MathUtils.RayLineResult.PARALLEL_OVERLAPPING)
            {
                //first edge is both parallel and overlapping the ray, so just return this face as left or right             
                if (edge1 != NextEdge(edge0))
                    throw new System.ApplicationException("If res0 is parallel, expected res1 to be the next edge");
                if (res1 == MathUtils.RayLineResult.PARALLEL_OVERLAPPING)
                {
                    if(Math.Abs(1-Vector3D.DotProduct(edge0.Direction,edge1.Direction)) > MathUtils.EPSILON)
                        throw new System.ApplicationException("If res0 is parallel and res1 is parallel, res1 must be colinear");
                    //pick side and return
                    if (centre_cp <= 0)
                        inside_face = this;
                }
                else if (res1 == MathUtils.RayLineResult.INTERSECTING_POS0)
                {
                    //pick side and return
                    if (centre_cp <= 0)
                        inside_face = this;
                }
                else
                {
                    throw new System.ApplicationException("If res0 is parallel, expected res1 must be overlapping (if colinear) or pos0");
                }
            }
            else if (res0 == MathUtils.RayLineResult.INTERSECTING_POS0)
            {
                //first edge overlaps its starting vertex
                if (res1 == MathUtils.RayLineResult.UNKOWN)
                { 
                    //no intersection (we just clipped the first vertex of edge0) - still pick side and return?
                    if (centre_cp <= 0)
                        inside_face = this;
                }
                else if(res1 == MathUtils.RayLineResult.INTERSECTING_POS0)
                {
                    if(edge1 == NextEdge(edge0) || edge1 == PrevEdge(edge0))
                        throw new System.ApplicationException("If res0 is pos0 and res1 is pos0, edge1 must not be neighbour of edge0");
                    
                    //got intersection - need to do vertex-vertex split
                    Face newface = Split(edge0, edge1);
                    Vector3D new_centre_offset = Centre - raystart;
                    double new_centre_cp = MathUtils.CrossXY(new_centre_offset, raydir);
                    if (new_centre_cp <= 0)
                    {
                        inside_face = this;
                        outside_face = newface;
                    }
                    else
                    {
                        outside_face = this;
                        inside_face = newface;
                    }
                }
                else if (res1 == MathUtils.RayLineResult.INTERSECTING_LINE)
                {
                    if (edge1 == PrevEdge(edge0))
                        throw new System.ApplicationException("If res0 is pos0 and res1 is line, edge1 must not be neighbour of edge0");

                    //got intersection - need to do vertex-edge split
                    Face newface = Split(edge0, edge1, param1);
                    Vector3D new_centre_offset = Centre - raystart;
                    double new_centre_cp = MathUtils.CrossXY(new_centre_offset, raydir);
                    if (new_centre_cp <= 0)
                    {
                        inside_face = this;
                        outside_face = newface;
                    }
                    else
                    {
                        outside_face = this;
                        inside_face = newface;
                    }
                }
                else if (res1 == MathUtils.RayLineResult.PARALLEL_OVERLAPPING)
                {
                    if (edge1 != Edges.Last())
                        throw new System.ApplicationException("If res1 is parallel, expected it to be the last edge");
                    if (edge0 != Edges.First())
                        throw new System.ApplicationException("If res1 is parallel, expected res0 to be the first edge");

                    //pick side and return
                    if (centre_cp <= 0)
                        inside_face = this;
                }
                else
                {
                    throw new System.ApplicationException("If res0 is pos0, res1 must be none, pos0 or line");
                }

            }
            else if (res0 == MathUtils.RayLineResult.INTERSECTING_LINE)
            {
                //first edge is overlaps the line
                if (res1 == MathUtils.RayLineResult.INTERSECTING_POS0)
                {
                    if (edge1 == NextEdge(edge0))
                        throw new System.ApplicationException("If res0 is line, edge1 must not be next neighbour of edge0");

                    //got intersection - need to do edge-vertex split
                    Face newface = Split(edge0, param0, edge1);
                    Vector3D new_centre_offset = Centre - raystart;
                    double new_centre_cp = MathUtils.CrossXY(new_centre_offset, raydir);
                    if (new_centre_cp <= 0)
                    {
                        inside_face = this;
                        outside_face = newface;
                    }
                    else
                    {
                        outside_face = this;
                        inside_face = newface;
                    }

                }
                else if (res1 == MathUtils.RayLineResult.INTERSECTING_LINE)
                {
                    //got intersection - need to do edge-edge split
                    Face newface = Split(edge0, param0, edge1, param1);
                    Vector3D new_centre_offset = Centre - raystart;
                    double new_centre_cp = MathUtils.CrossXY(new_centre_offset, raydir);
                    if (new_centre_cp <= 0)
                    {
                        inside_face = this;
                        outside_face = newface;
                    }
                    else
                    {
                        outside_face = this;
                        inside_face = newface;
                    }
                }
                else
                {
                    throw new System.ApplicationException("If res0 is line, res1 must be pos or line");
                }
            }
            else
            {
                if (res1 != MathUtils.RayLineResult.UNKOWN)
                    throw new System.ApplicationException("If res0 is no intersection, res1 should also be no intersection");

                //no intersection at all - still pick side and return?
                if (centre_cp <= 0)
                    inside_face = this;
            }
        }

        public void Clip2d(Face face_a, Face face_b, out List<Face> a_only, out Face overlap)
        {
            a_only = new List<Face>();
            overlap = null;
        }

        public void IntegrityCheck()
        {
            List<String> messages = new List<String>();
            if (!IntegrityCheck(messages))
            {
                StringBuilder builder = new StringBuilder(messages.Sum(a => a.Length+16));
                messages.AddRange(messages);
                throw new System.ApplicationException(builder + "\n");
            }
        }

        public bool IntegrityCheck(List<String> messages)
        {
            int ecount = Edges.Count;
            bool success = true;

            Point3D centre_pos = Centre;
            
            for (int i = 0; i < Edges.Count; i++)
            {
                if (Edges[i].Vertices[0] != Edges[(i + ecount - 1) % ecount].Vertices[1])
                {
                    messages.Add("Face check: Edges not linked together");
                    success = false;
                }
                if (Edges[i].Vertices[1] != Edges[(i + 1) % ecount].Vertices[0])
                {
                    messages.Add("Face check: Edges not linked together");
                    success = false;
                }
                if (!Edges[i].OwnerFaces.Contains(this))
                {
                    messages.Add("Missing edge->face link");
                    success = false;
                }
                if (Edges[i].OwnerMesh != OwnerMesh)
                {
                    messages.Add("Mismatching edge->mesh and face->mesh link");
                    success = false;
                }
                if (!Vertices.Contains(Edges[i].Vertices[0]))
                {
                    messages.Add("Missing face->edge.vertex[0] link");
                    success = false;
                }
                if (!Vertices.Contains(Edges[i].Vertices[1]))
                {
                    messages.Add("Missing face->edge.vertex[1] link");
                    success = false;
                }
                
                Vector3D v0_offset = centre_pos - Edges[i].Vertices[0].Pos;
                double dir_cp = MathUtils.CrossXY(Edges[i].Direction,v0_offset);
                if (dir_cp < 0)
                {
                    messages.Add("Edge is going in wrong direction");
                    success = false;
                }
            }
            foreach (Vertex v in Vertices)
            {
                if (!v.OwnerFaces.Contains(this))
                {
                    messages.Add("Missing vertex->face link");
                    success = false;
                }
                if (!v.OwnerShape.ContainsMesh(OwnerMesh))
                {
                    messages.Add("Mismatching vertex->shape->mesh and face->mesh link");
                    success = false;
                }
            }
            
            return success;
        }

    }
}
