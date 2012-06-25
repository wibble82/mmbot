using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;

namespace convexcad
{
    namespace Geometry
    {
        [Serializable]
        public class CSGSceneTestFIle
        {
            public CSGScene Scene = null;
            public Vertex[] ResultVertices = null;
            public Edge[] ResultEdges = null;
            public Face[] ResultFaces = null;
        }

        [Serializable]
        public class CSGScene
        {
            public Node Root = null;
            public static SafeScreenSpaceLines3D DebugLines = null;
            public static int TargetStage = -1;
            public static List<string> Stages = new List<string>();
            public static Node LastNode = null;

            public static bool NextStage(string stage_name)
            {
                if (TargetStage==-1 || Stages.Count <= TargetStage)
                {
                    Stages.Add(stage_name);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public static bool IsCurrentStage()
            {
                return TargetStage >= 0 && TargetStage == Stages.Count - 1;
            }

 
            public static CSGScene Load(string file_name)
            {
                CSGScene res = null;
                if (File.Exists(file_name))
                {
                    Stream strm = File.OpenRead(file_name);
                    BinaryFormatter deserializer = new BinaryFormatter();
                    deserializer.Binder = new AllowAllAssemblyVersionsDeserializationBinder();
                    res = (CSGScene)deserializer.Deserialize(strm);
                    strm.Close();
                }
                return res;
            }

            public void Save(string file_name)
            {
                Stream strm = File.Create(file_name);
                BinaryFormatter serializer = new BinaryFormatter();
                serializer.Serialize(strm, this);
                strm.Close();
            }

            public static CSGSceneTestFIle LoadTestFile(string file_name)
            {
                CSGSceneTestFIle res = null;
                if (File.Exists(file_name))
                {
                    Stream strm = File.OpenRead(file_name);
                    BinaryFormatter deserializer = new BinaryFormatter();
                    deserializer.Binder = new AllowAllAssemblyVersionsDeserializationBinder();
                    res = (CSGSceneTestFIle)deserializer.Deserialize(strm);
                    strm.Close();
                }
                return res;
            }

            public void SaveTestFile(string file_name)
            {
                CSGSceneTestFIle file = new CSGSceneTestFIle();
                file.Scene = this;
                if(Root == null)
                    Run();
                Root.GetWeldedGeometry(out file.ResultVertices, out file.ResultEdges);
                Stream strm = File.Create(file_name);
                BinaryFormatter serializer = new BinaryFormatter();
                serializer.Serialize(strm, file);
                strm.Close();
            }

            public CSGScene Run()
            {
                Root = Create();
                if (Root != null)
                    Root.Run();
                return this;
            }

            public virtual Node Create()
            {
                return null;
            }

            public Node Rectangle(double x, double y)
            {
                return new RectangleNode(x, y);
            }

            public Node Box(double x, double y, double z)
            {
                return new BoxNode(x, y, z);
            }

            public Node Translate(double x, double y, double z, params Node[] nodes)
            {
                return new TranslateNode(x, y, z, nodes);
            }

            public Node Rotate(double axisx, double axisy, double axisz, double angle, params Node[] nodes)
            {
                return new RotateNode(new Vector3D(axisx,axisy,axisz), angle, nodes);
            }

            public Node Union(params Node[] nodes)
            {
                return new UnionNode(nodes);
            }
            public Node Difference(params Node[] nodes)
            {
                return new DifferenceNode(nodes);
            }
            public Node Intersect(params Node[] nodes)
            {
                return new IntersectNode(nodes);
            }
        }
    }
}
