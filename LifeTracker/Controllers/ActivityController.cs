using System.Text.Json;
using System.Text.Json.Serialization;
using LifeTracker.Data;
using LifeTracker.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeTracker.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ActivityController : Controller
{
    private readonly TagStoreContext _db;

    public ActivityController(TagStoreContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<Activity>>> GetActivities()
    {
        if (_db.Activities == null)
        {
            return NotFound();
        }
        
        var options = new JsonSerializerOptions()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };
        
        var activities = await _db.Activities
            .Include(a => a.Tag)
            .ToListAsync();
        
        return Json(activities, options);
    }
        
    // GET: api/Activity
    [HttpGet("{id}")]
    public async Task<ActionResult<Activity>> GetActivity(int id)
    {
        if (_db.Activities == null)
        {
            return NotFound();
        }
        var activity = await _db.Activities.FindAsync(id);

        if (activity == null)
        {
            return NotFound();
        }

        return activity;
    }
        
    // PUT: api/Activity/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutActivity(int id, Activity activity)
    {
        if (id != activity.ActivityId)
        {
            return BadRequest();
        }

        _db.Entry(activity).State = EntityState.Modified;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ActivityExists(id))
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
        
    // POST: api/Activity
    [HttpPost]
    public async Task<ActionResult<Activity>> PostActivity(Activity activity)
    {
        if (_db.Activities == null)
        {
            return Problem("Entity set 'TagStoreContext.Activities'  is null.");
        }
        _db.Activities.Add(activity);
        await _db.SaveChangesAsync();

        return CreatedAtAction("GetActivity", new { id = activity.ActivityId }, activity);
    }

    // DELETE: api/Activity/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteActivity(int id)
    {
        if (_db.Activities == null)
        {
            return NotFound();
        }
        var activity = await _db.Activities.FindAsync(id);
        if (activity == null)
        {
            return NotFound();
        }

        _db.Activities.Remove(activity);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private bool ActivityExists(int id)
    {
        return (_db.Activities?.Any(e => e.ActivityId == id)).GetValueOrDefault();
    }
}