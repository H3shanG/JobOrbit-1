using JobOrbit.Application.DTOs.AdminSystemSettings;
using JobOrbit.Application.DTOs.Jobs;
using JobOrbit.Application.DTOs.Notifications;
using JobOrbit.Application.Interfaces;
using JobOrbit.Application.Services;
using JobOrbit.Domain;
using JobOrbit.Domain.Entities;

namespace JobOrbit.Tests;

public sealed class NotificationServiceTests
{
    [Fact] public async Task CreateAsync_StoresControlledNotificationWithSafeInternalUrl()
    { var repo=new FakeRepository();var service=new NotificationService(repo,new FakeSettings());var created=await service.CreateAsync(new(7,NotificationTypes.ApplicationSubmitted,"Submitted","Application submitted",ActionUrl:"/candidate/applications/3",EventKey:"app:3"));Assert.True(created);Assert.Equal("/candidate/applications/3",repo.Added!.ActionUrl); }
    [Fact] public async Task CreateAsync_RemovesUnsafeExternalActionUrl()
    { var repo=new FakeRepository();var service=new NotificationService(repo,new FakeSettings());await service.CreateAsync(new(7,NotificationTypes.ApplicationSubmitted,"Submitted","Application submitted",ActionUrl:"https://evil.test"));Assert.Null(repo.Added!.ActionUrl); }
    [Fact] public async Task CreateAsync_DoesNothingWhenNotificationsAreGloballyDisabled()
    { var repo=new FakeRepository();var service=new NotificationService(repo,new FakeSettings(false));Assert.False(await service.CreateAsync(new(7,NotificationTypes.ApplicationSubmitted,"Submitted","Application submitted")));Assert.Null(repo.Added); }
    [Fact] public async Task CreateAsync_DeduplicatesEventKeyPerRecipient()
    { var repo=new FakeRepository{Duplicate=true};var service=new NotificationService(repo,new FakeSettings());Assert.False(await service.CreateAsync(new(7,NotificationTypes.ApplicationSubmitted,"Submitted","Application submitted",EventKey:"app:3")));Assert.Null(repo.Added); }
    [Fact] public async Task ListAsync_ClampsUnsafePaginationValues()
    { var repo=new FakeRepository();var service=new NotificationService(repo,new FakeSettings());var query=new NotificationQuery{Page=-2,PageSize=1000};await service.ListAsync(7,query);Assert.Equal(1,query.Page);Assert.Equal(100,query.PageSize); }

    private sealed class FakeRepository:INotificationRepository
    {
        public Notification? Added{get;private set;} public bool Duplicate{get;init;}
        public Task AddAsync(Notification n,CancellationToken t){Added=n;return Task.CompletedTask;} public Task<bool> DeleteAsync(int u,int n,CancellationToken t)=>Task.FromResult(true);public Task<bool> EventExistsAsync(int u,string e,CancellationToken t)=>Task.FromResult(Duplicate);public Task<PagedResultDto<NotificationListItemDto>> ListAsync(int u,NotificationQuery q,CancellationToken t)=>Task.FromResult(new PagedResultDto<NotificationListItemDto>{Page=q.Page,PageSize=q.PageSize});public Task<int> MarkAllReadAsync(int u,CancellationToken t)=>Task.FromResult(0);public Task<bool> MarkReadAsync(int u,int n,CancellationToken t)=>Task.FromResult(true);public Task<bool> RecipientIsActiveAsync(int u,CancellationToken t)=>Task.FromResult(true);public Task<int> UnreadCountAsync(int u,CancellationToken t)=>Task.FromResult(0);
    }
    private sealed class FakeSettings(bool enabled=true):ISystemSettingsProvider
    {
        public Task<SystemSettingsDto> GetAsync(CancellationToken t=default)=>Task.FromResult(new SystemSettingsDto(SystemSettingDefaults.General,SystemSettingDefaults.Recruitment,SystemSettingDefaults.Uploads,SystemSettingDefaults.Security,SystemSettingDefaults.Notifications with{EnableNotifications=enabled},SystemSettingDefaults.Maintenance));
        public Task<object?> ResetSectionAsync(string s,int u,CancellationToken t=default)=>Task.FromResult<object?>(null);public Task SeedDefaultsAsync(CancellationToken t=default)=>Task.CompletedTask;public Task<object> UpdateSectionAsync(string s,object v,int u,CancellationToken t=default)=>Task.FromResult(v);
    }
}
