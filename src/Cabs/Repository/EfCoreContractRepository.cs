using LegacyFighter.Cabs.Entity;
using Microsoft.EntityFrameworkCore;

namespace LegacyFighter.Cabs.Repository;

public interface IContractRepository
{
  Task<List<Contract>> FindByPartnerName(string partnerName);
  Task<Contract> FindByAttachmentId(long? attachmentId);
  Task<Guid> FindContractAttachmentNoById(long? attachmentId);
  Task<Contract> Save(Contract contract);
  Task<Contract> Find(long? id);
}

internal class EfCoreContractRepository : IContractRepository
{
  private readonly SqLiteDbContext _context;

  public EfCoreContractRepository(SqLiteDbContext context)
  {
    _context = context;
  }

  public async Task<List<Contract>> FindByPartnerName(string partnerName)
  {
    return await _context.Contracts.Where(c => c.PartnerName == partnerName).ToListAsync();
  }

  public async Task<Contract> FindByAttachmentId(long? attachmentId)
  {
    return await _context.Contracts.FromSqlInterpolated(
      $"SELECT * FROM Contracts c JOIN ContractAttachments ca ON ca.ContractId = c.id WHERE ca.Id = {attachmentId}")
      .SingleOrDefaultAsync();
  }

  public async Task<Guid> FindContractAttachmentNoById(long? attachmentId)
  {
    return await _context.ContractAttachments
      .Where(c => c.Id == attachmentId)
      .Select(c => c.ContractAttachmentNo)
      .SingleAsync();
  }

  public async Task<Contract> Save(Contract contract)
  {
    _context.Contracts.Update(contract);
    await _context.SaveChangesAsync();
    return contract;
  }

  public async Task<Contract> Find(long? id)
  {
    return await _context.Contracts.FindAsync(id);
  }
}