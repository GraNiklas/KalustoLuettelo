using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;


public class AdminOnlyFilter : IActionFilter
{
    public void OnActionExecuted(ActionExecutedContext context)
    {
        
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var rooliId = context.HttpContext.Session.GetInt32("RooliId");

        if (rooliId != 40000)
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
        }
    }
}
