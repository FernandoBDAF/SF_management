namespace SFManagement;

public class UserResolverService
{
    private readonly IHttpContextAccessor _context;

    public UserResolverService(IHttpContextAccessor context)
    {
        _context = context;
    }

    public string GetUserId()
    {
        if (_context != null) return _context.HttpContext.User.Identity.Name;

        return string.Empty;
    }
}