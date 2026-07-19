using JobOrbit.Application.DTOs.Jobs;using JobOrbit.Application.DTOs.RecruiterInterviews;
namespace JobOrbit.Application.Interfaces;
public interface IRecruiterInterviewRepository
{
 Task<(RecruiterInterviewOutcome Outcome,int? InterviewId)>CreateAsync(int userId,CreateInterviewRequest request,CancellationToken token=default);Task<PagedResultDto<RecruiterInterviewListItemDto>>ListAsync(int userId,RecruiterInterviewQuery query,CancellationToken token=default);Task<RecruiterInterviewDetailsDto?>DetailsAsync(int userId,int interviewId,CancellationToken token=default);Task<RecruiterInterviewOutcome>UpdateAsync(int userId,int interviewId,UpdateInterviewRequest request,CancellationToken token=default);Task<RecruiterInterviewOutcome>CancelAsync(int userId,int interviewId,CancellationToken token=default);Task<RecruiterInterviewOutcome>CompleteAsync(int userId,int interviewId,CancellationToken token=default);Task<IReadOnlyList<ShortlistedApplicationDto>>ShortlistedAsync(int userId,CancellationToken token=default);
}
