using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

using SampleWcfProject.SampleServices;

namespace SampleWcfProject
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
			get { return this.boolValue; }
			set { this.boolValue = value; }
		}

		[DataMember]
		public string StringValue
		{
			get { return this.stringValue; }
			set { this.stringValue = value; }
		}
	}

	// NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
	// NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
	public class Service1 : IService1
	{
		private readonly ISampleSingletonService  sg1;
		private readonly ISampleTransientService2 st2;

		public Service1( ISampleSingletonService sg1, ISampleTransientService2 st2 )
		{
			this.sg1 = sg1 ?? throw new ArgumentNullException( nameof( sg1 ) );
			this.st2 = st2 ?? throw new ArgumentNullException( nameof( st2 ) );
		}

		public String GetData( int value )
		{
			return "Using Singleton service " + this.sg1.InstanceId + " and transient service " + this.st2.InstanceId;
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
