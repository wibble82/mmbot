using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace convexcad
{
    sealed class AllowAllAssemblyVersionsDeserializationBinder : System.Runtime.Serialization.SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            Type typeToDeserialize = null;
            String currentAssembly = Assembly.GetExecutingAssembly().FullName;

            typeToDeserialize = Type.GetType(String.Format("{0}, {1}", typeName, assemblyName));

            // Get the type using the typeName and assemblyName
            if (typeToDeserialize == null)
            {
                typeToDeserialize = Type.GetType(String.Format("{0}, {1}", typeName, currentAssembly));
            }

            if (typeToDeserialize == null)
            {
                typeToDeserialize = Type.GetType(String.Format("{0}, {1}", "convexcad.Geometry.Scene", currentAssembly));
            }

            return typeToDeserialize;
        }
    }
}
