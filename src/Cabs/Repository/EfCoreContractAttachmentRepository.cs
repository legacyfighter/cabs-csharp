using LegacyFighter.Cabs.Entity;
using Microsoft.EntityFrameworkCore;

namespace LegacyFighter.Cabs.Repository;

public interface IContractAttachmentRepository
{
  Task<ISet<ContractAttachment>> FindByContractId(long? contractId);
  Task<ContractAttachment> Find(long? attachmentId);
  Task<ContractAttachment> Save(ContractAttachment contractAttachment);
  Task DeleteById(long? attachmentId);
}

internal class EfCoreContractAttachmentRepository : IContractAttachmentRepository
{
  private readonly SqLiteDbContext _context;

  public EfCoreContractAttachmentRepository(SqLiteDbContext context)
  {
    _context = context;
  }

  public async Task<ISet<ContractAttachment>> FindByContractId(long? contractId)
  {
    return new HashSet<ContractAttachment>(
      await _context.ContractAttachments.Where(a => a.Contract.Id == contractId).ToListAsync());
  }

  public async Task<ContractAttachment> Find(long? attachmentId)
  {
    return await _context.ContractAttachments.FindAsync(attachmentId);
  }

  public async Task<ContractAttachment> Save(ContractAttachment contractAttachment)
  {
    _context.ContractAttachments.Update(contractAttachment);
    await _context.SaveChangesAsync();
    return contractAttachment;
  }

  public async Task DeleteById(long? attachmentId)
  {
    _context.ContractAttachments.Remove(await Find(attachmentId));
    await _context.SaveChangesAsync();
  }
}