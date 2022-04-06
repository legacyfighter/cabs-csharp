using LegacyFighter.Cabs.Repository;
using Microsoft.EntityFrameworkCore;

namespace LegacyFighter.Cabs.Agreements;

public interface IContractAttachmentDataRepository
{
  Task<ISet<ContractAttachmentData>> FindByContractAttachmentNoIn(List<Guid> attachmentIds);
  Task<int> DeleteByAttachmentId(long? attachmentId);
  Task<ContractAttachmentData> Save(ContractAttachmentData contractAttachmentData);
}

internal class EfCoreContractAttachmentDataRepository : IContractAttachmentDataRepository
{
  private readonly SqLiteDbContext _context;

  public EfCoreContractAttachmentDataRepository(SqLiteDbContext context)
  {
    _context = context;
  }

  public async Task<ISet<ContractAttachmentData>> FindByContractAttachmentNoIn(List<Guid> attachmentIds)
  {
    return new HashSet<ContractAttachmentData>(await _context.ContractAttachmentsData.Where(d => attachmentIds.Contains(d.ContractAttachmentNo))
      .ToListAsync());
  }

  public async Task<int> DeleteByAttachmentId(long? attachmentId)
  {
    return await _context.Database
      .ExecuteSqlInterpolatedAsync(
        $"delete FROM ContractAttachmentsData AS cad WHERE cad.contractAttachmentNo = (SELECT ca.contractAttachmentNo FROM ContractAttachments ca WHERE ca.id = {attachmentId})");
  }

  public async Task<ContractAttachmentData> Save(ContractAttachmentData contractAttachmentData)
  {
    _context.ContractAttachmentsData.Update(contractAttachmentData);
    await _context.SaveChangesAsync();
    return contractAttachmentData;
  }
}