using System.ComponentModel.DataAnnotations;

namespace TrustTrade.ViewModels;

public class PostEditVM
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public bool IsPublic { get; set; }
    
    public List<string> AvailableTags { get; set; } = new List<string>();

    public List<string> SelectedTags { get; set; } = new List<string>();
}