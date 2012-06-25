using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using convexcad.Shapes;

namespace convexcad
{
    [Serializable]
    public class RectangleTestScene : Scene
    {
        public override Node Create()
        {
            return Rectangle(1, 1);
        }
    }

    [Serializable]
    public class RectangleTestScene2 : Scene
    {
        public override Node Create()
        {
            return Rectangle(0.5, 2);
        }
    }

    [Serializable]
    public class RectangleTestScene3 : Scene
    {
        public override Node Create()
        {
            return Rectangle(2, 0.5);
        }
    }

    [Serializable]
    public class OverlappingRectangles : Scene
    {
        public override Node Create()
        {
        	return
        	Translate(0,0,0,
        		Rectangle(2,0.5),
        		Translate(1,0,0,
        			Rectangle(1,1)
        		)
        	);
        }
    }
    
    [Serializable]
    public class RectangleTranslated : Scene
    {
        public override Node Create()
        {
            return Translate(0.3,1.7,0,Rectangle(2, 0.5));
        }
    }
    [Serializable]
    public class RectangleTranslated2 : Scene
    {
        public override Node Create()
        {
            return Translate(-10.323453, 92.7738209, 8124.12, Rectangle(2.712093, 5.4239482734));
        }
    }
    [Serializable]
    public class RectangleTranslated3 : Scene
    {
        public override Node Create()
        {
            return Translate(-10.323453, 92.7738209, -1283.24, Rectangle(2.712093, 5.4239482734));
        }
    }
    [Serializable]
    public class RectangleRotateX : Scene
    {
        public override Node Create()
        {
            return Rotate(1, 0, 0, 45, Rectangle(1,2));
        }
    }
    [Serializable]
    public class RectangleRotateY : Scene
    {
        public override Node Create()
        {
            return Rotate(0, 1, 0, 135, Rectangle(1, 2));
        }
    }
    [Serializable]
    public class RectangleRotateZ : Scene
    {
        public override Node Create()
        {
            return Rotate(0, 0, 1, -120, Rectangle(1, 2));
        }
    }

    [Serializable]
    public class RectangleRotateMany : Scene
    {
        public override Node Create()
        {
            return Union(
                    Translate(-4, -4, 0, Rectangle(1, 1)),
                    Translate(-2, -4, 0, Rotate(0, 0, 1, 45, Rectangle(2, 1))),
                    Translate(0, -4, 0, Rotate(0, 0, 1, 90, Rectangle(2, 1))),
                    Translate(2, -4, 0, Rotate(0, 0, 1, 175, Rectangle(2, 1))),
                    Translate(4, -4, 0, Rotate(0, 0, 1, 296, Rectangle(2, 1))),
                    Translate(-3, -2, 0, Rotate(0, 1, 0, 45, Rectangle(2, 1))),
                    Translate(0,  -2, 0,  Rotate(0, 1, 0, 90, Rectangle(2, 1))),
                    Translate(3,  -2, 0,  Rotate(0, 1, 0, 175, Rectangle(2, 1))),
                    Translate(6,  -2, 0,  Rotate(0, 1, 0, 296, Rectangle(2, 1))),
                    Translate(-3, 0, 0,  Rotate(1, 0, 0, 45, Rectangle(2, 1))),
                    Translate(0,  0, 0,  Rotate(1, 0, 0, 90, Rectangle(2, 1))),
                    Translate(3,  0, 0,  Rotate(1, 0, 0, 175, Rectangle(2, 1))),
                    Translate(6,  0, 0,  Rotate(1, 0, 0, 296, Rectangle(2, 1)))
                     );
        }
    }

}
