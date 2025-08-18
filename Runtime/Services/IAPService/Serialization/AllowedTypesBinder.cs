using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

// Essential for strict binding.
// It's possible to create it on the project side, not on in the TEL.
namespace TapEmpire.Services
{
    public class AllowedTypesBinder : ISerializationBinder
    {
        private static readonly Dictionary<string, Type> AllowedTypes = new()
        {
            // { "AddResource", typeof(AddResourceProduct) },
            { "DisableAds", typeof(DisableAdsProduct) }
        };

        public Type BindToType(string assemblyName, string typeName)
        {
            if (AllowedTypes.TryGetValue(typeName, out var type))
                return type;

            throw new JsonSerializationException($"Disallowed type: {typeName}");
        }

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            foreach (var kvp in AllowedTypes)
            {
                if (kvp.Value == serializedType)
                {
                    assemblyName = null;
                    typeName = kvp.Key;
                    return;
                }
            }

            throw new JsonSerializationException($"Type not allowed: {serializedType.Name}");
        }
    }
}
