using TapEmpire.Services;
using TapEmpire.Services;

namespace TEL.Services
{
    public interface IABTestingService : IService
    {
        string Group { get; }
        
        System.Action OnGroupAssigned { get; set; }

        void SetGroup(string group);
    }
}
