using System;
using System.Collections.Generic;
using System.Reflection;

namespace DNX.Runtime
{
    /// <summary>
    /// Using reflection, you read Attribute definitions for classes, attributes, methods, and so on
    /// </summary>
    public static class AttributeHelper
    {
        /// <summary>
        /// Property Entry
        /// </summary>
        private struct AttrDictEntry
        {
            /// <summary>
            /// MemberInfo
            /// </summary>
            public object MemberInfo;
            /// <summary>
            /// AttributeType
            /// </summary>
            public Type AttributeType;
            /// <summary>
            /// Inherited
            /// </summary>
            public bool Inherited;
        }

        /// <summary>
        /// A private collection of static instances
        /// </summary>
        private static Dictionary<AttrDictEntry, Attribute> dictionary = new Dictionary<AttrDictEntry, Attribute>();



        /// <summary>
        /// Reads the Attribute definition on a class, attribute, or method
        /// </summary>
        /// <typeparam name="T">The type of the class, property, or method to be read</typeparam>
        /// <param name="element">An instance of a class, attribute, or method type</param>
        /// <returns></returns>
        public static T GetCustomerAttribute<T>(MemberInfo element) where T : Attribute
        {
            T result = default(T);
            System.Type attrType = typeof(T);

            AttrDictEntry key = CalculateKey(element, attrType, true);

            lock (AttributeHelper.dictionary)
            {
                if (AttributeHelper.dictionary.ContainsKey(key))
                    result = (T)AttributeHelper.dictionary[key];
                else
                {
                    result = (T)Attribute.GetCustomAttribute(element, attrType);
                    AttributeHelper.dictionary[key] = result;
                }
            }

            return result;
        }

        /// <summary>
        /// Private methods compute key-value pairs
        /// </summary>
        /// <param name="element">An instance of a class, attribute, or method type</param>
        /// <param name="attrType">Property Type</param>
        /// <param name="inherited">Inherited</param>
        /// <returns></returns>
        private static AttrDictEntry CalculateKey(object element, System.Type attrType, bool inherited)
        {
            AttrDictEntry key;

            key.MemberInfo = element;
            key.AttributeType = attrType;
            key.Inherited = inherited;

            return key;
        }
    }
}
