using Azure.Core;
using System.ComponentModel.DataAnnotations;

namespace Common.Abstracts;

public abstract class BaseRequest : IRequest
{
    public virtual bool IsValid(out List<ValidationResult> validationResults)
    {
        validationResults = new List<ValidationResult>();
        var context = new ValidationContext(this);

        // Валидация самого объекта
        var isValid = Validator.TryValidateObject(this, context, validationResults, true);

        // Валидация вложенных объектов
        var properties = GetType().GetProperties();
        foreach (var property in properties)
        {
            var value = property.GetValue(this);
            if (value != null)
            {
                var nestedResults = new List<ValidationResult>();
                var nestedContext = new ValidationContext(value);

                if (!Validator.TryValidateObject(value, nestedContext, nestedResults, true))
                {
                    foreach (var result in nestedResults)
                    {
                        var memberNames = result.MemberNames.Select(name => $"{property.Name}.{name}");
                        validationResults.Add(new ValidationResult(result.ErrorMessage, memberNames));
                    }
                }
            }
        }

        return isValid && !validationResults.Any();
    }

    public virtual Dictionary<string, string[]> GetValidationErrors()
    {
        var errors = new Dictionary<string, List<string>>();

        if (IsValid(out var validationResults))
            return new Dictionary<string, string[]>();

        foreach (var result in validationResults)
        {
            var fieldName = result.MemberNames.FirstOrDefault() ?? "General";
            if (!errors.ContainsKey(fieldName))
                errors[fieldName] = new List<string>();

            errors[fieldName].Add(result.ErrorMessage ?? "Ошибка валидации");
        }

        // Конвертируем List<string> в string[]
        var stringArrayErrors = errors.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToArray()
        );

        return stringArrayErrors;
    }
}