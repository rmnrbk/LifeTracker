using LifeTracker.Entity;

namespace LifeTracker.Services;

public static class EdgesMapper
{
    public static List<(int, int)> MapTagsToEdges(List<Tag> tags)
    {
        HashSet<(int, int)> edges = new HashSet<(int, int)>();

        foreach (var tag in tags)
        {
            foreach (var parent in tag.Parents)
            {
                if (!edges.Contains((tag.TagId, parent.TagId)) && !edges.Contains((parent.TagId, tag.TagId)))
                    edges.Add((parent.TagId, tag.TagId));
            }

            foreach (var child in tag.Children)
            {
                if (!edges.Contains((tag.TagId, child.TagId)) && !edges.Contains((child.TagId, tag.TagId)))
                    edges.Add((tag.TagId, child.TagId));
            }

            if (tag.Parents.Count == 0 && tag.Children.Count == 0)
            {
                edges.Add((tag.TagId, tag.TagId));
            }
        }

        return edges.ToList();
    }

    // private static (int, int) SortedPair(int a, int b)
    // {
    //     return a < b ? (a, b) : (b, a);
    // }
    
}