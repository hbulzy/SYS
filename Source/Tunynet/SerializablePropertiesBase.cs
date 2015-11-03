using System;
namespace Tunynet
{
	[System.Serializable]
	public abstract class SerializablePropertiesBase : ISerializableProperties
	{
		private PropertySerializer propertySerializer;
		private string propertyNames;
		private string propertyValues;
		protected PropertySerializer PropertySerializer
		{
			get
			{
				if (this.propertySerializer == null)
				{
					this.propertySerializer = new PropertySerializer(this.PropertyNames, this.PropertyValues);
				}
				return this.propertySerializer;
			}
		}
		public string PropertyNames
		{
			get
			{
				return this.propertyNames;
			}
			protected set
			{
				this.propertyNames = value;
			}
		}
		public string PropertyValues
		{
			get
			{
				return this.propertyValues;
			}
			protected set
			{
				this.propertyValues = value;
			}
		}
		public T GetExtendedProperty<T>(string propertyName)
		{
			return this.PropertySerializer.GetExtendedProperty<T>(propertyName);
		}
		public T GetExtendedProperty<T>(string propertyName, T defaultValue)
		{
			return this.PropertySerializer.GetExtendedProperty<T>(propertyName, defaultValue);
		}
		public void SetExtendedProperty(string propertyName, object propertyValue)
		{
			this.PropertySerializer.SetExtendedProperty(propertyName, propertyValue);
		}
		void ISerializableProperties.Serialize()
		{
			this.PropertySerializer.Serialize(ref this.propertyNames, ref this.propertyValues);
		}
	}
}
