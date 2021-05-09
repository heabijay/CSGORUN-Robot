using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGORUN_Robot.Extensions
{
    public static class MergePublicItems
    {
        public static void Merge<T>(this T source, T target) where T: class
        {
            source.Merge(target, source.GetType());
        }

        public static void Merge(this object source, object target, Type type)
        {
            var all = type.GetProperties();
            var props = type.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public).Where(t => t.CanWrite && t.CanRead);

            foreach (var prop in props)
            {
                if (prop.PropertyType.IsClass)
                {
                    if ()
                    prop.GetValue(source).Merge(prop.GetValue(target), prop.PropertyType);
                }
                else
                {
                    prop.SetValue(source, prop.GetValue(target));
                }
            }
        }
    }
}
