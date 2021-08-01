namespace ParameterConverter.Editor
{
    public class CellDataCommand
    {
        public const string Class = ".class";
        public const string Horizontal = ".horizontal";
        public const string Vertical = ".vertical";
        public const string End = ".end";
        public const string Callback = ".callback";

        public static readonly string[] CommandList = {
            Class,
            Horizontal,
            Vertical,
            End,
            Callback,
        };
    }
}
