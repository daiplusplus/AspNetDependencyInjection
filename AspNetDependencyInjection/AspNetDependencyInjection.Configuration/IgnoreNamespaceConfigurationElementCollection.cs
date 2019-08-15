using System;
using System.Configuration;

namespace AspNetDependencyInjection.Configuration
{
	/// <summary>
	///		Defines an optional custom 'ignoreNamespaces' collection that will
	///		contain the list of namspace prefixes to ignore when checking for
	///		dependencies to inject.
	/// </summary>
	public class IgnoreNamespaceConfigurationElementCollection : ConfigurationElementCollection
	{
		/// <summary>Constructs and initializes a new instance of <see cref="IgnoreNamespaceConfigurationElementCollection"/>.</summary>
		public IgnoreNamespaceConfigurationElementCollection()
		{
			NamespaceConfigurationElement element = new NamespaceConfigurationElement();
			this.BaseAdd( element, true );
		}

		#region ConfigurationElementCollection implementation

		/// <summary>Gets the type of the System.Configuration.ConfigurationElementCollection.</summary>
		public override ConfigurationElementCollectionType CollectionType
		{
			get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
		}

		/// <summary>Creates and returns a new <see cref="NamespaceConfigurationElement"/> object.</summary>
		protected override ConfigurationElement CreateNewElement()
		{
			return new NamespaceConfigurationElement();
		}

		/// <summary>Gets the element key for the specified configuration element (<paramref name="element"/>).</summary>
		protected override Object GetElementKey( ConfigurationElement element )
		{
			return ( (NamespaceConfigurationElement)element ).Prefix;
		}

		/// <summary>Adds the specified <paramref name="element"/> this System.Configuration.ConfigurationElementCollection.</summary>
		protected override void BaseAdd( ConfigurationElement element )
		{
			this.BaseAdd( element, true );
		}

		#endregion

		/// <summary>Returns the <see cref="NamespaceConfigurationElement"/> with the specified <paramref name="key"/>.</summary>
		public new NamespaceConfigurationElement this[String key]
		{
			get
			{
				return (NamespaceConfigurationElement)this.BaseGet( key );
			}
		}

		/// <summary>Returns the index of the specified <paramref name="element"/> in this collection.</summary>
		public Int32 IndexOf( NamespaceConfigurationElement element )
		{
			return this.BaseIndexOf( element );
		}

		/// <summary>Adds the specified <paramref name="element"/> to the collection.</summary>
		public void Add( NamespaceConfigurationElement element )
		{
			this.BaseAdd( element );
		}

		/// <summary>Removes the specified <paramref name="element"/> from the collection.</summary>
		public void Remove( NamespaceConfigurationElement element )
		{
			if( this.BaseIndexOf( element ) >= 0 )
			{
				this.BaseRemove( element.Prefix );
			}
		}

		/// <summary>Adds the <see cref="NamespaceConfigurationElement"/> at the specified <paramref name="index"/>.</summary>
		public void RemoveAt( Int32 index )
		{
			this.BaseRemoveAt( index );
		}

		/// <summary>Adds the <see cref="NamespaceConfigurationElement"/> with the specified <paramref name="key"/>.</summary>
		public void Remove( String key )
		{
			this.BaseRemove( key );
		}

		/// <summary>Removes all elements from this collection.</summary>
		public void Clear()
		{
			this.BaseClear();
		}
	}
}
