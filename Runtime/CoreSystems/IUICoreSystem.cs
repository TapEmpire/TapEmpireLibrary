namespace TapEmpire.CoreSystems
{
    public interface IUICoreSystem : ICoreSystem
    {
        public void BlockUI(bool shouldBlock);
    }
}