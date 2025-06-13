using AccountService.Application.Commands.ImageCommand;
using MediatR;
using Shared.Common.Services.Appwrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Application.Handlers.ImageHandler
{
    public class UploadImageHandler : IRequestHandler<UploadImageCommand, string>
    {
        private readonly IAppwriteService _appwriteService;
        public UploadImageHandler(IAppwriteService appwriteService)
        {
            _appwriteService = appwriteService;
        }
        public async Task<string> Handle(UploadImageCommand request, CancellationToken cancellationToken)
        {
            return await _appwriteService.UploadImage(request.Image);
        }
    }
}
