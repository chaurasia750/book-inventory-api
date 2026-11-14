using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace BookInventory.BuildingBlocks.Configuration.Extensions
{
    /// <summary>
    /// Prepends a route prefix to every attribute-routed controller so controllers declare only their own short route
    /// (<c>[Route("auth")]</c>) and the full route becomes <c>api/v{version}/auth</c>.
    /// </summary>
    public sealed class RoutePrefixConvention(string prefix) : IApplicationModelConvention
    {
        private readonly AttributeRouteModel _prefix = new(new RouteAttribute(prefix));

        public void Apply(ApplicationModel application)
        {
            foreach (var selector in application.Controllers.SelectMany(c => c.Selectors))
            {
                selector.AttributeRouteModel = selector.AttributeRouteModel is null
                    ? _prefix
                    : AttributeRouteModel.CombineAttributeRouteModel(_prefix, selector.AttributeRouteModel);
            }
        }
    }
}
