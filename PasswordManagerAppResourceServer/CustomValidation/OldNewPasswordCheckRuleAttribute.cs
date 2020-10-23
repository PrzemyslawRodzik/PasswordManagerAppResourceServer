
using PasswordManagerAppResourceServer.Models;
using System.ComponentModel.DataAnnotations;


namespace PasswordManagerAppResourceServer.CustomValidation
{
    public class OldNewPasswordCheckRuleAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            
            
            
            PasswordChangeRequest model = (PasswordChangeRequest)validationContext.ObjectInstance;
            string oldPassword = model.Password;
            string newPassword = (string)value;  
            if(oldPassword.Equals(newPassword))
                return new ValidationResult($"Selected password is the same as the previous one . Enter another one!", new[] { validationContext.MemberName });
            else
                return ValidationResult.Success;
        }   
        
        



    }
}