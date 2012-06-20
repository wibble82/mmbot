using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using convexcad.Geometry;

namespace convexcad
{
    [Serializable]
    public class UnionTest_RectRectOverlapping : CSGScene
    {
        public override Geometry.Node Create()
        {
            return Union(Rectangle(3,1.5),Translate(0.5,0.5,0,Rectangle(3,1)));
        }
    }

    [Serializable]
    public class UnionTest_RectRotRectOverlapping : CSGScene
    {
        public override Geometry.Node Create()
        {
            return Union(Rectangle(1, 3), Translate(0.5, 0.5, 0, Rotate(0,0,1,45,Rectangle(3, 1))));
        }
    }

    [Serializable]
    public class UnionTest_RectRectTouching : CSGScene
    {
        public override Geometry.Node Create()
        {
            return Union(Rectangle(1, 3), Translate(2, 0.5, 0, Rectangle(3, 1)));
        }
    }

    [Serializable]
    public class UnionTest_RectRectNotOverlapping : CSGScene
    {
        public override Geometry.Node Create()
        {
            return Union(Rectangle(1, 3), Translate(3, 0.5, 0, Rectangle(3, 1)));
        }
    }

    [Serializable]
    public class UnionTest_RectRectContainedA : CSGScene
    {
        public override Geometry.Node Create()
        {
            return Union(Rectangle(3, 3), Rectangle(1, 1));
        }
    }
    [Serializable]
    public class UnionTest_RectRectContainedB : CSGScene
    {
        public override Geometry.Node Create()
        {
            return Union(Rectangle(1, 1), Rectangle(3, 3));
        }
    }
    [Serializable]
    public class UnionTest_MultiRectTranslatedOverlapping : CSGScene
    {
        public override Geometry.Node Create()
        {
            return Union(
                            Rectangle(4, 3),
                            Translate(2, 2, 0, Rectangle(4, 3)),
                            Translate(-1, 3, 0, Rectangle(5, 6)),
                            Translate(2, 1, 0, Rectangle(7, 1)),
                            Translate(3, 1, 0, Rectangle(1, 7))
                        );
        }
    }

}
