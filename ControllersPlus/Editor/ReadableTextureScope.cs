using System;
using UnityEngine;

namespace FynnsTools.ControllerEditor
{
    internal sealed class ReadableTextureScope : IDisposable
    {
        internal bool adapterPolicy;
        internal Texture2D m_InterpreterPolicy;
        internal ReadableTextureScope(Texture2D texture)
        {
            if (!texture)
                throw new ArgumentNullException(nameof(texture));
            m_InterpreterPolicy = texture;
            if (texture.isReadable)
            {
                try
                {
                    texture.GetPixel(0, 0);
                    return;
                }
                catch (UnityException)
                {
                }
            }

            var previous = RenderTexture.active;
            var temporary = RenderTexture.GetTemporary(texture.width, texture.height);
            Texture2D copy = null;
            try
            {
                temporary.filterMode = FilterMode.Point;
                Graphics.Blit(texture, temporary);
                RenderTexture.active = temporary;
                copy = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
                copy.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0, false);
                copy.Apply(false, false);
                m_InterpreterPolicy = copy;
                adapterPolicy = true;
            }
            catch
            {
                if (copy)
                    UnityEngine.Object.DestroyImmediate(copy);
                throw;
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(temporary);
            }
        }

        public void Dispose()
        {
            if (!adapterPolicy)
                return;
            adapterPolicy = false;
            if (m_InterpreterPolicy)
                UnityEngine.Object.DestroyImmediate(m_InterpreterPolicy);
            m_InterpreterPolicy = null;
        }

        public static implicit operator Texture2D(ReadableTextureScope scope) => scope.m_InterpreterPolicy;
    }
}
