using System.ComponentModel.DataAnnotations;

namespace TrustTrade.ViewModels;

public class CreatePostVM
{
    [Required]
    public string? Title { get; set; }

    [Required]
    public string? Content { get; set; }

    public List<string> ExistingTags { get; set; } = new List<string>();

    public List<string> SelectedTags { get; set; } = new List<string>();
}