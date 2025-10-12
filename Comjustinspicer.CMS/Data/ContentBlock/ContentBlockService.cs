using Microsoft.EntityFrameworkCore;
using Comjustinspicer.CMS.Data.ContentBlock.Models;
using Comjustinspicer.CMS.Data.DbContexts;

namespace Comjustinspicer.CMS.Data.ContentBlock;

public sealed class ContentBlockService : IContentBlockService
{
	private readonly ContentBlockContext _db;

	public ContentBlockService(ContentBlockContext db)
	{
		_db = db ?? throw new ArgumentNullException(nameof(db));
	}

	public async Task<List<ContentBlockDTO>> GetAllAsync(CancellationToken ct = default)
	{
		return await _db.ContentBlocks
			.AsNoTracking()
			.OrderByDescending(c => c.Id)
			.ToListAsync(ct);
	}

	public async Task<ContentBlockDTO?> GetByIdAsync(Guid id, CancellationToken ct = default)
	{
		return await _db.ContentBlocks
			.AsNoTracking()
			.FirstOrDefaultAsync(c => c.Id == id, ct);
	}

	public async Task<bool> UpsertAsync(ContentBlockDTO contentBlock, CancellationToken ct = default)
	{
		if (contentBlock == null) throw new ArgumentNullException(nameof(contentBlock));

		contentBlock.ModificationDate = DateTime.UtcNow;

		if (contentBlock.Id == Guid.Empty)
		{
			// create
			contentBlock.Id = Guid.NewGuid();
			contentBlock.CreationDate = DateTime.UtcNow;

			_db.ContentBlocks.Add(contentBlock);
			await _db.SaveChangesAsync(ct);

			return true;
		}

		// update
		var existing = await _db.ContentBlocks.FirstOrDefaultAsync(c => c.Id == contentBlock.Id, ct);
		if (existing == null)
		{
			// create
			contentBlock.CreationDate = DateTime.UtcNow;

			_db.ContentBlocks.Add(contentBlock);
		}
		else
		{
			existing.Content = contentBlock.Content;
			existing.ModificationDate = contentBlock.ModificationDate;

			_db.ContentBlocks.Update(existing);
		}

		await _db.SaveChangesAsync(ct);

		return true;
	}

	public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
	{
		var existing = await _db.ContentBlocks.FirstOrDefaultAsync(c => c.Id == id, ct);
		if (existing == null) return false;

		_db.ContentBlocks.Remove(existing);
		await _db.SaveChangesAsync(ct);

		return true;
	}
}
