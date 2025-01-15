using Cysharp.Threading.Tasks;

namespace TapEmpire.Services
{
    public interface IIapHandler <T> where T: IapSettings
    {
        public UniTask Handle(T iapSettings);
    }

}