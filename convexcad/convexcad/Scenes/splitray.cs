using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using convexcad.Shapes;

namespace convexcad
{
    [Serializable]
    public class BasicSplitRayScene : Scene
    {
        public override Node Create()
        {
            return SplitByRay(0, 0, 0, 0, 1, 0, Rectangle(2,3));
        }
    }
    [Serializable]
    public class DiagonalSplitRayScene : Scene
    {
        public override Node Create()
        {
            return SplitByRay(0, 0, 0, 1, 1, 0, Rectangle(2,3));
        }
    }
    [Serializable]
    public class ColinearSplitRayScene : Scene
    {
        public override Node Create()
        {
            return SplitByRay(1, 0, 0, 0, 1, 0, Rectangle(2,3));
        }
    }
    [Serializable]
    public class VertexVertexSplitRayScene : Scene
    {
        public override Node Create()
        {
            return SplitByRay(0, 0, 0, 1, 1, 0, Rectangle(2,2));
        }
    }    
    [Serializable]
    public class VertexEdgeSplitRayScene : Scene
    {
        public override Node Create()
        {
            return SplitByRay(-1, -1, 0, 1, 0.5, 0, Rectangle(2,2));
        }
    }      
    [Serializable]
    public class TouchVertexSplitRayScene : Scene
    {
        public override Node Create()
        {
            return SplitByRay(-1, -1, 0, -1, 0.5, 0, Rectangle(2,2));
        }
    }      
    [Serializable]
    public class MultiSplitRayScene : Scene
    {
        public override Node Create()
        {
            return SplitByRay(0, 0, 0, 1, 1, 0, 
            					Translate(-2.5,-2.5,0,Rotate(0,0,1,45,Rectangle(2,2))),
            					Translate(3,3,0,Rectangle(1,1)),
            					Translate(1.5,2,0,Rotate(0,0,1,22,Rectangle(1,1))),
            					Rectangle(2,3)
            				);
        }
    }
    
}
