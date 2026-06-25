using System.ComponentModel.DataAnnotations;

namespace MiniHelpdesk.Web.ViewModels;

public class CreateTicketViewModel
{
    [Required(ErrorMessage = "Tytuł jest wymagany.")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Autor komentarza jest wymagany.")]
    [MaxLength(100)]
    public string FirstCommentAuthor { get; set; } = string.Empty;

    [Required(ErrorMessage = "Treść pierwszego komentarza jest wymagana.")]
    [MaxLength(2000)]
    public string FirstCommentContent { get; set; } = string.Empty;
}
