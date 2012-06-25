using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace convexcad.Shapes
{
    public class Vertex
    {
        public Shape OwnerShape;
        public List<Face> OwnerFaces = new List<Face>();
        public List<Edge> OwnerEdges = new List<Edge>();

        public Point3D Pos;
        public int Idx = -1;

        public Vertex() { }

        public override string ToString()
        {
            return String.Format("[{0}] Idx:{1:d}",Pos,Idx);
        }

    }
}
