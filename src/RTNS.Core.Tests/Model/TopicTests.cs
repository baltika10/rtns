using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using RTNS.Core.Model;

namespace RTNS.Core.Tests.Model
{
    public class TopicTests
    {
        [Test]
        public void Constructor_NameIsNull_ThrowsException()
        {
            Assert.That(() => new Topic(null), Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_NameIsEmpty_ThrowsException()
        {
            Assert.That(() => new Topic(string.Empty), Throws.ArgumentException);
        }

        [Test]
        public void Constructor_NameIsWhitespace_ThrowsException()
        {
            Assert.That(() => new Topic(" "), Throws.ArgumentException);
        }
    }
}
