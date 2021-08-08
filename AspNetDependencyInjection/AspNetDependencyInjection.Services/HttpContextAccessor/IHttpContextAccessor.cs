using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Web;

namespace AspNetDependencyInjection
{
	/// <summary>Implementations are always request-scoped. This service allows consumers to get the <see cref="HttpContextBase"/> for the current request.</summary>
	public interface IHttpContextAccessor
	{
		/// <summary>
		/// Gets the <see cref="HttpContextBase"/> for the current request.<br />
		/// Implementations will use a strong-reference to the <see cref="HttpContextBase"/> associated with the current HTTP request instead of using <see cref="HttpContext.Current"/> (which uses thread-local-storage), which means that <see cref="IHttpContextAccessor"/> references can be safely passed around to other threads.<br />
		/// To get the &quot;classic&quot;<see cref="System.Web.HttpContext"/> from a <see cref="HttpContextBase"/>, use <see cref="AndiExtensions.GetHttpContext(HttpContextBase)"/>.
		/// </summary>
		HttpContextBase HttpContext { get; }
	}
}
