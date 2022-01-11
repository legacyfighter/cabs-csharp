using LegacyFighter.Cabs.Entity;
using Microsoft.EntityFrameworkCore;

namespace LegacyFighter.Cabs.Repository;

public interface IContractAttachmentRepository
{
  Task<List<ContractAttachment>> FindByContract(Contract contract);
  Task<ContractAttachment> Find(long? attachmentId);
  Task Save(ContractAttachment contractAttachment);
  Task DeleteById(long? attachmentId);
}

internal class EfCoreContractAttachmentRepository : IContractAttachmentRepository
{
  private readonly SqLiteDbContext _context;

  public EfCoreContractAttachmentRepository(SqLiteDbContext context)
  {
    _context = context;
  }

  public async Task<List<ContractAttachment>> FindByContract(Contract contract)
  {
    return await _context.ContractAttachments.Where(a => a.Contract == contract).ToListAsync();
  }

  public async Task<ContractAttachment> Find(long? attachmentId)
  {
    return await _context.ContractAttachments.FindAsync(attachmentId);
  }

  public async Task Save(ContractAttachment contractAttachment)
  {
    _context.ContractAttachments.Update(contractAttachment);
    await _context.SaveChangesAsync();
  }

  public async Task DeleteById(long? attachmentId)
  {
    _context.ContractAttachments.Remove(await Find(attachmentId));
    await _context.SaveChangesAsync();
  }
}