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

        public static void SetPrivateField<TClass, TValue>(TClass target, string privateFieldName, TValue value)
        {
            var type = typeof(TClass);
            FieldInfo fieldInfo = type.GetField(privateFieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            if (fieldInfo != null)
            {
                fieldInfo.SetValue(target, value);
            }
        }

        /*public static T GetFieldValue<T>(System.Type type, string fieldName)
        {
            var type = 
        }*/
    }
}