using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Track.UnitTests
{
    [TestClass]
    public class TrackUnitTests
    {
        public static PropertyInfo GetPropertyInfo<T>(
             Expression<Func<T, object>> expression) =>
            expression.Body is UnaryExpression body ?
                (body.Operand is MemberExpression operand ? operand.Member : null) as PropertyInfo
                :
                (expression.Body is MemberExpression body1 ? body1.Member : null) as PropertyInfo;

        public static PropertyInfo[] GetPropertyInfos<T>(Expression<Func<T, object>>[] expressions) =>
            expressions.Select(GetPropertyInfo).ToArray();

        [TestMethod]
        public void SomePropertiesShouldHaveChanges()
        {
            var e1 = new E1 { P1 = "A" };
            Expression<Func<E1, object>>[] f = { w => w.P1 };
            var sut = e1.ToTrack(GetPropertyInfos(f));
            sut.Modified.P3 = "G";
            Assert.AreEqual(sut.Modified.P1, e1.P1);
            Assert.IsFalse(sut.HasChanges);
            sut.Modified.P1 = "A1";
            Assert.IsTrue(sut.HasChanges);
            sut.Modified.P1 = "A";
            Assert.IsFalse(sut.HasChanges);
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
        public void SomePropertiesShouldHaveChangesInCollection()
        {
            var items = new[] { new E1 { P1 = "A" }, new E1 { P1 = "A" } };
            var f = new Expression<Func<E1, object>>[] { w => w.P1 };
            var sut = items.ToTrackItems(GetPropertyInfos(f));

            Assert.IsFalse(sut.HasCollectionChanges);
            sut[0].Modified.P1 = "A1";
            Assert.IsTrue(sut.HasCollectionChanges);
            sut[0].Modified.P1 = "A";
            Assert.IsFalse(sut.HasCollectionChanges);
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

        [TestMethod]
        public void SomePropertiesShouldHaveChangesInCollectionWithNullElement()
        {
            var items = new[] { new E1 { P1 = "A" }, null, new E1 { P1 = "A" } };
            var f = new Expression<Func<E1, object>>[] { w => w.P1 };
            var sut = items.ToTrackItems(GetPropertyInfos(f));

            Assert.IsFalse(sut.HasCollectionChanges);
            sut[0].Modified.P1 = "A1";
            Assert.IsTrue(sut.HasCollectionChanges);
            sut[0].Modified.P1 = "A";
            Assert.IsFalse(sut.HasCollectionChanges);
        }
        [TestMethod]
        public void ShouldHaveChangesInCollectionWithNullElement()
        {
            var items = new[] { new E1 { P1 = "A" }, null, new E1 { P1 = "A", E = new E1() } };
            var sut = items.ToTrackItems();
            Assert.IsFalse(sut.HasCollectionChanges);
            sut[0].Modified.P1 = "A1";
            Assert.IsTrue(sut.HasCollectionChanges);
            sut[0].Modified.P1 = "A";
            Assert.IsFalse(sut.HasCollectionChanges);
        }
        [TestMethod]
        public void ShouldNotHaveChangesAfterAnElementIsReplaced()
        {
            var items = new[] { new E1 { P1 = "A" }, null, new E1 { P1 = "B" } };
            var sut = items.ToTrackItems();
            Assert.IsFalse(sut.HasCollectionChanges);
            sut.Remove(sut.Last());
            Assert.IsTrue(sut.HasCollectionChanges);
            var item3 = new E1 { P1 = "X" }.ToTrack();
            sut.Add(item3);
            Assert.IsTrue(sut.HasCollectionChanges);
            item3.Modified.P1 = "B";
            Assert.IsFalse(sut.HasCollectionChanges);
        }
        [TestMethod]
        public void ShouldHaveSomePropertiesChanged()
        {
            var items = new[] { new E1 { P1 = "A" }, null, new E1 { P1 = "B" } };
            var sut = items.ToTrackItems();
            Assert.AreEqual(0, sut.ModifiedPropertiesCount());
            sut[0].Modified.P1 = "C";
            Assert.AreEqual(1, sut.ModifiedPropertiesCount());
            sut[2].Modified.P1 = "C";
            Assert.AreEqual(2, sut.ModifiedPropertiesCount());
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