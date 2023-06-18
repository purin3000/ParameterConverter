using System;
using UnityEngine;

namespace ParameterConverter.Sample {
  [ConvertTarget("SampleData00")]
  public class SampleData : ScriptableObject {
    public enum DataType {
      DEFAULT,
      TYPE_A
    }

    [SerializeField]
    private string objName;
    public string ObjName => objName;

    [SerializeField]
    private Vector3 pos;
    public Vector3 Pos => pos;

    [SerializeField]
    private Item[] items;
    public Item[] Items => items;

    [Serializable]
    public class Item {
      [SerializeField]
      private int id;
      public int Id => id;

      [SerializeField]
      private DataType dataType;
      public DataType DataType => dataType;

      [SerializeField]
      private float floatValue;
      public float FloatValue => floatValue;

      [SerializeField]
      private string stringValue;
      public string StringValue => stringValue;

      [SerializeField]
      private Vector3 pos;
      public Vector3 Pos => pos;
    }
  }
}