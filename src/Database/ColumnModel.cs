namespace PRS.Database;

public class ColumnModel
{
    public string TableName { get; set; }
    public string ColumnName { get; set; }
    public string OrdinalPosition {get;set;}
    public string ColumnDefault { get; set; }
    public string IsNullable { get; set; }
    public string DataType { get; set; }
    public string CharacterMaximumLength { get; set; }

    public bool IsPrimaryKey { get; set; }
    public bool IsUnique { get; set; }

    // Identity info
    public bool IsIdentity { get; set; }
    public string IdentitySeed { get; set; }
    public string IdentityIncrement { get; set; }

    // Foreign key info (optional). Empty/null when the column is not a foreign key.
    public string ForeignKeyName { get; set; }
    public string ReferencedTableName { get; set; }
    public string ReferencedColumnName { get; set; }
}
