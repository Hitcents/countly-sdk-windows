using CountlySDK.Entities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountlyTests
{
    [TestFixture]
    public class CustomInfoTests
    {
        [Test]
        public void Add()
        {
            var info = new CustomInfo();
            info.Add("Test", "B");
            Assert.AreEqual(info["Test"], "B");
        }

        [Test]
        public void Indexer()
        {
            var info = new CustomInfo();
            info["Test"] = "B";
            Assert.AreEqual(info["Test"], "B");
        }

        [Test]
        public void Remove()
        {
            var info = new CustomInfo();
            info["Test"] = "B";
            info.Remove("Test");
            Assert.AreEqual(0, info.Items.Count);
        }

        [Test]
        public void IndexerNullRemoves()
        {
            var info = new CustomInfo();
            info["Test"] = "B";
            info["Test"] = null;
            Assert.AreEqual(0, info.Items.Count);
        }

        [Test]
        public void AddChanged()
        {
            bool changed = false;
            var info = new CustomInfo();
            info.CollectionChanged += () => changed = true;
            info.Add("Test", "B");
            Assert.IsTrue(changed);
        }

        [Test]
        public void AddSameDoesNotChange()
        {
            bool changed = false;
            var info = new CustomInfo();
            info.Add("Test", "B");
            info.CollectionChanged += () => changed = true;
            info.Add("Test", "B");
            Assert.IsFalse(changed);
        }

        [Test]
        public void IndexerSameDoesNotChange()
        {
            bool changed = false;
            var info = new CustomInfo();
            info["Test"] = "B";
            info.CollectionChanged += () => changed = true;
            info["Test"] = "B";
            Assert.IsFalse(changed);
        }
    }
}
