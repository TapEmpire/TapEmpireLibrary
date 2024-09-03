namespace TapEmpire.Services
{
    public interface IABTestingService : IService
    {
        string Group { get; }
        
        System.Action OnGroupAssigned { get; set; }

        void SetGroup(string group);
    }
}
