﻿using System;
namespace Terradue.OpenSearch.Engine
{
    [AttributeUsage (AttributeTargets.Class)]
    public class OpenSearchEngineExtensionAttribute : Attribute
    {
        //
        // Fields
        //
        private string nodeName;

        private string description;

        private string customAttributeTypeName;

        private Type customAttributeType;

        //
        // Properties
        //
        public string Description {
            get {
                return (this.description == null) ? string.Empty : this.description;
            }
            set {
                this.description = value;
            }
        }

        public Type ExtensionAttributeType {
            get {
                return this.customAttributeType;
            }
            set {
                this.customAttributeType = value;
                this.customAttributeTypeName = value.FullName;
            }
        }

        internal string ExtensionAttributeTypeName {
            get {
                return this.customAttributeTypeName ?? string.Empty;
            }
            set {
                this.customAttributeTypeName = value;
            }
        }

        public string NodeName {
            get {
                return (this.nodeName == null) ? string.Empty : this.nodeName;
            }
            set {
                this.nodeName = value;
            }
        }

        //
        // Constructors
        //
        public OpenSearchEngineExtensionAttribute ()
        {
        }

        public OpenSearchEngineExtensionAttribute (string nodeName)
        {
            this.nodeName = nodeName;
        }

        public OpenSearchEngineExtensionAttribute (string nodeName, string description)
        {
            this.nodeName = nodeName;
            this.description = description;
        }
    }
}
