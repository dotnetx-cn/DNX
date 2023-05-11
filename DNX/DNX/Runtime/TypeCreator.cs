using DNX.Core;
using System;
using System.Reflection;
using System.Text;

namespace DNX.Runtime
{
    /// <summary>
    /// Use late binding mode to generate instances dynamically
    /// </summary>
    public static class TypeCreator
    {
        /// <summary>
        /// Instance information private structure
        /// </summary>
        private struct TypeInfo
        {
            /// <summary>
            /// Instance assembly name
            /// </summary>
            public string AssemblyName;
            /// <summary>
            /// Instance type name
            /// </summary>
            public string TypeName;

            /// <summary>
            /// override
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return string.Format("{0}, {1}", this.TypeName, this.AssemblyName);
            }
        }

        /// <summary>
        /// Create an instance dynamically using post-binding mode.
        /// </summary>
        /// <param name="description">The full type name of the instance to create</param>
        /// <param name="constructorParams">Creates constructor arguments for the instance</param>
        /// <returns></returns>
        public static object CreateInstance(string description, params object[] constructorParams)
        {
            Type type = GetTypeInfo(description);

            ExceptionHelper.FalseThrow<TypeLoadException>(type != null, "type {0} load failture", description);

            return CreateInstance(type, constructorParams);
        }

        /// <summary>
        /// Creates an object based on type information that can be instantiated even if it does not have a public constructor
        /// </summary>
        /// <param name="type">The type information when the type is created</param>
        /// <param name="constructorParams">Creates constructor arguments for the instance</param>
        /// <returns></returns>
        public static object CreateInstance(Type type, params object[] constructorParams)
        {
            ExceptionHelper.FalseThrow<ArgumentNullException>(type != null, "type");
            ExceptionHelper.FalseThrow<ArgumentNullException>(constructorParams != null, "constructorParams");

            BindingFlags bf = BindingFlags.Instance | BindingFlags.Public;

            if (constructorParams.Length > 0)
            {
                Type[] types = new Type[constructorParams.Length];

                for (int i = 0; i < types.Length; i++)
                    types[i] = constructorParams[i].GetType();
                ConstructorInfo ci = type.GetConstructor(bf, null, CallingConventions.HasThis, types, null);

                if (ci != null)
                    return ci.Invoke(constructorParams);
            }
            else
            {
                return Activator.CreateInstance(type, true);
            }

            return null;
        }

        /// <summary>
        /// Get the type object from the type description
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        public static Type GetTypeInfo(string description)
        {
            ExceptionHelper.TrueThrow<ArgumentNullException>(string.IsNullOrEmpty(description), "description");

            Type result = Type.GetType(description);

            if (result == null)
            {
                TypeInfo ti = GenerateTypeInfo(description);

                AssemblyName aName = new AssemblyName(ti.AssemblyName);

                result = Type.GetType(ti.ToString());

                ExceptionHelper.FalseThrow<ArgumentNullException>(result != null, "Type information cannot be obtained {0}", ti.ToString());
            }

            return result;
        }

        /// <summary>
        /// Gets the default value for a data type
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>该类型的缺省值</returns>
        /// <remarks>Return null if the type is a reference type, otherwise return the default value of the value type. For example, 
        /// Int32 returns 0 and DateTime returns datetime.minvalue
        /// </remarks>
        public static object GetTypeDefaultValue(System.Type type)
        {
            ExceptionHelper.FalseThrow<ArgumentNullException>(type != null, "type");

            object result = null;

            if (type.IsValueType)
                result = TypeCreator.CreateInstance(type);
            else
                result = null;

            return result;
        }

        /// <summary>
        /// Create type instance information
        /// </summary>
        /// <param name="description">Instance description information</param>
        /// <returns></returns>
        private static TypeInfo GenerateTypeInfo(string description)
        {
            TypeInfo info = new TypeInfo();

            string[] typeParts = description.Split(',');

            info.TypeName = typeParts[0].Trim();

            StringBuilder strB = new StringBuilder(256);

            for (int i = 1; i < typeParts.Length; i++)
            {
                if (strB.Length > 0)
                    strB.Append(", ");

                strB.Append(typeParts[i]);
            }

            info.AssemblyName = strB.ToString().Trim();

            return info;
        }
    }
}
