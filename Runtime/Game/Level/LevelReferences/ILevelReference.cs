
namespace TapEmpire.Level
{
    public interface ILevelReference<T> : ILevelReference
    {
        T Reference { get; }
    }

    public interface ILevelReference
    {
    }
}