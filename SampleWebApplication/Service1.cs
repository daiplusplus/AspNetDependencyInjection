
namespace SampleWebApplication
{
	/// <summary>Sample service with no dependencies.</summary>
	public class Service1
	{
		/// <summary>Offers a greeting.</summary>
		/// <returns>A greeting from the service.</returns>
		public string SayHello()
		{
			return string.Format( "Hello from Service 1 [Object ID = {0}]", GetHashCode() );
		}
	}
}