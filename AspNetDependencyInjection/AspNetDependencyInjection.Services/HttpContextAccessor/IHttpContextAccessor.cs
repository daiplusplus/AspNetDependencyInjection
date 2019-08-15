using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Web;

namespace AspNetDependencyInjection
{
	/// <summary>Gets the <see cref="HttpContextBase"/> for the current request.</summary>
	public interface IHttpContextAccessor
	{
		/// <summary>Gets the <see cref="HttpContextBase"/> for the current request. Uses a strong reference to the <see cref="HttpContextBase"/> associated with the request instead of using <see cref="HttpContext.Current"/> (which uses thread-local-storage), so this service can be passed around to other threads so they can use the HttpContext that belongs to a different thread.</summary>
		HttpContextBase HttpContext { get; }
	}
}
