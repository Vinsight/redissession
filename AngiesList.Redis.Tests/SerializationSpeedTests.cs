using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using AngiesList.Redis.Extensions;

namespace AngiesList.Redis.Tests
{
    [TestClass]
    public class SerializationSpeedTests
    {
        [Serializable]
        public class User
        {
            public string Id { get; set; }
            public string UserName { get; set; }
            public string LoweredUserName { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public bool isApiUser { get; set; }
            public UsersInRole[] UsersInRoles { get; set; }
            public ApplicationSetting[] ApplicationsSettings { get; set; }
            public Organisation[] Organisations { get; set; }
            public UserContact UserContact { get; set; }
        }

        [Serializable]
        public class UserContact
        {
            public int ContactNum { get; set; }
            public string ContactCode { get; set; }
            public bool IsCarrier { get; set; }
            public bool IsCreditor { get; set; }
            public bool IsDebtor { get; set; }
            public bool IsEmployee { get; set; }
            public bool IsGrower { get; set; }
            public bool IsGrowerOperator { get; set; }
            public bool IsMailOrder { get; set; }
            public bool IsProductionOperator { get; set; }
            public bool IsSalesOperator { get; set; }
            public bool IsSalesTaxLiable { get; set; }
            public bool IsWineTaxLiable { get; set; }
            public bool IsWineTaxOwnUse { get; set; }
        }

        [Serializable]
        public class UsersInRole
        {
            public int EntryNum { get; set; }
            public string UserId { get; set; }
            public string RoleId { get; set; }
            public string OrganisationId { get; set; }
            public Organisation Organisation { get; set; }
            public Role Role { get; set; }
        }

        [Serializable]
        public class Role
        {
            public string ApplicationId { get; set; }
            public string Id { get; set; }
            public string Name { get; set; }
            public string LoweredName { get; set; }
            public object Description { get; set; }
            public bool IsReadOnly { get; set; }
            public Application Application { get; set; }
        }

        [Serializable]
        public class Application
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        [Serializable]
        public class ApplicationSetting
        {
            public string ApplicationId { get; set; }
            public string DefaultUrl { get; set; }
            public string Language { get; set; }
            public object LanguageVariant { get; set; }
        }

        [Serializable]
        public class Organisation
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }


        private void TestSerializer(IValueSerializer serializer, string name)
        {
            //First check we can set and get strings
            byte[] bytes;

            bytes = serializer.Serialize("TestString");
            var str = serializer.Deserialize(bytes) as string;

            Assert.AreEqual("TestString", str);

            //Then check performance of type serialization

            var userObject = JsonConvert.DeserializeObject<User>("{\"$id\":\"1\",\"ApplicationId\":\"00000000-0000-0000-0000-000000000000\",\"Id\":\"b2f29128-24c0-4a8a-b3cd-8bfc0378bd72\",\"UserName\":\"support@vinsight.net\",\"LoweredUserName\":\"support@vinsight.net\",\"MobileAlias\":null,\"IsAnonymous\":false,\"LastActivityDate\":\"0001-01-01T00:00:00\",\"FirstName\":\"Vinsight\",\"LastName\":\"Support\",\"isApiUser\":false,\"Application\":null,\"ApplicationReference\":{\"EntityKey\":null},\"UsersInRoles\":[{\"$id\":\"2\",\"EntryNum\":9,\"UserId\":\"b2f29128-24c0-4a8a-b3cd-8bfc0378bd72\",\"RoleId\":\"881ea4f6-d7db-4890-8418-15e84fc910b9\",\"OrganisationId\":\"c663e81d-57e2-4ba6-a40c-6c587101ae95\",\"Organisation\":{\"$id\":\"3\",\"Id\":\"00000000-0000-0000-0000-000000000000\",\"Name\":\"Vinsight Support\",\"LoweredName\":null,\"Connections\":[],\"UsersInRoles\":[{\"$ref\":\"2\"}],\"Subscriptions\":[],\"Applications\":[],\"Users\":[],\"RequestLogs\":[],\"ResourcePermissions\":[],\"ApplicationsSettings\":[],\"AuditLogEntries\":[],\"UserFacets\":[],\"EntityKey\":null},\"Org anisationReference\":{\"EntityKey\":null},\"Role\":{\"$id\":\"4\",\"ApplicationId\":\"700d72d5-946e-4fb0-aff2-75c4f61fd2c9\",\"Id\":\"881ea4f6-d7db-4890-8418-15e84fc910b9\",\"Name\":\"Admins\",\"LoweredName\":\"admins\",\"Description\":null,\"IsReadOnly\":false,\"Application\":{\"$id\":\"5\",\"Name\":\"vinsight\",\"LoweredName\":null,\"Id\":\"700d72d5-946e-4fb0-aff2-75c4f61fd2c9\",\"Description\":null,\"EntityKey\":null},\"ApplicationReference\":{\"EntityKey\":null},\"UsersInRoles\":[{\"$ref\":\"2\"}],\"ResourcePermissions\":[],\"EntityKey\":null},\"RoleReference\":{\"EntityKey\":null},\"User\":{\"$ref\":\"1\"},\"UserReference\":{\"EntityKey\":null},\"EntityKey\":null},{\"$id\":\"6\",\"EntryNum\":20,\"UserId\":\"b2f29128-24c0-4a8a-b3cd-8bfc0378bd72\",\"RoleId\":\"3e744d7f-d43d-41f4-ac4a-4bc1ac6fb1de\",\"OrganisationId\":\"c663e81d-57e2-4ba6-a40c-6c587101ae95\",\"Organisation\":{\"$id\":\"7\",\"Id\":\"00000000-0000-0000-0000-000000000000\",\"Name\":\"Vinsight Support\",\"LoweredName\":null,\"Connections\":[],\" UsersInRoles\":[{\"$ref\":\"6\"}],\"Subscriptions\":[],\"Applications\":[],\"Users\":[],\"RequestLogs\":[],\"ResourcePermissions\":[],\"ApplicationsSettings\":[],\"AuditLogEntries\":[],\"UserFacets\":[],\"EntityKey\":null},\"OrganisationReference\":{\"EntityKey\":null},\"Role\":{\"$id\":\"8\",\"ApplicationId\":\"700d72d5-946e-4fb0-aff2-75c4f61fd2c9\",\"Id\":\"3e744d7f-d43d-41f4-ac4a-4bc1ac6fb1de\",\"Name\":\"ApplicationAdmins\",\"LoweredName\":\"applicationadmins\",\"Description\":null,\"IsReadOnly\":false,\"Application\":{\"$id\":\"9\",\"Name\":\"vinsight\",\"LoweredName\":null,\"Id\":\"700d72d5-946e-4fb0-aff2-75c4f61fd2c9\",\"Description\":null,\"EntityKey\":null},\"ApplicationReference\":{\"EntityKey\":null},\"UsersInRoles\":[{\"$ref\":\"6\"}],\"ResourcePermissions\":[],\"EntityKey\":null},\"RoleReference\":{\"EntityKey\":null},\"User\":{\"$ref\":\"1\"},\"UserReference\":{\"EntityKey\":null},\"EntityKey\":null},{\"$id\":\"10\",\"EntryNum\":28,\"UserId\":\"b2f29128-24c0-4a8a-b3cd-8bfc0378bd72\",\"RoleId\":\" d17a5b3f-5394-4ebd-af11-56468974fce5\",\"OrganisationId\":\"c663e81d-57e2-4ba6-a40c-6c587101ae95\",\"Organisation\":{\"$id\":\"11\",\"Id\":\"00000000-0000-0000-0000-000000000000\",\"Name\":\"Vinsight Support\",\"LoweredName\":null,\"Connections\":[],\"UsersInRoles\":[{\"$ref\":\"10\"}],\"Subscriptions\":[],\"Applications\":[],\"Users\":[],\"RequestLogs\":[],\"ResourcePermissions\":[],\"ApplicationsSettings\":[],\"AuditLogEntries\":[],\"UserFacets\":[],\"EntityKey\":null},\"OrganisationReference\":{\"EntityKey\":null},\"Role\":{\"$id\":\"12\",\"ApplicationId\":\"700d72d5-946e-4fb0-aff2-75c4f61fd2c9\",\"Id\":\"d17a5b3f-5394-4ebd-af11-56468974fce5\",\"Name\":\"Users\",\"LoweredName\":\"users\",\"Description\":null,\"IsReadOnly\":false,\"Application\":{\"$id\":\"13\",\"Name\":\"vinsight\",\"LoweredName\":null,\"Id\":\"700d72d5-946e-4fb0-aff2-75c4f61fd2c9\",\"Description\":null,\"EntityKey\":null},\"ApplicationReference\":{\"EntityKey\":null},\"UsersInRoles\":[{\"$ref\":\"10\"}],\"ResourcePermissions\":[],\"EntityK ey\":null},\"RoleReference\":{\"EntityKey\":null},\"User\":{\"$ref\":\"1\"},\"UserReference\":{\"EntityKey\":null},\"EntityKey\":null}],\"Subscriptions\":[],\"Organisations\":[{\"$id\":\"14\",\"Id\":\"3e8b7d0a-8155-405d-8729-3811dd14a5fe\",\"Name\":\"Empty Test\",\"LoweredName\":null,\"Connections\":[],\"UsersInRoles\":[],\"Subscriptions\":[],\"Applications\":[],\"Users\":[{\"$ref\":\"1\"}],\"RequestLogs\":[],\"ResourcePermissions\":[],\"ApplicationsSettings\":[],\"AuditLogEntries\":[],\"UserFacets\":[],\"EntityKey\":null},{\"$id\":\"15\",\"Id\":\"6ab925a8-07e1-444d-aa9c-3af1f6c5a670\",\"Name\":\"My Test Company9\",\"LoweredName\":null,\"Connections\":[],\"UsersInRoles\":[],\"Subscriptions\":[],\"Applications\":[],\"Users\":[{\"$ref\":\"1\"}],\"RequestLogs\":[],\"ResourcePermissions\":[],\"ApplicationsSettings\":[],\"AuditLogEntries\":[],\"UserFacets\":[],\"EntityKey\":null},{\"$id\":\"16\",\"Id\":\"a40be8bb-e71f-4363-a042-437e401ab1ca\",\"Name\":\"My Test Company11\",\"LoweredName\":null,\"Connections\":[],\" UsersInRoles\":[],\"Subscriptions\":[],\"Applications\":[],\"Users\":[{\"$ref\":\"1\"}],\"RequestLogs\":[],\"ResourcePermissions\":[],\"ApplicationsSettings\":[],\"AuditLogEntries\":[],\"UserFacets\":[],\"EntityKey\":null},{\"$id\":\"17\",\"Id\":\"284440de-0a41-4c57-9984-4d19af8f20fb\",\"Name\":\"My Test Company6\",\"LoweredName\":null,\"Connections\":[],\"UsersInRoles\":[],\"Subscriptions\":[],\"Applications\":[],\"Users\":[{\"$ref\":\"1\"}],\"RequestLogs\":[],\"ResourcePermissions\":[],\"ApplicationsSettings\":[],\"AuditLogEntries\":[],\"UserFacets\":[],\"EntityKey\":null},{\"$id\":\"18\",\"Id\":\"c663e81d-57e2-4ba6-a40c-6c587101ae95\",\"Name\":\"Vinsight Support\",\"LoweredName\":null,\"Connections\":[],\"UsersInRoles\":[],\"Subscriptions\":[],\"Applications\":[],\"Users\":[{\"$ref\":\"1\"}],\"RequestLogs\":[],\"ResourcePermissions\":[],\"ApplicationsSettings\":[],\"AuditLogEntries\":[],\"UserFacets\":[],\"EntityKey\":null},{\"$id\":\"19\",\"Id\":\"c3c3f80c-2b34-4ba2-9e7f-9582a2c44ca9\",\"Name\":\"My Test C ompany5\",\"LoweredName\":null,\"Connections\":[],\"UsersInRoles\":[],\"Subscriptions\":[],\"Applications\":[],\"Users\":[{\"$ref\":\"1\"}],\"RequestLogs\":[],\"ResourcePermissions\":[],\"ApplicationsSettings\":[],\"AuditLogEntries\":[],\"UserFacets\":[],\"EntityKey\":null},{\"$id\":\"20\",\"Id\":\"f345c188-d288-4fc2-a269-bb9229eb0475\",\"Name\":\"My Test Company7\",\"LoweredName\":null,\"Connections\":[],\"UsersInRoles\":[],\"Subscriptions\":[],\"Applications\":[],\"Users\":[{\"$ref\":\"1\"}],\"RequestLogs\":[],\"ResourcePermissions\":[],\"ApplicationsSettings\":[],\"AuditLogEntries\":[],\"UserFacets\":[],\"EntityKey\":null},{\"$id\":\"21\",\"Id\":\"429db3c7-d022-4ee6-afcb-ec99d6236a1b\",\"Name\":\"My Test Company8\",\"LoweredName\":null,\"Connections\":[],\"UsersInRoles\":[],\"Subscriptions\":[],\"Applications\":[],\"Users\":[{\"$ref\":\"1\"}],\"RequestLogs\":[],\"ResourcePermissions\":[],\"ApplicationsSettings\":[],\"AuditLogEntries\":[],\"UserFacets\":[],\"EntityKey\":null},{\"$id\":\"22\",\"Id\":\"aeaae08 2-0fe4-4b98-b4da-ffba8b430e11\",\"Name\":\"My Test Company4\",\"LoweredName\":null,\"Connections\":[],\"UsersInRoles\":[],\"Subscriptions\":[],\"Applications\":[],\"Users\":[{\"$ref\":\"1\"}],\"RequestLogs\":[],\"ResourcePermissions\":[],\"ApplicationsSettings\":[],\"AuditLogEntries\":[],\"UserFacets\":[],\"EntityKey\":null}],\"ApplicationsSettings\":[{\"$id\":\"23\",\"Id\":\"00000000-0000-0000-0000-000000000000\",\"InheritSettingsId\":null,\"UserId\":null,\"ApplicationId\":\"700d72d5-946e-4fb0-aff2-75c4f61fd2c9\",\"DefaultUrl\":\"/Dashboards/Sales\",\"DockItems\":null,\"LastLoginContext\":null,\"CompanyRole\":null,\"Language\":\"en-NZ\",\"LanguageVariant\":null,\"OrganisationId\":null,\"Application\":null,\"ApplicationReference\":{\"EntityKey\":null},\"DerivedSettings\":[],\"BaseSettings\":null,\"BaseSettingsReference\":{\"EntityKey\":null},\"User\":{\"$ref\":\"1\"},\"UserReference\":{\"EntityKey\":null},\"Organisation\":null,\"OrganisationReference\":{\"EntityKey\":null},\"EntityKey\":null},{\"$id\":\"24\",\"Id\":\"00000000-0000-0000-0000-000000000000\",\"InheritSettingsId\":null,\"UserId\":null,\"ApplicationId\":\"06503493-67d9-4ccb-aa48-9c06649c5085\",\"DefaultUrl\":\"/\",\"DockItems\":null,\"LastLoginContext\":null,\"CompanyRole\":null,\"Language\":null,\"LanguageVariant\":null,\"OrganisationId\":null,\"Application\":null,\"ApplicationReference\":{\"EntityKey\":null},\"DerivedSettings\":[],\"BaseSettings\":null,\"BaseSettingsReference\":{\"EntityKey\":null},\"User\":{\"$ref\":\"1\"},\"UserReference\":{\"EntityKey\":null},\"Organisation\":null,\"OrganisationReference\":{\"EntityKey\":null},\"EntityKey\":null}],\"UsersSettings\":[],\"RequestLogs\":[],\"ResourcePermissions\":[],\"AuditLogEntries\":[],\"UserFacets\":[],\"ApiKey\":null,\"ApiUserCreatedByUser\":null,\"EntityKey\":null}");

            User user = null;

            var startedAt = DateTime.Now;
            for (var i = 0; i < 100; i++)
            {
                bytes = serializer.Serialize(userObject);
                user = serializer.Deserialize(bytes, typeof(User)) as User;
            }

            var endedAt = DateTime.Now;

            var timeTaken = (endedAt - startedAt).Milliseconds / 100.0;

            Assert.IsNotNull(user);
            Assert.AreEqual("support@vinsight.net", user.UserName);
            Assert.AreEqual("Vinsight", user.FirstName);

            Trace.WriteLine(name + ": " + timeTaken + " ms, " + bytes.Length + " bytes");
        }

        [TestMethod]
        public void TestingSerializationSpeed()
        {

            TestSerializer(new ClrBinarySerializer(), "CLRBinary");
            TestSerializer(new SSJsonSerializer(), "Service Stack");
            TestSerializer(new NewtonsoftJsonSerializer(), "Newtonsoft");


        }
    }
}
