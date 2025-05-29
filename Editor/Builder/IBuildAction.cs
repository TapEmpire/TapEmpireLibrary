namespace TapEmpire.Build
{
    public interface IBuildAction
    {
        void Apply(bool isDebug);
    }
}