using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using RTNS.Core.Model;

namespace RTNS.Core.Tests.Model
{
    public class SubscriptionTests
    {
        [Test]
        public void Constructor_SubscriberIsNull_ThrowsException()
        {
            Assert.That(() => 
                new Subscription(null, new[]{ new Topic("1") }), Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_TopicIsNull_ThrowsException()
        {
            Assert.That(() => 
                new Subscription(new Subscriber("id"), null), Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_TopicIsEmpty_ThrowsException()
        {
            Assert.That(() =>
                new Subscription(new Subscriber("id"), new Topic[0]), Throws.ArgumentException);
        }
    }
}
