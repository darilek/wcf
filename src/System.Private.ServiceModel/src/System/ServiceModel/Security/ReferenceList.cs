// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Xml;
using ID = System.IdentityModel;

namespace System.ServiceModel.Security
{
    internal class ReferenceList
    {
        internal static readonly XmlDictionaryString ElementName = ID.XD.XmlEncryptionDictionary.ReferenceList;
        internal static readonly XmlDictionaryString NamespaceUri = ID.XD.XmlEncryptionDictionary.Namespace;
    }
}
