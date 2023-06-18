using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace ParameterConverter.Sample {
   public static class UnityWebRequestExtension {
     
     public class Runner : MonoBehaviour {
     }
     
     public static UnityWebRequestAwaitable GetAwaiter(this UnityWebRequestAsyncOperation operation) {
       return new UnityWebRequestAwaitable(operation);
     }

     public class UnityWebRequestAwaitable : INotifyCompletion {
       private UnityWebRequestAsyncOperation _operation;
       private Action _continuation;

       public UnityWebRequestAwaitable(UnityWebRequestAsyncOperation operation) {
         _operation = operation;
         
         var go = new GameObject("Runner");
         var runner = go.AddComponent<Runner>();
         runner.StartCoroutine(CheckLoop(go));
       }
       
       public bool IsCompleted => _operation.isDone;
       public void OnCompleted(Action continuation) => _continuation = continuation;

       public void GetResult() { }

       private IEnumerator CheckLoop(GameObject go) {
         yield return _operation;
         _continuation?.Invoke();
         Object.DestroyImmediate(go);
       }
     }
   }
}