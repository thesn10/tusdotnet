namespace tusdotnet.Controllers
{
    /// <summary>
    /// Has to be one of CreateStatus(), BadRequest(), or Forbidden()
    /// </summary>
    public interface ICreateResult : ITusActionResult
    {
    }
}