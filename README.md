# DNX
A simple, lightweight and fast ORM framework.

> Demos

Config database connection string configuration in Web.config/App.config

```
<connectionStrings>
    <add providerName="System.Data.SqlClient" name="DNX.Models" connectionString="Data Source=localhost;Initial Catalog={database};User ID=sa;Password={password};" />
</connectionStrings>
```
> models template

```
using System;
using System.Runtime.Serialization;
using DNX.Data.Attributes;

namespance DNX.Models
{
    [Serializable]
    [DataContract]
    [DataSchema(DataSource = "DNX.Models", Name = "test_table", Schema = "dbo")]
    public class TEST_TABLE
    {
        /// <summary>
        /// ID
        /// </summary>
        [DataMember]
        [DataProperty(
            Name = "id", Description = "ID", PrimaryKey = true, Identity =  false,
            NativeType = "nvarchar", Length = 32, AllowNull = false, DefaultValue = "", Scale = 0,
            BindingFlag = BindingFlagType.All)]
        public string id 
        {
            get { return mid; }
            set { mid = value; }
        } private string mid = String.Empty;
        
        /// <summary>
        /// bucket
        /// </summary>
        [DataMember]
        [DataProperty(
            Name = "bucket", Description = "bucket", PrimaryKey = false, Identity =  false,
            NativeType = "nvarchar", Length = 128, AllowNull = true, DefaultValue = "", Scale = 0,
            BindingFlag = BindingFlagType.All)]
        public string bucket 
        {
            get { return mbucket; }
            set { mbucket = value; }
        } private string mbucket = String.Empty;
    }
}
```

> Insert Model

```
DNX.Models.TEST_TABLE table = new DNX.Models.TEST_TABLE();
table.ID = Guid.NewGuid().ToString("N");
table.bucket = "test";

Result result = DNX.Data.DbAccess.DbDataModelAdapter.Instance.InsertModels(table);

if(!result.Success)
    throw new Exception(result.ErrorExplanation);
```
