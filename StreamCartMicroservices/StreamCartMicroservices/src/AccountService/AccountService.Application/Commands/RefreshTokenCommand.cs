using AccountService.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Application.Commands
{
    public class RefreshTokenCommand : IRequest<AuthResultDto>
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
