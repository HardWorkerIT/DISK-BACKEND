using System.Linq;

namespace Common.Attributes;

/// <summary>
/// Роли пользователей
/// Соответствуют значениям из Users.Domain.Enums.EUserRoles
/// </summary>
public enum EUserRoles
{
    Admin = 1,
    Moderator = 2,
    User = 3
}

/// <summary>
/// Атрибут для пометки контроллеров и действий, требующих определенных ролей
/// Применяется к контроллерам или отдельным действиям
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireRoleAttribute : Attribute
{
    /// <summary>
    /// Роли, которым разрешен доступ к endpoint
    /// </summary>
    public EUserRoles[] AllowedRoles { get; }

    /// <summary>
    /// ID ролей для внутреннего использования
    /// </summary>
    internal int[] AllowedRoleIds => AllowedRoles.Select(r => (int)r).ToArray();

    /// <summary>
    /// Конструктор атрибута
    /// </summary>
    /// <param name="roles">Роли, которым разрешен доступ</param>
    public RequireRoleAttribute(params EUserRoles[] roles)
    {
        AllowedRoles = roles ?? throw new ArgumentNullException(nameof(roles));

        if (AllowedRoles.Length == 0)
        {
            throw new ArgumentException("Необходимо указать хотя бы одну роль", nameof(roles));
        }
    }
}