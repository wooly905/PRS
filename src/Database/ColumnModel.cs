namespace PRS.Database;

public class ColumnModel
{
    public string TableSchema { get; set; }
    public string TableName { get; set; }
    public string ColumnName { get; set; }
    public string OrdinalPosition {get;set;}
    public string ColumnDefault { get; set; }
    public string IsNullable { get; set; }
    public string DataType { get; set; }
    public string CharacterMaximumLength { get; set; }
}
