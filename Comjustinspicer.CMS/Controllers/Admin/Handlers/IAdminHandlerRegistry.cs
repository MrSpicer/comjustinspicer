namespace Comjustinspicer.CMS.Controllers.Admin.Handlers;

public interface IAdminHandlerRegistry
{
    IAdminCrudHandler? GetHandler(string contentType);
}
