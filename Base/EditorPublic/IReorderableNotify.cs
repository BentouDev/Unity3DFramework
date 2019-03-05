using Framework.Editor;

namespace Framework.Editor
{
    public interface INotify
    {
        bool IsFromPath(PropertyPath path);
    }

    public interface IReorderableNotify : INotify
    {
        void OnReordered(PropertyPath path, int oldIndex, int newIndex);
        void OnAdded(PropertyPath path);
        void OnRemoved(PropertyPath path, int index);
    }
}