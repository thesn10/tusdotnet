using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tusdotnet.Controllers;
using tusdotnet.RequestHandlers.Validation;

namespace tusdotnet.RequestHandlers
{
    internal interface IRequestHandler
    {
        RequestRequirement[] Requires { get; }
        Task<ITusActionResult> Invoke();
    }
}
