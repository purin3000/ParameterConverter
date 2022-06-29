using System.Collections;
using UnityEngine;

public class StaticCoroutine : MonoBehaviour {
  private static StaticCoroutine instance;

  public static void Dispatch(IEnumerator enumerator) {
    if (instance) {
      DestroyImmediate(instance);
    }
    var go = new GameObject("StaticCoroutine");
    instance = go.AddComponent<StaticCoroutine>();
    instance.StartCoroutine(enumerator);
  }
}