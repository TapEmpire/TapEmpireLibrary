using Newtonsoft.Json;

namespace TapEmpire.Services
{
    public interface IIapProduct
    {
        [JsonIgnore] string ProductId { get; }
    }
}