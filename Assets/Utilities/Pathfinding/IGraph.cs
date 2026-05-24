
namespace Utilities.Pathfinding
{
    public interface IGraph
    {
        void Initialize();
        void Reset();

        bool Contains(IAstarNode node);
    }
}
