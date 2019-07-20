using System;
using System.Configuration;

namespace Unity.WebForms.Configuration
{
	/// <summary>
	///		Defines an optional custom 'ignoreNamespaces' collection that will
	///		contain the list of namspace prefixes to ignore when checking for
	///		dependencies to inject.
	/// </summary>
	public class IgnoreNamespaceConfigurationCollection : ConfigurationElementCollection
	{
		public IgnoreNamespaceConfigurationCollection()
		{
			NamespaceConfigurationElement element = new NamespaceConfigurationElement();
			this.BaseAdd( element, true );
		}

		#region ConfigurationElementCollection implementation

		public override ConfigurationElementCollectionType CollectionType
		{
			get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new NamespaceConfigurationElement();
		}

		protected override Object GetElementKey( ConfigurationElement element )
		{
			return ( (NamespaceConfigurationElement)element ).Prefix;
		}

		protected override void BaseAdd( ConfigurationElement element )
		{
			this.BaseAdd( element, true );
		}

		#endregion

		public new NamespaceConfigurationElement this[String key]
		{
			get
			{
				return (NamespaceConfigurationElement)this.BaseGet( key );
			}
		}

		public Int32 IndexOf( NamespaceConfigurationElement element )
		{
			return this.BaseIndexOf( element );
		}

		public void Add( NamespaceConfigurationElement element )
		{
			this.BaseAdd( element );
		}

		public void Remove( NamespaceConfigurationElement element )
		{
			if( this.BaseIndexOf( element ) >= 0 )
			{
				this.BaseRemove( element.Prefix );
			}
		}

		public void RemoveAt( Int32 index )
		{
			this.BaseRemoveAt( index );
		}

		public void Remove( String key )
		{
			this.BaseRemove( key );
		}

		public void Clear()
		{
			this.BaseClear();
		}
	}
}
