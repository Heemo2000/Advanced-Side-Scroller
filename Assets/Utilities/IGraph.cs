
namespace Utilities
{
    public interface IGraph
    {
        void Initialize();
        void Reset();

        bool Contains(IAstarNode node);
    }
}
