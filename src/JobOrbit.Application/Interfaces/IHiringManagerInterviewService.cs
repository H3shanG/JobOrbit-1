using JobOrbit.Application.DTOs.HiringManagerInterviews;
using JobOrbit.Application.DTOs.Jobs;
namespace JobOrbit.Application.Interfaces;
public interface IHiringManagerInterviewService
{
    Task<PagedResultDto<HiringManagerInterviewListItemDto>> ListAsync(int userId,HiringManagerInterviewQuery query,CancellationToken token=default);
    Task<HiringManagerInterviewDetailsDto?> DetailsAsync(int userId,int interviewId,CancellationToken token=default);
}
