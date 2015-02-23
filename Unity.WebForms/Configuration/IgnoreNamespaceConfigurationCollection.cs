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
			var element = new NamespaceConfigurationElement();
			BaseAdd( element, true );
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

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((NamespaceConfigurationElement) element).Prefix;
		}

		protected override void BaseAdd(ConfigurationElement element)
		{
			BaseAdd (element, true );
		}

		#endregion

		public new NamespaceConfigurationElement this[string key]
		{
			get { return (NamespaceConfigurationElement) BaseGet(key); }
		}

		public int IndexOf(NamespaceConfigurationElement element)
		{
			return BaseIndexOf(element);
		}

		public void Add(NamespaceConfigurationElement element)
		{
			BaseAdd(element);
		}

		public void Remove(NamespaceConfigurationElement element)
		{
			if (BaseIndexOf(element) >= 0)
			{
				BaseRemove( element.Prefix );
			}
		}

		public void RemoveAt(int index)
		{
			BaseRemoveAt(index);
		}

		public void Remove(string key)
		{
			BaseRemove(key);
		}

		public void Clear()
		{
			BaseClear();
		}
	}
}
