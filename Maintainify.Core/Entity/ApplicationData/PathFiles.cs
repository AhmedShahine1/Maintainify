using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maintainify.Core.Entity.ApplicationData
{
    public class PathFiles
    {
        public string? Id { get; set; } 
        public string type { get; set; }
        public string Name { get; set; }
    }
}
