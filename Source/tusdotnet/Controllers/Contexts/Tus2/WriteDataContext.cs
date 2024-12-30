using System.IO.Pipelines;

namespace tusdotnet.Controllers.Contexts.Tus2
{
    public class WriteDataContext : Tus2Context
    {
        public PipeReader BodyReader { get; set; }
    }
}
