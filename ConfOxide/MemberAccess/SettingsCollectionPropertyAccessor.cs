﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;

namespace ConfOxide.MemberAccess {
	///<summary>A PropertyAccessor for a property that holds a generic collection of a <see cref="SettingsBase{T}"/> type.</summary>
	public class SettingsCollectionPropertyAccessor<TOwner, TCollection, TValue> :
		TypedPropertyAccessor<TOwner, TCollection>,
		IPropertyAccessor<TOwner>
		where TCollection : IList<TValue>
		where TValue : SettingsBase<TValue> {

		private readonly Action<TOwner> initializer;

		///<summary>Creates a <see cref="CollectionPropertyAccessor{TOwner, TCollection, TValue}"/> for the specified property.</summary>
		public SettingsCollectionPropertyAccessor(PropertyInfo property) : base(property) {
			var param = Expression.Parameter(typeof(TOwner));

			Expression creator;
			if (typeof(TCollection).IsInterface)
				creator = Expression.New(typeof(List<>).MakeGenericType(typeof(TValue)));
			else
				creator = Expression.New(typeof(TCollection));

			initializer = Expression.Lambda<Action<TOwner>>(
				Expression.Call(param, property.GetSetMethod(true), creator),
				param
			).Compile();
		}

		///<summary>Initializes this property to a new collection.  This is only called when the instance is first constructed.</summary>
		public void InitializeValue(TOwner instance) { initializer(instance); }

		///<summary>Copies the value of this property from one owning object to another.</summary>
		///<param name="from">The instance to read the value from.</param>
		///<param name="to">The instance to write the value to.</param>
		public void Copy(TOwner from, TOwner to) {
			var dest = GetValue(to);
			dest.Clear();
			foreach (var item in GetValue(from))
				dest.Add(item.CreateCopy());
		}

		///<summary>Resets the value of this property on an owning object to its default value.</summary>
		public void ResetValue(TOwner instance) {
			GetValue(instance).Clear();
		}

		///<summary>Compares the values of the property from two owning objects.</summary>
		public bool CompareValues(TOwner x, TOwner y) {
			TCollection vx = GetValue(x), vy = GetValue(y);
			if (vx.Count != vy.Count)
				return false;
			return vx.Zip(vy, (a, b) => a.IsEquivalentTo(b)).All(b => b);
		}

		///<summary>Reads this property's value from a JSON token into an instance.</summary>
		public void FromJson(TOwner instance, JToken token) {
			var dest = GetValue(instance);
			var jsonArray = (JArray)token;

			while (jsonArray.Count < dest.Count)
				dest.RemoveAt(dest.Count - 1);
			for (int i = dest.Count; i < jsonArray.Count; i++) {
				dest.Add(TypeAccessor<TValue>.CreateInstance());
			}
			for (int i = 0; i < dest.Count; i++) {
				dest[i].ReadJson((JObject)jsonArray[i]);
			}
		}

		///<summary>Updates the value of a JProperty to reflect this property's value.</summary>
		public void UpdateJsonProperty(TOwner instance, JProperty jsonProperty) {
			var jsonArray = jsonProperty.Value as JArray;
			if (jsonArray == null)
				jsonProperty.Value = jsonArray = new JArray();

			var source = GetValue(instance);

			while (source.Count < jsonArray.Count)
				jsonArray.RemoveAt(jsonArray.Count - 1);
			for (int i = 0; i < jsonArray.Count; i++) {
				source[i].UpdateJson((JObject)jsonArray[i]);
			}
			for (int i = jsonArray.Count; i < source.Count; i++) {
				jsonArray.Add(source[i].ToJson());
			}
		}
	}
}
