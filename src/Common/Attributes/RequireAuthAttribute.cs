namespace Common.Attributes;

/// <summary>
/// јтрибут дл€ пометки контроллеров и действий, требующих авторизации
/// ѕримен€етс€ к контроллерам или отдельным действи€м
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireAuthAttribute : Attribute
{
}
