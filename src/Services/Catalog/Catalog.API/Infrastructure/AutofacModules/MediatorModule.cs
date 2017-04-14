using Autofac;
using Autofac.Core;
using Catalog.API.Application.Commands;
using Catalog.API.Application.Decorators;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Catalog.API.Infrastructure.AutofacModules
{
    public class MediatorModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(IMediator).GetTypeInfo().Assembly)
                .AsImplementedInterfaces();

            // Register all the Command classes (they implement IAsyncRequestHandler) in assembly holding the Commands
            builder.RegisterAssemblyTypes(typeof(RemoveStockFromProducsCommand).GetTypeInfo().Assembly)
                .As(o => o.GetInterfaces()
                    .Where(i => i.IsClosedTypeOf(typeof(IAsyncRequestHandler<,>)))
                    .Select(i => new KeyedService("IAsyncRequestHandler", i))).AsImplementedInterfaces();            

            //builder
            //    .RegisterAssemblyTypes(typeof(CreateOrderCommandValidator).GetTypeInfo().Assembly)
            //    .Where(t => t.IsClosedTypeOf(typeof(IValidator<>)))
            //    .AsImplementedInterfaces();


            builder.Register<SingleInstanceFactory>(context =>
            {
                var componentContext = context.Resolve<IComponentContext>();

                return t => componentContext.Resolve(t);
            });

            builder.Register<MultiInstanceFactory>(context =>
            {
                var componentContext = context.Resolve<IComponentContext>();

                return t => (IEnumerable<object>)componentContext.Resolve(typeof(IEnumerable<>).MakeGenericType(t));
            });



            builder.RegisterGenericDecorator(typeof(LogDecorator<,>),
                    typeof(IAsyncRequestHandler<,>),
                    "IAsyncRequestHandler")
                    .Keyed("handlerDecorator", typeof(IAsyncRequestHandler<,>));

            //builder.RegisterGenericDecorator(typeof(ValidatorDecorator<,>),
            //        typeof(IAsyncRequestHandler<,>),
            //        fromKey: "handlerDecorator");
        }
    }
}
