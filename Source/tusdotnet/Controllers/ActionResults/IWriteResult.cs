namespace tusdotnet.Controllers
{
    /// <summary>
    /// Has to be one of WriteStatus(), BadRequest(), or Forbidden()
    /// </summary>
    public interface IWriteResult : ITusActionResult
    {
    }
}
