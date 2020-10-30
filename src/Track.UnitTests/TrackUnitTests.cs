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
            var sut = e1.ToTrack(null, null, GetPropertyInfos(f));
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
            var sut = items.ToTrackItems(null, GetPropertyInfos(f));

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
            var sut = items.ToTrackItems(null, GetPropertyInfos(f));

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

        [TestMethod]
        public void ShouldGetCollection()
        {
            var items = new[] { new E1 { P1 = "A" }, null, new E1 { P1 = "B" } };
            var sut = items.ToTrackItems();
            var modified = sut.GetCollection();
            Assert.AreEqual(3, modified.Count);
        }

        [TestMethod]
        public void ShouldNotifyItemPropertyChanged()
        {
            var items = new[] { new E1 { P1 = "A" }, null, new E1 { P1 = "B" } };
            var sut = items.ToTrackItems();
            int called = 0;
            sut.ItemPropertyChanged += (s, e) => called++;
            sut[0].Modified.P1 = "S";
            Assert.AreEqual(1, called);
        }
        [TestMethod]
        public void ShouldAcceptChanges()
        {
            var item = new E1 { P1 = "A" };
            var sut = new TrackItem<E1>();
            sut.Initialise(item);
            sut.Modified.P1 = "B";

            Assert.AreEqual("A", sut.Original.P1);
            sut.AcceptChanges();
            Assert.AreEqual("B", sut.Original.P1);
        }

        public class E1 : INotifyPropertyChanged, ICloneable
        {
            private string _p1;

            public string P1
            {
                get => _p1;
                set
                {
                    _p1 = value;
                    OnPropertyChanged();
                }
            }

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