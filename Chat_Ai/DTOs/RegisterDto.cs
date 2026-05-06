using System.ComponentModel.DataAnnotations;

namespace Chat_Ai.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "الاسم مطلوب")]
        [MaxLength(100, ErrorMessage = "الاسم لا يمكن أن يتجاوز 100 حرف")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صالح")]
        [MaxLength(100, ErrorMessage = "البريد الإلكتروني لا يمكن أن يتجاوز 100 حرف")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [MinLength(6, ErrorMessage = "كلمة المرور يجب أن تكون 6 أحرف على الأقل")]
        [MaxLength(100, ErrorMessage = "كلمة المرور لا يمكن أن تتجاوز 100 حرف")]
        public string Password { get; set; } = null!;
    }
}
