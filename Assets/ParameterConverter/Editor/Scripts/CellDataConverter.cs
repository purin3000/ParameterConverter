using System;
using System.Collections.Generic;
using ParameterConverter.Editor.Internal;
using UnityEngine;

namespace ParameterConverter.Editor {
  /// <summary>
  /// CellDataを入力として、オブジェクトを出力します
  /// </summary>
  public class CellDataConverter {
    private readonly CellData cellData;
    private readonly CellDataConverterContext converterContext;
    private string output;
    private ScriptableObject targetObject;
    private Type targetType;

    private CellDataConverter(CellDataConverterContext converterContext, CellData cellData) {
      this.converterContext = converterContext;

      this.cellData = cellData;
      targetType = null;
      targetObject = null;
      output = "";
    }

    public static void Convert(List<CellData> list) {
      var validatorList = new[] { new IdChecker() };
      var replaceStrList = new[] { new ReplacePathPathStr() };
      Convert(list, validatorList, replaceStrList);
    }

    public static void Convert(List<CellData> list, IReadOnlyList<IValueChecker> validatorList, IReadOnlyList<IReplacePathStr> replaceStrList) {
      var context = new CellDataConverterContext(validatorList, replaceStrList);
      foreach (var cellData in list) {
        var converter = new CellDataConverter(context, cellData);
        converter.Parse();
      }
      context.SaveAssets();
    }

    private void Parse() {
      for (var row = 0; row < cellData.RowLength; ++row)
      for (var col = 0; col < cellData.ColumunLength && row < cellData.RowLength; ++col) {
        var location = new Location(row, col);
        var str = cellData.GetCell(location).ToLower();
        var errorInfo = new CellParseInfo(cellData, location);

        if (!cellData.IsCommandCell(str)) {
          continue;
        }

        if (str == CellDataCommand.END) {
          return;
        }

        if (str == CellDataCommand.CLASS) {
          if (this.targetType != null) {
            throw new DuplicateDefinitionsException(errorInfo);
          }

          var (label, output) = GetCommandArg2(ref location);
          output = converterContext.ReplaceStr(output);

          var targetType = ReflectionUtil.GetCustomClass(label);
          if (targetType == null) {
            throw new ClassTypeNotFoundException(label, errorInfo);
          }

          var targetObject = converterContext.CreateObject(output, targetType);
          if (targetObject == null) {
            throw new ObjectNotFoundException(errorInfo);
          }

          this.output = output;
          this.targetType = targetType;
          this.targetObject = targetObject;
        } else if (str == CellDataCommand.HORIZONTAL) {
          if (targetObject == null) {
            throw new TargetObjectNotFoundException(errorInfo);
          }
          ParseHorizontal(ref location);
        } else if (str == CellDataCommand.VERTICAL) {
          if (targetObject == null) {
            throw new TargetObjectNotFoundException(errorInfo);
          }
          ParseVertical(ref location);
        } else if (str == CellDataCommand.CALLBACK) {
          var (attributeName, arg) = GetCommandArg2(ref location);
          converterContext.AddFunctionData(attributeName, arg, cellData.AssetPath, errorInfo);
        } else {
          throw new UnknownException(str, output, errorInfo);
        }

        (row, col) = (location.row, location.col);
      }
    }

    private bool FieldIsArray(string fieldName) {
      return fieldName.IndexOf("[]") != -1;
    }

    private void SetValue(string fieldName, Location location) {
      var valStr = cellData.GetCell(location);
      var errorInfo = new CellParseInfo(cellData, location);

      ReflectionUtil.SetFieldValue(targetType, targetObject, fieldName, valStr, converterContext, errorInfo);
    }

    private void SetArrayValue(string fieldName, Location location, int arrayIndex) {
      var valStr = cellData.GetCell(location);
      var errorInfo = new CellParseInfo(cellData, location);

      var memberStr = fieldName.Replace("[]", $"[{arrayIndex}]");
      ReflectionUtil.SetFieldValue(targetType, targetObject, memberStr, valStr, converterContext, errorInfo);
    }

    private (string item1, string item2) GetCommandArg2(ref Location location) {
      var item1 = cellData.GetCell(location.row, location.col + 1);
      var item2 = cellData.GetCell(location.row, location.col + 2);
      location = new Location(location.row, location.col + 3);
      return (item1, item2);
    }

    private void ParseHorizontal(ref Location location) {
      var rect = cellData.CalcTableRect(location.row, location.col + 1);

      var arrayIndex = 0;
      for (var col = rect.col.min; col < rect.col.max; ++col) {
        var fieldName = cellData.GetCell(rect.row.min, col);
        if (string.IsNullOrEmpty(fieldName)) // フィールド定義がない場合は無視
        {
          continue;
        }

        arrayIndex = converterContext.GetArrayOffset(output);
        for (var row = rect.row.min + 1; row < rect.row.max; ++row) {
          // 空白行は無視
          if (cellData.IsEmptyRow(row, rect.col.min, rect.col.max)) {
            continue;
          }

          if (FieldIsArray(fieldName)) {
            SetArrayValue(fieldName, new Location(row, col), arrayIndex++);
          } else {
            SetValue(fieldName, new Location(row, col));
            break;
          }
        }
      }

      converterContext.SetArrayOffset(output, arrayIndex);
      location = new Location(rect.row.max, 0);
    }

    private void ParseVertical(ref Location location) {
      var rect = cellData.CalcTableRect(location.row, location.col + 1);

      var arrayIndex = 0;
      for (var row = rect.row.min; row < rect.row.max; ++row) {
        var fieldName = cellData.GetCell(row, rect.col.min);
        if (string.IsNullOrEmpty(fieldName)) // フィールド定義がない場合は無視
        {
          continue;
        }

        arrayIndex = converterContext.GetArrayOffset(output);
        for (var col = rect.col.min + 1; col < rect.col.max; ++col) {
          // 空白行は無視
          if (cellData.IsEmptyCol(col, rect.row.min, rect.row.max)) {
            continue;
          }

          if (FieldIsArray(fieldName)) {
            SetArrayValue(fieldName, new Location(row, col), arrayIndex++);
          } else {
            SetValue(fieldName, new Location(row, col));
            break;
          }
        }
      }

      converterContext.SetArrayOffset(output, arrayIndex);
      location = new Location(rect.row.max, 0);
    }
  }
}