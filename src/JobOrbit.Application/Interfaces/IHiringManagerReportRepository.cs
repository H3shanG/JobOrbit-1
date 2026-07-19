using JobOrbit.Application.DTOs.HiringManagerReports;
namespace JobOrbit.Application.Interfaces;public interface IHiringManagerReportRepository{Task<HiringManagerReportDataDto?>GetAsync(int userId,HiringManagerReportFilter filter,CancellationToken token=default);}
