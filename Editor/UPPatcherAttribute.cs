using System;

namespace Rogue.UnityProjectPatcher.Editor {
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class UPPatcherAttribute: Attribute {
        public readonly string PackageName;
        public readonly bool RequiresBepInEx;
        
        public UPPatcherAttribute(string packageName, bool requiresBepInEx = false) {
            PackageName = packageName;
            RequiresBepInEx = requiresBepInEx;
        }
    }
}