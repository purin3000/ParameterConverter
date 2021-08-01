using System.Collections.Generic;
using System.Text;

namespace ParameterConverter.Editor
{
    public static class CSVReader
    {
        public static List<CellData> CreateCellData(string assetPath, Encoding encoding)
        {
            var obj = CSVSplitter.Split(assetPath, encoding);
            
            List<CellData> list = new List<CellData>();
            list.Add(new CellData(assetPath, assetPath.GetFileName(), obj));
            return list;
        }
    }
}

