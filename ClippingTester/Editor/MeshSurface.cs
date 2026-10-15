using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FynnsTools.Clipping
{
    public sealed class MeshSurface
    {
        public Renderer Renderer;
        public int Slot;
        internal Renderer SourceRenderer;
        public MeshSurface(Renderer renderer = null, int slot = -1) { Renderer = renderer; Slot = slot; }

        public static MeshSurface[] Normalize(IEnumerable<MeshSurface> surfaces)
        {
            return (surfaces ?? Enumerable.Empty<MeshSurface>()).Where(s => s != null && s.Renderer)
                .GroupBy(s => s.Renderer).SelectMany(group => group.Any(s => s.Slot < 0)
                    ? new[] { new MeshSurface(group.Key) }
                    : group.Select(s => s.Slot).Distinct().Select(slot => new MeshSurface(group.Key, slot))).ToArray();
        }
        public static bool ValidPair(IEnumerable<MeshSurface> body, IEnumerable<MeshSurface> clothing)
        {
            var a = Normalize(body); var b = Normalize(clothing);
            return a.Length > 0 && b.Length > 0 && !a.Any(x => b.Any(y => x.Renderer == y.Renderer &&
                (x.Slot < 0 || y.Slot < 0 || x.Slot == y.Slot)));
        }
    }
}
