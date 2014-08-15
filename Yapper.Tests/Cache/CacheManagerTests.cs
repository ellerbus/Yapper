using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Yapper.Cache;

namespace Yapper.Tests.Cache
{
    [TestClass]
    public class CacheManagerTests
    {
        [TestMethod]
        public void CacheManager_Should_AddAndExpire()
        {
            CacheManager sut = new CacheManager();

            var user = new { ID =1,Name="Me" };

            CacheKey key = new CacheKey("GETUSER-"+user.ID, new[] { "Users", "User-" + user.ID });

            CachePolicy cp = new CachePolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(60) };

            sut.Add(key, user, cp);

            Assert.IsNotNull(sut.Get(key));

            sut.Expire("Users");

            Assert.IsNull(sut.Get(key));
        }

        [TestMethod]
        public void CacheManager_Should_AddAndRemove()
        {
            CacheManager sut = new CacheManager();

            var user = new { ID = 2, Name = "Me" };

            CacheKey key = new CacheKey("GETUSER-" + user.ID, new[] { "Users", "User-" + user.ID });

            CachePolicy cp = new CachePolicy { AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(60) };

            sut.Add(key, user, cp);

            Assert.IsNotNull(sut.Get(key));

            object userRemoved = sut.Remove(key);

            Assert.AreEqual(user, userRemoved);
        }
    }
}
