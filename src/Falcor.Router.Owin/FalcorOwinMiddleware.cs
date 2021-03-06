﻿using System.Threading.Tasks;
using Microsoft.Owin;

namespace Falcor.Router.Owin
{
    public class FalcorOwinMiddleware : OwinMiddleware
    {
        public FalcorOwinMiddleware(OwinMiddleware next, FalcorOwinMiddlewareOptions options) : base(next)
        {
            _pathParser = options.ServiceLocator.Get<IPathParser>();
            _responseSerializer = options.ServiceLocator.Get<IResponseSerializer>();
            _falcorRouter = new Router(
                options.ServiceLocator.Get<IRouteResolver>(),
                options.ServiceLocator.Get<IPathCollapser>(),
                options.ServiceLocator.Get<IResponseBuilder>());
        }

        private readonly Router _falcorRouter;
        private readonly IPathParser _pathParser;
        private readonly IResponseSerializer _responseSerializer;


        public async override Task Invoke(IOwinContext ctx)
        {
            var path = ctx.Request.Query["path"] ?? ctx.Request.Query["paths"]; // not sure which is correct
            var result = await _falcorRouter.Execute(_pathParser.ParsePaths(path));
            var serializer = _responseSerializer;
            ctx.Response.Headers.Set("content-type", "application/json");

            await ctx.Response.WriteAsync(serializer.Serialize(result));
        }
    }
}
