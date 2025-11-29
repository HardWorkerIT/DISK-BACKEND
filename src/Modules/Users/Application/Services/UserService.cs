using Common.Abstracts;
using Common.DTOs;
using Common.Extensions;
using Common.Infrastructure.Abstracts;
using Common.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Users.Application.DTOs.Requests;
using Users.Application.Interfaces;
using Users.Application.Mappers;
using Users.Domain.Entities;

namespace Users.Application.Services;

public class UserService(IDbContext context) : IUserService
{
    private readonly IGenericRepository<User> _repository = new GenericRepository<User>(context);

    public async Task<User?> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        // Проверяем, что роль разрешена (Admin, Moderator или User)
        if (request.RoleId != 1 && request.RoleId != 2 && request.RoleId != 3)
        {
            throw new ArgumentException("Разрешены только роли Admin (1), Moderator (2) или User (3)");
        }

        // Проверяем, не существует ли уже пользователь с таким же логином
        User? existingUserByLogin = await _repository.FirstOrDefaultAsync(
            x => x.Login == request.Login && x.DeletedAt == default, cancellationToken);

        if (existingUserByLogin != null)
        {
            throw new ArgumentException("Пользователь с таким логином уже существует");
        }

        // Проверяем, не существует ли уже пользователь с таким же email
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            User? existingUserByEmail = await _repository.FirstOrDefaultAsync(
                x => x.Email == request.Email && x.DeletedAt == default, cancellationToken);

            if (existingUserByEmail != null)
            {
                throw new ArgumentException("Пользователь с таким email уже существует");
            }
        }

        // Проверяем обязательные поля
        if (string.IsNullOrWhiteSpace(request.Login))
        {
            throw new ArgumentException("Логин обязателен для заполнения");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Пароль обязателен для заполнения");
        }

        User user = UserMapper.GetModelWithPassword(request);

        if (string.IsNullOrWhiteSpace(user.Password))
        {
            throw new ArgumentException("Пароль не может быть пустым");
        }

        user.Password = EncryptExtension.EncryptSHA1(user.Password);

        return await _repository.CreateAsync(user, cancellationToken);
    }

    public async Task DeleteAsync(DeleteUserRequest request, CancellationToken cancellationToken)
    {
        // Запрещаем удаление пользователя с ID 1
        if (request.Id == 1)
        {
            throw new ArgumentException("Нельзя удалить пользователя с ID 1");
        }

        User? user = await _repository.FindByIdAsync(request.Id, cancellationToken);

        ArgumentNullException.ThrowIfNull(user);

        await _repository.RemoveAsync(user, cancellationToken);
    }

    public async Task<PagedResponse<User>> SelectAsync(GetUserRequest request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetWithCountAsync(request.PerInPage, request.Page, cancellationToken);
        return new PagedResponse<User>(items, totalCount, request.Page, request.PerInPage);
    }

    public async Task UpdateAsync(UpdateUserRequest request, CancellationToken cancellationToken)
    {
        // Получаем текущего пользователя
        User? currentUser = await _repository.FindByIdAsync(request.Id, cancellationToken);

        if (currentUser == null)
        {
            throw new ArgumentNullException(nameof(request.Id), "Пользователь не найден");
        }

        // Проверяем, не существует ли уже другой пользователь с таким же логином
        if (currentUser.Login != request.Login)
        {
            User? existingUserByLogin = await _repository.FirstOrDefaultAsync(
                x => x.Login == request.Login && x.Id != request.Id && x.DeletedAt == default, cancellationToken);

            if (existingUserByLogin != null)
            {
                throw new ArgumentException("Пользователь с таким логином уже существует");
            }
        }

        // Проверяем, не существует ли уже другой пользователь с таким же email
        if (!string.IsNullOrWhiteSpace(request.Email) && currentUser.Email != request.Email)
        {
            User? existingUserByEmail = await _repository.FirstOrDefaultAsync(
                x => x.Email == request.Email && x.Id != request.Id && x.DeletedAt == default, cancellationToken);

            if (existingUserByEmail != null)
            {
                throw new ArgumentException("Пользователь с таким email уже существует");
            }
        }

        User user = UserMapper.GetModelWithoutPassword(request);

        await _repository.UpdateAsync(user, cancellationToken);
    }
}