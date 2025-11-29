using Common.Abstracts;
using Common.Constants;
using Common.DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Standards.Presentation.Controllers;

[ApiController]
[Route("")]
[ApiExplorerSettings(GroupName = "Health")]
public class CHealth : BaseController
{
    public CHealth() { }

    [HttpGet]
    public ActionResult<string> Get()
    {
        return "OK";
    }
}