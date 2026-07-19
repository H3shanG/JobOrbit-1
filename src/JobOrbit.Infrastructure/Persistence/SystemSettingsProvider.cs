using System.Text.Json;
using JobOrbit.Application.DTOs.AdminSystemSettings;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace JobOrbit.Infrastructure.Persistence;

public sealed class SystemSettingsProvider(JobOrbitDbContext db, IMemoryCache cache) : ISystemSettingsProvider
{
    private const string CacheKey="JobOrbit.SystemSettings";
    private static readonly JsonSerializerOptions Json=new(JsonSerializerDefaults.Web);
    public async Task<SystemSettingsDto> GetAsync(CancellationToken token=default)
    {
        if(cache.TryGetValue(CacheKey,out SystemSettingsDto? hit)&&hit is not null)return hit;
        var rows=await db.SystemSettings.AsNoTracking().ToDictionaryAsync(x=>x.Key,x=>x.Value,token);
        T Read<T>(string key,T fallback){if(!rows.TryGetValue(key,out var value))return fallback;if(key==SystemSettingKeys.Notifications)value=value.Replace("\"enableEmailNotifications\"","\"enableNotifications\"",StringComparison.OrdinalIgnoreCase);return JsonSerializer.Deserialize<T>(value,Json)??fallback;}
        var result=new SystemSettingsDto(Read(SystemSettingKeys.General,SystemSettingDefaults.General),Read(SystemSettingKeys.Recruitment,SystemSettingDefaults.Recruitment),Read(SystemSettingKeys.Uploads,SystemSettingDefaults.Uploads),Read(SystemSettingKeys.Security,SystemSettingDefaults.Security),Read(SystemSettingKeys.Notifications,SystemSettingDefaults.Notifications),Read(SystemSettingKeys.Maintenance,SystemSettingDefaults.Maintenance));
        cache.Set(CacheKey,result,TimeSpan.FromMinutes(10));return result;
    }
    public async Task<object> UpdateSectionAsync(string section,object value,int actor,CancellationToken token=default)
    {
        var key=SystemSettingKeys.Sections[section];
        await using var tx=db.Database.IsRelational()?await db.Database.BeginTransactionAsync(token):null;
        var row=await db.SystemSettings.SingleAsync(x=>x.Key==key,token);row.Value=JsonSerializer.Serialize(value,value.GetType(),Json);row.UpdatedByUserId=actor;await db.SaveChangesAsync(token);if(tx is not null)await tx.CommitAsync(token);cache.Remove(CacheKey);return value;
    }
    public async Task<object?> ResetSectionAsync(string section,int actor,CancellationToken token=default)
    { if(!SystemSettingKeys.Sections.ContainsKey(section))return null;var d=Default(section);return await UpdateSectionAsync(section,d,actor,token); }
    public async Task SeedDefaultsAsync(CancellationToken token=default)
    {
        foreach(var pair in SystemSettingKeys.Sections){if(await db.SystemSettings.AnyAsync(x=>x.Key==pair.Value,token))continue;var value=Default(pair.Key);db.SystemSettings.Add(new SystemSetting{Key=pair.Value,Section=pair.Key,Value=JsonSerializer.Serialize(value,value.GetType(),Json),ValueType="json"});}
        await db.SaveChangesAsync(token);cache.Remove(CacheKey);
    }
    private static object Default(string s)=>s.ToLowerInvariant() switch{"general"=>SystemSettingDefaults.General,"recruitment"=>SystemSettingDefaults.Recruitment,"uploads"=>SystemSettingDefaults.Uploads,"security"=>SystemSettingDefaults.Security,"notifications"=>SystemSettingDefaults.Notifications,_=>SystemSettingDefaults.Maintenance};
}
