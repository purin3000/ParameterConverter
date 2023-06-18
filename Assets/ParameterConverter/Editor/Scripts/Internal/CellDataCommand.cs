namespace ParameterConverter.Editor.Internal {
  public static class CellDataCommand {
    public const string CLASS = ".class";
    public const string HORIZONTAL = ".horizontal";
    public const string VERTICAL = ".vertical";
    public const string END = ".end";
    public const string CALLBACK = ".callback";

    public static readonly string[] COMMAND_LIST = {
      CLASS,
      HORIZONTAL,
      VERTICAL,
      END,
      CALLBACK
    };
  }
}