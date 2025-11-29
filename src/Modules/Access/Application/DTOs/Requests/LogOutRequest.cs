using Common.Abstracts;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Access.Application.DTOs.Requests;


[SwaggerSchema(Title = "LogOutRequest", Description = "Запрос для выхода из аккаунта")]
public class LogOutRequest() : BaseRequest { }