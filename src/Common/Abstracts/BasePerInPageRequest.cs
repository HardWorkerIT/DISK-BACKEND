using System.ComponentModel.DataAnnotations;

namespace Common.Abstracts;

public abstract class BasePerInPageRequest : BaseRequest
{
    [Range(1, 100, ErrorMessage = "Размер страницы должен быть от 1 до 100")]
    public int PerInPage { get; set; } = 20;

    [Range(1, 1_000_000, ErrorMessage = "Номер страницы должен быть от 1 до 1000000")]
    public int Page { get; set; } = 1;
}

