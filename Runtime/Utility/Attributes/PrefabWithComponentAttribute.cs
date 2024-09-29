using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class PrefabWithComponentAttribute : PropertyAttribute
{
    public Type RequiredComponentType { get; private set; }

    public PrefabWithComponentAttribute(Type requiredComponentType)
    {
        if (!typeof(Component).IsAssignableFrom(requiredComponentType))
        {
            throw new ArgumentException("Type must be a Component.");
        }
        RequiredComponentType = requiredComponentType;
    }
}