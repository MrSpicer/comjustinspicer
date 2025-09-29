using Microsoft.EntityFrameworkCore;
using comjustinspicer.Data;
using comjustinspicer.Data.Models.Blog;

namespace comjustinspicer.Data.Models.Blog;

/// <summary>
/// Thin service providing basic CRUD operations for blog posts.
/// This is intentionally small and forwards calls to <see cref="BlogContext"/>.
/// </summary>
public sealed class PostService : IPostService
{
    private readonly BlogContext _db;

    public PostService(BlogContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<List<PostDTO>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Posts
            .AsNoTracking()
            .OrderByDescending(p => p.Id)
            .ToListAsync(ct);
    }

    public async Task<PostDTO?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Posts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<PostDTO> CreateAsync(PostDTO post, CancellationToken ct = default)
    {
        if (post == null) throw new ArgumentNullException(nameof(post));

        if (post.Id == Guid.Empty)
            post.Id = Guid.NewGuid();

        post.CreationDate = DateTime.UtcNow;
        post.ModificationDate = DateTime.UtcNow;
        post.PublicationDate = post.PublicationDate == default ? DateTime.UtcNow : post.PublicationDate;

        _db.Posts.Add(post);
        await _db.SaveChangesAsync(ct);

        return post;
    }

    public async Task<bool> UpdateAsync(PostDTO post, CancellationToken ct = default)
    {
        if (post == null) throw new ArgumentNullException(nameof(post));

        var existing = await _db.Posts.FirstOrDefaultAsync(p => p.Id == post.Id, ct);
        if (existing == null) return false;

        // minimal property updates
        existing.Title = post.Title;
        existing.Body = post.Body;
        existing.PublicationDate = post.PublicationDate;
        existing.AuthorName = post.AuthorName;
        existing.ModificationDate = DateTime.UtcNow;

        _db.Posts.Update(existing);
        await _db.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var existing = await _db.Posts.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (existing == null) return false;

        _db.Posts.Remove(existing);
        await _db.SaveChangesAsync(ct);

        return true;
    }
}
