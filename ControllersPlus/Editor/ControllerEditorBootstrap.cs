using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace FynnsTools.ControllerEditor
{
    [InitializeOnLoad]
    internal static class ControllerEditorBootstrap
    {
        private static bool initialized;
        static ControllerEditorBootstrap()
        {
            EditorApplication.delayCall += Initialize;
            AssemblyReloadEvents.beforeAssemblyReload += Shutdown;
            EditorApplication.quitting += Shutdown;
        }

        internal static void Initialize()
        {
            if (initialized)
                return;
            initialized = true;
            RuntimeHelpers.RunClassConstructor(typeof(ControllerEditor).TypeHandle);
            Run<EditorInitializeAttribute>();
            EditorApplication.update -= ControllerEditor.UpdateArrowAnimation;
            EditorApplication.update += ControllerEditor.UpdateArrowAnimation;
        }

        internal static void Shutdown()
        {
            if (!initialized)
                return;
            EditorApplication.update -= ControllerEditor.UpdateArrowAnimation;
            initialized = false;
            Run<EditorShutdownAttribute>();
        }

        private static void Run<T>()
            where T : EditorExtensionAttribute
        {
            var callbacks = typeof(ControllerEditorBootstrap).Assembly.GetTypes().SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)).Select(method => new
            {
            method, attribute = method.GetCustomAttribute<T>()}).Where(callback => callback.attribute != null).OrderBy(callback => callback.attribute._IssuerServer);
            foreach (var callback in callbacks)
            {
                try
                {
                    callback.method.Invoke(null, callback.attribute.m_IndexerServer);
                }
                catch (Exception exception)
                {
                    Debug.LogError("Controllers+ could not " + (typeof(T) == typeof(EditorInitializeAttribute) ? "initialize " : "shut down ") + callback.method.Name + ".");
                    Debug.LogException(exception is TargetInvocationException && exception.InnerException != null ? exception.InnerException : exception);
                }
            }
        }
    }
}
