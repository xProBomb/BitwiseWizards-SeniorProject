using System.ComponentModel.DataAnnotations;

namespace TrustTrade.ViewModels;

public class CreatePostVM
{
    [Display(Name = "Title")]
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(128, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 128 characters long.")]
    public string? Title { get; set; }

    [Display(Name = "Content")]
    [Required(ErrorMessage = "Content is required.")]
    [StringLength(1024, MinimumLength = 10, ErrorMessage = "Content must be between 10 and 1024 characters long.")]
    public string? Content { get; set; }

    [Display(Name = "Privacy Setting")]
    [Required(ErrorMessage = "Privacy setting is required.")]
    public bool? IsPublic { get; set; }

    // Tag properties
    [Display(Name = "Tags")]
    public List<string> ExistingTags { get; set; } = new List<string>();

    [MaxLength(5, ErrorMessage = "No more than 5 tags are allowed.")]
    public List<string> SelectedTags { get; set; } = new List<string>();
}