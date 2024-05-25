using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maintainify.Core.Helpers
{
    public enum OrderStatus
    {
        Preparing, // تم التأكيد من العميل ولم يتم الموافقة عليه بعد من قبل مقدم الخدمة 
        Confirmed, // تم الموافقة عليه من قبل مقدم الخدمة
        WithDriver, // قيد الوصول
        Finished,  // منتهية 
        Cancelled, // ملغية
    }
}
