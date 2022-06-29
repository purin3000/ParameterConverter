using System;
using ParameterConverter;
using UnityEngine;

[ParameterTarget("SampleData00")]
public class SampleData : ScriptableObject {
  public enum DataType {
    DEFAULT,
    TYPE_A
  }

  [SerializeField] private string objName;

  [SerializeField] private Vector3 pos;

  [SerializeField] private Item[] items;

  public string ObjName => objName;
  public Vector3 Pos => pos;
  public Item[] Items => items;

  [Serializable]
  public class Item {
    [SerializeField] private int id;

    [SerializeField] private DataType dataType;

    [SerializeField] private float floatValue;

    [SerializeField] private string stringValue;

    [SerializeField] private Vector3 pos;

    public int Id => id;
    public DataType DataType => dataType;
    public float FloatValue => floatValue;
    public string StringValue => stringValue;
    public Vector3 Pos => pos;
  }
}