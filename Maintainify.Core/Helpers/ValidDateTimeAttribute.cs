using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maintainify.Core.Helpers
{
    public class ValidDateTimeAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return false;
            }

            string dateTimeString = value as string;
            if (string.IsNullOrEmpty(dateTimeString))
            {
                return false;
            }

            return DateTime.TryParse(dateTimeString, out _);
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} is not in a valid datetime format.";
        }
    }
}
