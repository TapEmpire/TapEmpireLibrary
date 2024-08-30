
using System;
using Newtonsoft.Json.Linq;

namespace TapEmpire.Services
{
    public interface IRemoteSerializable
    {
        string TokenName => String.Empty;

        void DeserializeJson(JToken token);
        string SerializeJson();
    }
}
