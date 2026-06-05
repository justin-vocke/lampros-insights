using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LamprosInsights.Application.Features.Analytics.Dtos
{
    public class SqlValidationResult
    {
        public List<string> Errors { get; } = [];
        public bool IsValid => Errors.Count == 0;
    }
}
