using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopService.Domain.Enums
{
    public enum WalletTransactionType
    {
        Withdraw,  //Rút tiền
        Deposit, //NẠp tiền
        Commission, //tiền từ order
        System //mua membership
    }
    public enum WalletTransactionStatus
    {
        Success,
        Failed,
        Pending,
        Canceled,
        Retry
    }
}
