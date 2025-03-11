using System.ComponentModel.DataAnnotations;

namespace TrustTrade.ViewModels;

public class CreatePostVM
{
    [Required(ErrorMessage = "Title is required.")]
    [MinLength(5, ErrorMessage = "Title must be at least 5 characters long.")]
    public string? Title { get; set; }

    [Required(ErrorMessage = "Content is required.")]
    [MinLength(10, ErrorMessage = "Content must be at least 10 characters long.")]
    public string? Content { get; set; }

    public List<string> ExistingTags { get; set; } = new List<string>();

    public List<string> SelectedTags { get; set; } = new List<string>();
}