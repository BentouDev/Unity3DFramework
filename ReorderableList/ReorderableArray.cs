using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Malee {

	[Serializable]
	public abstract class ReorderableArray<T> : ICloneable, IList<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable {

		[SerializeField]
		private List<T> array = new List<T>();

		public ReorderableArray()
			: this(0) {
		}

		public ReorderableArray(int length) {

			array = new List<T>(length);
		}

		public T this[int index] {

			get { return array[index]; }
			set { array[index] = value; }
		}
		
		public int Length {
			
			get { return array.Count; }
		}

		public bool IsReadOnly {

			get { return false; }
		}

		object IList.this[int index]
		{
			get => this[index];
			set => this[index] = (T) value;
		}

		public void CopyTo(Array array, int index)
		{
			CopyTo((T[]) array, index);
		}

		public int Count {

			get { return array.Count; }
		}

		public bool IsSynchronized { get; }
		public object SyncRoot { get; }

		public object Clone() {

			return new List<T>(array);
		}

		public void CopyFrom(IEnumerable<T> value) {

			array.Clear();
			array.AddRange(value);
		}

		public bool Contains(T value) {

			return array.Contains(value);
		}

		public int IndexOf(T value) {

			return array.IndexOf(value);
		}

		public void Insert(int index, T item) {

			array.Insert(index, item);
		}

		public void Remove(object value)
		{
			Remove((T) value);
		}

		public void RemoveAt(int index) {

			array.RemoveAt(index);
		}

		public bool IsFixedSize { get; }

		public void Add(T item) {

			array.Add(item);
		}

		public int Add(object value)
		{
			Add((T) value);
			return Count - 1;
		}

		public void Clear() {

			array.Clear();
		}

		public bool Contains(object value)
		{
			return Contains((T) value);
		}

		public int IndexOf(object value)
		{
			return IndexOf((T) value);
		}

		public void Insert(int index, object value)
		{
			Insert(index, (T) value);
		}

		public void CopyTo(T[] array, int arrayIndex) {

			this.array.CopyTo(array, arrayIndex);
		}

		public bool Remove(T item) {

			return array.Remove(item);
		}

		public T[] ToArray() {

			return array.ToArray();
		}

		public IEnumerator<T> GetEnumerator() {

			return array.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {

			return array.GetEnumerator();
		}
	}
}
