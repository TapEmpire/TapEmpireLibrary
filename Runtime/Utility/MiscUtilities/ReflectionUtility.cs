using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace TapEmpire.Utility
{
    public static class ReflectionUtility
    {
        public static IEnumerable<FieldInfo> GetFields(System.Type type)
        {
            return type
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public static bool IsFieldNull<TClass>(TClass target, string privateFieldName)
        {
            var type = typeof(TClass);
            FieldInfo fieldInfo = type.GetField(privateFieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            return fieldInfo.GetValue(target) == null;
        }

        public static TValue GetPrivateField<TClass, TValue>(TClass target, string privateFieldName)
        {
            var type = typeof(TClass);
            FieldInfo fieldInfo = type.GetField(privateFieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            return fieldInfo != null ? (TValue)fieldInfo.GetValue(target) : default;
        }

        public static void SetPrivateField<TClass, TValue>(TClass target, string privateFieldName, TValue value)
        {
            var type = typeof(TClass);
            FieldInfo fieldInfo = type.GetField(privateFieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            if (fieldInfo != null)
            {
                fieldInfo.SetValue(target, value);
            }
        }

        public static void AddToPrivateField<TClass, TValue>(TClass target, string privateFieldName, TValue value)
        {
            var type = typeof(TClass);
            FieldInfo fieldInfo = type.GetField(privateFieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            if (fieldInfo != null)
            {
                var list = fieldInfo.GetValue(target) as List<TValue>;
                list.Add(value);
                // fieldInfo.SetValue(target, value);
            }
        }

        /*public static T GetFieldValue<T>(System.Type type, string fieldName)
        {
            var type = 
        }*/
    }
}
