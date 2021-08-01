using System;
using System.Collections.Generic;
using UnityEngine;

namespace ParameterConverter.Editor
{
    public class CellDataConverter
    {
        private CellDataContext context;
        
        private CellData cellData;
        private Type targetType;
        private ScriptableObject targetObject;
        private string output;

        public static void Convert(CellDataContext context, List<CellData> list)
        {
            foreach (var cellData in list) {
                var converter = new CellDataConverter(context, cellData);

                converter.Parse();
            }
         
            context.SaveAssets();
        }

        private CellDataConverter(CellDataContext context, CellData cellData) {
            this.context = context;

            this.cellData = cellData;
            this.targetType = null;
            this.targetObject = null;
            this.output = "";
        }

        private void Parse()
        {
            for (int row = 0; row < cellData.RowLength; ++row) {
                for (int col = 0; col < cellData.ColumunLength && row < cellData.RowLength; ++col) {
                    var location = new Location(row, col);
                    var str = cellData.GetCell(location).ToLower();
                    var errorInfo = new CellDataErrorInfo(cellData, location);

                    if (!cellData.IsCommandCell(str)) {
                        continue;
                    }
                    
                    if (str == CellDataCommand.End) {
                        return;

                    } else if (str == CellDataCommand.Class) {
                        if (this.targetType != null) throw new DuplicateDefinitionsException(errorInfo);

                        var (label, output) = GetCommandArg2(ref location);
                        var targetType = ReflectionUtil.GetCustomClass(label);
                        if (targetType == null) throw new ClassTypeNotFoundException(label, errorInfo);

                        var targetObject = context.CreateObject(output, targetType);
                        if (targetObject == null) throw new ObjectNotFoundException(errorInfo);

                        this.output = output;
                        this.targetType = targetType;
                        this.targetObject = targetObject;

                    } else if (str == CellDataCommand.Horizontal) {
                        if (targetObject == null) throw new TargetObjectNotFoundException(errorInfo);
                        ParseHorizontal(ref location);

                    } else if (str == CellDataCommand.Vertical) {
                        if (targetObject == null) throw new TargetObjectNotFoundException(errorInfo);
                        ParseVertical(ref location);

                    } else if (str == CellDataCommand.Callback) {
                        var (attributeName, arg) = GetCommandArg2(ref location);
                        context.AddFunctionData(attributeName, arg, cellData.AssetPath, errorInfo);

                    } else {
                        throw new UnknownException(str, output, errorInfo);
                    }
                    (row, col) = (location.row, location.col);
                }
            }
        }

        private bool FieldIsArray(string fieldName)
        {
            return fieldName.IndexOf("[]") != -1;
        }

        private void SetValue(string fieldName, Location location)
        {
            var valStr = cellData.GetCell(location);
            var errorInfo = new CellDataErrorInfo(cellData, location);

            ReflectionUtil.SetFieldValue(targetType, targetObject, fieldName, valStr, context.Validators, errorInfo);
        }

        private void SetArrayValue(string fieldName, Location location, int arrayIndex)
        {
            var valStr = cellData.GetCell(location);
            var errorInfo = new CellDataErrorInfo(cellData, location);

            var memberStr = fieldName.Replace("[]", $"[{arrayIndex}]");
            ReflectionUtil.SetFieldValue(targetType, targetObject, memberStr, valStr, context.Validators, errorInfo);
        }

        private (string item1, string item2) GetCommandArg2(ref Location location)
        {
            var item1 = cellData.GetCell(location.row, location.col + 1);
            var item2 = cellData.GetCell(location.row, location.col + 2);
            location = new Location(location.row, location.col + 3);
            return (item1, item2);
        }

        private void ParseHorizontal(ref Location location)
        {
            var rect = cellData.CalcTableRect(location.row, location.col + 1);

            int arrayIndex = 0;
            for (int col = rect.col.min; col < rect.col.max; ++col) {
                var fieldName = cellData.GetCell(rect.row.min, col);
                if (string.IsNullOrEmpty(fieldName)) {
                    // フィールド定義がない場合は無視
                    continue;
                }

                arrayIndex = context.GetArrayOffset(output);
                for (int row = rect.row.min + 1; row < rect.row.max; ++row) {
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
            context.SetArrayOffset(output, arrayIndex);
            location = new Location(rect.row.max, 0);
        }

        private void ParseVertical(ref Location location)
        {
            var rect = cellData.CalcTableRect(location.row, location.col + 1);

            int arrayIndex = 0;
            for (int row = rect.row.min; row < rect.row.max; ++row) {
                var fieldName = cellData.GetCell(row, rect.col.min);
                if (string.IsNullOrEmpty(fieldName)) {
                    // フィールド定義がない場合は無視
                    continue;
                }

                arrayIndex = context.GetArrayOffset(output);
                for (int col = rect.col.min+1; col < rect.col.max; ++col) {
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
            context.SetArrayOffset(output, arrayIndex);
            location = new Location(rect.row.max, 0);
        }
    }
}

