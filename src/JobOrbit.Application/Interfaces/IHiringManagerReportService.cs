using JobOrbit.Application.DTOs.HiringManagerReports;
namespace JobOrbit.Application.Interfaces;public interface IHiringManagerReportService{Task<HiringManagerReportDataDto?>GetAsync(int userId,HiringManagerReportFilter filter,CancellationToken token=default);}
