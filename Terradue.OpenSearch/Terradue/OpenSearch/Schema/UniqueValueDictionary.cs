//
//  UniqueValueDictionary.cs
//
//  Author:
//       Emmanuel Mathot <emmanuel.mathot@terradue.com>
//
//  Copyright (c) 2014 Terradue

using System;
using System.Linq;
using System.Collections.Generic;

namespace Terradue.OpenSearch.Schema {
    public class UniqueValueDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public new void Add(TKey key, TValue value)
        {
            if (this.ContainsValue(value))
            {
                throw new ArgumentException(string.Format("value {0} already exist", value));
            }
            base.Add(key, value);
        }

        public new TValue this[TKey key]
        {
            get
            {
                return base[key];
            }
            set
            {
                if (this.ContainsValue(value))
                {
                    throw new ArgumentException(string.Format("value {0} already exist", value));
                }

                base[key] = value;
            }
        }
    }
}

