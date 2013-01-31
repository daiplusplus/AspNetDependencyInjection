
namespace SampleWebApplication
{
	/// <summary>
	///		Sample service with no dependencies.
	/// </summary>
	public class Service1
	{
		/// <summary>
		///		Offers a greeting.
		/// </summary>
		/// <returns>
		///		A greeting from the service.
		/// </returns>
		public string SayHello()
		{
			return "Hello from Service 1";
		}
	}
}