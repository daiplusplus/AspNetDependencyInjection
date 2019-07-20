using System;

using Microsoft.Web.Infrastructure.DynamicModuleHelper;

namespace Unity.WebForms
{
	/// <summary>Performs any required initialization of this assembly before the application starts.</summary>
	public class PreApplicationStart
	{
		private static Boolean _isStarting;

		/// <summary>Registers the <see cref="UnityHttpModule" /> Http Module for dependency injection using Unity.</summary>
		public static void PreStart()
		{
			if( !_isStarting )
			{
				_isStarting = true;
				DynamicModuleUtility.RegisterModule( typeof( UnityHttpModule ) );
			}
		}
	}
}
