using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Common.Services.Appwrite
{
    public interface IAppwriteService
    {
        Task<string> UploadImage(IFormFile image);
    }
}
