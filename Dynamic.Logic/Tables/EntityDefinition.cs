using Dynamic.Core.Data;
namespace Dynamic.Logic;
public class EntityDefinition : IStorable
{
    public EntityDefinition(string id, string entityName, string description, string rolePermissionsRequiredRead, string rolePermissionsRequiredWrite, Dictionary<string, Field<object>> entityFields)
    {
        Id = id;
        EntityName = entityName;
        Description = description;
        RolePermissionsRequiredRead = rolePermissionsRequiredRead;
        RolePermissionsRequiredWrite = rolePermissionsRequiredWrite;
        EntityFields = entityFields;
    }

    public string Id { get; set; }
    public string EntityName { get; set; }
    public string Description { get; set; }
    public string RolePermissionsRequiredRead { get; set; }
    public string RolePermissionsRequiredWrite { get; set; }
    public Dictionary<string, Field<object>> EntityFields { get; set; }

}