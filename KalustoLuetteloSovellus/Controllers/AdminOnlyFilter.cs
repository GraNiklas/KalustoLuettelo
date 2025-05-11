using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;


public class AdminOnlyFilter : IActionFilter
{
    public void OnActionExecuted(ActionExecutedContext context)
    {
        
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var rooli = context.HttpContext.Session.GetString("Rooli");

        if (rooli != "Admin")
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
        }
    }
}
