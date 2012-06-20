using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using convexcad.Geometry;

namespace convexcad
{
    [Serializable]
    public class TestScene : CSGScene
    {
        public string GetString() { return "hello"; }

        public override Geometry.Node Create()
        {
            return Rectangle(1, 1);
 /*           Union(
                Rectangle(4, 3),
                Translate(2, 2, 0, Rectangle(4, 3)),
                 Translate(-1, 3, 0, Rectangle(5, 6)),
                Translate(2, 1, 0, Rectangle(7, 1)),
                Translate(3, 1, 0, Rectangle(1, 7))
            );
*/
        }
    }
}
