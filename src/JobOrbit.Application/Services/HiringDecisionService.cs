using JobOrbit.Application.DTOs.HiringDecisions;using JobOrbit.Application.DTOs.Jobs;using JobOrbit.Application.Interfaces;using JobOrbit.Domain.Enums;
namespace JobOrbit.Application.Services;
public sealed class HiringDecisionService(IHiringDecisionRepository repository):IHiringDecisionService
{
 public Task<PagedResultDto<HiringDecisionListItemDto>>ListAsync(int id,HiringDecisionQuery q,CancellationToken t=default){q.Page=Math.Max(1,q.Page);q.PageSize=Math.Clamp(q.PageSize,1,50);return repository.ListAsync(id,q,t);}public Task<HiringDecisionDetailsDto?>DetailsAsync(int id,int app,CancellationToken t=default)=>repository.DetailsAsync(id,app,t);
 public Task<HiringDecisionMutationResult>CreateAsync(int id,int app,CreateHiringDecisionRequest r,CancellationToken t=default)=>Enum.TryParse<ManagerHiringDecision>(r.Decision,true,out var d)?repository.CreateAsync(id,app,d,r.Notes?.Trim(),t):Task.FromResult(new HiringDecisionMutationResult(HiringDecisionMutationOutcome.InvalidDecision));
 public Task<HiringDecisionMutationResult>UpdateAsync(int id,int app,UpdateHiringDecisionRequest r,CancellationToken t=default)=>Enum.TryParse<ManagerHiringDecision>(r.Decision,true,out var d)?repository.UpdateAsync(id,app,d,r.Notes?.Trim(),t):Task.FromResult(new HiringDecisionMutationResult(HiringDecisionMutationOutcome.InvalidDecision));
 public Task<HiringFunnelDto>FunnelAsync(int id,CancellationToken t=default)=>repository.FunnelAsync(id,t);
}
