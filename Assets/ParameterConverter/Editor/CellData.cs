using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ParameterConverter.Editor
{
    public struct Location
    {
        public Location(int row, int col)
        {
            this.row = row;
            this.col = col;
        }
        public int row { get; private set; }
        public int col { get; private set; }
    }

    /// <summary>
    /// �R�����g�̓N���A���ꂽ��ԂőS�ăZ�����i�[����܂�
    /// </summary>
    public class CellData
    {
        private static readonly Regex RegLineComment = new Regex(@"^#[^#]?");
        private static readonly Regex RegCellComment = new Regex(@"^##[^#]?");

        public string SheetName { get; private set; }
        public string AssetPath { get; private set; }
        
        private string[,] cells;

        public struct MinMax
        {
            public MinMax(int min, int max)
            {
                this.min = min;
                this.max = max;
            }
            public int min;
            public int max;
        }

        public struct TableRect
        {
            public TableRect(MinMax row, MinMax col)
            {
                this.row = row;
                this.col = col;
            }
            public MinMax row;
            public MinMax col;
        }

        /// <summary>
        /// �ŏI�s�擾
        /// �󔒂ŋ�؂��čŏI���擾����ꍇ��GetLastRow()���g�p���邱��
        /// </summary>
        public int RowLength { get { return cells.GetLength(0); } }

        /// <summary>
        /// �ŏI��擾
        /// �󔒂ŋ�؂��čŏI���擾����ꍇ��GetLastColumn()���g�p���邱��
        /// </summary>
        public int ColumunLength { get { return cells.GetLength(1); } }

        public CellData(string assetPath, string sheetName, string[,] cells)
        {
            this.AssetPath = assetPath;
            this.SheetName = sheetName;
            this.cells = cells;

            for (int row = 0; row < RowLength; ++row) {
                for (int col = 0; col < ColumunLength; ++col) {
                    var str = cells[row, col];

                    if (string.IsNullOrEmpty(str) || RegCellComment.IsMatch(str)) {
                        cells[row, col] = "";
                    } else if (RegLineComment.IsMatch(str)) {
                        for (int col2 = col; col2 < ColumunLength; ++col2) {
                            cells[row, col2] = "";
                        }
                        break;
                    }
                }
            }
        }

        public string GetCell(Location location) => GetCell(location.row, location.col);

        /// <summary>
        /// �Z���擾
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public string GetCell(int row, int col)
        {
            if (row < RowLength && col < ColumunLength) {
                return cells[row, col];
            }
            return "";
        }

        public bool IsCommandCell(string str)
        {
            var s = str.ToLower();
            return CellDataCommand.CommandList.Any(ss => ss == s);
        }

        public bool IsEmptyRow(int row, int colMin, int colMax)
        {
            for (int col = colMin; col < colMax; ++col) {
                var str = GetCell(row, col);
                if (string.IsNullOrEmpty(str)) {
                    continue;
                }
                return false;
            }
            return true;
        }

        public bool IsEmptyCol(int col, int rowMin, int rowMax)
        {
            for (int row = rowMin; row < rowMax; ++row) {
                var str = GetCell(row, col);
                if (string.IsNullOrEmpty(str)) {
                    continue;
                }
                return false;
            }
            return true;
        }

        public TableRect CalcTableRect(int row, int col)
        {
            var (rowMin, colMin) = SkipEmptyCell(row, col);
            var (rowMax, colMax) = CalcTableLimit(row, col);
            return new TableRect(new MinMax(rowMin, rowMax), new MinMax(colMin, colMax));
        }

        public (int row, int col) SkipEmptyCell(int row, int col)
        {
            for (; row < RowLength; ++row) {
                for (; col < ColumunLength; ++col) {
                    var str = GetCell(row, col);
                    if (string.IsNullOrEmpty(str)) {
                        continue;
                    }
                    return (row, col);
                }
                col = 0;
            }
            return (RowLength, ColumunLength);
        }

        /// <summary>
        /// �w�肵���Z�����N�_�Ƃ����e�[�u���̍ő�T�C�Y���擾
        /// ���炩�̃R�}���h�ɂ���ĉ�͂��I��
        /// </summary>
        /// <param name="startRow"></param>
        /// <param name="startCol"></param>
        public (int row, int col) CalcTableLimit(int startRow, int startCol)
        {
            int lastRow = RowLength;
            int lastCol = ColumunLength;

            for (int row = startRow + 1; row < RowLength; ++row) {
                var str = GetCell(row, startCol);
                if (IsCommandCell(str)) {
                    lastRow = row;
                    break;
                }
            }

            for (int col = startCol + 1; col < ColumunLength; ++col) {
                var str = GetCell(startRow, col);
                if (IsCommandCell(str)) {
                    lastCol = col;
                    break;
                }
            }

            return (lastRow, lastCol);
        }

    }
}

