using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EngineCore.Utils
{
    public static class Miscellaneous
    {
        /// <summary>
		/// Checks whether type has attribute of given type
		/// </summary>
		/// <typeparam name="AttributeType"></typeparam>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool HasAttribute<T>(this Type type) where T : Attribute
        {
            return type.GetCustomAttributes(typeof(T), true).Any();
        }

        /// <summary>
        /// Searches all loaded assemblies for all public subclasses of given type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type[] GetAllClassesWithAttribute<T>() where T : Attribute
        {
            List<Type> types = new List<Type>();

            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {

                foreach (var t in a.GetTypes())
                {
                    if (t.HasAttribute<T>())
                    {
                        types.Add(t);
                    }
                }
            }

            return types.ToArray();
        }
    }
}
