using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace convexcad.Shapes
{
    public class Primitives
    {
        public static Shape Rectangle(Vector3D size)
        {
            Shape s = new Shape();
            Mesh m = s.CreateConvex();
            Face f = m.CreateFace(4);
            Vector3D halfsize = size*0.5;

            f.Vertices[0].Pos = new Point3D(-halfsize.X, -halfsize.Y, 0);
            f.Vertices[1].Pos = new Point3D(-halfsize.X,  halfsize.Y, 0);
            f.Vertices[2].Pos = new Point3D( halfsize.X,  halfsize.Y, 0);
            f.Vertices[3].Pos = new Point3D( halfsize.X, -halfsize.Y, 0);

            return s;
        }
        

    }
}
