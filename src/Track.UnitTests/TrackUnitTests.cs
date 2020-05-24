using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Track.UnitTests
{
    [TestClass]
    public class TrackUnitTests
    {
        [TestMethod]
        public void SomePropertiesShouldHaveChanges()
        {
            var e1 = new E1 { P1 = "A" };
            Expression<Func<E1, object>>[] f = { w => w.P1 };
            var sut = e1.ToTrack(f);
            sut.Modified.P3 = "G";
            Assert.AreEqual(sut.Modified.P1, e1.P1);
            Assert.IsFalse(sut.HasChanges);
            sut.Modified.P1 = "A1";
            Assert.IsTrue(sut.HasChanges);
            sut.Modified.P1 = "A";
            Assert.IsFalse(sut.HasChanges);
        }
        [TestMethod]
        public void SomePropertiesShouldHaveChangesInCollection()
        {
            var items = new[] { new E1 { P1 = "A" }, new E1 { P1 = "A" } };
            var sut = items.ToTrackItems(new Expression<Func<E1, object>>[] { w => w.P1 });

            Assert.IsFalse(sut.HasCollectionChanges);
            sut[0].Modified.P1 = "A1";
            Assert.IsTrue(sut.HasCollectionChanges);
            sut[0].Modified.P1 = "A";
            Assert.IsFalse(sut.HasCollectionChanges);
        }
        [TestMethod]
        public void ShouldHaveChanges()
        {
            var e1 = new E1 { P1 = "A" };
            var sut = e1.ToTrack();
            Assert.AreEqual(sut.Modified.P1, e1.P1);
            Assert.IsFalse(sut.HasChanges);
            sut.Modified.P1 = "A1";
            Assert.IsTrue(sut.HasChanges);
            sut.Modified.P1 = "A";
            Assert.IsFalse(sut.HasChanges);
        }
        [TestMethod]
        public void ShouldHaveChangesInCollection()
        {
            var items = new[] { new E1 { P1 = "A" }, new E1 { P1 = "A", E = new E1() } };
            var sut = items.ToTrackItems();
            Assert.IsFalse(sut.HasCollectionChanges);
            sut[0].Modified.P1 = "A1";
            Assert.IsTrue(sut.HasCollectionChanges);
            sut[0].Modified.P1 = "A";
            Assert.IsFalse(sut.HasCollectionChanges);
        }
        public class E1 : INotifyPropertyChanged, ICloneable
        {
            public string P1 { get; set; }
            public string P2 { get; }
            public string P3 { get; set; }
            public E1 E { get; set; }
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            public object Clone() => this.MemberwiseClone();
        }
    }
}