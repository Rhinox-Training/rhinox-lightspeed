using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.PackageManager.Requests;

namespace Rhinox.Lightspeed.Editor
{
    public class RequestAwaiter : INotifyCompletion
    {
        private Request _asyncOp;
        private Action _continuation;

        public RequestAwaiter(Request asyncOp)
        {
            _asyncOp = asyncOp;
            EditorApplication.update += Progress;
        }

        ~RequestAwaiter()
        {
            EditorApplication.update -= Progress;
        }

        public bool IsCompleted => _asyncOp.IsCompleted;

        public void Progress()
        {
            if (!IsCompleted) return;
            
            EditorApplication.update -= Progress;
            _continuation?.Invoke();
        }

        public void GetResult()
        {
        }

        public void OnCompleted(Action continuation)
        {
            _continuation = continuation;
        }
    }

    public static partial class ExtensionMethods
    {
        public static RequestAwaiter GetAwaiter(this Request asyncOp)
        {
            return new RequestAwaiter(asyncOp);
        }
    }
}