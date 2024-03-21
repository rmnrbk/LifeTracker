using LifeTracker.Data;
using LifeTracker.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LifeTracker.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TagsController : Controller
{
    private readonly TagStoreContext _db;

    public TagsController(TagStoreContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<Tag>>> GetTags()
    {
        var options = new JsonSerializerOptions()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        var tags = await _db.Tags
            .Include(t => t.Parents)
            .Include(t => t.Activities)
            .ToListAsync();

        return Json(tags, options);
    }
        
    // GET: api/Tags/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Tag>> GetTag(int id)
    {
        if (_db.Tags == null)
        {
            return NotFound();
        }
        var tag = await _db.Tags.FindAsync(id);

        if (tag == null)
        {
            return NotFound();
        }

        return tag;
    }
        
    // POST: api/tags
    [HttpPost]
    public async Task<ActionResult<Tag>> PostTag(Tag tag)
    {
        if (_db.Tags == null)
        {
            return Problem("Entity set 'TagStoreContext.Tags'  is null.");
        }

        var parentIds = tag.Parents.Select(t => t.TagId).ToList();
        var newTag = new Tag
        {
            Name = tag.Name,
            Parents = _db.Tags
                .Where(t => parentIds.Contains(t.TagId))
                .ToList(),
        };
            
        _db.Tags.Add(newTag);
        await _db.SaveChangesAsync();

        return CreatedAtAction("GetTag", new { id = tag.TagId }, tag);
    }
    
    // PUT: api/Tags/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutTag(int id, Tag tag)
    {
        if (id != tag.TagId)
        {
            return BadRequest();
        }
        
        var existingTag = await _db.Tags
            .Include(t => t.Parents)
            .Include(t => t.Children)
            .FirstOrDefaultAsync(t => t.TagId == id);

        if (existingTag == null)
        {
            return NotFound();
        }
        
        // _db.Entry(tag).State = EntityState.Modified;
        _db.Entry(existingTag).CurrentValues.SetValues(tag);
        
        // Update relationships (Parents) manually
        existingTag.Parents.Clear(); // Clear existing parents
        foreach (var parent in tag.Parents)
        {
            var parentTag = await _db.Tags.FindAsync(parent.TagId);
            if (parentTag != null)
            {
                existingTag.Parents.Add(parentTag);
            }
        }
        
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TagExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }
    
    // DELETE: api/Tags/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTag(int id)
    {
        if (_db.Tags == null)
        {
            return NotFound();
        }

        var tag = _db.Tags
            .Include(t => t.Parents)
            .Include(t => t.Children)
            // .Include(t => t.Activities)
            .ToList()
            .Find(t => t.TagId == id);
        
        if (tag == null)
        {
            return NotFound();
        }
        
        var parentIdsOfDeleted = tag.Parents
            .Select(p => p.TagId)
            .ToList();
        
        var childrenIdsOfDeleted = tag.Children
            .Select(t => t.TagId)
            .ToList();

        var existingChildren = _db.Tags
            .Include(t => t.Parents)
            .Where(t => childrenIdsOfDeleted.Contains(t.TagId))
            .ToList();

        foreach (var existingChild in existingChildren)
        {
            existingChild.Parents.Remove(tag);
            existingChild.Parents.AddRange(tag.Parents);
        }
        
        var existingParents = _db.Tags
            .Include(t => t.Children)
            .Where(t => parentIdsOfDeleted.Contains(t.TagId))
            .ToList();
        
        foreach (var existingParent in existingParents)
        {
            existingParent.Children.Remove(tag);
            existingParent.Children.AddRange(tag.Children);
        }

        _db.Tags.Remove(tag);
        await _db.SaveChangesAsync();

        return NoContent();
    }
    
    private bool TagExists(int id)
    {
        return (_db.Tags?.Any(e => e.TagId == id)).GetValueOrDefault();
    }
}