namespace HybridMicroOrm;

public class HybridMicroOrmOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string TableName { get; set; } = Constants.DefaultTableName;
}