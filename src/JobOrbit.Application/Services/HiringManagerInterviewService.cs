using JobOrbit.Application.DTOs.HiringManagerInterviews;using JobOrbit.Application.DTOs.Jobs;using JobOrbit.Application.Interfaces;
namespace JobOrbit.Application.Services;
public sealed class HiringManagerInterviewService(IHiringManagerInterviewRepository repository):IHiringManagerInterviewService
{
 public Task<PagedResultDto<HiringManagerInterviewListItemDto>>ListAsync(int userId,HiringManagerInterviewQuery query,CancellationToken token=default){query.Page=Math.Max(1,query.Page);query.PageSize=Math.Clamp(query.PageSize,1,50);query.Sort=string.IsNullOrWhiteSpace(query.Sort)?"upcoming":query.Sort;return repository.ListAsync(userId,query,token);}
 public Task<HiringManagerInterviewDetailsDto?>DetailsAsync(int userId,int interviewId,CancellationToken token=default)=>repository.DetailsAsync(userId,interviewId,token);
}
