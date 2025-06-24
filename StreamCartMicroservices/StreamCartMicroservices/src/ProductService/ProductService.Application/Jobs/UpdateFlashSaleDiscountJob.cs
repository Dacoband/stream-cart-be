using ProductService.Application.Interfaces;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Infrastructure.Jobs
{
    public class UpdateFlashSaleDiscountJob : IJob
    {
        private readonly IFlashSaleJobService _flashSaleJobService;

        public UpdateFlashSaleDiscountJob(IFlashSaleJobService flashSaleJobService)
        {
            _flashSaleJobService = flashSaleJobService;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            await _flashSaleJobService.UpdateDiscountPricesAsync();
        }
    }
}
