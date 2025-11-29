namespace Common.Abstracts;

public abstract class BasePerInPageRequest : BaseRequest
{
    public int PerInPage { get; set; } = 20;
    public int Page { get; set; } = 1;
}

