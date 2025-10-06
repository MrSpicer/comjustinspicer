using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Comjustinspicer.Services;

public class UserService
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	public UserService(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public bool IsUserAuthor =>
		_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true &&
		(_httpContextAccessor.HttpContext.User.IsInRole("Admin") || _httpContextAccessor.HttpContext.User.IsInRole("Editor"));
}