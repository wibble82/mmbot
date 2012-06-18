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

            sealed class AllowAllAssemblyVersionsDeserializationBinder : System.Runtime.Serialization.SerializationBinder
            {
                public override Type BindToType(string assemblyName, string typeName)
                {
                    Type typeToDeserialize = null;

                    String currentAssembly = Assembly.GetExecutingAssembly().FullName;

                    // In this case we are always using the current assembly
                    assemblyName = currentAssembly;

                    if (typeName == "convexcad.TestScene")
                        typeName = "convexcad.Geometry.CSGScene";

                    // Get the type using the typeName and assemblyName
                    typeToDeserialize = Type.GetType(String.Format("{0}, {1}",
                        typeName, assemblyName));
                    
                    return typeToDeserialize;
                }
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
