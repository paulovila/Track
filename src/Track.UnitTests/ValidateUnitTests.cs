using System;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Track.UnitTests
{
    [TestClass]
    public class ValidateUnitTests
    {
        [TestMethod]
        public void ShouldValidate()
        {
            var sut = new Test1Validate { Item = new E().ToTrack() };

            Assert.IsTrue(sut.HasValidations);

            sut.Item.Modified.P1 = "A";
            Assert.IsFalse(sut.HasValidations);

            sut.Item.Modified.P1 = null;
            Assert.IsTrue(sut.HasValidations);
        }

        public class Test1Validate : Validate<E>
        {
            public override void OnRefreshErrors()
            {
                IsRequired(w => w.P1);
            }
        }

        public class E : INotifyPropertyChanged, ICloneable
        {
            private string _p1;
            public event PropertyChangedEventHandler PropertyChanged;

            public string P1
            {
                get => _p1;
                set
                {
                    if (_p1 == value) return;
                    _p1 = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(P1)));
                }
            }

            public object Clone() => MemberwiseClone();
        }
    }
}