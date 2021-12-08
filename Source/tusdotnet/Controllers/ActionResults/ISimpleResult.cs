namespace tusdotnet.Controllers
{
    /// <summary>
    /// Has to be one of Ok(), BadRequest(), Forbidden(), or Unauthorized()
    /// </summary>
    public interface ISimpleResult : ITusActionResult
    {

    }
}