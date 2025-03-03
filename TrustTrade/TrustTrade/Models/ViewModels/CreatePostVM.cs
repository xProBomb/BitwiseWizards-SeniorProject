using System.ComponentModel.DataAnnotations;

namespace TrustTrade.ViewModels;

public class CreatePostVM
{
    [Required]
    public required string Title { get; set; }

    [Required]
    public required string Content { get; set; }

    public List<string> ExistingTags { get; set; } = new List<string>();

    public List<string> SelectedTags { get; set; } = new List<string>();

    public List<string> NewTags { get; set; } = new List<string>();
}