using System;
using System.Linq;
using UnityEngine;

namespace FynnsTools.MultiUploader.Editor
{
    internal sealed class QuestConversionIgnore
    {
        readonly GameObject[] roots;

        public QuestConversionIgnore(GameObject[] roots)
        {
            this.roots = roots ?? Array.Empty<GameObject>();
        }

        public bool Contains(Transform target) => target != null && roots.Any(root =>
            root != null && (target == root.transform || target.IsChildOf(root.transform)));

        public bool ContainsBinding(Transform animationRoot, string path) =>
            Contains(string.IsNullOrEmpty(path) ? animationRoot : animationRoot.Find(path));
    }
}
