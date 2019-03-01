using Framework.Editor;

namespace Framework.Editor
{
    public interface INotify
    {
        bool IsFromPath(PropertyPath path);
    }
    
    public interface IReorderableNotify : INotify
    {
        void OnReordered(int oldIndex, int newIndex);
        void OnAdded();
        void OnRemoved(int index);
    }
}