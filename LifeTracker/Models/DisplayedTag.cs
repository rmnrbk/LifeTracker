using LifeTracker.Entity;

namespace LifeTracker.Models;

public class DisplayedTag
{
    private readonly System.Timers.Timer aTimer = new(1000);
    public Tag Tag { get; set; }
    
    public DateTime? LastStartTime { get; set; }
    public DateTime? LastEndTime { get; set; }
    public int LastSecondsSpent { get; set; }
    public int TotalSecondsSpent { get; set; }
    public int ChildrenActivated { get; set; }
    public DisplayedTagStatus Status { get; set; } = DisplayedTagStatus.Inactive;

    public static event Action OnChange;
    private static void NotifyStateChanged() => OnChange?.Invoke();

    public DisplayedTag()
    {
        aTimer.Elapsed += CountUpTimer;
    }

    private void CountUpTimer(Object source, System.Timers.ElapsedEventArgs e)
    {
        LastSecondsSpent += 1;
        TotalSecondsSpent += 1;

        NotifyStateChanged();
    }

    public void EnableTimer(bool resetLastSecondsSpent=true)
    {
        if (resetLastSecondsSpent)
            LastSecondsSpent = 0;
        aTimer.Enabled = true;
    }
    
    public void DisableTimer()
    {
        aTimer.Enabled = false;
    }
}

public enum DisplayedTagStatus
{
    Inactive,
    DependentlyDisabled,    // children of toggled tag
    DependentlyActive,      // parents of toggled tag
    DirectlyActive          // toggled tag
}