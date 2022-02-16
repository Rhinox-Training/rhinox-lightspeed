using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

namespace Rhinox.Lightspeed
{
	/*
	// Usage example:
	UnityWebRequest www = new UnityWebRequest();
	// ...
	await www.SendWebRequest();
	Debug.Log(req.downloadHandler.text);
	*/
	public class UnityWebRequestAwaiter : INotifyCompletion
	{
		private UnityWebRequestAsyncOperation asyncOp;
		private Action continuation;

		public UnityWebRequestAwaiter(UnityWebRequestAsyncOperation asyncOp)
		{
			this.asyncOp = asyncOp;
			asyncOp.completed += OnRequestCompleted;
		}

		public bool IsCompleted
		{
			get { return asyncOp.isDone; }
		}

		public void GetResult()
		{
		}

		public void OnCompleted(Action continuation)
		{
			this.continuation = continuation;
		}

		private void OnRequestCompleted(AsyncOperation obj)
		{
			continuation?.Invoke();
		}
	}

	public static class ExtensionMethods
	{
		public static UnityWebRequestAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOp)
		{
			return new UnityWebRequestAwaiter(asyncOp);
		}
	}
}