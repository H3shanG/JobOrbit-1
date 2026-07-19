using JobOrbit.Application.Auditing;namespace JobOrbit.Application.Interfaces;public interface IAuditService{Task WriteAsync(AuditEvent auditEvent,CancellationToken token=default);}
