using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Framework.Utils;

namespace Framework.Editor
{
    public class PathBuilder<T>
    {
        private readonly Type _originType;
        private PropertyPath _path;

        public PathBuilder()
        {
            _originType = typeof(T);
        }

        private PathBuilder(PropertyPath path)
        {
            _originType = typeof(T);
            _path = path;
        }

        public PathBuilder<TElement> ByListOf<TElement>(Func<T, string> propGetter)
        {
            if (_path == null)
            {
                _path = new PropertyPath();
            }

            var name = propGetter(default);
            
            // -- 

            FieldInfo memberInfo = _originType.GetMember(name).FirstOrDefault(m => m.MemberType == MemberTypes.Field) as FieldInfo;
            if (memberInfo == null)
                throw new Exception($"Theres no member named {name} in {typeof(T)}!");

            // instance of IList<TElement>
            var instanceType = memberInfo.GetUnderlyingType();

            if (!typeof(IList<TElement>).IsAssignableFrom(instanceType))
                throw new Exception($"Member named {name} is not of IList<{typeof(TElement)}>!");

            _path.Append(name, instanceType);

            return new PathBuilder<TElement>(_path);
        }

        public PathBuilder<U> By<U>(Func<T, string> propGetter)
        {
            if (_path == null)
            {
                _path = new PropertyPath();
            }

            var name = propGetter(default);

            // -- 

            FieldInfo memberInfo = _originType.GetMember(name).FirstOrDefault(m => m.MemberType == MemberTypes.Field) as FieldInfo;
            if (memberInfo == null)
                throw new Exception($"Theres no member named {name} in {typeof(T)}!");

            var typeOfU = typeof(U);

            if (!typeOfU.IsAssignableFrom(memberInfo.GetUnderlyingType()))
                throw new Exception($"Member named {name} is not of type {typeOfU}!");

            _path.Append(name, typeOfU);

            return new PathBuilder<U>(_path);
        }

        public PropertyPath Path()
        {
            return _path;
        }
    }
}