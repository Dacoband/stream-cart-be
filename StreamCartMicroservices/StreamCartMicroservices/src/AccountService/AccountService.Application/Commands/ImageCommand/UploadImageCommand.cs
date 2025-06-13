using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Application.Commands.ImageCommand
{
    public class UploadImageCommand : IRequest<string>
    {
        public UploadImageCommand() { }

        public UploadImageCommand(IFormFile image)
        {
            Image = image;
        }

        public IFormFile Image { get; set; } = default!;

    }
}
