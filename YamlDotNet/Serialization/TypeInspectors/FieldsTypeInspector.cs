//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) 2014 Antoine Aubry

//  Permission is hereby granted, free of charge, to any person obtaining a copy of
//  this software and associated documentation files (the "Software"), to deal in
//  the Software without restriction, including without limitation the rights to
//  use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//  of the Software, and to permit persons to whom the Software is furnished to do
//  so, subject to the following conditions:

//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.

//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace YamlDotNet.Serialization.TypeInspectors
{
	/// <summary>
	/// Returns the public fields of a type.
	/// </summary>
	public sealed class FieldsTypeInspector : TypeInspectorSkeleton
	{
		private readonly ITypeResolver _typeResolver;

		public FieldsTypeInspector(ITypeResolver typeResolver)
		{
			if (typeResolver == null)
			{
				throw new ArgumentNullException("typeResolver");
			}

			_typeResolver = typeResolver;
		}

		public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
		{
			return type
				.GetFields(BindingFlags.Instance | BindingFlags.Public)
				.Select(p => (IPropertyDescriptor)new ReflectionFieldDescriptor(p, _typeResolver));
		}

		private sealed class ReflectionFieldDescriptor : IPropertyDescriptor
		{
			private readonly FieldInfo _fieldInfo;
			private readonly ITypeResolver _typeResolver;

			public ReflectionFieldDescriptor(FieldInfo fieldInfo, ITypeResolver typeResolver)
			{
				_fieldInfo = fieldInfo;
				_typeResolver = typeResolver;
			}

			public string Name { get { return _fieldInfo.Name; } }
			public Type Type { get { return _fieldInfo.FieldType; } }
			public Type TypeOverride { get; set; }
			public bool CanWrite { get { return true; } }

			public void Write(object target, object value)
			{
				_fieldInfo.SetValue(target, value);
			}

			public T GetCustomAttribute<T>() where T : Attribute
			{
				var attributes = _fieldInfo.GetCustomAttributes(typeof(T), true);
				return attributes.Length > 0
					? (T)attributes[0]
					: null;
			}

			public IObjectDescriptor Read(object target)
			{
				var fieldValue = _fieldInfo.GetValue(target);
				var actualType = TypeOverride ?? _typeResolver.Resolve(Type, fieldValue);
				return new ObjectDescriptor(fieldValue, actualType, Type);
			}
		}
	}
}
