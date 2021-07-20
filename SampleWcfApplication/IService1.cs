using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace AspNetDependencyInjection.Wcf
{
	// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
	[ServiceContract]
	public interface IService1
	{

		[OperationContract]
		string GetData( int value );

		[OperationContract]
		CompositeType GetDataUsingDataContract( CompositeType composite );

		// TODO: Add your service operations here
	}


	// Use a data contract as illustrated in the sample below to add composite types to service operations.
	[DataContract]
	public class CompositeType
	{
		bool boolValue = true;
		string stringValue = "Hello ";

		[DataMember]
		public bool BoolValue
		{
			get { return boolValue; }
			set { boolValue = value; }
		}

		[DataMember]
		public string StringValue
		{
			get { return stringValue; }
			set { stringValue = value; }
		}
	}

	// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
	// NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
	public class Service1 : IService1
	{
		public string GetData( int value )
		{
			return string.Format( "You entered: {0}", value );
		}

		public CompositeType GetDataUsingDataContract( CompositeType composite )
		{
			if( composite == null )
			{
				throw new ArgumentNullException( "composite" );
			}
			if( composite.BoolValue )
			{
				composite.StringValue += "Suffix";
			}
			return composite;
		}
	}
}
