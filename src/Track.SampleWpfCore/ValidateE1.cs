namespace Track.SampleWpfCore
{
    public class ValidateE1 : Validate<E1>
    {
        public override void OnRefreshErrors()
        {
            IsRequired(w => w.P1);
        }
    }
}