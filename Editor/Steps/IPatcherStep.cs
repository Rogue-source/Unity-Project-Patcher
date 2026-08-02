using Cysharp.Threading.Tasks;

namespace Rogue.UnityProjectPatcher.Editor.Steps {
    public interface IPatcherStep {
        UniTask<StepResult> Run();
        void OnComplete(bool failed);
    }
}