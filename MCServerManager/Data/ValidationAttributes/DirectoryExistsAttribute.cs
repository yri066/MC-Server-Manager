using System.ComponentModel.DataAnnotations;

namespace MCServerManager.Data.ValidationAttributes
{
	public class DirectoryExistsAttribute : ValidationAttribute
	{
		public string GetErrorMessage() => "Указанная директория не найдена";

		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			if (!Directory.Exists((string)value!))
			{
				return new ValidationResult(GetErrorMessage());
			}

			return ValidationResult.Success;
		}
	}
}
