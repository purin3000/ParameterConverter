using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;

namespace ParameterConverter
{
    public static class CSVSplitter
    {
        const string DQ_CODE = "$_DQDQ_$";
        static readonly Regex regSplit = new Regex(",|\"([^\"]+)\"");

        public static string[,] Split(string filePath, Encoding encoding)
        {
            List<string[]> cells = new List<string[]>();
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(stream, encoding)) {
                while (!reader.EndOfStream) {
                    var line = reader.ReadLine();

                    line = line.Replace("\"\"", DQ_CODE);
                    var splitWords = regSplit.Split(line);
                    var replaceWords = splitWords.Select(str => str.Replace(DQ_CODE, "\"\"")).ToArray();
                    cells.Add(replaceWords);
                }
            }

            var maxCells = cells.Max(list => list.Length);

            string[,] cells2 = new string[cells.Count, maxCells];
            foreach (var (col, words) in cells.Indexed()) {
                foreach (var (row, word) in words.Indexed()) {
                    cells2[col, row] = word;
                }
            }

            var cell2 = cells.Select(list => list.ToArray()).ToArray();
            return cells2;
        }
    }


}

