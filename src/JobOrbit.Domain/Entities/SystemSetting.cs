using JobOrbit.Domain.Common;

namespace JobOrbit.Domain.Entities;

public sealed class SystemSetting : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string ValueType { get; set; } = "json";
    public int? UpdatedByUserId { get; set; }
    public User? UpdatedByUser { get; set; }
}
