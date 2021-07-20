
namespace SampleWebApplication
{
	/// <summary>
	///		Sample service that has a dependency on another service.
	/// </summary>
	/// <remarks>This is using the preferred injection technique (constructor).</remarks>
	public class Service2
	{
		/// <summary>Backing field for dependent service.</summary>
		private Service1 _service1;

		/// <summary>
		///		Initializes a new instance of the service with an injected
		///		dependency.
		/// </summary>
		/// <param name="svc1">Injected dependency, <see cref="Service1" /></param>
		public Service2( Service1 svc1 )
		{
			this._service1 = svc1;
		}

		/// <summary>
		///		Offers a greeting.
		/// </summary>
		/// <returns>
		///		A greeting from this service combined with the greeting from
		///		the dependent service.
		/// </returns>
		public string SayHello()
		{
			return string.Format( "{0} (Called from Service2 [Object ID = {1}])", this._service1.SayHello(), this.GetHashCode() );
		}
	}
}