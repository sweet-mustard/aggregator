using System;
using AggregatR.Autofac;
using AggregatR.Example.Domain;
using AggregatR.Example.WebHost.Projections;
using AggregatR.Example.WebHost.Projections.Infrastructure;
using AggregatR.Persistence;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AggregatR.Example.WebHost
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IContainer ApplicationContainer { get; private set; }

        public IConfiguration Configuration { get; }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            var builder = new ContainerBuilder();
            builder.Populate(services);
            builder
                .RegisterModule<AggregatRModule>();

            builder
                .RegisterCommandHandlersInAssemblyOf<HandlerLocator>()
                .RegisterEventHandlersInAssemblyOf<HandlerLocator>()
                .RegisterType<Persistence.EventStore.EventStore>()
                .WithParameter("connectionString", Configuration.GetConnectionString("EventStore"))
                .As<IEventStore<string, object>>()
                .SingleInstance();

            builder
                .RegisterType<UserProjection>()
                .AsImplementedInterfaces()
                .SingleInstance();

            builder
                .RegisterType<EventStoreProjector>()
                .WithParameter("connectionString", Configuration.GetConnectionString("EventStore"))
                .SingleInstance();

            return new AutofacServiceProvider(ApplicationContainer = builder.Build());
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.MapWhen(x => !x.Request.Path.Value.StartsWith("/api"), builder =>
            {
                builder.UseMvc(routes =>
                {
                    routes.MapSpaFallbackRoute(
                        name: "spa-fallback",
                        defaults: new { controller = "Home", action = "Index" });
                });
            });

            app.ApplicationServices.GetService<EventStoreProjector>().Start().Wait();

            appLifetime.ApplicationStopped.Register(() =>
            {
                ApplicationContainer.Dispose();
            });
        }
    }
}