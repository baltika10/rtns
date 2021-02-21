using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using RTNS.Core.Model;

namespace RTNS.Core.Tests.Model
{
    public class SubscriberTests
    {
        [Test]
        public void Constructor_IdIsNull_ThrowsException()
        {
            Assert.That(() => new Subscriber(null), Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_IdIsEmpty_ThrowsException()
        {
            Assert.That(() => new Subscriber(string.Empty), Throws.ArgumentException);
        }

        [Test]
        public void Constructor_IdIsWhitespace_ThrowsException()
        {
            Assert.That(() => new Subscriber(" "), Throws.ArgumentException);
        }
    }
}
