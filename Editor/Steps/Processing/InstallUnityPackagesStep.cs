using System.Linq;
using UnityEditor.PackageManager.Requests;
using Cysharp.Threading.Tasks;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace Rogue.UnityProjectPatcher.Editor.Steps
{
    public readonly struct InstallUnityPackagesStep : IPatcherStep
    {
        public async UniTask<StepResult> Run()
        {
            var settings = this.GetSettings();

            foreach (var package in settings.RequiredUnityPackages)
            {
                if (PackageInstalled(package))
                    continue;

                var request = Client.Add(package);

                while (!request.IsCompleted)
                    await UniTask.Yield();

                if (request.Status == StatusCode.Failure)
                {
                    UnityEngine.Debug.LogError(request.Error.message);
                    return StepResult.Failure;
                }
            }

            return StepResult.RestartEditor;
        }

        public void OnComplete(bool failed) { }

        private static bool PackageInstalled(string packageName)
{
    ListRequest request = Client.List(true);

    while (!request.IsCompleted)
    {
    }

    if (request.Status != StatusCode.Success)
        return false;

    foreach (var package in request.Result)
    {
        if (package.name == packageName)
            return true;
    }

    return false;
}
    }
}