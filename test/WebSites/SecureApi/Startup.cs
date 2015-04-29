﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using Swashbuckle.Application;
using Swashbuckle.Swagger;
using SecureApi.Swagger;

namespace SecureApi
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
        }

        // This method gets called by a runtime.
        // Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            // Uncomment the following line to add Web API services which makes it easier to port Web API 2 controllers.
            // You will also need to add the Microsoft.AspNet.Mvc.WebApiCompatShim package to the 'dependencies' section of project.json.
            // services.AddWebApiConventions();

            services.AddSwagger(s =>
            {
                s.SwaggerGenerator(opts =>
                {
                    opts.SecurityDefinitions.Add("oauth2", new SecurityScheme
                    {
                        type = "oauth2",
                        flow = "implicit",
                        authorizationUrl = "http://petstore.swagger.io/api/oauth/dialog",
                        scopes = new Dictionary<string, string>
                        {
                            { "read", "read access" },
                            { "write", "write access" }
                        }
                    });

                    opts.OperationFilter<AssignSecurityRequirements>();
                });
            });
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Configure the HTTP request pipeline.
            app.UseStaticFiles();

            // Add MVC to the request pipeline.
            app.UseMvc(routes =>
            {
                routes.EnableSwagger();
                routes.EnableSwaggerUi();
            });
            // Add the following route for porting Web API 2 controllers.
            // routes.MapWebApiRoute("DefaultApi", "api/{controller}/{id?}");

            // TOOD: Figure out oauth middleware to validate token
            //app.UseOAuthBearerAuthentication(opts =>
            //{
            //});
        }
    }
}
