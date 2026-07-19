using System.ComponentModel.DataAnnotations;
using JobOrbit.Application.DTOs.Jobs;
namespace JobOrbit.Application.DTOs.HiringDecisions;
public sealed class HiringDecisionQuery { public string? Search{get;set;} public string? Decision{get;set;} public int? JobId{get;set;} public int Page{get;set;}=1; public int PageSize{get;set;}=10; public string Sort{get;set;}="newest"; }
public sealed class HiringDecisionListItemDto { public int ApplicationId{get;init;} public string CandidateName{get;init;}=""; public int JobId{get;init;} public string JobTitle{get;init;}=""; public decimal OverallScore{get;init;} public string Recommendation{get;init;}=""; public string Decision{get;init;}="Pending"; public DateTime? DecidedOn{get;init;} }
public sealed class HiringDecisionEvaluationDto { public int EvaluationId{get;init;} public decimal OverallScore{get;init;} public string Recommendation{get;init;}=""; public string EvaluatorName{get;init;}=""; public string? Comments{get;init;} public DateTime EvaluatedAt{get;init;} }
public sealed class HiringDecisionDetailsDto { public int ApplicationId{get;init;} public string CandidateName{get;init;}=""; public string? ProfessionalTitle{get;init;} public int JobId{get;init;} public string JobTitle{get;init;}=""; public string ApplicationStatus{get;init;}=""; public string? InterviewStatus{get;init;} public DateTime? InterviewDate{get;init;} public IReadOnlyList<HiringDecisionEvaluationDto> Evaluations{get;init;}=[]; public decimal OverallScore{get;init;} public string Recommendation{get;init;}=""; public string Decision{get;init;}="Pending"; public string? DecisionNotes{get;init;} public string? DecidedBy{get;init;} public DateTime? DecidedAt{get;init;} public bool IsFinal{get;init;} }
public class HiringDecisionRequest { [Required] public string Decision{get;init;}=""; [MaxLength(4000)] public string? Notes{get;init;} }
public sealed class CreateHiringDecisionRequest:HiringDecisionRequest;
public sealed class UpdateHiringDecisionRequest:HiringDecisionRequest;
public sealed class HiringFunnelDto { public int Shortlisted{get;init;} public int Interviewed{get;init;} public int Evaluated{get;init;} public int Held{get;init;} public int Hired{get;init;} public int Rejected{get;init;} }
public enum HiringDecisionMutationOutcome { Success,NotFound,NoEvaluation,NoInterview,DuplicateFinal,InvalidTransition,InvalidDecision }
public sealed record HiringDecisionMutationResult(HiringDecisionMutationOutcome Outcome,HiringDecisionDetailsDto? Decision=null);
