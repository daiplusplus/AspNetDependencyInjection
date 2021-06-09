using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

using Microsoft.Extensions.DependencyInjection;

namespace AspNetDependencyInjection.Internal
{
	/// <summary>Implementation of <see cref="IControllerActivator"/> (which is used by <see cref="DefaultControllerFactory"/>, in-turn used by <see cref="ControllerBuilder"/>) that uses <see cref="AspNetDependencyInjection.ApplicationDependencyInjection"/>.</summary>
	public class DependencyInjectionMvcControllerActivator : IControllerActivator
	{
		private readonly ApplicationDependencyInjection di;

		internal DependencyInjectionMvcControllerActivator( ApplicationDependencyInjection di )
		{
			this.di = di ?? throw new ArgumentNullException(nameof(di));
		}

		/// <summary>
		/// The default <c><see cref="System.Web.Mvc.DefaultControllerFactory"/>.DefaultControllerActivator</c> first tries passing the <see cref="IController"/>'s concrete <see cref="Type"/> to <see cref="IDependencyResolver.GetService(Type)"/>.<br />
		/// If that fails then it uses <see cref="Activator.CreateInstance(Type)"/> directly. It does not pass a <see cref="IServiceProvider"/> to <see cref="Activator"/> (e.g. with <see cref="Microsoft.Extensions.DependencyInjection.ActivatorUtilities.CreateInstance(IServiceProvider, Type, object[])"/>).<br />
		/// <br />
		/// Whereas this implementation does.</summary>
		/// <param name="requestContext"></param>
		/// <param name="controllerType"></param>
		/// <returns></returns>
		public IController Create( RequestContext requestContext, Type controllerType )
		{
			if( requestContext is null ) throw new ArgumentNullException( nameof( requestContext ) );
			if( controllerType is null ) throw new ArgumentNullException( nameof( controllerType ) );

			Object instance;
			try
			{
				IServiceProvider sp = this.di.GetServiceProviderForHttpContext( requestContext.HttpContext );
				instance = ActivatorUtilities.GetServiceOrCreateInstance( sp, controllerType );
			}
			catch( Exception ex )
			{
				const String msg = @"An error occurred when trying to create a controller of type ""{0}\"".";
				throw new InvalidOperationException( String.Format( CultureInfo.CurrentCulture, msg, controllerType.FullName ), innerException: ex );
			}

			if( instance is null )
			{
				return null;
			}
			else if( instance is IController controller )
			{
				return controller;
			}
			else
			{
				if( instance is IDisposable disposable )
				{
					disposable.Dispose();
				}

				const String msg = @"The requested type ""{0}"" is not an " + nameof(IController) + @" type.";
				throw new InvalidOperationException( String.Format( CultureInfo.CurrentCulture, msg, controllerType.FullName ) );
			}
		}
	}
}
