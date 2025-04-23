namespace TrustTrade.ViewModels;
using System.ComponentModel.DataAnnotations;

public class ContactSupportViewModel
{
    [Required(ErrorMessage = "Please enter your name.")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Please enter a message.")]
    public string Message { get; set; }

    [Required(ErrorMessage = "Please select a category.")]
    public string Tag { get; set; }
}

