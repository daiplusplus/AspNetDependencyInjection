using System;
using System.Web;

namespace Unity.WebForms
{
	/// <summary>Gets the <see cref="HttpContextBase"/> for the current request in a thread-safe manner (by using an immutable instance field reference to the <see cref="HttpContextBase"/> available when the request was started).</summary>
	public class DefaultHttpContextAccessor : IHttpContextAccessor
	{
		/// <summary>Constructs a new instance of <see cref="DefaultHttpContextAccessor"/> using the provided <paramref name="httpContextBase"/>.</summary>
		/// <param name="httpContextBase">Required. Cannot be null (otherwise an <see cref="ArgumentNullException"/> will be thrown).</param>
		public DefaultHttpContextAccessor( HttpContextBase httpContextBase )
		{
			this.HttpContext = httpContextBase ?? throw new ArgumentNullException(nameof(httpContextBase));
		}

		/// <summary>Always returns the same <see cref="HttpContextBase"/> instance that was passed into this instance's constructor. This service is </summary>
		public HttpContextBase HttpContext { get; }
	}

	// TODO: When would *anyone* ever want to use `ThreadLocalStorageHttpContextAccessor`?
	// If those types need some form of compatibility with the thread-unsafe `HttpContext.Current` then they can use it directly.

	#if ANY_GOOD_REASONS_FOR_THIS

	/// <summary>Implements <see cref="IHttpContextAccessor"/> by simply returning <see cref="HttpContext.Current"/> wrapped in a new <see cref="HttpContextWrapper"/>.</summary>
	public class ThreadLocalStorageHttpContextAccessor : IHttpContextAccessor
	{
		/// <summary>Always returns <c>new HttpContextWrapper( System.Web.HttpContext.Current )</c>.</summary>
		public HttpContextBase HttpContext
		{
			get
			{
				return new HttpContextWrapper( System.Web.HttpContext.Current );
			}
		}
	}

	#endif
}
